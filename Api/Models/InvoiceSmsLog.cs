namespace Api.Models;

public class InvoiceSmsLog
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public string Mobile { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Pending, Sent, Failed
    public string Status { get; set; } = "Pending";

    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}