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
        private readonly string _baseUrl2 = "http://cb.zhafar.id";

        public ZhafarServices(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ZhafarServices> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
            _httpClient.Timeout = TimeSpan.FromMilliseconds(2890); // Set a timeout for the HTTP client
            this._config = config;
            this._logger = logger;
            _baseUrl = this._config.GetValue<string>("Api:Zhafar") ?? "";
            if (string.IsNullOrEmpty(_baseUrl))
            {
                logger.LogWarning("Api:Zhafar is not configured");
            }
        }
        public async Task<ClickbaitResponse?> GetClickbaitResponseAsync(string text)
        {
            try
            {
                _logger.LogDebug("Sending clickbait check request");
                var requestData = new { text };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use the v2 clickbait endpoint as the primary source
                var response = await _httpClient.PostAsync($"{_baseUrl2}/predict", content);
                _logger.LogDebug("Received response from clickbait API");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<ClickbaitResponse>(responseString);

                    if (responseData != null)
                    {
                        _logger.LogInformation($"Clickbait check result: {responseData.IsClickbait} (Probability: {string.Join(", ", responseData.ClickbaitProbability ?? new Dictionary<string, double>())})");
                        return responseData;
                    }
                }
                else
                {
                    _logger.LogWarning($"API returned non-success status code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while checking clickbait");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking clickbait");
            }
            return null;
        }
    }
}
