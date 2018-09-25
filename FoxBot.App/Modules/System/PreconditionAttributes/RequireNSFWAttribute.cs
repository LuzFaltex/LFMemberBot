using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.PreconditionAttributes
{
    public class RequireNSFWAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel)
            {
                return Task.FromResult(PreconditionResult.FromError("Command must be run in Guild Text Channel"));
            }
            else if (context.Channel is ITextChannel channel && channel.IsNsfw)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Channel is not marked as NSFW"));
            }
        }
    }
}
