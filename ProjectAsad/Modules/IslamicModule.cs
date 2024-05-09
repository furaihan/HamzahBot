using Discord.Interactions;
using Microsoft.Extensions.Logging;
using ProjectAsad.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAsad.Modules
{
    internal class IslamicModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<IslamicModule> _logger;
        private readonly HasanService _hasanService;
        public IslamicModule(HasanService hasanService, ILogger<IslamicModule> logger)
        {
            _hasanService = hasanService;
            _logger = logger;

            _logger.LogInformation("IslamicModule initialized");
        }

        [SlashCommand("prayer", "Get prayer schedule for today")]
        public async Task GetPrayerSchedule([Summary(name: "city", description: "city where you live")]string city)
        {
            var prayerCity = await _hasanService.SearchCity(city);

            if (prayerCity == null)
            {
                await RespondAsync("City not found");
                return;
            }
            if (prayerCity?.Status == false)
            {
                await RespondAsync("City not found, please use another search query");
                return;
            }

            var prayerSchedule = await _hasanService.GetPrayerScheduleToday(prayerCity?.Id ?? -1);

            if (prayerSchedule == null)
            {
                await RespondAsync("Error while getting prayer schedule");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Prayer schedule for {prayerCity?.Name} ({DateTime.Today:dd MMMM yyyy})");
            sb.AppendLine();
            sb.AppendLine($"Fajr: {prayerSchedule.Fajr}");
            sb.AppendLine($"Dhuhr: {prayerSchedule.Dhuhr}");
            sb.AppendLine($"Asr: {prayerSchedule.Asr}");
            sb.AppendLine($"Maghrib: {prayerSchedule.Maghrib}");
            sb.AppendLine($"Isha: {prayerSchedule.Isha}");

            await RespondAsync(sb.ToString());
        }
    }
}
