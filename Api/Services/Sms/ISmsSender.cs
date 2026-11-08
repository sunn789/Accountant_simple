namespace Api.Services.Sms;

public record SmsResult(
    bool Success,
    string? ProviderMessageId = null,
    string? ErrorMessage = null
);

public interface ISmsSender
{
    Task<SmsResult> SendAsync(
        string mobile,
        string message,
        CancellationToken cancellationToken = default
    );
}