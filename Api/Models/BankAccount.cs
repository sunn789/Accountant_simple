namespace Api.Models;

public class BankAccount
{
    public int Id { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string? AccountHolder { get; set; }

    public string? AccountNumber { get; set; }
    public string? ShebaNumber { get; set; }
    public string? CardNumber { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}