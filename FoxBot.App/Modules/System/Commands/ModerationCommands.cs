using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Structs;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    [Group("mod")]
    [Alias("Moderator")]
    public class ModerationCommands : ModuleBase
    {
        private readonly ApplicationConfiguration appConfig;

        public ModerationCommands(ApplicationConfiguration applicationConfiguration)
        {
            appConfig = applicationConfiguration;
        }

        [Command()]
        [Alias("help")]
        [RequireCommandChannel]
        public async Task ShowHelp()
        {
            string prefix = appConfig.GetGuildConfiguration(Context).CommandPrefix;

            StringBuilder mod = new StringBuilder();
            mod.AppendLine($"{prefix}mod help - Shows this dialogue");
            mod.AppendLine($"{prefix}mod kick <user> <reason> - Kicks the specified user from the server.");
            mod.AppendLine($"{prefix}mod ban <user> [duration] <reason> - Bans the specified user for the duration specified. Duration defaults to permaban");
            mod.AppendLine($"{prefix}mod bonk <user id> [<username>#<discrim>] <reason> - Bans a user who is not on the server.");
            mod.AppendLine();
            mod.AppendLine("Duration should be provided in the following format: 5w4d3h2m1s.");
            mod.AppendLine("You need only provide the necessary components of the - for instance, a 5 minute ban need only specify \"5m\"");

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.AddField("Command Information", $"Command prefix: `{prefix}`\r\nArgumentSyntax: [optional] <required>");
            embedBuilder.AddField("Commands", mod.ToString());

            await ReplyAsync("", embed: embedBuilder.Build());

            
        }

        [Command("kick")]
        [RequireModerator]
        public async Task KickUser(IGuildUser user, [Remainder][NotNullOrWhiteSpace] string reason)
        {
            var guildConfig = appConfig.GetGuildConfiguration(Context);

            SocketGuildUser sUser = user as SocketGuildUser;

            EmbedBuilder errorEmbed = new EmbedBuilder
            {
                Color = Color.Red
            };

            if (Context.User.Id == user.Id)
            {
                errorEmbed.Description = "You cannot kick yourself.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (user.Id == Context.Guild.OwnerId)
            {
                errorEmbed.Description = "You cannot kick the owner.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (user.Id == Context.Client.CurrentUser.Id)
            {
                errorEmbed.Description = "You cannot kick me.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else
            {
                errorEmbed.Title = "👢";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"**Kick**");
                sb.AppendLine($"**User**: {sUser.Username}#{sUser.Discriminator} ({sUser.Id})");
                sb.AppendLine($"**Reason**: {reason}");
                sb.AppendLine($"**Responsible Moderator**: {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})");

                errorEmbed.Description = sb.ToString();

                SocketTextChannel channel = await Context.Guild.GetTextChannelAsync(guildConfig.BotLogChannel) as SocketTextChannel;
                await channel.SendMessageAsync("", embed: errorEmbed.Build());

                await user.KickAsync(reason, new RequestOptions() { AuditLogReason = reason });
            }
        }

        [Command("ban")]
        [RequireModerator]
        public async Task BanUser(IGuildUser user, TimeSpan? duration = null, [Remainder][NotNullOrWhiteSpace] string reason = "")
        {
            var guildConfig = appConfig.GetGuildConfiguration(Context);

            SocketGuildUser sUser = user as SocketGuildUser;

            EmbedBuilder errorEmbed = new EmbedBuilder
            {
                Color = Color.Red
            };

            if (Context.User.Id == user.Id)
            {
                errorEmbed.Description = "You cannot ban yourself.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (user.Id == Context.Guild.OwnerId)
            {
                errorEmbed.Description = "You cannot ban the owner.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (user.Id == Context.Client.CurrentUser.Id)
            {
                errorEmbed.Description = "You cannot ban me.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else
            {
                errorEmbed.Title = "🔨";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"**Ban**");
                sb.AppendLine($"**User**: {sUser.Username}#{sUser.Discriminator} ({sUser.Id})");
                sb.AppendLine($"**Reason**: {reason}");
                sb.AppendLine($"**Responsible Moderator**: {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})");

                errorEmbed.Description = sb.ToString();

                SocketTextChannel channel = await Context.Guild.GetTextChannelAsync(guildConfig.BotLogChannel) as SocketTextChannel;
                await channel.SendMessageAsync("", embed: errorEmbed.Build());

                await Context.Guild.AddBanAsync(user, 0, reason, new RequestOptions() { AuditLogReason = reason });

                if (duration.HasValue)
                {
                    DateTimeOffset unbanDate = DateTimeOffset.UtcNow.Add(duration.Value);
                    BanConfiguration bc = new BanConfiguration(sUser.Id, Context.User.Id, reason, Instant.FromDateTimeOffset(unbanDate));
                    guildConfig.TempBans.Add(bc);
                    await ConfigurationParser.WriteChanges(appConfig);
                }
            }
        }

        [Command("bonk")]
        [RequireModerator]
        public async Task BonkUser(ulong userID, string UsernameAndDiscrim = "", [Remainder][NotNullOrWhiteSpace] string reason = "")
        {
            var guildConfig = appConfig.GetGuildConfiguration(Context);

            string Username = UsernameAndDiscrim.Split(new char[] { '#' })[0];
            string Discrim = UsernameAndDiscrim.Split(new char[] { '#' })[1];

            EmbedBuilder errorEmbed = new EmbedBuilder
            {
                Color = Color.Red
            };

            if (Context.User.Id == userID)
            {
                errorEmbed.Description = "You cannot ban yourself.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (userID == Context.Guild.OwnerId)
            {
                errorEmbed.Description = "You cannot ban the owner.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else if (userID == Context.Client.CurrentUser.Id)
            {
                errorEmbed.Description = "You cannot ban me.";
                await ReplyAsync("", embed: errorEmbed.Build());
            }
            else
            {
                errorEmbed.Title = "👁";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"**Bonk**");
                sb.AppendLine($"**User**: {Username}#{Discrim} ({userID})");
                sb.AppendLine($"**Reason**: {reason}");
                sb.AppendLine($"**Responsible Moderator**: {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})");

                errorEmbed.Description = sb.ToString();

                if (await Context.Guild.GetTextChannelAsync(guildConfig.BotLogChannel) is SocketTextChannel channel)
                {
                    await channel.SendMessageAsync("", embed: errorEmbed.Build());
                }
                else
                {
                    await ReplyAsync("", embed: errorEmbed.Build());
                }

                guildConfig.Bonks.Add(new BonkConfiguration(userID, Context.User.Id, reason));
                await ConfigurationParser.WriteChanges(appConfig);
            }

        }
    }
}
