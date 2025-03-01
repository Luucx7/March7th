using Discord.WebSocket;
using Infrastructure;
using Infrastructure.Entity;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Discord.Events
{
    public class GuildJoinedEventHandler
    {
        private string LOG_PREFIX = "[GUILD_JOIN]";
        private readonly DiscordSocketClient _discordSocket;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GuildJoinedEventHandler> _logger;

        public GuildJoinedEventHandler(DiscordSocketClient client, IServiceProvider serviceProvider, ILogger<GuildJoinedEventHandler> logger)
        {
            _discordSocket = client;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task OnGuildJoinedAsync(SocketGuild guild)
        {
            Task.Run(async () =>
            {
                try
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    MarchDbContext dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();

                    ServerEntity? serverEntity = await dbContext.Servers.Where((s) => s.server_id == guild.Id).FirstOrDefaultAsync();
                    if (serverEntity == null)
                    {
                        await JoinNewGuild(guild, scope);
                    } else
                    {
                        await JoinExistingGuild(guild, scope, serverEntity);
                    }
                } catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });

            return Task.CompletedTask;
        }

        private async Task JoinNewGuild(SocketGuild guild, IServiceScope scope)
        {
            ServerEntity newServer = new()
            {
                server_id = guild.Id,
                server_zone = "0",
                active = true,
                ready = false,
                added_at = DateTime.UtcNow,
                banned_at = null,
                removed_at = null
            };

            MarchDbContext dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();
            await dbContext.Servers.AddAsync(newServer);
            await dbContext.SaveChangesAsync();

            await SyncGuildMembers(guild, scope);
        }
        private async Task JoinExistingGuild(SocketGuild guild, IServiceScope scope, ServerEntity guildEntity)
        {
            if (guildEntity.banned_at.HasValue)
            {
                IUser guildOwner = await _discordSocket.GetUserAsync(guild.OwnerId);

                await guildOwner.SendMessageAsync($"Tentativa de me adicionar ao servidor {guild.Name} negada. Este servidor foi banido.");
                await guild.LeaveAsync();

                return;
            }

            ServerRepository serverRepository = scope.ServiceProvider.GetRequiredService<ServerRepository>();
            await serverRepository.ReactivateServer(guild.Id);

            await SyncGuildMembers(guild, scope);
        }

        private async Task SyncGuildMembers(SocketGuild guild, IServiceScope scope)
        {
            UsersRepository userRepo = scope.ServiceProvider.GetRequiredService<UsersRepository>();
            ServerRepository serverRepository = scope.ServiceProvider.GetRequiredService<ServerRepository>();

            await guild.DownloadUsersAsync();

            List<ulong> usersIds = guild.Users.Where(u => !u.IsBot).Select((u) => u.Id).ToList();
            await userRepo.SyncServerMembers(guild.Id, usersIds);
            guild.PurgeUserCache();

            await serverRepository.ServerReady(guild.Id);
        }
    }
}
