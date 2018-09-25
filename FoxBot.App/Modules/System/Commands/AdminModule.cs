using Discord;
using Discord.Commands;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    [Group("admin")]
    public partial class AdminModule : ModuleBase
    {
        private readonly ApplicationConfiguration AppConfig;

        public AdminModule(ApplicationConfiguration appConfig)
        {
            AppConfig = appConfig;
        }


        [Command("help")]
        [RequireCommandChannel]
        public async Task ShowHelp()
        {
            string prefix = AppConfig.GetGuildConfiguration(Context).CommandPrefix;


            StringBuilder admin = new StringBuilder(1024);
            admin.AppendLine($"{prefix}admin help - Shows this dialog");
            admin.AppendLine($"{prefix}admin restart - Restarts the bot.");
            admin.AppendLine($"{prefix}admin stop - Stops the bot.");
            admin.AppendLine();
            admin.AppendLine($"{prefix}admin guild - Configure the guild");
            admin.AppendLine($"{prefix}admin roles - Configure this guild's roles");
            // admin.AppendLine($"{prefix}admin bans - Configure this guild's bans");
            // admin.AppendLine($"{prefix}admin bonks - Configure this guild's bonks");

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.AddField("Command Information", $"Command prefix: `{prefix}`\r\nArgumentSyntax: [optional] <required>");
            embedBuilder.AddField("Commands", admin.ToString());

            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("restart")]
        [RequireCommandChannel]
        [RequireAdmin]
        public async Task Restart()
        {
            await ReplyAsync("🔄");
            AppConfig.BotState = BotState.Restart;
        }

        [Command("stop")]
        [RequireCommandChannel]
        [RequireAdmin]
        public async Task Stop()
        {
            await ReplyAsync("👋");
            await Task.Delay(500);
            AppConfig.BotState = BotState.Stop;
        }
    }
}
