using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    public class RoleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ApplicationConfiguration appConfig;

        public RoleCommands(ApplicationConfiguration applicationConfiguration)
        {
            appConfig = applicationConfiguration;
        }

        /// <summary>
        /// Converts the requested roles to <see cref="AssignableRole"/>s
        /// Then grants the allowed roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        [Command("iam")]
        [RequireCommandChannel]
        public async Task IAmCommand(params IRole[] roles)
        {
            var guildConfig = appConfig.GetGuildConfiguration(Context) as GuildConfiguration;
            SocketGuildUser user = Context.User as SocketGuildUser;

            List<SocketRole> rolesToAdd = new List<SocketRole>();
            Dictionary<SocketRole, string> failAdd = new Dictionary<SocketRole, string>();

            // Sort through the requested roles and get the corresponding IAssignableRole
            // Ensure roles can be assigned
            // If so, add them to rolesToAdd
            foreach (var role in roles)
            {
                var assignable = guildConfig.GetAssignableRole(role.Id);

                if (assignable?.AssignmentMethod == AssignmentMethod.Self)
                {
                    // Check for prerequisite
                    switch (assignable.PrerequisiteType)
                    {
                        case PrerequisiteType.None: rolesToAdd.Add(role as SocketRole); break;
                        case PrerequisiteType.Role:
                            if (user.Roles.Any(x => x.Id == assignable.RolePrerequisite))
                            {
                                rolesToAdd.Add(role as SocketRole);
                            }
                            else
                            {
                                var preRole = user.Guild.GetRole(assignable.RolePrerequisite);
                                failAdd.Add(role as SocketRole, $"Cannot add the {role.Name} role without first having the {preRole.Name} role.");
                            }
                            break;
                        case PrerequisiteType.Period:
                            if (Instant.FromDateTimeOffset(user.JoinedAt.Value).Plus(assignable.MembershipDuration.ToDuration()).CompareTo(Instant.FromDateTimeUtc(DateTime.UtcNow)) <= 0)
                            {
                                rolesToAdd.Add(role as SocketRole);
                            }
                            else
                            {
                                failAdd.Add(role as SocketRole, $"Cannot add the {role.Name} role. You have not been here for {assignable.MembershipDuration.ToString()}!");
                            }
                            break;
                    }
                }
                else
                {
                    failAdd.Add(role as SocketRole, $"Cannot add the {role.Name} role as it is not assignable at this time.");
                }                
            }

            await user.AddRolesAsync(rolesToAdd);

            StringBuilder sb = new StringBuilder();
            if (rolesToAdd.Count > 0)
            {
                sb.AppendLine($"Adding the following role(s): {string.Join(", ", rolesToAdd)}");
            }
            if (failAdd.Count > 0)
            {
                sb.AppendLine("Encountered the following errors:");
                foreach (var kvp in failAdd)
                {
                    sb.AppendLine($"\t{kvp.Value}");
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = sb.ToString(),
                Color = (failAdd.Count > 0) ? Color.Red : Color.Green,
            };

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("iamnot")]
        [RequireCommandChannel]
        public async Task IAmNotCommand(params IRole[] roles)
        {
            var guildConfig = appConfig.GetGuildConfiguration(Context) as GuildConfiguration;
            SocketGuildUser user = Context.User as SocketGuildUser;

            List<SocketRole> rolesToRemove = new List<SocketRole>();
            Dictionary<SocketRole, string> failRemove = new Dictionary<SocketRole, string>();

            foreach (var role in roles)
            {
                var assignable = guildConfig.GetAssignableRole(role.Id);

                if (assignable == null)
                {
                    failRemove.Add(role as SocketRole, $"Cannot remove the {role.Name} role because it's not self-assignable.");
                }
                else
                {
                    rolesToRemove.Add(role as SocketRole);
                }
            }

            await user.RemoveRolesAsync(rolesToRemove);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Removing the following role(s): {string.Join(", ", rolesToRemove)}");
            if (failRemove.Count > 0)
            {
                sb.AppendLine("Encountered the following errors:");
                foreach (var kvp in failRemove)
                {
                    sb.AppendLine($"\t{kvp.Value}");
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = sb.ToString(),
                Color = (failRemove.Count > 0) ? Color.Red : Color.Green,
            };

            await ReplyAsync("", embed: embed.Build());
        }
    }
}
