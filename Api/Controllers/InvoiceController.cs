using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return Ok(invoices);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
        {
            try
            {
                if (invoice.Date == default(DateTime))
                {
                    invoice.Date = DateTime.Now;
                }

                // مدیریت فروشنده
                if (invoice.Seller != null && invoice.Seller.Id == 0)
                {
                    _context.Parties.Add(invoice.Seller);
                    await _context.SaveChangesAsync();
                    invoice.SellerId = invoice.Seller.Id;
                }
                else if (invoice.Seller != null)
                {
                    invoice.SellerId = invoice.Seller.Id;
                }
                invoice.Seller = null; // جلوگیری از ثبت تکراری

                // مدیریت خریدار
                if (invoice.Buyer != null && invoice.Buyer.Id == 0)
                {
                    _context.Parties.Add(invoice.Buyer);
                    await _context.SaveChangesAsync();
                    invoice.BuyerId = invoice.Buyer.Id;
                }
                else if (invoice.Buyer != null)
                {
                    invoice.BuyerId = invoice.Buyer.Id;
                }
                invoice.Buyer = null; // جلوگیری از ثبت تکراری

                // ⚠️ خط محاسبه TotalPrice حذف شد (چون فقط خواندنی است و خودش محاسبه می‌شود)

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "فاکتور با موفقیت ذخیره شد", InvoiceId = invoice.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "خطا در ذخیره‌سازی", Error = ex.Message, InnerError = ex.InnerException?.Message });
            }
        }
    }
}