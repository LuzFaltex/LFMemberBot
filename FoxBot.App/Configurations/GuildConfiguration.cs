using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Structs;
using Newtonsoft.Json;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace FoxBot.App.Configurations
{
    [JsonObject]
    public class GuildConfiguration : IGuildConfiguration
    {
        public GuildConfiguration()
        {

        }
        public GuildConfiguration(string name, ulong id, string commandPrefix, ulong everyoneRole)
        {
            Name = name;
            Id = id;
            CommandPrefix = commandPrefix;
            EveryoneRole = everyoneRole;
            RoleCategories.Add(new RoleCategory(new AssignableRole(everyoneRole, AssignmentMethod.Locked, PrerequisiteType.None)));
        }

        [JsonConstructor]
        public GuildConfiguration(ulong id, HashSet<RoleCategory> roleCategories, List<BonkConfiguration> bonkConfigurations)
        {
            Id = id;
            RoleCategories.UnionWith(roleCategories);
            Bonks.AddRange(bonkConfigurations);
        }

        public IAssignableRole GetAssignableRole(ulong roleId)
        {
            IAssignableRole result = null;
            IRoleCategory resultCategory = null;

            foreach(var category in RoleCategories)
            {
                result = category.ChildRoles.FirstOrDefault(x => x.RoleId == roleId);
                if (result != null)
                {
                    resultCategory = category;
                    break;
                }
            }

            return result;
        }

        [JsonIgnore]
        public IRoleCategory DefaultCategory
        {
            get
            {
                return RoleCategories.FirstOrDefault(x => x.Id == EveryoneRole);
            }
        }

        public bool Equals(IGuildConfiguration other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Gets or sets the name of the guild.
        /// </summary>
        /// <value>The name of the guild.</value>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the guild identifier.
        /// </summary>
        /// <value>The guild identifier.</value>
        public ulong Id { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or sets the command prefix.
        /// </summary>
        /// <value>The command prefix.</value>
        public string CommandPrefix { get; set; } = "!";

        public ulong EveryoneRole { get; set; }

        /// <summary>
        /// Gets or sets the server management role.
        /// </summary>
        /// <value>The server management role.</value>
        public ulong AdminRole { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or sets the moderator role.
        /// </summary>
        /// <value>The moderator role.</value>
        public ulong ModeratorRole { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or sets the member role.
        /// </summary>
        /// <value>The member role.</value>
        public ulong MemberRole { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Whether or not to auto-issue categorical roles
        /// </summary>
        public bool AutoIssueRoleCategories { get; set; } = true;

        /// <summary>
        /// List of roles in each of their categories
        /// </summary>
        public HashSet<IRoleCategory> RoleCategories { get; set; } = new HashSet<IRoleCategory>();

        /// <summary>
        /// Gets or sets a list of channels that the bot listens to
        /// </summary>
        /// <value>Command channels</value>
        public HashSet<ulong> CommandChannels { get; set; } = new HashSet<ulong>();

        /// <summary>
        /// Gets or sets a channel that the bot will log to.
        /// </summary>
        public ulong BotLogChannel { get; set; } = ulong.MaxValue;

        /// <summary>
        /// Gets or set a list of role aliases
        /// </summary>
        /// <value>A list of role aliases</value>
        public List<(ulong role, string alias)> RoleAliases { get; set; } = new List<(ulong role, string alias)>();

        /// <summary>
        /// Gets or sets a list of pre-emtively banned users
        /// </summary>
        /// <value>List of pre-emptively banned users</value>
        public List<IBonkConfiguration> Bonks { get; set; } = new List<IBonkConfiguration>();

        /// <summary>
        /// Gets or sets a list of temporarily banned users
        /// </summary>
        /// <value>List of temporarily banned users</value>
        public List<BanConfiguration> Bans { get; set; } = new List<BanConfiguration>();

        /// <summary>
        /// Gets or sets a list of temporarily banned users
        /// </summary>
        /// <value>List of temporarily banned users</value>
        public List<BanConfiguration> TempBans
        {
            get
            {
                return Bans.Where(x => x.UnbanDate != Instant.FromUnixTimeMilliseconds(0)).ToList();
            }
        }

        /// <summary>
        /// How to handle new users
        /// </summary>
        public JoinMode JoinMode { get; set; } = JoinMode.All;

        /// <summary>
        /// Represents the message to whic users must react to join the server
        /// </summary>
        public ulong HoldingMessageID { get; set; } = ulong.MaxValue;

        /// <summary>
        /// The text for the holding message
        /// </summary>
        public string HoldingMessageText { get; set; } =
            "Welcome to {ServerName}! Before joining, please review our rules. When you are ready, react to this message using :thumbsup:.";

        /// <summary>
        /// The channel users are held in before joining.
        /// </summary>
        public ulong HoldingChannel { get; set; } = ulong.MaxValue;

        /// <summary>
        ///  The channels in which welcome messages are sent
        /// </summary>
        public ulong WelcomeChannel { get; set; } = ulong.MaxValue;

        /// <summary>
        /// The message to send in the welcome message.
        /// </summary>
        public string WelcomeMessageText { get; set; } =
            "Welcome, {user}! We're glad to have you. Be sure to run over to {channel:461640015592816650} and get yourself some roles! You can see a list of these roles using the `!roles` command.";

        /// <summary>
        /// A list of server rules
        /// </summary>
        public List<string> Rules { get; set; } = new List<string>();

        /// <summary>
        /// The channel in which to post the rules in
        /// </summary>
        public ulong RulesChannel { get; set; } = ulong.MaxValue;

        /// <summary>
        /// The ID of the current rules message
        /// </summary>
        public ulong RulesMessage { get; set; } = ulong.MaxValue;

        /// <summary>
        /// A list of uniquely-named tags
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}
