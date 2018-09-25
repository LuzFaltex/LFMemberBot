using Discord;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Services
{
    public class UserService
    {
        private readonly DiscordSocketClient _discord;
        private readonly LogHelper _logHelper;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private static readonly string LogSource = "UserService";

        public UserService(DiscordSocketClient discord, LogHelper logHelper, IApplicationConfiguration appConfig)
        {
            _discord = discord;
            _logHelper = logHelper;
            _applicationConfiguration = appConfig;

            _discord.UserJoined += UserJoinedAsync;
        }

        public async Task BanUser(SocketGuildUser user, IUser mod, string reason, bool fromBonk = false)
        {
            var appConfig = _applicationConfiguration as ApplicationConfiguration;
            var guildConfig = appConfig.GetGuildConfiguration(user.Guild.Id);

            // Build an embed
            EmbedBuilder embedMessage = new EmbedBuilder
            {
                Color = Color.Red,
                Title = "🔨"
            };

            // Build the message
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Type: {(fromBonk ? "Bonk -> " : "" )}Ban");
            sb.AppendLine($"User: {user.Username}#{user.Discriminator} ({user.Id})");
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine($":Responsible Moderator: {mod.Username}#{mod.Discriminator} ({mod.Id})");

            embedMessage.Description = sb.ToString();

            // Post the message
            SocketTextChannel channel = user.Guild.GetTextChannel(appConfig.GetGuildConfiguration(user.Guild.Id).BotLogChannel);
            await channel.SendMessageAsync("", embed: embedMessage.Build());

            await _logHelper.InfoAsync(sb.ToString(), LogSource);

            // Ban the user
            await user.Guild.AddBanAsync(user, 0, reason, new RequestOptions() { AuditLogReason = reason });
        }

        public async Task IssueCategoryRoles(SocketGuildUser user)
        {
            var appConfig = _applicationConfiguration as ApplicationConfiguration;
            var guildConfig = appConfig.GetGuildConfiguration(user.Guild.Id) as GuildConfiguration;

            List<IRole> rolesToAdd = new List<IRole>();

            // No need to check if we need to, that's handled by the UserJoinedAsync handler
            foreach (var roleCategory in guildConfig.RoleCategories)
            {
                if (roleCategory.CategoryRole.RoleId == guildConfig.EveryoneRole)
                {
                    continue;
                }

                rolesToAdd.Add(user.Guild.GetRole(roleCategory.CategoryRole.RoleId));
            }

            if (rolesToAdd.Count > 0)
            {
                try
                {
                    await _logHelper.DebugAsync($"Adding divider roles to {user.Username}#{user.Discriminator}", LogSource);
                    await user.AddRolesAsync(rolesToAdd);
                }
                catch (Exception ex)
                {
                    await _logHelper.DebugAsync($"Failed to add divider roles to {user.Username}#{user.Discriminator}: {ex.Message}", LogSource);
                }
            }
        }

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            var appConfig = _applicationConfiguration as ApplicationConfiguration;
            var guildConfig = appConfig.GetGuildConfiguration(user.Guild.Id);

            await _logHelper.InfoAsync($"{user.Username}#{user.Discriminator} has joined the server.", LogSource);

            // See if the user has a bonk filed against them
            IBonkConfiguration bonk = guildConfig.Bonks.FirstOrDefault(x => x.Victim == user.Id);
            if(bonk != null)
            {

                IUser mod = user.Guild.GetUser(bonk.Moderator);

                await BanUser(user, mod, bonk.Reason, true);

                // Remove the bonk entry
                appConfig.GetGuildConfiguration(user.Guild.Id).Bonks.Remove(bonk);

                return;
            }
            
            // Issue the role categories
            if (guildConfig.AutoIssueRoleCategories)
            {
                await IssueCategoryRoles(user);
            }

            if (guildConfig.JoinMode == Core.Enums.JoinMode.All)
            {
                await user.WelcomeUser(guildConfig);
            }
        }
    }
}
