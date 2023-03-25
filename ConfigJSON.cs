using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot
{
    internal struct ConfigJSON
    {
        // Property to store the bot token read from the configuration file
        [JsonProperty("token")]
        public string Token { get; private set; }

        // Property to store the command prefix read from the configuration file
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }
}
