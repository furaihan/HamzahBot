using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectAsad.Services;
using Discord.Interactions;
using Microsoft.Extensions.Logging;


using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddIniFile("appsettings.ini");
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<DiscordSocketClient>();
                    services.AddSingleton(p => new InteractionService(p.GetRequiredService<DiscordSocketClient>()));
                    services.AddSingleton<JokeService>();
                    services.AddSingleton<JikanService>();
                    services.AddSingleton<HasanService>();
                    services.AddSingleton<ZhafarServices>();
                    services.AddHostedService<DiscordService>();
                    services.AddHostedService<DiscordInteractionService>();
                    services.AddHttpClient();
                })
                .Build();

await host.RunAsync();
