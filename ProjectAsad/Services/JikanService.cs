using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using System.Text.Json;

namespace ProjectAsad.Services
{
    internal class JikanService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<JikanService> _logger;
        private readonly string _baseUrl;

        public JikanService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<JikanService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;

            _baseUrl = _config.GetSection("Api")["Jikan"] ?? "https://api.jikan.moe/v4";
        }
        public async Task<Anime?> GetFirstAnimeByQuery(string query)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
                var response = await client.GetAsync($"{_baseUrl}/anime?q={query}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get anime from Jikan API: {StatusCode}", response.StatusCode);
                    return null;
                }
                var json = await response.Content.ReadAsStringAsync();
                var firstAnime = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("data").EnumerateArray().FirstOrDefault();

                Anime anime = new()
                {
                    Titles = firstAnime.GetProperty("titles").EnumerateArray().Select(title => new AnimeTitle
                    {
                        Title = title.GetProperty("title").GetString() ?? "",
                        Type = title.GetProperty("type").GetString() ?? ""
                    }).ToList(),
                    ImageUrl = firstAnime.GetProperty("images")
                                         .GetProperty("webp")
                                         .GetProperty("large_image_url")
                                         .GetString() ?? "",
                    Status = firstAnime.GetProperty("status")
                                       .GetString() ?? "",
                    Synopsis = firstAnime.GetProperty("synopsis")
                                         .GetString(),
                    Genres = firstAnime.GetProperty("genres")
                                       .EnumerateArray()
                                       .Select(genre=> genre.GetProperty("name").GetString() ?? "").ToList(),
                    Demographics = firstAnime.GetProperty("demographics")
                                             .EnumerateArray() 
                                             .Select(demographic => demographic.GetProperty("name").GetString() ?? "").ToList(),
                    Themes = firstAnime.GetProperty("themes")
                                       .EnumerateArray()
                                       .Select(theme => theme.GetProperty("name").GetString() ?? "").ToList(),
                    Producers = firstAnime.GetProperty("producers")
                                          .EnumerateArray()
                                          .Select(producer => producer.GetProperty("name").GetString() ?? "").ToList(),
                    Studios = firstAnime.GetProperty("studios")
                                        .EnumerateArray()
                                        .Select(studio => studio.GetProperty("name").GetString() ?? "").ToList(),
                    Licensors = firstAnime.GetProperty("licensors")
                                          .EnumerateArray()
                                          .Select(licensor => licensor.GetProperty("name").GetString() ?? "").ToList()
                };
                return anime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching anime from Jikan API");
                return null;
            }
        }
    }
}
