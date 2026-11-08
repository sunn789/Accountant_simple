namespace Api.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalInvoices { get; set; } // تعداد کل فاکتورها
        public decimal TotalRevenue { get; set; } // مجموع فروش کل (ریال)
        public int TodayInvoices { get; set; } // تعداد فاکتورهای امروز
        public decimal TodayRevenue { get; set; } // فروش امروز (ریال)
        public int TotalCustomers { get; set; } // تعداد مشتریان (خریداران) یکتا
    }
}