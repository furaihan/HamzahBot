using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProjectAsad.Services
{
    internal class JikanService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<JikanService> _logger;
        private readonly string _baseUrl;

        public JikanService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<JikanService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
            _config = config;
            _logger = logger;
            _baseUrl = _config.GetValue<string>("Api:Jikan") ?? "https://api.jikan.moe/v4";
        }

        public async Task<Anime?> GetFirstAnimeByQuery(string query)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/anime?q={query}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStreamAsync();
                var data = await JsonNode.ParseAsync(json).ConfigureAwait(false);
                var firstAnime = data?["data"]?.AsArray().FirstOrDefault();

                if (firstAnime == null)
                    return null;

                return new Anime
                {
                    Titles = firstAnime["titles"]?.AsArray()
                        .Select(title => new AnimeTitle
                        {
                            Title = title?["title"]?.GetValue<string>() ?? string.Empty,
                            Type = title?["type"]?.GetValue<string>() ?? string.Empty
                        })
                        .ToList() ?? [],
                    ImageUrl = firstAnime["images"]?["webp"]?["large_image_url"]?.GetValue<string>() ?? string.Empty,
                    Status = firstAnime["status"]?.GetValue<string>() ?? string.Empty,
                    Synopsis = firstAnime["synopsis"]?.GetValue<string>() ?? string.Empty,
                    Episodes = firstAnime["episodes"]?.GetValue<int>(),
                    Score = firstAnime["score"]?.GetValue<float>(),
                    Popularity = firstAnime["popularity"]?.GetValue<int>(),
                    Genres = firstAnime["genres"]?.AsArray().Select(genre => genre?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? [],
                    Demographics = firstAnime["demographics"]?.AsArray().Select(demographic => demographic?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? [],
                    Themes = firstAnime["themes"]?.AsArray().Select(theme => theme?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? [],
                    Producers = firstAnime["producers"]?.AsArray().Select(producer => producer?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? [],
                    Studios = firstAnime["studios"]?.AsArray().Select(studio => studio?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? [],
                    Licensors = firstAnime["licensors"]?.AsArray().Select(licensor => licensor?["name"]?.GetValue<string>() ?? string.Empty).ToList() ?? []
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching anime from Jikan API");
                return null;
            }
        }
    }
}