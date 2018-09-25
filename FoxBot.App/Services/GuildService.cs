using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Services
{
    public class GuildService
    {
        private readonly DiscordSocketClient _discord;
        private readonly LogHelper _logHelper;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private static readonly string LogSource = "GuildService";

        public GuildService(DiscordSocketClient discord, LogHelper logHelper, IApplicationConfiguration appConfig)
        {
            _discord = discord;
            _logHelper = logHelper;
            _applicationConfiguration = appConfig;

            _discord.GuildUpdated += _discord_GuildUpdated;
        }

        private async Task _discord_GuildUpdated(SocketGuild oldGuild, SocketGuild newGuild)
        {
            if (oldGuild.Name != newGuild.Name)
            {
                var guildConfig = (_applicationConfiguration as ApplicationConfiguration).GetGuildConfiguration(oldGuild.Id);
                guildConfig.Name = newGuild.Name;

                await ConfigurationParser.WriteChanges(_applicationConfiguration);
            }
        }
    }
}
