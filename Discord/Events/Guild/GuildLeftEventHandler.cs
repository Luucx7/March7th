using Discord.WebSocket;
using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Events.Guild
{
    public class GuildLeftEventHandler
    {
        private readonly DiscordSocketClient _discordSocket;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GuildLeftEventHandler> _logger;

        public GuildLeftEventHandler(DiscordSocketClient client, IServiceProvider serviceProvider, ILogger<GuildLeftEventHandler> logger)
        {
            _discordSocket = client;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task OnGuildLeftAsync(SocketGuild guild)
        {
            Task.Run(async () =>
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ServerRepository serverRepository = scope.ServiceProvider.GetRequiredService<ServerRepository>();

                await serverRepository.RemoveServer(guild.Id);
            });

            return Task.CompletedTask;
        }
    }
}
