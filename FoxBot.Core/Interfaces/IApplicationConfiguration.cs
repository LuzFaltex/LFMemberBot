using FoxBot.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.Core.Interfaces
{
    public interface IApplicationConfiguration
    {
        /// <summary>
        /// Current status of the bot
        /// </summary>
        BotState BotState { get; set; }

        /// <summary>
        /// Gets or sets the token
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// Gets or sets the owner
        /// </summary>
        ulong Owner { get; set; }

        /// <summary>
        /// Gets or sets the Version
        /// </summary>
        Version BotVersion { get; set; }

        /// <summary>
        /// A list of the guilds the bot is configured to manage
        /// </summary>
        List<IGuildConfiguration> Guilds { get; set; }
    }
}
