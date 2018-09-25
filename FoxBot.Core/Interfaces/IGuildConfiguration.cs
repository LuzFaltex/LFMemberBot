using FoxBot.Core.Enums;
using FoxBot.Core.Structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FoxBot.Core.Interfaces
{
    public interface IGuildConfiguration : IEquatable<IGuildConfiguration>
    {
        /// <summary>
        /// Gets or sets the name of the guild.
        /// </summary>
        /// <value>The name of the guild.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the guild identifier.
        /// </summary>
        /// <value>The guild identifier.</value>
        ulong Id { get; }

        /// <summary>
        /// Gets or sets the command prefix.
        /// </summary>
        /// <value>The command prefix.</value>
        string CommandPrefix { get; set; }

        /// <summary>
        /// Gets or sets the server management role.
        /// </summary>
        /// <value>The server management role.</value>
        ulong AdminRole { get; set; }

        /// <summary>
        /// Gets or sets the moderator role.
        /// </summary>
        /// <value>The moderator role.</value>
        ulong ModeratorRole { get; set; }

        /// <summary>
        /// Gets or sets the member role.
        /// </summary>
        /// <value>The member role.</value>
        ulong MemberRole { get; set; }

        bool AutoIssueRoleCategories { get; set; }

        /// <summary>
        /// List of roles in each of their categories
        /// </summary>
        HashSet<IRoleCategory> RoleCategories { get; set; }

        /// <summary>
        /// Gets or sets a list of channels that the bot listens to
        /// </summary>
        /// <value>Command channels</value>
        HashSet<ulong> CommandChannels { get; set; }

        /// <summary>
        /// Gets or sets a channel that the bot will log to.
        /// </summary>
        ulong BotLogChannel { get; set; }

        /// <summary>
        /// Gets or set a list of role aliases
        /// </summary>
        /// <value>A list of role aliases</value>
        List<(ulong role, string alias)> RoleAliases { get; set; }

        /// <summary>
        /// Gets or sets a list of pre-emtively banned users
        /// </summary>
        /// <value>List of pre-emptively banned users</value>
        List<IBonkConfiguration> Bonks { get; set; }

        /// <summary>
        /// Gets or sets a list of banned users
        /// </summary>
        /// <value>List of banned users</value>
        List<BanConfiguration> Bans { get; set; }

        /// <summary>
        /// Gets or sets a list of temporarily banned users
        /// </summary>
        /// <value>List of temporarily banned users</value>
        List<BanConfiguration> TempBans { get; }

        /// <summary>
        /// Determines the JoinMode used by the Guild
        /// </summary>
        JoinMode JoinMode { get; set; }

        /// <summary>
        /// Represents the message to which users must react to join the server
        /// </summary>
        ulong HoldingMessageID { get; set; }

        /// <summary>
        /// The holding message text
        /// </summary>
        string HoldingMessageText { get; set; }

        /// <summary>
        /// The channel users are held in before joining.
        /// </summary>
        ulong HoldingChannel { get; set; }

        /// <summary>
        ///  The channels in which welcome messages are sent
        /// </summary>
        ulong WelcomeChannel { get; set; }

        /// <summary>
        /// The message to send in the welcome message.
        /// </summary>
        string WelcomeMessageText { get; set; }

        /// <summary>
        /// A list of server rules
        /// </summary>
        List<string> Rules { get; set; }

        /// <summary>
        /// The channel in which to post the rules in
        /// </summary>
        ulong RulesChannel { get; set; }

        /// <summary>
        /// The ID of the current rules message
        /// </summary>
        ulong RulesMessage { get; set; }

        /// <summary>
        /// A list of uniquely-named tags
        /// </summary>
        Dictionary<string, string> Tags { get; set; }
    }
}
