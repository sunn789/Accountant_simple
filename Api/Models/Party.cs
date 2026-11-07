namespace Api.Models;
// 1. مدل طرفین (فروشنده و خریدار)

public class Party
{
    public int Id { get; set; }
    public string Name { get; set; } // نام شخص حقیقی/حقوقی
    public string EconomicCode { get; set; } // شماره اقتصادی
    public string RegistrationNumber { get; set; } // شماره ثبت
    public string NationalId { get; set; } // شماره ملی
    public string Province { get; set; } // استان
    public string City { get; set; } // شهر
    public string Address { get; set; } // نشانی کامل
    public string PostalCode { get; set; } // کد پستی
    public string Phone { get; set; } // تلفن
    public string Mobile { get; set; } // همراه
}
