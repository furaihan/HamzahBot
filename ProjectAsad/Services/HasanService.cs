using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using System.Text.Json.Nodes;

namespace ProjectAsad.Services
{
    internal class HasanService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<HasanService> _logger;
        private readonly string _baseUrl;

        public HasanService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<HasanService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HamzahBot on discord");
            _config = config;
            _logger = logger;
            _baseUrl = _config.GetValue<string>("Api:Myquran") ?? "https://api.myquran.com/v2";
        }

        public async Task<PrayerCity?> SearchCity(string city)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/sholat/kota/cari/{city}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStreamAsync();
                var data = await JsonNode.ParseAsync(json).ConfigureAwait(false);

                return new PrayerCity
                {
                    Status = data?["status"]?.GetValue<bool>(),
                    Id = int.Parse(data?["data"]?[0]?["id"]?.GetValue<string>() ?? ""),
                    Name = data?["data"]?[0]?["lokasi"]?.GetValue<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while searching city");
                return null;
            }
        }
        public async Task<PrayerSchedule?> GetPrayerScheduleToday(int cityId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/sholat/jadwal/{cityId}/{DateTime.Today:yyyy-MM-dd}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStreamAsync();
                var data = await JsonNode.ParseAsync(json).ConfigureAwait(false);

                var jadwal = data?["data"]?["jadwal"]?.AsObject();

                return new PrayerSchedule
                {
                    CityId = cityId,
                    Date = jadwal?["tanggal"]?.GetValue<string>(),
                    Imsak = jadwal?["imsak"]?.GetValue<string>(),
                    Fajr = jadwal?["subuh"]?.GetValue<string>(),
                    Sunrise = jadwal?["terbit"]?.GetValue<string>(),
                    Dhuhr = jadwal?["dzuhur"]?.GetValue<string>(),
                    Asr = jadwal?["ashar"]?.GetValue<string>(),
                    Maghrib = jadwal?["maghrib"]?.GetValue<string>(),
                    Isha = jadwal?["isya"]?.GetValue<string>()
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting prayer schedule");
                return null;
            }
        }
    }
}
