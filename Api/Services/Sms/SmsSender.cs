using System.Net.Http.Json;

namespace Api.Services.Sms;

public class SmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SmsSender(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<SmsResult> SendAsync(
        string mobile,
        string message,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["Sms:Endpoint"];
        var apiKey = _configuration["Sms:ApiKey"];

        if (string.IsNullOrWhiteSpace(endpoint))
            return new SmsResult(
                false,
                ErrorMessage: "SMS endpoint is not configured."
            );

        if (string.IsNullOrWhiteSpace(mobile))
            return new SmsResult(
                false,
                ErrorMessage: "Mobile number is empty."
            );

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint
        );

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("X-API-Key", apiKey);
        }

        request.Content = JsonContent.Create(new
        {
            to = mobile,
            message
        });

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                return new SmsResult(
                    false,
                    ErrorMessage: $"HTTP {(int)response.StatusCode}: {error}"
                );
            }

            var responseBody = await response.Content.ReadFromJsonAsync<
                SmsProviderResponse
            >(cancellationToken: cancellationToken);

            return new SmsResult(
                true,
                responseBody?.MessageId
            );
        }
        catch (Exception ex) when (
            ex is HttpRequestException or TaskCanceledException)
        {
            return new SmsResult(
                false,
                ErrorMessage: ex.Message
            );
        }
    }

    private sealed class SmsProviderResponse
    {
        public string? MessageId { get; set; }
    }
}