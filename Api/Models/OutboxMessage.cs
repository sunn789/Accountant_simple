namespace Api.Models;

public class OutboxMessage
{
    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    // Pending, Processing, Completed, Failed
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }
    public string? LastError { get; set; }

    public DateTime? NextAttemptAt { get; set; }
    public int? RelatedEntityId { get; set; }
}