using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Invoice;

public class InvoiceIssuingService
{
    private readonly AppDbContext _context;

    public InvoiceIssuingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnqueueAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Invoices
            .AnyAsync(
                i => i.Id == invoiceId,
                cancellationToken
            );

        if (!exists)
            throw new InvalidOperationException(
                "Invoice does not exist."
            );

        var alreadyQueued = await _context.OutboxMessages
            .AnyAsync(
                x => x.Type == "IssueInvoice"
                     && x.RelatedEntityId == invoiceId,
                cancellationToken
            );

        if (alreadyQueued)
            return;

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToHexString(tokenBytes);

        var payload = JsonSerializer.Serialize(
            new IssueInvoicePayload(invoiceId, token)
        );

        _context.OutboxMessages.Add(new OutboxMessage
        {
            Type = "IssueInvoice",
            Payload = payload,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            RelatedEntityId = invoiceId,
            NextAttemptAt = DateTime.UtcNow
        });
    }

    public static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        return Convert.ToHexString(
            SHA256.HashData(bytes)
        );
    }

    public record IssueInvoicePayload(
        int InvoiceId,
        string AccessToken
    );
}