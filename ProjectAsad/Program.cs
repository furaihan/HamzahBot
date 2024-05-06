using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectAsad.Services;
using Discord.Interactions;

// get all files in the current directory
//string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
//Console.WriteLine("Files in the current directory:");
//foreach (string file in files)
//{
//    Console.WriteLine(file);
//}

using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddIniFile("appsettings.ini");
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<DiscordSocketClient>();
                    services.AddSingleton<InteractionService>();
                    services.AddSingleton<JokeService>();
                    services.AddHostedService<DiscordService>();
                    services.AddHostedService<DiscordInteractionService>();
                    services.AddHttpClient();
                })
                .Build();

await host.RunAsync();
