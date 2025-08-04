# HamzahBot (ProjectAsad) 🤖

A feature-rich Discord bot built with .NET 8 and Discord.Net library, offering various entertainment and utility commands.

## Features 🎯

### 🎭 Entertainment
- **Jokes**: Get random jokes to lighten up your server
- **Anime**: Search and get information about anime series
- **General Commands**: Basic utility commands like ping

### 🎮 Modules
- **General Module**: Basic bot functionality and ping commands
- **Joke Module**: Random joke generation
- **Anime Module**: Anime information lookup powered by Jikan API
- **Islamic Module**: Islamic-related features
- **ML Module**: Machine Learning related functionality

## Tech Stack 💻

- **.NET 8**: Modern C# runtime
- **Discord.Net 3.15.2**: Discord API wrapper
- **Microsoft.Extensions.Hosting**: Dependency injection and hosting
- **Microsoft.Extensions.Configuration**: Configuration management
- **Newtonsoft.Json**: JSON serialization
- **Jikan API**: MyAnimeList unofficial API

## Prerequisites 📋

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Discord Bot Token (from [Discord Developer Portal](https://discord.com/developers/applications))

## Setup & Installation 🚀

1. **Clone the repository**
   ```bash
   git clone https://github.com/furaihan/HamzahBot.git
   cd HamzahBot
   ```

2. **Configure the bot**
   Create an `appsettings.ini` file in the `ProjectAsad` directory:
   ```ini
   [Discord]
   Token=YOUR_DISCORD_BOT_TOKEN_HERE
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the bot**
   ```bash
   dotnet run --project ProjectAsad
   ```

## Commands 📜

### Slash Commands
- `/ping` - Check bot latency and responsiveness
- `/anime [search]` - Search for anime information
- `/joke` - Get a random joke

*More commands available in different modules*

## Project Structure 📁

```
ProjectAsad/
├── Program.cs              # Main entry point
├── ProjectAsad.csproj      # Project configuration
├── Model/                  # Data models
├── Modules/                # Discord command modules
│   ├── GeneralModule.cs    # Basic commands
│   ├── AnimeModule.cs      # Anime-related commands
│   ├── JokeModule.cs       # Joke commands
│   ├── IslamicModule.cs    # Islamic features
│   └── MLModule.cs         # ML functionality
├── Services/               # Business logic services
│   ├── DiscordService.cs
│   ├── JikanService.cs     # Anime API service
│   ├── JokeService.cs      # Joke generation
│   └── ...
└── Properties/             # Assembly info
```

## Development 🛠️

### Adding New Commands
1. Create a new module in the `Modules` folder
2. Inherit from `InteractionModuleBase<SocketInteractionContext>`
3. Use `[SlashCommand]` attribute for slash commands
4. Register services in `Program.cs` if needed

### Adding New Services
1. Create service class in `Services` folder
2. Register in `Program.cs` using dependency injection
3. Inject into modules as needed

## Contributing 🤝

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License 📄

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments 🙏

- [Discord.Net](https://github.com/discord-net/Discord.Net) - Discord API wrapper
- [Jikan API](https://jikan.moe/) - MyAnimeList unofficial API
- [Microsoft.Extensions](https://docs.microsoft.com/en-us/dotnet/core/extensions/) - .NET Extensions

---

⭐ Star this repository if you found it helpful!