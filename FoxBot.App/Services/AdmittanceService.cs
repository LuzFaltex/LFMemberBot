using Discord;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Services
{
    public class AdmittanceService
    {
        private readonly DiscordSocketClient _discord;
        private readonly LogHelper _logHelper;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly string LogSource = "Admittance";

        /// <summary>
        /// This service is responsible for authorizing users
        /// to join the server. It functions using an enum that
        /// evaluates the <see cref="JoinMode"/>
        /// </summary>
        public AdmittanceService(DiscordSocketClient discord, LogHelper logHelper, IApplicationConfiguration appConfig)
        {
            _discord = discord;
            _logHelper = logHelper;
            _applicationConfiguration = appConfig;

            _discord.ReactionAdded += ReactionAdded;
            _discord.UserJoined += UserJoined;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            var appConfig = _applicationConfiguration as ApplicationConfiguration;
            var guildConfig = appConfig.GetGuildConfiguration(user.Guild.Id);

            if (guildConfig.JoinMode == JoinMode.All && guildConfig.MemberRole != ulong.MaxValue)
            {
                // Add the member role to the user
                await _logHelper.DebugAsync($"Giving the member role to {user.Username}#{user.Discriminator}.", LogSource);
                await user.AddRoleAsync(user.Guild.GetRole(guildConfig.MemberRole));
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {

            var appConfig = _applicationConfiguration as ApplicationConfiguration;
            var guildChannel = channel as SocketGuildChannel;
            var guildConfig = appConfig.GetGuildConfiguration(guildChannel.Guild.Id);

            if (guildConfig.JoinMode == JoinMode.Vote)
            {

                var userMessage = await message.GetOrDownloadAsync();
                var thumbsUp = new Emoji("👍");
                SocketGuildUser reactionUser = reaction.User.IsSpecified ? reaction.User.Value as SocketGuildUser : null;

                if (reaction.MessageId == guildConfig.HoldingMessageID && !reactionUser.IsBot)
                {
                    if (reaction.Emote.Name != thumbsUp.Name)
                    {
                        await userMessage.RemoveAllReactionsAsync();
                        await userMessage.AddReactionAsync(thumbsUp);
                    }
                    else // Someone reacted with a thumbsup emoji
                    {
                        if (reactionUser != null)
                        {
                            await _logHelper.DebugAsync($"Giving the member role to {reactionUser.Username}#{reactionUser.Discriminator}.", LogSource);
                            await reactionUser.AddRoleAsync(reactionUser.Guild.GetRole(guildConfig.MemberRole));
                            await reactionUser.WelcomeUser(guildConfig);
                            await Task.Delay(500);
                            await userMessage.RemoveReactionAsync(thumbsUp, reactionUser);
                        }
                    }
                }
            }
        }
    }
}
