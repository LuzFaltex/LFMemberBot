using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FoxBot.App.Configurations;
using FoxBot.App.Modules.System.PreconditionAttributes;
using FoxBot.Core.Services;
using FoxBot.Core.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.Commands
{
    [Name("Repl"), Summary("Execute & demonstrate code snippets")]
    public class ReplCommand : ModuleBase<SocketCommandContext>
    {
        private readonly CodePasteService _pasteService;

        private static readonly HttpClient _client = new HttpClient();

        private readonly ApplicationConfiguration _appConfig;
        private readonly LogHelper _logHelper;

        public ReplCommand(ApplicationConfiguration applicationConfiguration, LogHelper logHelper, CodePasteService pasteService)
        {
            _appConfig = applicationConfiguration;
            _logHelper = logHelper;
            _pasteService = pasteService;
        }

        [Command("exec", RunMode = RunMode.Sync)]
        [Alias("eval")]
        [Summary("Executes the given C# code and returns the result")]
        [RequireCommandChannel]
        public async Task ReplInvoke([Remainder] string code)
        {
            if (code.Length > 1000)
            {
                await ReplyAsync("Exec failed: code is greater than 1000 characters in length");
                return;
            }

            var guildUser = Context.User as SocketGuildUser;
            var message = await Context.Channel.SendMessageAsync("Working...");

            string content = FormatUtilities.BuildContent(code);

            var tokenSource = new CancellationTokenSource(30000);

            // For testing in testing environment ONLY
            /*
            var parsedResult = await ReplService.ExecuteAsync(content, tokenSource.Token);

            var embed = await BuildEmbed(guildUser, parsedResult);
            */
            var embed = new EmbedBuilder().WithDescription("Not yet implemented");

            await message.ModifyAsync(m => { m.Content = ""; m.Embed = embed.Build(); });
        }

        private async Task<EmbedBuilder> BuildEmbed(SocketGuildUser guildUser, Result parsedResult)
        {
            string returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
            string consoleOut = parsedResult.ConsoleOut;

            var embed = new EmbedBuilder()
                .WithTitle("Eval Result")
                .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Successful" : "Failed")
                .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? Color.Green : Color.Red)
                .WithAuthor(a => a.WithIconUrl(Context.User.GetAvatarUrl()).WithName(guildUser?.Nickname ?? Context.User.Username))
                .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(parsedResult.Code, "cs")));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnTypeName ?? "null"}")
                                     .WithValue(Format.Code($"{returnValue.TruncateTo(1000)}", "json")));
                await embed.UploadToServiceIfBiggerThan(returnValue, "json", 1000, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(consoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(consoleOut.TruncateTo(1000), "txt")));
                await embed.UploadToServiceIfBiggerThan(returnValue, "txt", 1000, _pasteService);
            }

            if (!string.IsNullOrEmpty(parsedResult.Exception))
            {
                var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
                                     .WithValue(Format.Code(diffFormatted.TruncateTo(1000), "diff")));
                await embed.UploadToServiceIfBiggerThan(diffFormatted, "diff", 1000, _pasteService);
            }

            return embed;
        }
    }
}
