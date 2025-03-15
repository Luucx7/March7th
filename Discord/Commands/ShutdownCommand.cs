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

                var embed = new EmbedBuilder()
                    .WithTitle("Hora de dormir")
                    .WithDescription("Encerrando o bot...")
                    .WithColor(Color.DarkRed)
                    .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-sleeping.png?raw=true")
                    .Build();

                await RespondAsync(embed: embed);

                _lifecycleService.StopApplication();
            } else
            {
                _logger.LogWarning("The user {user_id} tried to request the bot shutdown, but did not had permission.", userId);

                var embed = new EmbedBuilder()
                    .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-angry.png?raw=true")
                    .WithColor(Color.Red)
                    .WithDescription("Apenas o dono do bot pode realizar esta ação")
                    .Build();

                await RespondAsync(embed: embed);
            }
        }
    }
}
