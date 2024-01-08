using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ProjectMonitors.Managers.Discord.Services
{
public class CommandHandler
{
    private readonly IServiceProvider _services;
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;


    public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client)
    {
        _services = services;
        _commands = commands;
        _client = client;

    }

    public async void Initialize()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        _client.MessageReceived += HandleCommandAsync;
        
    }
    

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage {Source: MessageSource.User} message) return;
        var argPos = 0;
        if (!message.HasStringPrefix("$", ref argPos))
            return;
        var context = new SocketCommandContext(_client, message);
        await _commands.ExecuteAsync(
            context, 
            argPos,
            _services);
    }
}
}