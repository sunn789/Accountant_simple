using Api.BackgroundServices;
using Api.Data;
using Api.Services.AmountToWords;
using Api.Services.Invoice;
using Api.Services.Pdf;
using Api.Services.Sms;
using Api.Storage;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ۱. افزودن سرویس‌های کنترلرها
builder.Services.AddControllers();

/// ۲. پیکربندی دیتابیس SQLite
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory); // ساخت پوشه در صورت عدم وجود
}

var dbPath = Path.Combine(dataDirectory, "invoices.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ۳. پیکربندی CORS برای اجازه دادن به فرانت‌اند React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // آدرس‌های React
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ۴. افزودن سرویس‌های OpenAPI (همان کد قبلی شما)
builder.Services.AddOpenApi();


builder.Services.AddSingleton<IAmountToWords, PersianAmountToWords>();

builder.Services.AddScoped<InvoiceIssuingService>();

builder.Services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();

builder.Services.AddSingleton<InvoiceFileStorage>();

builder.Services.AddHttpClient<ISmsSender, SmsSender>();

builder.Services.AddHostedService<InvoiceOutboxWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // اعمال مایگریشن‌های دات‌نت
}

// ۵. پیکربندی خط لوله درخواست‌ها (Middleware Pipeline)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // فعال‌سازی endpoint تولید مستندات OpenAPI

    // (اختیاری) برای استفاده از Swagger UI جهت تست راحت‌تر API
    // برای فعال‌سازی، پکیج Swashbuckle.AspNetCore.SwaggerUi را نصب کنید
    app.MapScalarApiReference(options =>
 {
     options
         .WithTitle("API سیستم فاکتور من") // عنوان دلخواه
         .WithClassicLayout()           // استفاده از چیدمان کلاسیک
         .ForceDarkMode()               // اجبار به حالت تاریک
         .ExpandAllTags()               // باز کردن همه تگ‌ها به صورت پیش‌فرض
         .SortTagsAlphabetically();     // مرتب‌سازی الفبایی تگ‌ها
 });
}

//app.UseHttpsRedirection();

// ۶. فعال‌سازی CORS (باید قبل از UseAuthorization و MapControllers باشد)
app.UseCors("AllowReactApp");

// ۷. فعال‌سازی مسیریابی کنترلرها
app.MapControllers();

app.Run();