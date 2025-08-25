using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAsad.Modules
{
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<InteractionService> _logger;

        public GeneralModule(ILogger<InteractionService> logger)
        {
            _logger = logger;
        }

        [SlashCommand("ping", "Ping to this bot")]
        public async Task Ping()
        {
            _logger.LogInformation("Ping command received");
            int pingMs = (int)Context.Client.Latency;
            string name = Context.User?.GlobalName ?? Context.User?.Username ?? "there";

            try
            {
                await RespondAsync($"Pong {name}! ({pingMs}ms)");
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HttpRequestException while responding to ping");
                try { await Context.Channel.SendMessageAsync("There was a network issue sending the response."); } catch { }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while responding to ping");
                try { await Context.Channel.SendMessageAsync("An unexpected error occurred."); } catch { }
            }
        }
        [SlashCommand("say", "Make this bot say something")]
        public async Task Say(string stringToSay)
        {
            await RespondAsync(stringToSay);
        }
    }
}
