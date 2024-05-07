using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
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
            var anime = await _jikanService.GetFirstAnimeByQuery(query);
            if (anime is null)
            {
                await RespondAsync("Failed to get anime information");
                return;
            }
            _logger.LogInformation("Got anime information: {Anime}", anime.Titles.FirstOrDefault()?.Title);
            var embed = new EmbedBuilder()
                .WithTitle(anime.Titles.FirstOrDefault()?.Title)
                .WithDescription(anime.Synopsis)
                .WithImageUrl(anime.ImageUrl)
                .WithColor(Color.Blue)
                .AddField("Status", anime.Status)
                .AddField("Genres", string.Join(", ", anime.Genres))
                .AddField("Demographics", string.Join(", ", anime.Demographics))
                .AddField("Themes", string.Join(", ", anime.Themes)).Build();
            await RespondAsync(embed: embed);
        }
    }
}
