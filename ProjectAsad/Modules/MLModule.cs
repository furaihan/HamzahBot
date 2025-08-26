using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using ProjectAsad.Services;

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

    [SlashCommand("clickbait", "Periksa apakah judul berita merupakan clickbait")]
    public async Task CheckClickbait([Summary(name: "title", description: "judul berita yang akan diperiksa")] string title)
        {
            await DeferAsync(); // Defer the response as the API call might take some time
            ClickbaitResponse? clickbaitResponse = await _zhafarServices.GetClickbaitResponseAsync(title);

            var embedBuilder = new EmbedBuilder();

            if (clickbaitResponse is not null)
            {
                var isClickbait = clickbaitResponse.IsClickbait == 1;
                var confidencePercentage = clickbaitResponse.ClickbaitProbability?.GetValueOrDefault("1", 0.0) ?? 0.0;

                Color embedColor = GetConfidenceColor(confidencePercentage, isClickbait);

                var statusEmoji = isClickbait ? "🚨" : "✅";
                var confidenceEmoji = GetConfidenceEmoji(confidencePercentage);

                embedBuilder
                    .WithTitle($"{statusEmoji} Hasil Analisis Clickbait")
                    .WithDescription($"📝 **Teks yang dianalisis:**\n> {(title.Length > 100 ? title.Substring(0, 100) + "..." : title)}")
                    .WithColor(embedColor);

                var resultValue = isClickbait
                    ? $"**🎯 TERDETEKSI CLICKBAIT**\nKonten ini tampak menggunakan taktik clickbait"
                    : $"**📰 BUKAN CLICKBAIT**\nKonten ini tampak asli";

                embedBuilder.AddField($"{statusEmoji} Putusan", resultValue, false);

                var confidenceBar = CreateProgressBar(confidencePercentage, 10);
                embedBuilder.AddField(
                    $"{confidenceEmoji} Tingkat Keyakinan",
                    $"**{confidencePercentage:P1}**\n{confidenceBar}\n{GetConfidenceDescription(confidencePercentage)}",
                    true
                );

                embedBuilder.AddField(
                    "🔍 Prediksi",
                    $"```{clickbaitResponse.Prediction?.ToUpper()}```",
                    true
                );

                if (clickbaitResponse.ClickbaitProbability != null)
                {
                    var legitProb = clickbaitResponse.ClickbaitProbability.GetValueOrDefault("0", 0.0);
                    var clickbaitProb = clickbaitResponse.ClickbaitProbability.GetValueOrDefault("1", 0.0);

                    embedBuilder.AddField(
                        "📊 Rincian Probabilitas",
                        $"📰 Bukan Clickbait: **{legitProb:P1}**\n🎯 Clickbait: **{clickbaitProb:P1}**",
                        false
                    );
                }

                embedBuilder
                    .WithFooter($"✨ Analisis selesai • Status: {clickbaitResponse.Status} • Ditenagai oleh AI",
                               "https://cdn.discordapp.com/emojis/1234567890123456789.png") // Optional: Add your bot's icon
                    .WithCurrentTimestamp();
            }
            else
            {
                embedBuilder
                    .WithTitle("❌ Terjadi Kesalahan Analisis")
                    .WithDescription("🚫 **Ups! Terjadi kesalahan**\nKami mengalami masalah saat menganalisis teks Anda.")
                    .WithColor(Color.DarkRed)
                    .AddField("📝 Teks Anda", $"> {(title.Length > 100 ? title.Substring(0, 100) + "..." : title)}", false)
                    .AddField("🔧 Apa yang harus dilakukan?", "• Coba lagi beberapa saat lagi\n• Periksa apakah teks Anda valid\n• Hubungi dukungan jika masalah berlanjut", false)
                    .WithFooter("⚠️ Terjadi kesalahan • Silakan coba lagi nanti")
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
