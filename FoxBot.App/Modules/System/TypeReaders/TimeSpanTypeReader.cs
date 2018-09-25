using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FoxBot.App.Modules.System.TypeReaders
{
    /// <summary>
    /// Takes a text-based timespan and parses it
    /// </summary>
    /// <example>!command 1d2h3m</example>
    internal class TimeSpanTypeReader : TypeReader
    {
        private static Regex _pattern = new Regex(@"(\d+[wdhms])", RegexOptions.Compiled);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext _, string rawInput, IServiceProvider __)
        {
            var result = TimeSpan.Zero;
            var input = rawInput.ToLower();
            var matches = _pattern.Matches(input)
                .Cast<Match>()
                .Select(m => m.Value);

            foreach (var match in matches)
            {
                var amount = double.Parse(match.Substring(0, match.Length - 1));
                switch (match[match.Length -1])
                {
                    case 'w': result = result.Add(TimeSpan.FromDays(amount * 7)); break;
                    case 'd': result = result.Add(TimeSpan.FromDays(amount)); break;
                    case 'h': result = result.Add(TimeSpan.FromHours(amount)); break;
                    case 'm': result = result.Add(TimeSpan.FromMinutes(amount)); break;
                    case 's': result = result.Add(TimeSpan.FromSeconds(amount)); break;
                }
            }

            return result == TimeSpan.Zero
                ? Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"))
                : Task.FromResult(TypeReaderResult.FromSuccess(result));

        }
    }
}
