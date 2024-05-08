using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using ProjectAsad.Model;
using ProjectAsad.Services;

namespace ProjectAsad.Modules
{
    internal class AnimeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly JikanService _jikanService;
        private readonly ILogger<AnimeModule> _logger;

        public AnimeModule(JikanService jikanService, ILogger<AnimeModule> logger)
        {
            _jikanService = jikanService;
            _logger = logger;
            _logger.LogInformation("AnimeModule initialized");
        }

        [SlashCommand("anime", "Get information about an anime")]
        public async Task Anime([Summary(description: "The name of the anime")] string query)
        {
            Anime? anime = await GetAnimeInformationAsync(query);
            if (anime is null)
            {
                await RespondAsync("Failed to get anime information");
                return;
            }

            _logger.LogInformation("Got anime information: {Anime}", anime.Titles.FirstOrDefault()?.Title);

            Embed embed = CreateAnimeEmbed(anime);

            await RespondAsync(embed: embed);
        }

        private async Task<Anime?> GetAnimeInformationAsync(string query)
        {
            return await _jikanService.GetFirstAnimeByQuery(query);
        }

        private Embed CreateAnimeEmbed(Anime anime)
        {
            EmbedBuilder embedBuilder = new();
            string titleDefault = anime.Titles.FirstOrDefault()?.Title ?? "";
            string? titleJapanese = anime.Titles.Where(t => t.Type == "Japanese").FirstOrDefault()?.Title;
            if (titleJapanese is not null)
            {
                titleDefault += $" ({titleJapanese})";
            }
            embedBuilder.WithTitle(titleDefault);
            embedBuilder.WithDescription(anime.Synopsis);
            embedBuilder.WithImageUrl(anime.ImageUrl);
            embedBuilder.AddField("Status", anime.Status, inline: true);
            if (anime.Episodes is not null)
            {
                embedBuilder.AddField("Episodes", anime.Episodes.ToString(), inline: true);
            }
            if (anime.Genres.Count != 0)
            {
                embedBuilder.AddField("Genres", string.Join(", ", anime.Genres), inline: true);
            }
            if (anime.Score is not null)
            {
                embedBuilder.AddField("Score", anime.Score.ToString() + "/10", inline: true);
            }
            if (anime.Popularity is not null)
            {
                embedBuilder.AddField("Popularity", anime.Popularity.ToString(), inline: true);
            }
            return embedBuilder.Build();
        }
    }
}