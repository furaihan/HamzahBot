using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAsad.Modules
{
    public class GeneralModule(ILogger<InteractionService> logger) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<InteractionService> _logger = logger;

        [SlashCommand("ping", "Ping to this bot")]
        public async Task Ping()
        {
            _logger.LogInformation("Ping command received");
            int pingMs = (int)Context.Client.Latency;
            string name = Context.User.GlobalName;
            await RespondAsync($"Pong {name}! ({pingMs}ms)");
        }
        [SlashCommand("say", "Make this bot say something")]
        public async Task Say(string stringToSay)
        {
            await RespondAsync(stringToSay);
        }
    }
}
