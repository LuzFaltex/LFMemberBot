using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    public class GeneralCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ApplicationConfiguration _applicationConfiguration;

        public GeneralCommands(ApplicationConfiguration appConfig)
        {
            _applicationConfiguration = appConfig;
        }

        [Command("help")]
        [RequireCommandChannel]
        public async Task ShowHelp()
        {
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(Context);
            string prefix = guildConfig.CommandPrefix;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{prefix}help - Shows this command");
            sb.AppendLine($"{prefix}iam <role> - Join a role");
            sb.AppendLine($"{prefix}iamnot <role> - Leave a role");
            sb.AppendLine($"{prefix}roles - Show a list of roles you can join");
            sb.AppendLine($"{prefix}members - Shows a current count of members");
            sb.AppendLine($"{prefix}ping - ensure the bot is functioning");
            sb.AppendLine($"{prefix}About - Information about the bot");
            sb.AppendLine($"{prefix}Rule <number> - Returns the rule in question");
            sb.AppendLine($"{prefix}Tag <tag> - Prints a pre-built message");
            sb.AppendLine($"{prefix}Tags - Prints a list of the current tags");
            sb.AppendLine();
            sb.AppendLine($"{prefix}mod help - show moderation commands");
            sb.AppendLine($"{prefix}admin help - show admin commands");

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.AddField("Command Information", $"Command prefix: `{prefix}`\r\nArgumentSyntax: [optional] <required>");
            embedBuilder.AddField("Commands", sb.ToString());

            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("ping")]
        [RequireCommandChannel]
        public Task PingPong() => ReplyAsync("Pong!");

        [Command("members")]
        [RequireCommandChannel]
        public Task GetMemberCount() => ReplyAsync($"{Context.Guild.MemberCount} members in the guild.");

        [Command("about")]
        [RequireCommandChannel]
        public async Task GetBotInfo()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Built by Foxtrek_64",
                    Url = "https://www.LuzFaltex.com"
                },
                Title = "LuzFaltex Management Bot",
                Description = "A unified management bot for use across multiple servers.",
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Version",
                        Value = _applicationConfiguration.BotVersion.ToString()
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Current Guilds",
                        Value = _applicationConfiguration.Guilds.Count
                    }
                },
                Color = Color.Green
            };
            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("rule")]
        public Task GetRule(int ruleNumber)
        {
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(Context);

            if (ruleNumber > guildConfig.Rules.Count)
            {
                return ReplyAsync("Invalid rule ID");
            }
            else
            {
                return ReplyAsync($"Rule {ruleNumber}: {guildConfig.Rules[ruleNumber - 1]}");
            }
        }

        [Command("tag")]
        public Task GetTag(string key)
        {
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(Context);

            if (guildConfig.Tags.ContainsKey(key))
            {
                return ReplyAsync(guildConfig.Tags[key]);
            }
            else
            {
                return ReplyAsync("Unknown Tag");
            }
        }

        [Command("tags")]
        public Task GetTags()
        {
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(Context);

            return ReplyAsync("Tags: " + Environment.NewLine + string.Join(", ", guildConfig.Tags.Keys));
        }

        [Command("welcome")]
        [RequireCommandChannel]
        [RequireAdmin]
        public Task WelcomeUser(SocketGuildUser user) => user.WelcomeUser(_applicationConfiguration.GetGuildConfiguration(Context));

    }
}
