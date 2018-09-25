using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.TypeReaders;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FoxBot.App
{
    public class CommandHandler
    {
        #region properties
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private ApplicationConfiguration _applicationConfiguration;
        private LogHelper _logger;
        private const string LogSource = "CmdHandler";
        #endregion

        internal async Task InstallCommandsAsync(DiscordSocketClient client, CommandService commands, IServiceProvider services, IApplicationConfiguration applicationConfiguration, LogHelper logger)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _applicationConfiguration = applicationConfiguration as ApplicationConfiguration;
            _logger = logger;

            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += MessageReceived;

            // Install Typereaders
            _commands.AddTypeReader<TimeSpan>(new TimeSpanTypeReader());

            // Discover all commands in this assembly and load them
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Don't process the command if it was a system message
            if (!(rawMessage is SocketUserMessage message)) { return; }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Create the command context
            var context = new SocketCommandContext(_client, message);
            var guildConfig = _applicationConfiguration.GetGuildConfiguration(context) as GuildConfiguration;

            // Determine if the message is a command.
            if (message.HasStringPrefix(guildConfig.CommandPrefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command
                await _logger.DebugAsync($"{context.User.Username}#{context.User.Discriminator} ran command: {message.Content}", LogSource);
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                // Handle errors
                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.BadArgCount:
                        case CommandError.Exception:
                        case CommandError.ObjectNotFound:
                        case CommandError.ParseFailed:
                        case CommandError.UnmetPrecondition:
                        case CommandError.Unsuccessful:
                            await context.Channel.SendMessageAsync(result.ErrorReason);
                            await _logger.CriticalAsync(result.ErrorReason, LogSource);
                            break;
                    }
                }
            }
            else // It's not a command
            {
                // Could just fall out, but I'm leaving this section here
                // in case I want to add handling later.
                return;
            }

        }
    }
}