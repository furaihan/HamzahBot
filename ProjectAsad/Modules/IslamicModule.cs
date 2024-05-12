using Discord.Interactions;
using Microsoft.Extensions.Logging;
using ProjectAsad.Services;
using Discord;
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

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle($"Prayer Schedule for {prayerCity?.Name}");
            embedBuilder.WithDescription($"Today's prayer schedule for {prayerCity?.Name}");
            embedBuilder.AddField("Fajr", prayerSchedule.Fajr, inline: true);
            embedBuilder.AddField("Sunrise", prayerSchedule.Sunrise, inline: true);
            embedBuilder.AddField("Dhuhr", prayerSchedule.Dhuhr, inline: true);
            embedBuilder.AddField("Asr", prayerSchedule.Asr, inline: true);
            embedBuilder.AddField("Maghrib", prayerSchedule.Maghrib, inline: true);
            embedBuilder.AddField("Isha", prayerSchedule.Isha, inline: true);
            embedBuilder.WithFooter("Prayer schedule from myquran.com");
            embedBuilder.WithColor(Color.Green);

            await RespondAsync(embed: embedBuilder.Build());
        }
    }
}
