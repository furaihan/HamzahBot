using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectAsad.Modules;
using System.Reflection;

namespace ProjectAsad.Services
{
    internal class DiscordInteractionService : IHostedService
    {
        private readonly InteractionService _interaction;
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _service;
        private readonly ILogger<InteractionService> _logger;

        public DiscordInteractionService(InteractionService interaction, 
                                        DiscordSocketClient discord, 
                                        IConfiguration config, 
                                        IServiceProvider service, 
                                        ILogger<InteractionService> logger)
        {
            _interaction = interaction;
            _discord = discord;
            _config = config;
            _service = service;
            _logger = logger;

            _interaction.Log += msg =>
            {
                switch (msg.Severity)
                {
                    case LogSeverity.Critical:
                        _logger.LogCritical(msg.ToString());
                        break;
                    case LogSeverity.Error:
                        _logger.LogError(msg.ToString());
                        break;
                    case LogSeverity.Warning:
                        _logger.LogWarning(msg.ToString());
                        break;
                    case LogSeverity.Info:
                        _logger.LogInformation(msg.ToString());
                        break;
                    case LogSeverity.Verbose:
                        _logger.LogTrace(msg.ToString());
                        break;
                    case LogSeverity.Debug:
                        _logger.LogDebug(msg.ToString());
                        break;
                }
                return Task.CompletedTask;
            };
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.Ready += () => _interaction.RegisterCommandsGloballyAsync(true);
            _discord.InteractionCreated += OnInteractionAsync;
            _logger.LogInformation("Registering commands...");

            await _interaction.AddModuleAsync(typeof(GeneralModule), _service);
            await _interaction.AddModuleAsync(typeof(JokeModule), _service);
            await _interaction.AddModuleAsync(typeof(AnimeModule), _service);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interaction.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, interaction);
                var result = _interaction.ExecuteCommandAsync(context, _service);


                if (!result.IsCompletedSuccessfully)
                {
                    _logger.LogError("Command execution failed: {Error}", result.Exception);
                    await context.Channel.SendMessageAsync($"Error: {result}");
                }
            }
            catch (Exception)
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync().ContinueWith(msg => 
                    { 
                        msg.Result.DeleteAsync();
                    });
                }
            }
        }
    }
}
