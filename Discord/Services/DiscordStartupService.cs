using Configuration;
using Discord.Utility;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discord.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILogger<DiscordSocketClient> _logger;
        private readonly ConfigurationManager _configurationManager;

        public DiscordStartupService(DiscordSocketClient discord, ILogger<DiscordSocketClient> logger, ConfigurationManager configurationManager)
        {
            _discord = discord;
            _logger = logger;
            _configurationManager = configurationManager;

            _discord.Log += msg => LogHelper.OnLogAsync(_logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discord.LoginAsync(TokenType.Bot, _configurationManager.DiscordConfiguration.Token);
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}
