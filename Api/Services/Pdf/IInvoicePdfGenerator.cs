using Api.Models;
using InvoiceEntity = Api.Models.Invoice;
namespace Api.Services.Pdf;

public interface IInvoicePdfGenerator
{
    Task<byte[]> GenerateAsync(
        InvoiceEntity invoice,
        CompanyProfile company,
        CancellationToken cancellationToken = default
    );
}