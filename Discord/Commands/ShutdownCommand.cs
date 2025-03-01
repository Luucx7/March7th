using Configuration;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Discord.Commands
{
    public class ShutdownCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IHostApplicationLifetime _lifecycleService;
        private readonly ConfigurationManager _configurationManager;

        public ShutdownCommand(IHostApplicationLifetime lifecycleService, ConfigurationManager configurationManager)
        {
            _lifecycleService = lifecycleService;
            _configurationManager = configurationManager;
        }

        [RequireOwner]
        [SlashCommand("shutdown", "Shuts down the bot.", true, Interactions.RunMode.Async)]
        public async Task Shutdown()
        {
            ulong userId = base.Context.User.Id;

            if (_configurationManager.DiscordConfiguration.Owner == userId)
            {
                await RespondAsync("Shutting down...", ephemeral: true);

                _lifecycleService.StopApplication();
            } else
            {
                await RespondAsync("Only the bot owner use this command.", ephemeral: true);
            }
        }
    }
}
