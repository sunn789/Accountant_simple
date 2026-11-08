using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.DTOs;
using System.Globalization;
using Api.Services.Invoice;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly InvoiceIssuingService _invoiceIssuingService;

        public InvoicesController(AppDbContext context, InvoiceIssuingService invoiceIssuingService)
        {
            _context = context;
            _invoiceIssuingService = invoiceIssuingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? buyerName = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null)
        {
            ///
            /// temp
            ///
            Console.WriteLine($"Received EndDate: {endDate:yyyy-MM-dd HH:mm:ss}");


            var query = _context.Invoices
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .Include(i => i.Items)
                .AsQueryable();

            /// temp
            var invoiceDates = await _context.Invoices
                .OrderBy(i => i.Date)
                .Select(i => new { i.Id, i.Date })
                .ToListAsync();

            foreach (var invoice in invoiceDates)
            {
                Console.WriteLine(
                    $"Invoice: {invoice.Id} | Date: {invoice.Date:yyyy-MM-dd HH:mm:ss}"
                );
            }
            ///
            // ۱. اعمال فیلتر تاریخ (اگر کاربر تاریخی انتخاب کرده باشد)
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(i => i.Date >= start);
            }

            if (endDate.HasValue)
            {
                var endExclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(i => i.Date < endExclusive);
            }
            // فیلتر نام خریدار
            if (!string.IsNullOrWhiteSpace(buyerName))
            {
                var searchName = buyerName.Trim();

                query = query.Where(i =>
                    i.Buyer!.Name.Contains(searchName)
                );
            }

            // فیلتر حداقل مبلغ
            if (minAmount.HasValue)
            {
                query = query.Where(i =>
                    i.Items.Sum(item =>
                        (item.Quantity * item.UnitPrice)
                        - item.Discount
                        + item.Tax
                    ) >= minAmount.Value
                );
            }

            // فیلتر حداکثر مبلغ
            if (maxAmount.HasValue)
            {
                query = query.Where(i =>
                    i.Items.Sum(item =>
                        (item.Quantity * item.UnitPrice)
                        - item.Discount
                        + item.Tax
                    ) <= maxAmount.Value
                );
            }
            // ۲. محاسبه کل رکوردها قبل از صفحه‌بندی
            var totalCount = await query.CountAsync();

            // ۳. اعمال صفحه‌بندی و مرتب‌سازی
            var invoices = await query
                .OrderByDescending(i => i.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<Invoice>
            {
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                Items = invoices
            };

            return Ok(result);
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInvoiceById(
            int id,
            CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(
                    i => i.Id == id,
                    cancellationToken
                );

            if (invoice is null)
                return NotFound(new
                {
                    message = "فاکتور مورد نظر پیدا نشد."
                });

            return Ok(invoice);
        }


        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary()
        {
            // منطقه زمانی کسب‌وکار: تهران
            var tehranTimeZone = GetTehranTimeZone();

            // تاریخ و ساعت فعلی تهران
            var tehranNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                tehranTimeZone
            );

            // ابتدای امروز و ابتدای فردا
            var startOfDay = DateTime.SpecifyKind(
                tehranNow.Date,
                DateTimeKind.Unspecified
            );

            var endOfDay = startOfDay.AddDays(1);

            // فاکتورهای امروز
            var todayInvoices = _context.Invoices
                .AsNoTracking()
                .Where(i =>
                    i.Date >= startOfDay &&
                    i.Date < endOfDay
                );

            // تعداد فاکتورهای امروز
            var invoiceCount = await todayInvoices.CountAsync();

            // مجموع مبلغ اقلام فاکتورهای امروز
            // معادل TotalAmount در مدل Invoice
            var totalAmount = await todayInvoices
                .SelectMany(i => i.Items)
                .SumAsync(item =>
                    (item.Quantity * item.UnitPrice)
                    - item.Discount
                    + item.Tax
                );

            // تبدیل تاریخ میلادی به شمسی
            var persianCalendar = new PersianCalendar();

            var jalaliDate = string.Format(
                "{0:0000}/{1:00}/{2:00}",
                persianCalendar.GetYear(tehranNow),
                persianCalendar.GetMonth(tehranNow),
                persianCalendar.GetDayOfMonth(tehranNow)
            );

            return Ok(new
            {
                Date = startOfDay.ToString("yyyy-MM-dd"),
                JalaliDate = jalaliDate,
                InvoiceCount = invoiceCount,
                TotalAmount = totalAmount
            });
        }

        // دریافت منطقه زمانی تهران در ویندوز و لینوکس
        private static TimeZoneInfo GetTehranTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Iran Standard Time"
                );
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(
    [FromBody] Invoice invoice,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken
                );

            try
            {
                if (invoice.Date == default)
                {
                    invoice.Date = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        GetTehranTimeZone()
                    );
                }

                // مدیریت فروشنده
                if (invoice.Seller != null && invoice.Seller.Id == 0)
                {
                    _context.Parties.Add(invoice.Seller);

                    await _context.SaveChangesAsync(cancellationToken);

                    invoice.SellerId = invoice.Seller.Id;
                }
                else if (invoice.Seller != null)
                {
                    invoice.SellerId = invoice.Seller.Id;
                }

                invoice.Seller = null;

                // مدیریت خریدار
                if (invoice.Buyer != null && invoice.Buyer.Id == 0)
                {
                    _context.Parties.Add(invoice.Buyer);

                    await _context.SaveChangesAsync(cancellationToken);

                    invoice.BuyerId = invoice.Buyer.Id;
                }
                else if (invoice.Buyer != null)
                {
                    invoice.BuyerId = invoice.Buyer.Id;
                }

                invoice.Buyer = null;

                // ثبت فاکتور
                _context.Invoices.Add(invoice);

                await _context.SaveChangesAsync(cancellationToken);

                // ثبت عملیات صدور در Outbox
                await _invoiceIssuingService.EnqueueAsync(
                    invoice.Id,
                    cancellationToken
                );

                // ذخیره پیام Outbox
                await _context.SaveChangesAsync(cancellationToken);

                // تأیید نهایی تمام عملیات
                await transaction.CommitAsync(cancellationToken);

                return Ok(new
                {
                    Message = "فاکتور با موفقیت ذخیره شد",
                    InvoiceId = invoice.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(
                    CancellationToken.None
                );

                return BadRequest(new
                {
                    Message = "خطا در ذخیره‌سازی فاکتور",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }
    }
}