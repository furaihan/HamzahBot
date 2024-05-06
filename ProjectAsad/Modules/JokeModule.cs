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
    internal class JokeModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly JokeService _jokeService;
        private readonly ILogger<JokeModule> _logger;
        public JokeModule(JokeService jokeService, ILogger<JokeModule> logger)
        {
            _jokeService = jokeService;
            _logger = logger;

            _logger.LogInformation("JokeModule initialized");
        }
        [SlashCommand("joke", "Get a random joke from icanhazdadjoke API")]
        public async Task Joke()
        {
            var joke = await _jokeService.GetRandomJoke();
            await RespondAsync(joke);
        }
    }
}
