using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProjectAsad.Services
{
    internal class DiscordService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscordSocketClient> _logger;

        public DiscordService(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordSocketClient> logger)
        {
            _discord = client;
            _configuration = configuration;
            _logger = logger;

            _discord.Log += msg =>
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
            var section = _configuration.GetSection("Discord");
            await _discord.LoginAsync(TokenType.Bot, section["Token"]);
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.StopAsync();
        }
    }
}
