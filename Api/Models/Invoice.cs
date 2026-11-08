namespace Api.Models;
// 1. مدل طرفین (فروشنده و خریدار)




// 3. مدل خود فاکتور
public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }  = string.Empty;
    public DateTime Date { get; set; } // تاریخ
    
    public int SellerId { get; set; }
    public Party? Seller { get; set; } // مشخصات فروشنده
    
    public int BuyerId { get; set; }
    public Party? Buyer { get; set; } // مشخصات خریدار
    
    public List<InvoiceItem> Items { get; set; } = new(); // لیست کالاها
    
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice); // جمع کل قابل پرداخت
    public string PaymentType { get; set; }  = string.Empty;// نقدی / غیر نقدی
    public string Notes { get; set; } = string.Empty; // توضیحات
}