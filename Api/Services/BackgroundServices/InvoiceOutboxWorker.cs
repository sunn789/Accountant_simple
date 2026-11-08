using System.Text.Json;
using Api.Data;
using Api.Models;
using Api.Services.Invoice;
using Api.Services.Pdf;
using Api.Services.Sms;
using Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace Api.BackgroundServices;

public class InvoiceOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvoiceOutboxWorker> _logger;
    private readonly IConfiguration _configuration;

    public InvoiceOutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<InvoiceOutboxWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Invoice outbox worker failed."
                );
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken
            );
        }
    }

    private async Task ProcessPendingMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var message = await db.OutboxMessages
            .Where(x =>
                x.Type == "IssueInvoice" &&
                x.Status == "Pending" &&
                (x.NextAttemptAt == null ||
                 x.NextAttemptAt <= DateTime.UtcNow))
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (message is null)
            return;

        message.Status = "Processing";

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await ProcessInvoiceAsync(
                scope.ServiceProvider,
                db,
                message,
                cancellationToken
            );

            message.Status = "Completed";
            message.ProcessedAt = DateTime.UtcNow;
            message.Payload = string.Empty;
            message.LastError = null;

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.LastError = ex.Message;

            if (message.RetryCount >= 5)
            {
                message.Status = "Failed";
            }
            else
            {
                message.Status = "Pending";

                var delayMinutes = Math.Min(
                    Math.Pow(2, message.RetryCount),
                    60
                );

                message.NextAttemptAt =
                    DateTime.UtcNow.AddMinutes(delayMinutes);
            }

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Processing invoice outbox message {MessageId} failed.",
                message.Id
            );
        }
    }

    private async Task ProcessInvoiceAsync(
        IServiceProvider services,
        AppDbContext db,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload =
            JsonSerializer.Deserialize<
                InvoiceIssuingService.IssueInvoicePayload
            >(message.Payload)
            ?? throw new InvalidOperationException(
                "Invalid invoice outbox payload."
            );

        var invoice = await db.Invoices
            .Include(x => x.Seller)
            .Include(x => x.Buyer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == payload.InvoiceId,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                "Invoice not found."
            );

        var company = await db.CompanyProfiles
            .Include(x => x.DefaultBankAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "Company profile is not configured."
            );

        var pdfGenerator =
            services.GetRequiredService<IInvoicePdfGenerator>();

        var storage =
            services.GetRequiredService<InvoiceFileStorage>();

        var smsSender =
            services.GetRequiredService<ISmsSender>();

        var document = await db.InvoiceDocuments
            .FirstOrDefaultAsync(
                x => x.InvoiceId == invoice.Id,
                cancellationToken
            );

        if (document is null)
        {
            var pdf = await pdfGenerator.GenerateAsync(
                invoice,
                company,
                cancellationToken
            );

            var saved = await storage.SaveAsync(
                invoice.Id,
                pdf,
                cancellationToken
            );

            document = new InvoiceDocument
            {
                InvoiceId = invoice.Id,
                FilePath = saved.RelativePath,
                FileHash = saved.Hash,
                AccessTokenHash =
                    InvoiceIssuingService.HashToken(
                        payload.AccessToken
                    ),
                GeneratedAt = DateTime.UtcNow,
                IsActive = true
            };

            db.InvoiceDocuments.Add(document);

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!document.IsActive)
            throw new InvalidOperationException(
                "Invoice document access has been disabled."
            );

        var mobile = invoice.Buyer!.Mobile;

        if (string.IsNullOrWhiteSpace(mobile))
            throw new InvalidOperationException(
                "Buyer mobile number is not available."
            );

        var baseUrl = _configuration["PublicInvoice:BaseUrl"]
            ?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException(
                "Public invoice base URL is not configured."
            );

        var url = $"{baseUrl}/i/{payload.AccessToken}";

        var messageText =
            $"خریدار گرامی، فاکتور شماره {invoice.InvoiceNumber} " +
            $"برای شما صادر شد. مشاهده و دریافت فاکتور: {url}";

        var smsLog = new InvoiceSmsLog
        {
            InvoiceId = invoice.Id,
            Mobile = mobile,
            Message = messageText,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.InvoiceSmsLogs.Add(smsLog);

        await db.SaveChangesAsync(cancellationToken);

        var result = await smsSender.SendAsync(
            mobile,
            messageText,
            cancellationToken
        );

        if (!result.Success)
        {
            smsLog.Status = "Failed";
            smsLog.ErrorMessage = result.ErrorMessage;

            await db.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                result.ErrorMessage ?? "SMS sending failed."
            );
        }

        smsLog.Status = "Sent";
        smsLog.ProviderMessageId = result.ProviderMessageId;
        smsLog.SentAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}