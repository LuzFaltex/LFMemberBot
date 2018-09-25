using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    public partial class AdminModule
    {
        [Group("guild")]
        public class GuildModule : ModuleBase
        {
            private readonly ApplicationConfiguration _appConfig;

            public GuildModule(ApplicationConfiguration appConfig)
            {
                _appConfig = appConfig;
            }

            [Command("help")]
            [RequireCommandChannel]
            public async Task ShowHelp()
            {
                string prefix = _appConfig.GetGuildConfiguration(Context).CommandPrefix;

                StringBuilder admin = new StringBuilder(1024);
                admin.AppendLine($"{prefix}admin guild help - Shows this dialogue");
                admin.AppendLine($"{prefix}admin guild prefix [newPrefix] - Gets the current prefix or sets the prefix to the specified string.");
                admin.AppendLine($"{prefix}admin guild register - Registers this guild with the bot config");
                admin.AppendLine();
                admin.AppendLine($"{prefix}admin guild adminrole [role] - Gets or sets the admin role. Set to `Everyone` to unset.");
                admin.AppendLine($"{prefix}admin guild modrole [role] - Gets or sets the moderator role. Set to `Everyone` to unset.");
                admin.AppendLine($"{prefix}admin guild memberrole [role] - Gets or sets the member role. Set to `Everyone` to unset.");
                admin.AppendLine($"{prefix}admin guild welcomechannel [channel] - Gets or sets the channel used to welcome users.");
                admin.AppendLine($"{prefix}admin guild welcomemessage [message] - Gets or sets the message used to welcome users.");
                admin.AppendLine($"{prefix}admin guild holdingchannel [channel] - Gets or sets the channel used for the holding message.");
                admin.AppendLine($"{prefix}admin guild holdingmessage [message] - Gets or sets the message users must upvote to join. Holding channel must be set! Use `none` to disable.");
                admin.AppendLine($"{prefix}admin guild autoissue [bool] - Gets or sets whether to auto-issue divider roles");
                admin.AppendLine($"{prefix}admin guild joinmode [all|vote|manual] - Gets or sets how to handle users joining");
                admin.AppendLine($"{prefix}admin guild commandchannels [<add|remove> <channel>] - Gets, adds to, or removes a channel from the current listing of command channels");
                admin.AppendLine($"{prefix}admin guild aliases [<add|remove> <role> <alias>] - Gets, adds to, or removes an alias from the current listing of role aliases");
                admin.AppendLine($"{prefix}admin guild rules [add [position] <rule>|remove <position>] - Lists, adds, or removes a rule");
                admin.AppendLine($"{prefix}admin guild rulechannel [channel] - Gets or sets the channel in which rules are displayed");
                admin.AppendLine($"{prefix}admin guild tag <<add <key> <value>>|<remove <key>>> - Adds or removes a tag");


                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder.AddField("Command Information", $"Command prefix: `{prefix}`\r\nArgumentSyntax: [optional] <required>");
                embedBuilder.AddField("Commands", admin.ToString());

                await ReplyAsync("", embed: embedBuilder.Build());
            }

            [Command("prefix")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetPrefix()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Current guild prefix: {guildConfig.CommandPrefix}");
            }

            [Command("prefix")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetPrefix([NotNullOrWhiteSpace] string newPrefix)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                if (!guildConfig.CommandPrefix.Equals(newPrefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    await ReplyAsync($"The prefix is already `{newPrefix}`!");
                }
                else
                {
                    var oldPrefix = guildConfig.CommandPrefix;
                    guildConfig.CommandPrefix = newPrefix;

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync($"The guild prefix has been changed: `{oldPrefix}` => `{newPrefix}`");
                }
            }

            [Command("getcategories")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetCategories()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                StringBuilder sb = new StringBuilder();
                foreach (var category in guildConfig.RoleCategories)
                {
                    string roleName;
                    if (category.Id == ulong.MaxValue)
                    {
                        roleName = "General";
                    }
                    else { roleName = Context.Guild.GetRole(category.Id).Name; }
                    sb.AppendLine(roleName);
                }
                await ReplyAsync(sb.ToString());
            }

            [Command("register")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task Register()
            {
                GuildConfiguration guildConfiguration = new GuildConfiguration(Context.Guild.Name, Context.Guild.Id, "!", Context.Guild.EveryoneRole.Id);

                if (_appConfig.GetGuildConfiguration(Context) != null)
                {
                    _appConfig.Guilds.Add(guildConfiguration);
                    await ReplyAsync("Registered!");
                }

                await ReplyAsync("Error: This guild already exists! Please modify it instead.");
            }

            [Command("adminrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetAdminRole()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Current administrative role: { (guildConfig.AdminRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.AdminRole).Name) }");
            }

            [Command("adminrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetAdminRole(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                string oldRole = guildConfig.AdminRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.AdminRole).Name;

                guildConfig.AdminRole = (role.Equals(Context.Guild.EveryoneRole) ? ulong.MaxValue : role.Id);

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Administrative role updated: {oldRole} => {role.Name}");
            }

            [Command("modrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetModRole()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Current moderation role: { (guildConfig.ModeratorRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.ModeratorRole).Name) }");
            }

            [Command("modrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetModRole(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                string oldRole = guildConfig.ModeratorRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.ModeratorRole).Name;

                guildConfig.ModeratorRole = (role.Equals(Context.Guild.EveryoneRole) ? ulong.MaxValue : role.Id);

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Moderation role updated: {oldRole} => {role.Name}");
            }

            [Command("memberrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetMemberRole()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Current member role: { (guildConfig.MemberRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.MemberRole).Name) }");
            }

            [Command("memberrole")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetMemberRole(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                string oldRole = guildConfig.MemberRole == ulong.MaxValue ? "Unset" : Context.Guild.GetRole(guildConfig.MemberRole).Name;

                guildConfig.MemberRole = (role.Equals(Context.Guild.EveryoneRole) ? ulong.MaxValue : role.Id);

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Member role updated: {oldRole} => {role.Name}");
            }

            [Command("welcomechannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetWelcomeChannel()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);
                var channel = await Context.Guild.GetChannelAsync(guildConfig.WelcomeChannel);

                await ReplyAsync($"Welcome channel: { (channel == null ? "unset" : channel.Name) }");
            }

            [Command("welcomechannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetWelcomeChannel(IChannel channel)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);
                IMessageChannel oldChannel = (IMessageChannel)(await Context.Guild.GetChannelAsync(guildConfig.WelcomeChannel));

                // Set up a new channel
                guildConfig.WelcomeChannel = channel.Id;

                await ConfigurationParser.WriteChanges(_appConfig);
                await ReplyAsync($"Welcome channel updated: {(oldChannel.Name ?? "unset")} => {channel.Name}");
            }

            [Command("welcomemessage")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetWelcomeMessage()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Welcome message: {guildConfig.WelcomeMessageText}");
            }

            [Command("welcomemessage")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetWelcomeMessage([Remainder, NotNullOrWhiteSpace] string message)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                if (message.Equals("none"))
                {
                    guildConfig.WelcomeMessageText = string.Empty;

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync("Welcome message cleared. Users will no longer be welcomed.");
                }
                else
                {

                    guildConfig.WelcomeMessageText = message;

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync("Welcome message updated!");
                }
            }

            [Command("holdingchannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetHoldingChannel()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);
                var channel = await Context.Guild.GetChannelAsync(guildConfig.HoldingChannel);

                await ReplyAsync($"Holding channel: { (channel == null ? "unset" : channel.Name) }");
            }

            [Command("holdingchannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetHoldingChannel(IChannel channel)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);
                IMessageChannel oldChannel = (IMessageChannel)(await Context.Guild.GetChannelAsync(guildConfig.HoldingChannel));

                // Set up a new channel
                if (oldChannel == null)
                {
                    guildConfig.HoldingChannel = channel.Id;
                }
                else
                {
                    // Delete the old message, if it exists
                    SocketUserMessage oldMessage = (SocketUserMessage)(await oldChannel?.GetMessageAsync(guildConfig.HoldingMessageID));
                    await oldMessage?.DeleteAsync();
                }

                // Post the new message
                RestUserMessage newMessage = await (channel as SocketTextChannel).SendMessageAsync(guildConfig.HoldingMessageText);
                await newMessage.AddReactionAsync(new Emoji("👍"));

                guildConfig.HoldingMessageID = newMessage.Id;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Holding channel updated: {(oldChannel?.Name ?? "unset")} => {channel.Name}");
            }

            [Command("holdingmessage")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetHoldingMessage()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Holding message: {guildConfig.HoldingMessageText}");
            }

            [Command("holdingmessage")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetHoldingMessage([Remainder, NotNullOrWhiteSpace] string message)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                if (message.Equals("none"))
                {
                    guildConfig.HoldingMessageText = string.Empty;
                    IMessageChannel channel = (IMessageChannel)(await Context.Guild.GetChannelAsync(guildConfig.HoldingChannel));
                    var currentMessage = await channel?.GetMessageAsync(guildConfig.HoldingMessageID);
                    await currentMessage?.DeleteAsync();

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync("Holding message cleared.");
                }
                else
                {
                    IMessageChannel channel = (IMessageChannel)(await Context.Guild.GetChannelAsync(guildConfig.HoldingChannel));
                    IUserMessage currentMessage = (IUserMessage)(await channel?.GetMessageAsync(guildConfig.HoldingMessageID));

                    if (currentMessage == null)
                    {
                        currentMessage = (SocketUserMessage)(await channel.SendMessageAsync(message.Replace("{ServerName}", Context.Guild.Name)));
                        await currentMessage.AddReactionAsync(new Emoji("👍"));

                        guildConfig.HoldingMessageID = currentMessage.Id;
                        guildConfig.HoldingMessageText = message;

                        await ConfigurationParser.WriteChanges(_appConfig);

                        await ReplyAsync("Holding message set!");
                    }
                    else
                    {
                        await currentMessage.ModifyAsync(x => x.Content = message.Replace("{ServerName}", Context.Guild.Name));

                        guildConfig.HoldingMessageText = message;

                        await ConfigurationParser.WriteChanges(_appConfig);

                        await ReplyAsync("Holding message updated!");
                    }
                }
            }

            [Command("autoissue")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetAutoIssue()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                await ReplyAsync($"Auto-issue category roles?: {guildConfig.AutoIssueRoleCategories}");
            }

            [Command("autoissue")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetAutoIssue(bool value)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                bool oldValue = guildConfig.AutoIssueRoleCategories;

                guildConfig.AutoIssueRoleCategories = value;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Auto-issue category roles updated: {oldValue} => {value}");
            }

            [Command("joinmode")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetJoinMode()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Options:");
                sb.AppendLine("`All` - (Default) Allows all users to join the server. If a Member role is specified, it will automatically be granted on join.");
                sb.AppendLine("`Vote` - Members must react to a holding message using :thumbsup: before being granted the member role.");
                sb.AppendLine("`Manual` - A staff member must manually grant the user a member role. Similar to `All`, but does not automatically grant the Member role.");
                sb.AppendLine();
                sb.AppendLine($"Current value: {guildConfig.JoinMode}");

                await ReplyAsync(sb.ToString());
            }

            [Command("joinmode")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetJoinMode(JoinMode joinMode)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context);

                JoinMode oldMode = guildConfig.JoinMode;

                guildConfig.JoinMode = joinMode;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"JoinMode updated: {oldMode} => {joinMode}");
            }

            [Command("ruleschannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public Task GetRulesChannel()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;
                return ReplyAsync($"Current Rules Channel: <#{guildConfig.RulesChannel}>");
            }

            [Command("ruleschannel")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetRulesChannel(ITextChannel channel)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var oldChannel = await Context.Guild.GetChannelAsync(guildConfig.RulesChannel);

                // Set rule channel
                guildConfig.RulesChannel = channel.Id;

                // Remove old message if applicable
                if (oldChannel != null)
                {
                    var oldRulesMessage = await (oldChannel as SocketTextChannel).GetMessageAsync(guildConfig.RulesMessage) as SocketUserMessage;
                    await oldRulesMessage?.DeleteAsync();
                }

                // Post new rules message
                if (guildConfig.Rules.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int x = 0; x < guildConfig.Rules.Count; x++)
                    {
                        sb.AppendLine($"{x - 1}: {guildConfig.Rules[x]}");
                    }
                    var newMessage = await channel.SendMessageAsync(sb.ToString());

                    guildConfig.RulesMessage = newMessage.Id;
                }

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"RulesChannel updated: {oldChannel?.Name ?? "unset"} => {channel.Name}");
            }

            [Group("commandchannels")]
            public class CommandChannels : ModuleBase
            {
                private readonly ApplicationConfiguration _appConfig;

                public CommandChannels(ApplicationConfiguration appConfig)
                {
                    _appConfig = appConfig;
                }

                [Command]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task GetCommandChannels()
                {

                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Currently registered command channels:");
                    foreach (ulong channelId in guildConfig.CommandChannels)
                    {
                        var channel = await Context.Guild.GetChannelAsync(channelId);

                        if (channel != null)
                        {
                            sb.AppendLine(channel.Name);
                        }
                    }

                    await ReplyAsync(sb.ToString());
                }

                [Command("add")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task AddCommandChannel(ITextChannel channel)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    if (guildConfig.CommandChannels.Add(channel.Id))
                    {
                        await ConfigurationParser.WriteChanges(_appConfig);
                        await ReplyAsync($"Registered {channel.Name} as a command channel!");
                    }
                    else
                    {
                        await ReplyAsync($"{channel.Name} is already registered as a command channel.");
                    }
                }

                [Command("remove")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task RemoveCommandChannel(ITextChannel channel)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    if (guildConfig.CommandChannels.Remove(channel.Id))
                    {
                        await ReplyAsync($"{channel.Name} is no longer a command channel.");
                    }
                    else
                    {
                        await ReplyAsync($"{channel.Name} is not a registered command channel.");
                    }
                }
            }

            [Group("aliases")]
            public class Aliases : ModuleBase
            {
                private readonly ApplicationConfiguration _appConfig;

                public Aliases(ApplicationConfiguration appConfig)
                {
                    _appConfig = appConfig;
                }

                [Command]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task GetAliases()
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Currently registered role aliases:");
                    foreach (var (role, alias) in guildConfig.RoleAliases)
                    {
                        IRole cRole = Context.Guild.GetRole(role);
                        sb.AppendLine($"{cRole.Name} is also known as '{alias}'");
                    }

                    await ReplyAsync(sb.ToString());
                }

                [Command("add")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task AddAlias(IRole role, [NotNullOrWhiteSpace] string alias)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    if (guildConfig.RoleAliases.Contains((role.Id, alias)))
                    {
                        await ReplyAsync($"{alias} is already registered as an alias of {role.Name}");
                        return;
                    }

                    guildConfig.RoleAliases.Add((role.Id, alias));

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync($"Added {alias} as an alias for {role.Name}");
                }

                [Command("remove")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task RemoveAlias(IRole role, [NotNullOrWhiteSpace] string alias)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    if (guildConfig.RoleAliases.Contains((role.Id, alias)))
                    {
                        guildConfig.RoleAliases.Remove((role.Id, alias));
                        await ConfigurationParser.WriteChanges(_appConfig);
                        await ReplyAsync($"Removed {alias} as an alias for {role.Name}");
                    }
                    else
                    {
                        await ReplyAsync($"{alias} is not an alias for {role.Name}");
                        return;
                    }
                }
            }

            [Group("rules")]
            public class Rules : ModuleBase
            {
                private readonly ApplicationConfiguration _appConfig;

                public Rules(ApplicationConfiguration appConfig)
                {
                    _appConfig = appConfig;
                }

                [Command("add")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task AddRule(int position = -1, [NotNullOrWhiteSpace, Remainder] string rule = "")
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    // Guarantee it has a position, even if that's at the end
                    if (position == -1 || position > guildConfig.Rules.Count) { position = guildConfig.Rules.Count; }
                    else { position--; }

                    // Add the rule to the listing
                    guildConfig.Rules.Insert(position, rule);

                    // Modify the rule listing, if the message exists
                    if (await Context.Guild.GetChannelAsync(guildConfig.RulesChannel) is SocketTextChannel rulesChannel && rulesChannel != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int x = 0; x < guildConfig.Rules.Count; x++)
                        {
                            sb.AppendLine($"{x + 1}: {guildConfig.Rules[x]}");
                        }

                        try
                        {
                            if (await rulesChannel.GetMessageAsync(guildConfig.RulesMessage) is SocketUserMessage rulesMessage && rulesMessage != null)
                            {
                                await rulesMessage.ModifyAsync(x => x.Content = sb.ToString());
                            }
                            else { throw new InvalidOperationException("rulesMessage was null"); }
                        }
                        catch (Exception ex)
                        {
                            var newMessage = await rulesChannel.SendMessageAsync(sb.ToString());
                            guildConfig.RulesMessage = newMessage.Id;
                        }
                    }

                    await ConfigurationParser.WriteChanges(_appConfig);

                    await ReplyAsync($"Rule added at position {position}: \"{rule}\"");
                }

                [Command("remove")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task RemoveRule(int position)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);

                    if (position > guildConfig.Rules.Count)
                    {
                        await ReplyAsync("Invalid rule ID");
                    }
                    else
                    {
                        guildConfig.Rules.RemoveAt(position - 1);

                        await ConfigurationParser.WriteChanges(_appConfig);

                        await ReplyAsync($"Rule {position} has been removed");
                    }
                }

                [Command("update")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task UpdateRuleListing()
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context);
                    // Modify the rule listing, if the message exists
                    if (await Context.Guild.GetChannelAsync(guildConfig.RulesChannel) is SocketTextChannel rulesChannel)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int x = 0; x < guildConfig.Rules.Count; x++)
                        {
                            sb.AppendLine($"{x + 1}: {guildConfig.Rules[x]}");
                        }

                        try
                        {
                            var rulesMessage = await rulesChannel?.GetMessageAsync(guildConfig.RulesMessage) as SocketUserMessage;
                            await rulesMessage?.ModifyAsync(x => x.Content = sb.ToString());
                        }
                        catch
                        {
                            await rulesChannel.SendMessageAsync(sb.ToString());
                        }
                        await ReplyAsync("Rules updated!");
                    }
                    else
                    {
                        await ReplyAsync("No rules channel");
                    }
                }
            }

            [Group("tag")]
            public class Tag : ModuleBase
            {
                private readonly ApplicationConfiguration _appConfig;

                public Tag(ApplicationConfiguration appConfig)
                {
                    _appConfig = appConfig;
                }

                [Command("add")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task AddTag([NotNullOrWhiteSpace] string key, [NotNullOrWhiteSpace] string value)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                    if (guildConfig.Tags.TryAdd(key, value))
                    {
                        await ConfigurationParser.WriteChanges(_appConfig);

                        await ReplyAsync($"Tag `{key}` Added");
                    }
                    else
                    {
                        await ReplyAsync($"Could not add `{key}`. Does it already exist?");
                    }
                }

                [Command("remove")]
                [RequireCommandChannel]
                [RequireAdmin]
                public async Task RemoveTag([NotNullOrWhiteSpace] string key)
                {
                    var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                    if (guildConfig.Tags.Remove(key))
                    {
                        await ConfigurationParser.WriteChanges(_appConfig);

                        await ReplyAsync($"Tag `{key}` removed");
                    }
                    else
                    {
                        await ReplyAsync($"Failed to remove `{key}`. Does the tag exist?");
                    }
                }
            }
        }
    }
}
