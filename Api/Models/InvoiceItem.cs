namespace Api.Models;
// 1. مدل طرفین (فروشنده و خریدار)


// 2. مدل اقلام فاکتور
public class InvoiceItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty; // شرح کالا یا خدمات
    public int Quantity { get; set; } // تعداد
    public decimal UnitPrice { get; set; } // مبلغ واحد
    public decimal Discount { get; set; } // تخفیف
    public decimal Tax { get; set; } // مالیات
    public decimal TotalPrice => (Quantity * UnitPrice) - Discount + Tax; // جمع کل
}