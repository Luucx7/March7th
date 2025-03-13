using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Commands
{
    public class TestCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IServiceProvider _serviceProvider;

        public TestCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [SlashCommand("teste", "teste do luc", false, Interactions.RunMode.Async)]
        public async Task Test()
        {
            ulong userId = base.Context.User.Id;

            var scope = this._serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();
            var socket = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();

            List<ulong> mutualServers = await dbContext.ServerUsers.Where((su) => su.user_id == userId).Select((su) => su.server_id).ToListAsync();

            List<SocketGuild> guilds = socket.Guilds.Where((g) => mutualServers.Contains(g.Id)).ToList();

            string servers = string.Join(" - ", guilds.Select((g) => g.Name));

            await RespondAsync($"{servers}");
        }
    }
}
