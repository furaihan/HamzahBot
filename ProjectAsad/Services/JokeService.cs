using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Text.Json;

namespace ProjectAsad.Services
{
    internal class JokeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<JokeService> logger)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _config = config;
        private readonly ILogger<JokeService> _logger = logger;

        public async Task<string> GetRandomJoke()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiEndPoint = _config.GetSection("Api")["Icanhazdadjoke"];
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
                var response = await client.GetAsync(apiEndPoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var joke = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("joke").GetString();

                client.Dispose();
                return joke ?? "No joke found. but the http request was successful. :), it might be a problem with the API.";
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching joke");
                return "My sincerest apologies, but I couldn't find a joke for you. the API might be down. :(";
            }
        }
    }
}
