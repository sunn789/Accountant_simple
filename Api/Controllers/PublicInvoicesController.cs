using Api.Data;
using Api.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Services.Invoice;

namespace Api.Controllers;

[ApiController]
[Route("i")]
public class PublicInvoicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly InvoiceFileStorage _storage;

    public PublicInvoicesController(
        AppDbContext context,
        InvoiceFileStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> ViewInvoice(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length != 64)
            return NotFound();

        var tokenHash = InvoiceIssuingService.HashToken(token);

        var document = await _context.InvoiceDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.AccessTokenHash == tokenHash &&
                    x.IsActive &&
                    (x.ExpiresAt == null ||
                     x.ExpiresAt > DateTime.UtcNow),
                cancellationToken
            );

        if (document is null)
            return NotFound("لینک فاکتور معتبر نیست یا منقضی شده است.");

        var file = await _storage.ReadAsync(
            document.FilePath,
            cancellationToken
        );

        if (file is null)
            return NotFound("فایل فاکتور پیدا نشد.");

        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        return File(
            file,
            "application/pdf",
            $"Invoice-{document.InvoiceId}.pdf"
        );
    }
}