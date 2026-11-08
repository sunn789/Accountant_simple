namespace Api.Models;

public class CompanyProfile
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? EconomicCode { get; set; }
    public string? NationalId { get; set; }
    public string? RegistrationNumber { get; set; }

    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }

    public string? Phone { get; set; }
    public string? Mobile { get; set; }

    // مسیر نسبی فایل‌ها در فضای ذخیره‌سازی خصوصی
    public string? LogoPath { get; set; }
    public string? StampPath { get; set; }
    public string? SignaturePath { get; set; }

    public int? DefaultBankAccountId { get; set; }
    public BankAccount? DefaultBankAccount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}