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
            // Register commands when the client is ready. Await inside the handler so failures are logged.
            _discord.Ready += async () =>
            {
                try
                {
                    await _interaction.RegisterCommandsGloballyAsync(true);
                    _logger.LogInformation("Commands registered globally on Ready.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register commands globally on Ready.");
                }
            };

            _discord.InteractionCreated += OnInteractionAsync;
            _logger.LogInformation("Registering commands...");

            await _interaction.AddModuleAsync(typeof(GeneralModule), _service);
            await _interaction.AddModuleAsync(typeof(JokeModule), _service);
            await _interaction.AddModuleAsync(typeof(AnimeModule), _service);
            await _interaction.AddModuleAsync(typeof(IslamicModule), _service);
            await _interaction.AddModuleAsync(typeof(MLModule), _service);

            _logger.LogInformation("All Commands registered");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Unsubscribe to avoid dangling handlers and dispose the interaction service.
            _discord.InteractionCreated -= OnInteractionAsync;
            _interaction.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, interaction);

                // Await the execution so exceptions are surfaced here and the result can be inspected.
                var result = await _interaction.ExecuteCommandAsync(context, _service);

                if (result is not null && !result.IsSuccess)
                {
                    _logger.LogError("Command execution failed: {Error}", result.ErrorReason ?? "Unknown");
                    if (context.Channel != null)
                        await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while executing interaction");

                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    try
                    {
                        var original = await interaction.GetOriginalResponseAsync();
                        if (original != null)
                            await original.DeleteAsync();
                    }
                    catch (Exception delEx)
                    {
                        _logger.LogError(delEx, "Failed to delete original response after exception");
                    }
                }
            }
        }
    }
}
