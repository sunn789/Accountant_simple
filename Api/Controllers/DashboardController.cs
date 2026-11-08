using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.DTOs;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context) => _context = context;

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ۱. تعداد کل فاکتورها
            var totalInvoices = await _context.Invoices.CountAsync();

            // ۲. تعداد فاکتورهای امروز
            var todayInvoices = await _context.Invoices
                .CountAsync(i => i.Date >= today && i.Date < tomorrow);

            // ۳. مجموع فروش کل (با استفاده از آیتم‌ها و فرمول محاسبه TotalPrice)
            var totalRevenue = await _context.InvoiceItems
                .SumAsync(item => (decimal?)((item.Quantity * item.UnitPrice) - item.Discount + item.Tax)) ?? 0;

            // ۴. فروش امروز
            var todayRevenue = await _context.Invoices
                .Where(i => i.Date >= today && i.Date < tomorrow)
                .SelectMany(i => i.Items)
                .SumAsync(item => (decimal?)((item.Quantity * item.UnitPrice) - item.Discount + item.Tax)) ?? 0;

            // ۵. تعداد مشتریان یکتا (بر اساس BuyerId)
            var totalCustomers = await _context.Invoices
                .Select(i => i.BuyerId)
                .Distinct()
                .CountAsync();

            var stats = new DashboardStatsDto
            {
                TotalInvoices = totalInvoices,
                TotalRevenue = totalRevenue,
                TodayInvoices = todayInvoices,
                TodayRevenue = todayRevenue,
                TotalCustomers = totalCustomers
            };

            return Ok(stats);
        }
    }
}