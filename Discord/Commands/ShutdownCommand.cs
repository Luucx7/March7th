using Configuration;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discord.Commands
{
    public class ShutdownCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IHostApplicationLifetime _lifecycleService;
        private readonly ConfigurationManager _configurationManager;
        private readonly ILogger<ShutdownCommand> _logger;

        public ShutdownCommand(IHostApplicationLifetime lifecycleService, ConfigurationManager configurationManager, ILogger<ShutdownCommand> logger)
        {
            _lifecycleService = lifecycleService;
            _configurationManager = configurationManager;
            _logger = logger;
        }

        [RequireOwner]
        [SlashCommand("shutdown", "Shuts down the bot.", true, Interactions.RunMode.Async)]
        public async Task Shutdown()
        {
            ulong userId = base.Context.User.Id;

            if (_configurationManager.DiscordConfiguration.Owner == userId)
            {
                _logger.LogWarning("Bot shutdown requested by {user_id}", userId);
                await RespondAsync("Shutting down...", ephemeral: true);

                _lifecycleService.StopApplication();
            } else
            {
                _logger.LogWarning("The user {user_id} tried to request the bot shutdown, but did not had permission.", userId);
                await RespondAsync("Only the bot owner use this command.", ephemeral: true);
            }
        }
    }
}
