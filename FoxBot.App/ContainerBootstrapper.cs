using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Services;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Services;
using FoxBot.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App
{
    public class ContainerBootstrapper
    {
        public ContainerBootstrapper()
        {

        }

        #region private properties

        // Discord Client
        private static CommandService commands;
        private static DiscordSocketClient client;
        private static IServiceProvider services;
        private static CommandHandler commandHandler;
        private static ApplicationConfiguration applicationConfiguration;
        private static ReliabilityService reliabilityService;
        private static LogHelper logHelper;
        private static UserService userService;
        private static AdmittanceService admittanceService;

        #endregion

        #region private methods
        private Task PrintHeaders()
        {
            if (applicationConfiguration is ApplicationConfiguration appConfig)
            {
                Console.WriteLine($"FoxBot version {applicationConfiguration.BotVersion}");
                Console.WriteLine($"Copyright {appConfig.CopyrightYear} LuzFaltex, all rights reserved");
                Console.WriteLine();
            }
            return Task.CompletedTask;
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task CheckConfig(IApplicationConfiguration applicationConfiguration)
        {
            bool tokenChanged = false;
            bool ownerChanged = false;
            if (string.IsNullOrWhiteSpace(applicationConfiguration.Token))
            {
                await logHelper.InfoAsync("[Config] Please paste your bot token here: ", "ConfigBuilder");
                do
                {
                    applicationConfiguration.Token = Console.ReadLine();
                    tokenChanged = true;
                } while (!tokenChanged);
            }

            if (applicationConfiguration.Owner.Equals(ulong.MaxValue))
            {
                await logHelper.InfoAsync("[Config] Please paste your user ID here: ", "ConfigBuilder");
                do
                {
                    ownerChanged = ulong.TryParse(Console.ReadLine(), out ulong result);
                    applicationConfiguration.Owner = result;
                } while (!ownerChanged);
            }

            if (tokenChanged || ownerChanged)
            {
                await ConfigurationParser.WriteChanges(applicationConfiguration);
            }
        }
        #endregion

        public async Task Invoke()
        {
            // Initialize the properties
            client = new DiscordSocketClient();
            commands = new CommandService();
            logHelper = new LogHelper(Log);
            reliabilityService = new ReliabilityService(client, logHelper);

            applicationConfiguration = await ConfigurationParser.GetApplicationConfiguration();
            await CheckConfig(applicationConfiguration);

            admittanceService = new AdmittanceService(client, logHelper, applicationConfiguration);
            userService = new UserService(client, logHelper, applicationConfiguration);

            // Set up handlers
            client.Log += Log;
            client.JoinedGuild += JoinedGuild;
            client.LeftGuild += LeftGuild;

            // Configure services
            // TODO: Inject ApplicationConfiguration instead of IApplicationConfiguration
            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton(reliabilityService)
                .AddSingleton(userService)
                .AddSingleton(admittanceService)
                .AddSingleton<InteractiveService>()
                .AddSingleton(applicationConfiguration)
                .AddSingleton(logHelper)
                .AddSingleton<CodePasteService>()
                .BuildServiceProvider();



            // Initialize the command handler
            commandHandler = new CommandHandler();
            await commandHandler.InstallCommandsAsync(client, commands, services, applicationConfiguration, logHelper);

            // Log in and start the bot
            await client.LoginAsync(TokenType.Bot, applicationConfiguration.Token);
            await client.StartAsync();

            applicationConfiguration.BotState = BotState.Running;

            await PrintHeaders();

            while (applicationConfiguration.BotState != BotState.Stop)
            {
                if (applicationConfiguration.BotState == BotState.Restart)
                {
                    // Stop the bot and log out
                    await client.LogoutAsync();
                    await client.StopAsync();

                    // Log in and start the bot
                    await client.LoginAsync(TokenType.Bot, applicationConfiguration.Token);
                    await client.StartAsync();

                    applicationConfiguration.BotState = BotState.Running;
                }
            }

            // Stop the bot and log out
            await client.LogoutAsync();
            await client.StopAsync();

        }

        private async Task LeftGuild(SocketGuild guild)
        {
            var appConfig = applicationConfiguration as ApplicationConfiguration;

            appConfig.Guilds.RemoveAll(x => x.Id == guild.Id);

            await logHelper.InfoAsync($"Left guild {guild.Name}", "BotConfig");
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            GuildConfiguration guildConf = new GuildConfiguration(guild.Name, guild.Id, "!", guild.EveryoneRole.Id);
            var appConfig = applicationConfiguration as ApplicationConfiguration;

            appConfig.Guilds.Add(guildConf);

            await ConfigurationParser.WriteChanges(appConfig);

            await logHelper.InfoAsync($"Joined guild {guild.Name}", "BotConfig");
        }
    }
}
