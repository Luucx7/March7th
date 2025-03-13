using Castle.Core.Logging;
using Discord.Interactions;
using Discord.Services;
using Discord.WebSocket;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Events.User
{
    public class UserJoinedEventHandler
    {
        private readonly DiscordSocketClient _discordSocket;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserJoinedEventHandler> _logger;

        public UserJoinedEventHandler(DiscordSocketClient client, IServiceProvider serviceProvider, ILogger<UserJoinedEventHandler> logger)
        {
            _discordSocket = client;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task OnUserJoinedAsync(SocketGuildUser guildUser)
        {
            Task.Run(async() =>
            {
                if (guildUser.IsBot) return;

                using IServiceScope scope = _serviceProvider.CreateScope();
                MarchDbContext dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();

                ServerUserEntity? serverUser = await dbContext.ServerUsers.Where((su) => su.server_id == guildUser.Guild.Id && su.user_id == guildUser.Id).FirstOrDefaultAsync();
                if (serverUser != null)
                {
                    _logger.LogWarning("User {username} ({userid}) joined server {serverid} where it was already registered as a member", guildUser.Username, guildUser.Id, guildUser.Guild.Id);
                    return;
                }

                ServerUserEntity serverUserEntity = new()
                {
                    server_id = guildUser.Guild.Id,
                    user_id = guildUser.Id
                };

                await dbContext.ServerUsers.AddAsync(serverUserEntity);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("User {username} ({user_id}) joined server {server_id}", guildUser.Username, guildUser.Id, guildUser.Guild.Id);
            });

            return Task.CompletedTask;
        }
    }
}
