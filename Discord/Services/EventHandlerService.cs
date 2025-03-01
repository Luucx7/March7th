using Discord.Events;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Discord.Services
{
    public class EventHandlerService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILogger<EventHandlerService> _logger;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public EventHandlerService(DiscordSocketClient discord, InteractionService interactions, IServiceProvider services, ILogger<EventHandlerService> logger)
        {
            _discord = discord;
            _interactions = interactions;
            _services = services;
            _logger = logger;
        }

        public Task RegisterEvents()
        {
            GuildJoinedEventHandler guildJoined = _services.GetRequiredService<GuildJoinedEventHandler>();
             _discord.JoinedGuild += guildJoined.OnGuildJoinedAsync;

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.RegisterEvents();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
