using System.Security.Cryptography;

namespace Api.Storage;

public class InvoiceFileStorage
{
    private readonly string _rootPath;

    public InvoiceFileStorage(IConfiguration configuration)
    {
        _rootPath = configuration["InvoiceStorage:RootPath"]
            ?? Path.Combine(
                AppContext.BaseDirectory,
                "PrivateFiles",
                "Invoices"
            );
    }

    public async Task<(string RelativePath, string Hash)> SaveAsync(
        int invoiceId,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var relativeDirectory = Path.Combine(
            now.Year.ToString(),
            now.Month.ToString("00")
        );

        var directory = Path.Combine(_rootPath, relativeDirectory);

        Directory.CreateDirectory(directory);

        var fileName = $"invoice-{invoiceId}.pdf";
        var fullPath = Path.Combine(directory, fileName);

        await File.WriteAllBytesAsync(
            fullPath,
            content,
            cancellationToken
        );

        var hash = Convert.ToHexString(
            SHA256.HashData(content)
        );

        var relativePath = Path.Combine(
            relativeDirectory,
            fileName
        ).Replace('\\', '/');

        return (relativePath, hash);
    }

    public async Task<byte[]?> ReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(
            fullPath,
            cancellationToken
        );
    }

    public string GetFullPath(string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(_rootPath);
        var fullPath = Path.GetFullPath(
            Path.Combine(
                normalizedRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar)
            )
        );

        var rootWithSeparator =
            normalizedRoot.TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid invoice file path."
            );
        }

        return fullPath;
    }
}