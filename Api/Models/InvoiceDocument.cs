namespace Api.Models;

public class InvoiceDocument
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;

    // فقط Hash توکن در دیتابیس ذخیره می‌شود.
    public string AccessTokenHash { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}