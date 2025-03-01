using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration.ConfigurationTypes
{
    public class DiscordConfiguration
    {
        public string Token { get; set; }
        public ulong Owner { get; set; }
        public ulong AdminServer { get; set; }
    }
}
