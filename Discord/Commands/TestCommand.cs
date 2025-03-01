using Discord.Interactions;
using Infrastructure;
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

            var servers = await dbContext.Servers.ToListAsync();

            await RespondAsync($"oiii {servers.Count}");
        }
    }
}
