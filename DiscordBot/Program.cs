using Configuration;
using Discord.Commands;
using Discord.Events;
using Discord.Interactions;
using Discord.Services;
using Discord.WebSocket;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console() 
                .WriteTo.Debug()
                .WriteTo.File("logs/app.log")
                .CreateLogger();

            // Criação do host com configuração de DI
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog(logger)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configuração do logger
                    services.AddLogging();

                    ConfigurationManager configurationManager = new();

                    services.AddSingleton(configurationManager); // Gerenciador de configuração
                    services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configurationManager.ConfigurationRoot);
                    
                    services.AddScoped<MarchDbContext>();
                    services.AddScoped<UsersRepository>();
                    services.AddScoped<ServerRepository>();

                    services.AddSingleton<DiscordSocketConfig>(new DiscordSocketConfig 
                    { 
                        GatewayIntents = Discord.GatewayIntents.All,
                        AlwaysDownloadUsers = false,
                        AlwaysDownloadDefaultStickers = false,
                        AlwaysResolveStickers = false,
                        FormatUsersInBidirectionalUnicode = false,
                        LogGatewayIntentWarnings = false
                    });



                    // Registrando os serviços do bot
                    services.AddSingleton<DiscordSocketClient>(); // Cliente do Discord
                    services.AddSingleton<DiscordStartupService>(); // Serviço para iniciar o bot

                    // Registrando o InteractionService manualmente
                    services.AddSingleton<InteractionService>(provider =>
                    {
                        // O InteractionService precisa de um IServiceProvider para resolver suas dependências.
                        var discordClient = provider.GetRequiredService<DiscordSocketClient>();

                        return new InteractionService(discordClient.Rest);
                    });

                    // Registrando o InteractionHandlingService como Singleton
                    services.AddSingleton<InteractionHandlingService>();

                    // Adicionando os eventos
                    EventHandlerService.RegisterServices(services);

                    // Configuração de IHostedService
                    services.AddHostedService<DiscordStartupService>(); // Registrando o DiscordStartupService como IHostedService
                    services.AddHostedService<InteractionHandlingService>(); // Registrando o InteractionHandlingService como IHostedService
                    services.AddHostedService<EventHandlerService>();
                })
                .Build();

            // Iniciar o host e os serviços registrados
            await host.RunAsync();
        }
    }
}
