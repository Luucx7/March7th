using Discord.WebSocket;
using Infrastructure.Entity;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Discord.Events.User
{
    public class UserLeftEventHandler
    {
        private readonly DiscordSocketClient _discordSocket;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserLeftEventHandler> _logger;

        public UserLeftEventHandler(DiscordSocketClient client, IServiceProvider serviceProvider, ILogger<UserLeftEventHandler> logger)
        {
            _discordSocket = client;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            Task.Run(async () =>
            {
                if (user.IsBot) return; // Bots don't have birthdays... or do they?

                using IServiceScope scope = _serviceProvider.CreateScope();
                MarchDbContext dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();

                ServerUserEntity? serverUser = await dbContext.ServerUsers.Where((su) => su.user_id == user.Id && su.server_id == guild.Id).FirstOrDefaultAsync();
                if (serverUser == null)
                {
                    _logger.LogWarning("User {username} ({userid}) left server {serverid} where it was not registered as a member", user.Username, user.Id, guild.Id);
                    return;
                }

                dbContext.ServerUsers.Remove(serverUser);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("User {username} ({userid}) left server {serverid}", user.Username, user.Id, guild.Id);
            });

            return Task.CompletedTask;
        }
    }
}
