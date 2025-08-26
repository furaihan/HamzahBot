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

                // Safety net: defer if it looks like it might take a while
                // Only for slash commands that haven't been deferred yet
                if (interaction is ISlashCommandInteraction slashCmd && !interaction.HasResponded)
                {
                    // Optional: defer here as safety net for slow command resolution
                    // await slashCmd.DeferAsync();
                }

                var result = await _interaction.ExecuteCommandAsync(context, _service);

                // Handle command execution failures
                if (result is not null && !result.IsSuccess)
                {
                    _logger.LogError("Command execution failed: {Error} for command {Command}",
                        result.ErrorReason ?? "Unknown",
                        interaction is ISlashCommandInteraction cmd ? cmd.Data.Name : "Unknown");

                    // Properly respond to the interaction
                    await RespondToInteractionError(interaction, result.ErrorReason ?? "Command failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while executing interaction {InteractionId} from user {UserId}",
                    interaction.Id, interaction.User.Id);

                // Try to respond to the user about the error
                await HandleInteractionException(interaction, ex);
            }
        }

        private async Task RespondToInteractionError(SocketInteraction interaction, string error)
        {
            try
            {
                var userFriendlyError = GetUserFriendlyError(error);

                if (!interaction.HasResponded)
                {
                    if (interaction is ISlashCommandInteraction slashCmd)
                    {
                        await slashCmd.RespondAsync($"❌ {userFriendlyError}", ephemeral: true);
                    }
                }
                else
                {
                    // If already responded/deferred, use follow-up
                    if (interaction is ISlashCommandInteraction slashCmd)
                    {
                        await slashCmd.FollowupAsync($"❌ {userFriendlyError}", ephemeral: true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send error response to user");

                // Last resort: try sending to channel if we have context
                try
                {
                    if (interaction.Channel != null)
                    {
                        await interaction.Channel.SendMessageAsync($"❌ Something went wrong with that command.");
                    }
                }
                catch
                {
                    // Give up - at least we logged it
                }
            }
        }

        private async Task HandleInteractionException(SocketInteraction interaction, Exception ex)
        {
            try
            {
                if (!interaction.HasResponded)
                {
                    if (interaction is ISlashCommandInteraction slashCmd)
                    {
                        await slashCmd.RespondAsync("❌ An unexpected error occurred. Please try again later.", ephemeral: true);
                    }
                }
                else
                {
                    // Already responded, try to follow up
                    if (interaction is ISlashCommandInteraction slashCmd)
                    {
                        await slashCmd.FollowupAsync("❌ An unexpected error occurred. Please try again later.", ephemeral: true);
                    }
                }
            }
            catch (Exception responseEx)
            {
                _logger.LogError(responseEx, "Failed to respond to user after exception");

                // Don't try to delete original response - if we're here, 
                // the interaction likely failed before any response was sent
            }
        }

        private static string GetUserFriendlyError(string technicalError)
        {
            return technicalError switch
            {
                var error when error.Contains("permission", StringComparison.OrdinalIgnoreCase)
                    => "You don't have permission to use this command.",
                var error when error.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                    => "The command timed out. Please try again.",
                var error when error.Contains("rate", StringComparison.OrdinalIgnoreCase)
                    => "You're using commands too quickly. Please wait a moment.",
                var error when error.Contains("unknown", StringComparison.OrdinalIgnoreCase)
                    => "The command failed. Please try again later.",
                _ => "Something went wrong with that command."
            };
        }
    }
}
