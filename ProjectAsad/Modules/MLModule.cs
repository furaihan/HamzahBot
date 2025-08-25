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
                var isClickbait = clickbaitResponse.IsClickbait == 1;
                var confidencePercentage = clickbaitResponse.ClickbaitProbability?.GetValueOrDefault("1", 0.0) ?? 0.0;

                Color embedColor = GetConfidenceColor(confidencePercentage, isClickbait);

                var statusEmoji = isClickbait ? "🚨" : "✅";
                var confidenceEmoji = GetConfidenceEmoji(confidencePercentage);

                embedBuilder
                    .WithTitle($"{statusEmoji} Clickbait Analysis Results")
                    .WithDescription($"📝 **Analyzed Text:**\n> {(text.Length > 100 ? text.Substring(0, 100) + "..." : text)}")
                    .WithColor(embedColor);

                var resultValue = isClickbait
                    ? $"**🎯 CLICKBAIT DETECTED**\nThis content appears to use clickbait tactics"
                    : $"**📰 LEGITIMATE CONTENT**\nThis content appears to be genuine";

                embedBuilder.AddField($"{statusEmoji} Verdict", resultValue, false);

                var confidenceBar = CreateProgressBar(confidencePercentage, 10);
                embedBuilder.AddField(
                    $"{confidenceEmoji} Confidence Level",
                    $"**{confidencePercentage:P1}**\n{confidenceBar}\n{GetConfidenceDescription(confidencePercentage)}",
                    true
                );

                embedBuilder.AddField(
                    "🔍 Prediction",
                    $"```{clickbaitResponse.Prediction?.ToUpper()}```",
                    true
                );

                if (clickbaitResponse.ClickbaitProbability != null)
                {
                    var legitProb = clickbaitResponse.ClickbaitProbability.GetValueOrDefault("0", 0.0);
                    var clickbaitProb = clickbaitResponse.ClickbaitProbability.GetValueOrDefault("1", 0.0);

                    embedBuilder.AddField(
                        "📊 Probability Breakdown",
                        $"📰 Legitimate: **{legitProb:P1}**\n🎯 Clickbait: **{clickbaitProb:P1}**",
                        false
                    );
                }

                embedBuilder
                    .WithFooter($"✨ Analysis completed • Status: {clickbaitResponse.Status} • Powered by AI",
                               "https://cdn.discordapp.com/emojis/1234567890123456789.png") // Optional: Add your bot's icon
                    .WithCurrentTimestamp();
            }
            else
            {
                embedBuilder
                    .WithTitle("❌ Analysis Error")
                    .WithDescription("🚫 **Oops! Something went wrong**\nWe encountered an issue while analyzing your text.")
                    .WithColor(Color.DarkRed)
                    .AddField("📝 Your Text", $"> {(text.Length > 100 ? text.Substring(0, 100) + "..." : text)}", false)
                    .AddField("🔧 What to do?", "• Try again in a few moments\n• Check if your text is valid\n• Contact support if the issue persists", false)
                    .WithFooter("⚠️ Error occurred • Please try again later")
                    .WithCurrentTimestamp();
            }

            await FollowupAsync(embed: embedBuilder.Build());
        }

        private Color GetConfidenceColor(double confidence, bool isClickbait)
        {
            if (isClickbait)
            {
                return confidence switch
                {
                    >= 0.9 => Color.DarkRed,
                    >= 0.7 => Color.Red,
                    >= 0.5 => Color.Orange,
                    _ => Color.Gold
                };
            }
            else
            {
                return confidence switch
                {
                    >= 0.9 => Color.DarkGreen,
                    >= 0.7 => Color.Green,
                    >= 0.5 => Color.Gold,
                    _ => Color.Gold
                };
            }
        }

        private string GetConfidenceEmoji(double confidence)
        {
            return confidence switch
            {
                >= 0.95 => "🔥", // Very high confidence
                >= 0.85 => "⚡", // High confidence
                >= 0.70 => "💪", // Good confidence
                >= 0.60 => "👍", // Moderate confidence
                >= 0.50 => "🤔", // Low confidence
                _ => "❓"        // Very low confidence
            };
        }

        private string CreateProgressBar(double percentage, int length = 10)
        {
            int filled = (int)Math.Round(percentage * length);
            int empty = length - filled;

            string filledBar = new string('█', filled);
            string emptyBar = new string('░', empty);

            return $"`{filledBar}{emptyBar}`";
        }

        private string GetConfidenceDescription(double confidence)
        {
            return confidence switch
            {
                >= 0.95 => "*Extremely confident*",
                >= 0.85 => "*Very confident*",
                >= 0.70 => "*Confident*",
                >= 0.60 => "*Moderately confident*",
                >= 0.50 => "*Somewhat uncertain*",
                _ => "*Low confidence*"
            };
        }
    }
}
