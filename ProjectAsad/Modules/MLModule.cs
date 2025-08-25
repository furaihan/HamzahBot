using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using ProjectAsad.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAsad.Modules
{
    internal class MLModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<MLModule> _logger;
        private readonly ZhafarServices _zhafarServices;

        public MLModule(ILogger<MLModule> logger, ZhafarServices zhafarServices)
        {
            _logger = logger;
            _zhafarServices = zhafarServices;

            _logger.LogInformation("MLModule initialized");
        }

        [SlashCommand("clickbait", "Check if a text is clickbait")]
        public async Task CheckClickbait([Summary(name: "text", description: "text to check")] string text)
        {
            await DeferAsync(); // Defer the response as the API call might take some time

            ClickbaitResponse? clickbaitResponse = await _zhafarServices.GetClickbaitResponseAsync(text);

            var embedBuilder = new EmbedBuilder();

            if (clickbaitResponse is not null)
            {
                embedBuilder.WithTitle("Clickbait Analysis")
                    .WithDescription($"Analyzed text: {text}")
                    .WithColor(clickbaitResponse.IsClickbait ? Color.Red : Color.Green)
                    .AddField("Is Clickbait", clickbaitResponse.IsClickbait ? "Yes" : "No", true)
                    .AddField("Confidence", $"{clickbaitResponse.ClickbaitProbability:P2}", true)
                    .AddField("Prediction", clickbaitResponse.Prediction, true)
                    .WithFooter($"Status: {clickbaitResponse.Status}")
                    .WithCurrentTimestamp();
            }
            else
            {
                embedBuilder.WithTitle("Clickbait Analysis Error")
                    .WithDescription("Sorry, there was an error processing your request.")
                    .WithColor(Color.Red)
                    .AddField("Analyzed Text", text)
                    .WithFooter("Please try again later.")
                    .WithCurrentTimestamp();
            }

            await FollowupAsync(embed: embedBuilder.Build());
        }
        [SlashCommand("clickbait2", "Check if a text is clickbait v2")]
        public async Task CheckClickbait2([Summary(name: "text", description: "text to check")] string text)
        {
            await DeferAsync(); // Defer the response as the API call might take some time
            ClickbaitResponse2? clickbaitResponse = await _zhafarServices.GetClickbaitResponse2Async(text);

            var embedBuilder = new EmbedBuilder();

            if (clickbaitResponse is not null)
            {
                embedBuilder.WithTitle("Clickbait v2 Analysis")
                    .WithDescription($"Analyzed text: {text}")
                    .WithColor(clickbaitResponse.IsClickbait == 1 ? Color.Red : Color.Green)
                    .AddField("Is Clickbait", clickbaitResponse.IsClickbait == 1 ? "Yes" : "No", true)
                    .AddField("Confidence", $"{clickbaitResponse.ClickbaitProbability:P2}", true)
                    .AddField("Prediction", clickbaitResponse.Prediction, true)
                    .WithFooter($"Status: {clickbaitResponse.Status}")
                    .WithCurrentTimestamp();
            }
            else
            {
                embedBuilder.WithTitle("Clickbait v2 Analysis Error")
                    .WithDescription("Sorry, there was an error processing your request.")
                    .WithColor(Color.Red)
                    .AddField("Analyzed Text", text)
                    .WithFooter("Please try again later.")
                    .WithCurrentTimestamp();
            }

            await FollowupAsync(embed: embedBuilder.Build());
        }
    }
}
