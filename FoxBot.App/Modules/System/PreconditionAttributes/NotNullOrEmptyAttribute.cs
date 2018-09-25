using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.PreconditionAttributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class NotNullOrEmptyAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            switch (value)
            {
                case string message when string.IsNullOrEmpty(message):
                    return Task.FromResult(PreconditionResult.FromError($"Parameter {parameter.Name} cannot be empty."));
                case string message when !string.IsNullOrEmpty(message):
                    return Task.FromResult(PreconditionResult.FromSuccess());
                default:
                    return Task.FromResult(PreconditionResult.FromError($"Parameter {parameter.Name} must be a string"));
            }
        }
    }
}
