﻿using Discord.Events.Guild;
using Discord.Events.User;
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

            GuildLeftEventHandler guildLeft = _services.GetRequiredService<GuildLeftEventHandler>();
            _discord.LeftGuild += guildLeft.OnGuildLeftAsync;

            UserJoinedEventHandler userJoined = _services.GetRequiredService<UserJoinedEventHandler>();
            _discord.UserJoined += userJoined.OnUserJoinedAsync;

            UserLeftEventHandler userLeft = _services.GetRequiredService<UserLeftEventHandler>();
            _discord.UserLeft += userLeft.OnUserLeftAsync;

            return Task.CompletedTask;
        }

        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<GuildJoinedEventHandler>();
            services.AddSingleton<GuildLeftEventHandler>();
            services.AddSingleton<UserJoinedEventHandler>();
            services.AddSingleton<UserLeftEventHandler>();
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
