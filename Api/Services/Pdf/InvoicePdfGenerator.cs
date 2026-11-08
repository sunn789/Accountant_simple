using System.Globalization;
using System.Net;
using Api.Models;
using Api.Services.AmountToWords;
using Microsoft.Playwright;
using Microsoft.AspNetCore.Hosting;
using InvoiceEntity = Api.Models.Invoice;

namespace Api.Services.Pdf;

public class InvoicePdfGenerator : IInvoicePdfGenerator
{
    private readonly IAmountToWords _amountToWords;
    private readonly IWebHostEnvironment environment;

    public InvoicePdfGenerator(IAmountToWords amountToWords, IWebHostEnvironment environment)
    {
        _amountToWords = amountToWords;
        this.environment = environment;
    }
    private string? GetImageDataUri(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var fullPath = Path.Combine(
            environment.WebRootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar)
        );

        if (!File.Exists(fullPath))
            return null;

        var bytes = File.ReadAllBytes(fullPath);

        var extension = Path.GetExtension(fullPath)
            .ToLowerInvariant();

        var mimeType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
    }
    private string GetFontDataUri(string fileName)
    {
        var fontPath = Path.Combine(
            environment.ContentRootPath,
            "Resources",
            "Fonts",
            fileName
        );

        if (!File.Exists(fontPath))
            throw new FileNotFoundException(
                $"Font file not found: {fontPath}"
            );

        var bytes = File.ReadAllBytes(fontPath);

        return Convert.ToBase64String(bytes);
    }
    public async Task<byte[]> GenerateAsync(
           InvoiceEntity invoice,
        CompanyProfile company,
        CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            }
        );

        var page = await browser.NewPageAsync(
            new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1200,
                    Height = 1600
                }
            }
        );

        var html = BuildHtml(invoice, company);

        await page.SetContentAsync(
            html,
            new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            }
        );

        return await page.PdfAsync(
            new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                PreferCSSPageSize = true,
                Margin = new Margin
                {
                    Top = "12mm",
                    Bottom = "12mm",
                    Right = "12mm",
                    Left = "12mm"
                }
            }
        );
    }

    private string BuildHtml(
        InvoiceEntity invoice,
        CompanyProfile company)
    {
        var culture = CultureInfo.GetCultureInfo("fa-IR");

        string Money(decimal value) =>
            value.ToString("#,##0", culture);

        string Text(string? value) =>
            WebUtility.HtmlEncode(value ?? "-");
        var regularFont = GetFontDataUri(
            "Vazirmatn-Regular.ttf"
        );

        var boldFont = GetFontDataUri(
            "Vazirmatn-Bold.ttf"
        );
        var rows = string.Join(
            "",
            invoice.Items.Select((item, index) =>
            {
                var total =
                    (item.Quantity * item.UnitPrice)
                    - item.Discount
                    + item.Tax;

                return $"""
                    <tr>
                        <td>{index + 1}</td>
                        <td>{Text(item.Description)}</td>
                        <td>{Money(item.Quantity)}</td>
                        <td>{Money(item.UnitPrice)}</td>
                        <td>{Money(item.Discount)}</td>
                        <td>{Money(item.Tax)}</td>
                        <td>{Money(total)}</td>
                    </tr>
                    """;
            })
        );

        var subtotal = invoice.Items.Sum(
            item => item.Quantity * item.UnitPrice
        );

        var discount = invoice.Items.Sum(item => item.Discount);
        var tax = invoice.Items.Sum(item => item.Tax);
        var totalAmount = invoice.TotalAmount;

        var bank = company.DefaultBankAccount;

        var logo = GetImageDataUri(company.LogoPath);
        var stamp = GetImageDataUri(company.StampPath);
        var signature = GetImageDataUri(company.SignaturePath);

        var date = invoice.Date.ToString(
            "yyyy/MM/dd",
            culture
        );

        return $$"""
        <!DOCTYPE html>
        <html lang="fa" dir="rtl">
        <head>
            <meta charset="UTF-8">
            <style>
                * {
                    box-sizing: border-box;
                }
                @font-face {
                font-family: 'Vazirmatn';
                src: url('data:font/ttf;base64,{{regularFont}}')
                format('truetype');
                font-weight: 400;
            }

                @font-face {
                    font-family: 'Vazirmatn';
                    src: url('data:font/ttf;base64,{{regularFont}}')
                        format('truetype');
                    font-weight: 400;
                    font-style: normal;
                }

                @font-face {
                    font-family: 'Vazirmatn';
                    src: url('data:font/ttf;base64,{{boldFont}}')
                        format('truetype');
                    font-weight: 700;
                    font-style: normal;
                }

                * {
                    box-sizing: border-box;
                }

                body {
                    font-family: 'Vazirmatn', sans-serif;
                    direction: rtl;
                    color: #263449;
                    font-size: 11px;
                    margin: 0;
                    line-height: 1.9;
                }

              

                .header {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    border: 1px solid #d9e0e9;
                    border-radius: 12px;
                    padding: 16px;
                    margin-bottom: 14px;
                }

                .company {
                    display: flex;
                    gap: 14px;
                    align-items: center;
                }

                .logo {
                    width: 76px;
                    height: 76px;
                    object-fit: contain;
                }

                .company-name {
                    font-size: 18px;
                    font-weight: bold;
                }

                .document-title {
                    border: 1px solid #d9e0e9;
                    border-radius: 10px;
                    padding: 12px 18px;
                    text-align: center;
                }

                .document-title strong {
                    font-size: 17px;
                }

                .section {
                    border: 1px solid #d9e0e9;
                    border-radius: 10px;
                    padding: 12px;
                    margin-bottom: 12px;
                    break-inside: avoid;
                }

                .section-title {
                    font-size: 12px;
                    font-weight: bold;
                    color: #245b91;
                    margin-bottom: 8px;
                }

                .parties {
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 12px;
                }

                .party {
                    border: 1px solid #d9e0e9;
                    border-radius: 10px;
                    padding: 12px;
                }

                .party-title {
                    font-weight: bold;
                    font-size: 12px;
                    margin-bottom: 7px;
                    color: #245b91;
                }

                .items {
                    width: 100%;
                    border-collapse: separate;
                    border-spacing: 0;
                    overflow: hidden;
                    border: 1px solid #d9e0e9;
                    border-radius: 10px;
                }

                .items th {
                    background: #edf3f9;
                    color: #263449;
                    font-weight: bold;
                }

                .items th,
                .items td {
                    padding: 9px 7px;
                    border-left: 1px solid #e5eaf0;
                    border-bottom: 1px solid #e5eaf0;
                    text-align: center;
                }

                .items tr:last-child td {
                    border-bottom: none;
                }

                .items th:last-child,
                .items td:last-child {
                    border-left: none;
                }

                .totals {
                    width: 48%;
                    margin-right: auto;
                    margin-top: 14px;
                }

                .total-row {
                    display: flex;
                    justify-content: space-between;
                    padding: 4px 8px;
                }

                .grand-total {
                    background: #edf3f9;
                    border-radius: 8px;
                    padding: 10px;
                    font-size: 14px;
                    font-weight: bold;
                }

                .words {
                    background: #f6f8fb;
                    border: 1px solid #d9e0e9;
                    border-radius: 9px;
                    padding: 10px;
                    margin-top: 12px;
                }

                .bank {
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 8px;
                }

                .signature-area {
                    display: flex;
                    justify-content: space-between;
                    margin-top: 22px;
                    min-height: 130px;
                }

                .signature-box {
                    width: 42%;
                    text-align: center;
                    position: relative;
                }

                .stamp {
                    max-width: 110px;
                    max-height: 90px;
                    object-fit: contain;
                }

                .signature {
                    max-width: 150px;
                    max-height: 65px;
                    object-fit: contain;
                }

                .muted {
                    color: #64748b;
                }

                @page {
                    size: A4;
                    margin: 12mm;
                }
                 .company-logo {
                    max-width: 150px;
                    max-height: 80px;
                }
                .company-stamp,
                .company-signature {
                    max-width: 120px;
                    max-height: 100px;
                }
            </style>
        </head>
        <body>

            <header class="header">
                <div class="company">
                   {{(logo is not null
    ? $"<img class=\"logo\" src=\"{logo}\" />"
    : "")}}
                    <div>
                        <div class="company-name">{{Text(company.Name)}}</div>
                        <div>شناسه ملی: {{Text(company.NationalId)}}</div>
                        <div>شماره اقتصادی: {{Text(company.EconomicCode)}}</div>
                        <div>شماره ثبت: {{Text(company.RegistrationNumber)}}</div>
                        <div>{{Text(company.Address)}}</div>
                        <div>تلفن: {{Text(company.Phone)}}</div>
                    </div>
                </div>

                <div class="document-title">
                    <strong>صورتحساب فروش</strong>
                    <div>شماره: {{Text(invoice.InvoiceNumber)}}</div>
                    <div>تاریخ: {{date}}</div>
                </div>
            </header>

            <div class="parties">
                <section class="party">
                    <div class="party-title">مشخصات فروشنده</div>
                    <div>نام: {{Text(invoice.Seller!.Name)}}</div>
                    <div>شناسه ملی: {{Text(invoice.Seller.NationalId)}}</div>
                    <div>کد اقتصادی: {{Text(invoice.Seller.EconomicCode)}}</div>
                    <div>نشانی: {{Text(invoice.Seller.Address)}}</div>
                    <div>تلفن: {{Text(invoice.Seller.Phone)}}</div>
                </section>

                <section class="party">
                    <div class="party-title">مشخصات خریدار</div>
                    <div>نام: {{Text(invoice.Buyer!.Name)}}</div>
                    <div>شناسه ملی: {{Text(invoice.Buyer.NationalId)}}</div>
                    <div>کد اقتصادی: {{Text(invoice.Buyer.EconomicCode)}}</div>
                    <div>نشانی: {{Text(invoice.Buyer.Address)}}</div>
                    <div>تلفن: {{Text(invoice.Buyer.Phone)}}</div>
                </section>
            </div>

            <section class="section" style="margin-top:12px">
                <div class="section-title">شرح کالا و خدمات</div>

                <table class="items">
                    <thead>
                        <tr>
                            <th>ردیف</th>
                            <th>شرح کالا / خدمت</th>
                            <th>تعداد</th>
                            <th>مبلغ واحد</th>
                            <th>تخفیف</th>
                            <th>مالیات</th>
                            <th>مبلغ کل</th>
                        </tr>
                    </thead>
                    <tbody>
                        {{rows}}
                    </tbody>
                </table>

                <div class="totals">
                    <div class="total-row">
                        <span>جمع قبل از تخفیف</span>
                        <strong>{{Money(subtotal)}}</strong>
                    </div>
                    <div class="total-row">
                        <span>تخفیف</span>
                        <strong>{{Money(discount)}}</strong>
                    </div>
                    <div class="total-row">
                        <span>مالیات و عوارض</span>
                        <strong>{{Money(tax)}}</strong>
                    </div>
                    <div class="total-row grand-total">
                        <span>مبلغ نهایی</span>
                        <strong>{{Money(totalAmount)}} ریال</strong>
                    </div>
                </div>

                <div class="words">
                    <strong>مبلغ به حروف:</strong>
                    {{Text(_amountToWords.Convert(totalAmount, "ریال"))}}
                </div>
            </section>

            <section class="section">
                <div class="section-title">اطلاعات پرداخت</div>
                <div class="bank">
                    <div>بانک: {{Text(bank?.BankName)}}</div>
                    <div>صاحب حساب: {{Text(bank?.AccountHolder)}}</div>
                    <div>شماره حساب: {{Text(bank?.AccountNumber)}}</div>
                    <div>شماره شبا: {{Text(bank?.ShebaNumber)}}</div>
                </div>
            </section>

            <section class="section">
                <div>توضیحات: {{Text(invoice.Notes)}}</div>

                <div class="signature-area">
                    <div class="signature-box">
                        <strong>مهر فروشنده</strong><br>
                       {{(stamp is not null
    ? $"<img class=\"stamp\" src=\"{stamp}\" />"
    : "")}}
                    </div>

                    <div class="signature-box">
                        <strong>امضا</strong><br>
                        {{(signature is not null
    ? $"<img class=\"signature\" src=\"{signature}\" />"
    : "")}}
                    </div>
                </div>
            </section>

        </body>
        </html>
        """;
    }

    private static string? ImageSource(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (!File.Exists(path))
            return null;

        var extension = Path.GetExtension(path).ToLowerInvariant();

        var mimeType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => null
        };

        if (mimeType is null)
            return null;

        var bytes = File.ReadAllBytes(path);

        return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
    }
}