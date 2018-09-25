using Discord;
using Discord.Commands;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    public partial class AdminModule
    {
        [Group("roles")]
        public class RolesModule : ModuleBase
        {
            private readonly ApplicationConfiguration _appConfig;

            public RolesModule(ApplicationConfiguration appConfig)
            {
                _appConfig = appConfig;
            }

            [Command("help")]
            [RequireCommandChannel]
            public async Task ShowHelp()
            {
                string prefix = _appConfig.GetGuildConfiguration(Context).CommandPrefix;

                StringBuilder admin = new StringBuilder(1024);
                admin.AppendLine($"{prefix}admin roles help - Shows this dialogue");
                admin.AppendLine($"{prefix}admin roles getroles - Sends a CSV file to the admin with a list of roles and their IDs.");
                admin.AppendLine();
                admin.AppendLine($"{prefix}admin roles register <role> <category> - Creates an assignable role from the provided role. If order is unspecified, it goes to the end.");
                admin.AppendLine($"{prefix}admin roles decommission <role> - Makes a role no longer assignable.");
                admin.AppendLine();
                admin.AppendLine($"{prefix}admin roles category [<role> <category>] - Sets the category this role belongs to.");
                admin.AppendLine($"{prefix}admin roles assignment [<role> <locked|self>] - Changes the assignment method for this role.");
                admin.AppendLine($"{prefix}admin roles prerequisitetype [<role> <none|role|period|both>] - Changes the prerequisite for this type. Set to none to disable.");
                admin.AppendLine($"{prefix}admin roles prerequisite [<role> <none|role|duration>] - Sets the prerequisite. Either \"none\", a role, or a duration.");
                admin.AppendLine();
                admin.AppendLine($"{prefix}admin roles promote <role> - Promotes an assignable role to a role category");
                admin.AppendLine($"{prefix}admin roles demote <role> - Demotes a category role back to an assignable role");

                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder.AddField("Command Information", $"Command prefix: `{prefix}`\r\nArgumentSyntax: [optional] <required>");
                embedBuilder.AddField("Commands", admin.ToString());

                await ReplyAsync("", embed: embedBuilder.Build());
            }

            [Command("getroles")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetRoles()
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Name,ID,Position,Assignable");
                foreach (IRole role in Context.Guild.Roles.OrderByDescending(x => x.Position))
                {
                    string roleName = role.Name.Replace("\"", "\"\"");
                    bool isAssignable = guildConfig.GetAssignableRole(role.Id) != null;
                    sb.AppendLine($"{roleName},=\"{role.Id}\",{role.Position},{isAssignable}");
                }

                IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())))
                {
                    await channel.SendFileAsync(stream, $"{_appConfig.GetGuildConfiguration(Context).Name} roles.csv");
                }

                await ReplyAsync("📬");
            }

            [Command("register")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task RegisterRole(IRole role, IRole category = null)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                if (guildConfig.GetAssignableRole(role.Id) != null)
                {
                    await ReplyAsync($"{role.Name} is already an assignable role.");
                    return;
                }

                AssignableRole assignableRole = new AssignableRole(role, AssignmentMethod.Self, PrerequisiteType.None, order: role.Position);

                bool notFound = false;
                var result = guildConfig.RoleCategories.FirstOrDefault(x => x.Id == (category?.Id ?? guildConfig.EveryoneRole));

                if (result == null)
                {
                    result = guildConfig.DefaultCategory;
                    notFound = true;
                }

                assignableRole.CategoryId = result.Id;

                result.ChildRoles.Add(assignableRole);

                string resultName = Context.Guild.GetRole(result.Id).Name;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"{(notFound ? "The requested category could not be found." : "")} The role `{role.Name}` was registered and added to the `{resultName}` category.");
            }

            [Command("decommission")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task Decommission(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("This role is not eligible for decommission.");
                    return;
                }
                else
                {
                    guildConfig.RoleCategories.FirstOrDefault(x => x.Id == assignableRole.CategoryId).ChildRoles.Remove(assignableRole);
                    await ConfigurationParser.WriteChanges(_appConfig);
                    await ReplyAsync("Role decommissioned.");
                }

            }

            [Command("category")]
            [RequireCommandChannel]
            public async Task GetCategory(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("This role is not assignable.");
                }
                else
                {
                    await ReplyAsync($"The category for role `{role.Name}` is `{Context.Guild.GetRole(assignableRole.CategoryId).Name}`.");
                }
            }

            [Command("category")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetCategory(IRole role, IRole category)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                if (guildConfig.RoleCategories.FirstOrDefault(x => x.Id == role.Id) != null)
                {
                    await ReplyAsync($"{role.Name} is already a category.");
                    return;
                }

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("The requested role isn't assignable! Please register this role first.");
                    return;
                }

                RoleCategory newCategory = (guildConfig.RoleCategories.FirstOrDefault(x => category.Id == x.Id) ?? guildConfig.DefaultCategory) as RoleCategory;
                RoleCategory oldCategory = (guildConfig.RoleCategories.FirstOrDefault(x => assignableRole.CategoryId == x.Id)) as RoleCategory;

                if (oldCategory == newCategory)
                {
                    await ReplyAsync("The new role already exists in the requested category.");
                    return;
                }

                assignableRole.CategoryId = newCategory.Id;
                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"The role category has been changed: `{Context.Guild.GetRole(oldCategory.Id)}` => `{Context.Guild.GetRole(newCategory.Id)}`");
            }

            [Command("assignment")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetAssignmentMethod(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("This role is not assignable.");
                }
                else
                {
                    await ReplyAsync($"The assigment method for role `{role.Name}` is  `{assignableRole.AssignmentMethod}`.");
                }
            }

            [Command("assignment")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetAssingmentMethod(IRole role, AssignmentMethod method)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                AssignmentMethod oldMethod = assignableRole.AssignmentMethod;
                assignableRole.AssignmentMethod = method;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Assignment method for `{role.Name}` updated: `{oldMethod}` => `{method}`");
            }

            [Command("prerequisitetype")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetPrerequisiteType(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("This role is not assignable.");
                }
                else
                {
                    await ReplyAsync($"The prerequisite type for role `{role.Name}` is typeof `{assignableRole.PrerequisiteType}`.");
                }
            }

            [Command("prerequisitetype")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetPrerequisiteType(IRole role, PrerequisiteType prerequisiteType)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                PrerequisiteType oldType = assignableRole.PrerequisiteType;
                assignableRole.PrerequisiteType = prerequisiteType;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Prerequisite type for `{role.Name}` updated: `{oldType}` => `{prerequisiteType}`");
            }

            [Command("prerequisite")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task GetPrerequisite(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("This role is not assignable.");
                }
                else
                {
                    await ReplyAsync($"The membership for role `{role.Name}` is {(assignableRole.MembershipDuration.ToString() ?? "unset")}.");
                    await ReplyAsync($"The role prerequisite for role `{role.Name}` is {(assignableRole.RolePrerequisite != Context.Guild.EveryoneRole.Id ? Context.Guild.GetRole(assignableRole.RolePrerequisite).Name : "unset")}.");
                }
            }

            [Command("prerequisite")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetRolePrerequisite(IRole role, IRole prerequisite)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                string oldConfig;
                if (assignableRole.RolePrerequisite == ulong.MaxValue || assignableRole.RolePrerequisite == Context.Guild.EveryoneRole.Id)
                {
                    oldConfig = "unset";
                }
                else
                {
                    oldConfig = Context.Guild.GetRole(assignableRole.RolePrerequisite).Name;
                }

                string newConfig = prerequisite == Context.Guild.EveryoneRole ? "unset" : prerequisite.Name;

                assignableRole.RolePrerequisite = prerequisite.Id;

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Role prerequisite updated for `{role.Name}`: `{oldConfig}` => `{newConfig}`");
            }

            [Command("prerequisite")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task SetDurationPrerequisite(IRole role, TimeSpan duration)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                Period oldDuration = assignableRole.MembershipDuration;

                assignableRole.MembershipDuration = duration.ToPeriod();

                await ConfigurationParser.WriteChanges(_appConfig);

                await ReplyAsync($"Duration prerequisite updated for `{role.Name}`: `{oldDuration}` => `{duration}`");
            }

            [Command("promote")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task PromoteRole(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);

                if (assignableRole == null)
                {
                    await ReplyAsync("Role cannot be promoted as it is not assignable.");
                    return;
                }

                RoleCategory roleCategory = new RoleCategory(assignableRole);

                if (!guildConfig.RoleCategories.Contains(roleCategory))
                {
                    guildConfig.RoleCategories.Add(roleCategory);
                    await ReplyAsync($"`{role.Name}` has been promoted to a category role");
                }
                else
                {
                    await ReplyAsync($"`{role.Name}` is already a category role");
                    return;
                }
            }
            
            [Command("demote")]
            [RequireCommandChannel]
            [RequireAdmin]
            public async Task DemoteRole(IRole role)
            {
                var guildConfig = _appConfig.GetGuildConfiguration(Context) as GuildConfiguration;

                var assignableRole = guildConfig.GetAssignableRole(role.Id);
                var roleCategory = guildConfig.RoleCategories.FirstOrDefault(x => x.Id == assignableRole.RoleId);

                if (roleCategory == null)
                {
                    await ReplyAsync($"`{role.Name}` could not be demoted as it is not a category.");
                    return;
                }

                foreach (var arole in roleCategory.ChildRoles)
                {
                    arole.CategoryId = guildConfig.DefaultCategory.Id;
                }

                guildConfig.RoleCategories.Remove(roleCategory);

                await ReplyAsync($"`{role.Name}` has been demoted to an assignable role");
            }
        }
    }
}
