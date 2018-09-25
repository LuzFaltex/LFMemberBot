using Discord.Commands;
using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.PreconditionAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireCommandChannelAttribute : PreconditionAttribute
    {
        public List<ulong> AllowedChannels { get; }
        public bool IncludeConfigValues { get; }

        /// <summary>
        /// Require that the channel the command is being invoked in is authorized as per the configuration
        /// </summary>
        public RequireCommandChannelAttribute() : this(true, new ulong[0]) { }

        /// <summary>
        /// Require that the channel the command is being invoked in is one of these
        /// </summary>
        /// <param name="IncludeConfigValues">Whether to include those defined in the guild config</param>
        /// <param name="channels">A list of additional channels</param>
        public RequireCommandChannelAttribute(bool IncludeConfigValues, params ulong[] channels)
        {
            AllowedChannels = channels.ToList();
            this.IncludeConfigValues = IncludeConfigValues;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var appConfig = services.GetService<ApplicationConfiguration>();

            if (IncludeConfigValues)
            {
                var tempCollection = ((GuildConfiguration)appConfig.GetGuildConfiguration(context)).CommandChannels;
                if (tempCollection != null)
                {
                    AllowedChannels.AddRange(tempCollection);
                }
            }

            if (context.User.Id == appConfig.Owner)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else if (AllowedChannels.Any(x => x == context.Channel.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Invalid channel"));
            }
        }
    }
}
