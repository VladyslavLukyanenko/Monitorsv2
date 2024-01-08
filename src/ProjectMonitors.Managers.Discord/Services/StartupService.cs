using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProjectMonitors.Managers.Discord.Services
{
    public class StartupService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<StartupService> _logger;
        private readonly CommandHandler _commandHandler;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;

        public StartupService(IServiceProvider services, ILogger<StartupService> logger, CommandHandler commandHandler)
        {
            _services = services;
            _logger = logger;
            _commandHandler = commandHandler;

            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandService = _services.GetRequiredService<CommandService>();
            _config = _services.GetRequiredService<IConfiguration>();
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(TokenType.Bot, _config["Discord:Token"]);
            await _client.StartAsync();
            _commandHandler.Initialize();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }
    }
}