using Configuration;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Discord.Interactions;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Services
{
    public class InteractionHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionHandlingService> _logger;
        private readonly ConfigurationManager _configurationManager;

        public InteractionHandlingService(
            DiscordSocketClient discord,
            InteractionService interactions,
            IServiceProvider services,
            ConfigurationManager config,
            ILogger<InteractionHandlingService> logger)
        {
            _discord = discord;
            _interactions = interactions;
            _services = services;
            _logger = logger;
            _configurationManager = config;

            //_discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
            _discord.Ready += async () =>
            {
                await _interactions.RegisterCommandsGloballyAsync(true);
                //var register = await _interactions.RegisterCommandsToGuildAsync(1335440466107306066, true);
            };
            _discord.InteractionCreated += OnInteractionAsync;

            _interactions.Log += msg => LogHelper.OnLogAsync(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Espera o bot estar pronto para registrar os comandos.
            Assembly discordAssembly = Assembly.Load("Discord");
            await _interactions.AddModulesAsync(discordAssembly, _services);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interactions.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_discord, interaction);
                var result = await _interactions.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
            catch
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(msg => msg.Result.DeleteAsync());
                }
            }
        }
    }
}
