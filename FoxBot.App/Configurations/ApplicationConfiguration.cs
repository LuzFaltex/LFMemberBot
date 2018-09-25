using Discord.Commands;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FoxBot.App.Configurations
{
    public class ApplicationConfiguration : IApplicationConfiguration
    {
        public ApplicationConfiguration()
        {
            Guilds = new List<IGuildConfiguration>();
        }

        [JsonConstructor]
        public ApplicationConfiguration(List<GuildConfiguration> guildConfigurations)
        {
            Guilds = new List<IGuildConfiguration>();
            if (guildConfigurations != null && guildConfigurations.Count > 0)
            {
                Guilds.AddRange(guildConfigurations);
            }
        }

        [DebuggerStepThrough]
        public IGuildConfiguration GetGuildConfiguration(ICommandContext context) => GetGuildConfiguration(context.Guild.Id);
        [DebuggerStepThrough]
        public IGuildConfiguration GetGuildConfiguration(ulong Id)
        {
            return Guilds.FirstOrDefault(x => x.Id == Id);
        }

        /// <summary>
        /// Get copyright date range
        /// </summary>
        public string CopyrightYear
        {
            get
            {
                return $"2014{((DateTime.Now.Year > 2018) ? $" - {DateTime.Now.Year}" : "")}";
            }
        }

        /// <summary>
        /// The state of the bot
        /// </summary>
        public BotState BotState { get; set; } = BotState.NotStarted;

        /// <summary>
        /// Gets or set the bot token
        /// </summary>
        public string Token { get; set; } = "";

        /// <summary>
        /// Gets or sets the owner of the bot.
        /// </summary>
        public ulong Owner { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or sets the bot token
        /// </summary>
        public Version BotVersion { get; set; } = new Version(1, 0, 0);
       
        /// <summary>
        /// A list of guilds the bot is configured to manage
        /// </summary>
        public List<IGuildConfiguration> Guilds { get; set; }
    }
}
