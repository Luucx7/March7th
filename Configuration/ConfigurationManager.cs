using Configuration.ConfigurationTypes;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class ConfigurationManager
    {
        private const string ENVIRONMENT_VARIABLE = "APPLICATION_ENV";
        private const string DEFAULT_ENVIRONMENT_VARIABLE = "Development";

        // Root
        public IConfigurationRoot ConfigurationRoot { get; }

        // Custom configurations
        public DiscordConfiguration DiscordConfiguration { get; }

        public ConfigurationManager()
        {
            string environment = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE) ?? DEFAULT_ENVIRONMENT_VARIABLE;

            this.ConfigurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

            this.DiscordConfiguration = this.ConfigurationRoot.GetRequiredSection("Discord").Get<DiscordConfiguration>() ?? throw new KeyNotFoundException("Discord configuration is missing or invalid.");
        }
    }
}
