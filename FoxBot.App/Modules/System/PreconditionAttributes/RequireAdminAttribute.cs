using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.PreconditionAttributes
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        public IApplicationConfiguration ApplicationConfiguration { get; set; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ApplicationConfiguration = services.GetService<ApplicationConfiguration>();
            var appConfig = ApplicationConfiguration as ApplicationConfiguration;

            // Get the ID of the bot's owner
            ulong ownerID = appConfig.Owner;

            IGuildConfiguration guildConfiguration = appConfig.GetGuildConfiguration(context);

            if (context.User.Id == ownerID ||
                ((SocketGuildUser)context.User).IsMemberOf(guildConfiguration.AdminRole))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Access denied"));
            }
        }
    }
}
