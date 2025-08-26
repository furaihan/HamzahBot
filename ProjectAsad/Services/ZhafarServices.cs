using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;

namespace ProjectAsad.Services
{
    internal class ZhafarServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ZhafarServices> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ZhafarServices(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ZhafarServices> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
            _httpClient.Timeout = TimeSpan.FromMilliseconds(2890); // Set a timeout for the HTTP client
            _config = config;
            _logger = logger;
            _baseUrl = _config.GetValue<string>("Api:Zhafar") ?? "http://cb.zhafar.id";
            if (string.IsNullOrEmpty(_baseUrl))
            {
                logger.LogWarning("Api:Zhafar is not configured");
            }
        }
        public async Task<ClickbaitResponse?> GetClickbaitResponseAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty or null text provided for clickbait check");
                return null;
            }

            try
            {
                _logger.LogDebug("Sending clickbait check request for text length: {TextLength}", text.Length);

                var requestData = new { text };
                var json = JsonSerializer.Serialize(requestData, _jsonSerializerOptions);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync($"{_baseUrl}/predict", content, cancellationToken);

                _logger.LogDebug("Received response from clickbait API with status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (string.IsNullOrWhiteSpace(responseString))
                    {
                        _logger.LogWarning("Received empty response from clickbait API");
                        return null;
                    }

                    var responseData = JsonSerializer.Deserialize<ClickbaitResponse>(responseString, _jsonSerializerOptions);

                    if (responseData != null)
                    {
                        var probabilityLog = responseData.ClickbaitProbability?.Any() == true
                            ? string.Join(", ", responseData.ClickbaitProbability.Select(kvp => $"{kvp.Key}: {kvp.Value:F3}"))
                            : "No probability data";

                        _logger.LogInformation("Clickbait check result: {IsClickbait} (Probabilities: {Probabilities})",
                            responseData.IsClickbait, probabilityLog);
                        return responseData;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize clickbait API response");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("API returned non-success status code: {StatusCode}. Response: {ErrorContent}",
                        response.StatusCode, errorContent);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError("Timeout while checking clickbait: {Message}", ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogInformation("Clickbait check was cancelled: {Message}", ex.Message);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while checking clickbait: {Message}", httpEx.Message);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while checking clickbait: {Message}", jsonEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking clickbait: {Message}", ex.Message);
            }

            return null;
        }
    }
}
