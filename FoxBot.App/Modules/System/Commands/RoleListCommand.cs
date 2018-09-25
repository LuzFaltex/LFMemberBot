using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    public class RoleListCommand : InteractiveBase<SocketCommandContext>
    {
        private readonly ApplicationConfiguration _applicationConfiguration;

        public RoleListCommand(ApplicationConfiguration appConfig)
        {
            _applicationConfiguration = appConfig;
        }

        /// <summary>
        /// Paginated role list by divider
        /// </summary>
        /// <returns></returns>
        [Command("roles")]
        [RequireCommandChannel]
        public async Task ListRoles()
        {
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(Context);

            List<string> pages = new List<string>();

            List<IRoleCategory> categories = guildConfig.RoleCategories.ToList();
            categories.Sort((x, y) => x.Order.CompareTo(y.Order));

            foreach (var category in categories)
            {
                string categoryName = Context.Guild.GetRole(category.CategoryRole.RoleId).Name;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Below are a list of roles you can assign to yourself.");
                sb.AppendLine($"Category: {(categoryName.Equals(Context.Guild.EveryoneRole.Name) ? "General" : categoryName.Replace("=","").Trim())}");
                sb.AppendLine("```");

                List<IAssignableRole> categoryRoles = category.ChildRoles.ToList();
                categoryRoles.Sort((x, y) => x.Order.CompareTo(y.Order));

                foreach (var roleInfo in category.ChildRoles.OrderByDescending(x => x.Order))
                {
                    if (guildConfig.RoleCategories.FirstOrDefault(x => x.Id == roleInfo.RoleId) != null)
                    {
                        continue;
                    }

                    var role = Context.Guild.GetRole(roleInfo.RoleId);

                    sb.AppendLine($"{role.Name}{(string.IsNullOrWhiteSpace(roleInfo.Description) ? "" : $" - {roleInfo.Description}")}");
                }

                sb.AppendLine("```");
                sb.AppendLine($"Use `{guildConfig.CommandPrefix}iam role` to join a role.");
                sb.AppendLine($"Use `{guildConfig.CommandPrefix}iamnot role` to leave a role");
                sb.AppendLine();
                sb.AppendLine("You may string multiple roles into a single command.");
                sb.AppendLine("Roles with spaces MUST be in quotes.");
                sb.AppendLine($"For example: `{guildConfig.CommandPrefix}iam role1` or `{guildConfig.CommandPrefix}iam role1 role2 \"role with spaces\" role3`");

                // Add the page
                pages.Add(sb.ToString());
            }

            var msg = new PaginatedMessage
            {
                Pages = pages
            };

            await PagedReplyAsync(msg);
        }
    }
}
