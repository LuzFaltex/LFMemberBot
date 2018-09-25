using Discord;
using Discord.WebSocket;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Services;
using FoxBot.Core.Structs;
using NodaTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FoxBot.Core.Utilities
{
    public static class Extensions
    {
        // Collection -> ReadOnlyCollection()
        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue>(this ICollection<TValue> source) => new CollectionWrapper<TValue>(source, () => source.Count);
        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TKey, TValue>(this IDictionary<TKey, TValue> source) => new CollectionWrapper<TValue>(source.Select(x => x.Value), () => source.Count);
        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue, TSource>(this IEnumerable<TValue> query, IReadOnlyCollection<TSource> source) => new CollectionWrapper<TValue>(query, () => source.Count);
        public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue>(this IEnumerable<TValue> query, Func<int> countFunc) => new CollectionWrapper<TValue>(query, countFunc);

        // SocketGuildUser Extensions
        public static bool IsMemberOf(this SocketGuildUser user, ulong roleID) => IsMemberOf(user, user.Guild.GetRole(roleID));
        public static bool IsMemberOf(this SocketGuildUser user, IRole role)
        {
            return user.Roles.Contains(role);
        }

        // DateTimeOffset -> DateTime
        [Obsolete]
        public static DateTime ToDateTime(this DateTimeOffset offset)
        {
            if (offset.Offset.Equals(TimeSpan.Zero))
            {
                return offset.UtcDateTime;
            }
            else if (offset.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(offset.DateTime)))
            {
                return DateTime.SpecifyKind(offset.DateTime, DateTimeKind.Local);
            }
            else
            {
                return offset.DateTime;
            }
        }
        

        // String
        public static bool Contains(this string source, string toCheck, StringComparison comparison)
        {
            return source?.IndexOf(toCheck, comparison) >= 0;
        }

        public static string TruncateTo(this string str, int length)
        {
            if (str.Length < length)
            {
                return str;
            }

            return str.Substring(0, length);
        }
        

        // Dictionary
        /// <summary>
        /// Adds the values from the specified Dictionary to this dictionary's collection.
        /// </summary>
        /// <param name="additional">The dictionary to add to this collection</param>
        /// <param name="ignoreDuplicates">If true, any duplicate keys will simply be ignored and skipped.</param>
        /// <exception cref="ArgumentException">Thrown when a duplicate key is found and <paramref name="ignoreDuplicates"/> is false.</exception>
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> additional, bool ignoreDuplicates = true)
        {
            // By checking for duplicates first, we ensure that AddRange is nuclear,
            // i.e. it either is 100% successful and the source is amended or it fails
            // and the source is unchanged
            if (!ignoreDuplicates)
            {
                List<Exception> exceptions = new List<Exception>();

                foreach (var kvp in additional)
                {
                    if (source.ContainsKey(kvp.Key))
                    {
                        exceptions.Add(new ArgumentException("Duplicate Key Found", kvp.Key.ToString()));
                    }
                }

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
            
            foreach (var kvp in additional)
            {
                source.TryAdd(kvp.Key, kvp.Value);
            }
        }

        internal struct CollectionWrapper<TValue> : IReadOnlyCollection<TValue>
        {
            private readonly IEnumerable<TValue> _query;
            private readonly Func<int> _countFunc;

            // It's okay that this count is affected by race conditions-
            // We're wrapping a concurrent collection and that's to be expected
            public int Count => _countFunc();

            public CollectionWrapper(IEnumerable<TValue> query, Func<int> countFunc)
            {
                _query = query;
                _countFunc = countFunc;
            }

            public IEnumerator<TValue> GetEnumerator() => _query.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _query.GetEnumerator();
        }

        // Duration => Period and back
        public static TimeSpan ToTimeSpan(this Period p)
        {
            return new TimeSpan(p.Days, (int)p.Hours, (int)p.Minutes, (int)p.Seconds, (int)p.Milliseconds);
        }

        public static Period ToPeriod(this TimeSpan timeSpan)
        {
            PeriodBuilder pb = new PeriodBuilder
            {
                Days = (int)Math.Floor(timeSpan.TotalDays),
                Hours = timeSpan.Hours,
                Minutes = timeSpan.Minutes,
                Seconds = timeSpan.Seconds,
                Milliseconds = timeSpan.Milliseconds
            };

            return pb.Build();
        }

        // SocketGuild
        public static Task AddBanAsync(this SocketGuild guild, BanConfiguration config) => guild.AddBanAsync(config.Victim, reason: config.Reason, options: new RequestOptions() { AuditLogReason = config.Reason });

        //SocketGuildUser
        public static async Task WelcomeUser(this SocketGuildUser user, IGuildConfiguration guildConfig)
        {
            // Welcome the user, but not if it's a bot
            if (!string.IsNullOrWhiteSpace(guildConfig.WelcomeMessageText) && !user.IsBot)
            {
                // Perform replacements
                var message = guildConfig.WelcomeMessageText;
                SocketTextChannel channel = user.Guild.GetTextChannel(guildConfig.WelcomeChannel);
                string pattern = @"({user}|{servername}|{[a-zA-Z]+:\d+})";

                
                MatchCollection matches = Regex.Matches(message, pattern);

                foreach (Match match in matches)
                {
                    if (match.Value.ToLowerInvariant().Equals("{user}"))
                    {
                        message = message.Replace("{user}", user.Mention);
                    }
                    else if (match.Value.ToLowerInvariant().Equals("{servername}"))
                    {
                        message = message.Replace("{servername}", user.Guild.Name);
                    }
                    else if (match.Value.StartsWith("{channel:"))
                    {
                        if (ulong.TryParse(match.Value.Split(':')[1].Replace("}", ""), out ulong channelId))
                        {
                            message = message.Replace(match.Value, user.Guild.GetTextChannel(channelId).Mention);
                        }
                    }
                    else if (match.Value.StartsWith("{user:"))
                    {
                        if (ulong.TryParse(match.Value.Split(':')[1].Replace("}", ""), out ulong userId))
                        {
                            message = message.Replace(match.Value, user.Guild.GetUser(userId).Mention);
                        }
                    }
                    else { continue; }
                }

                await channel.SendMessageAsync(message);
            }
        }

        // EmbedBuilder
        public static async Task UploadToServiceIfBiggerThan(this EmbedBuilder embed, string content, string contentType, uint size, CodePasteService service)
        {
            if (content.Length > size)
            {
                try
                {
                    var resultLink = await service.UploadCode(content, contentType);
                    embed.AddField(a => a.WithName("More...").WithValue($"[View on Hastebin]({resultLink})"));
                }
                catch (WebException we)
                {
                    embed.AddField(a => a.WithName("More...").WithValue(we.Message));
                }
            }
        }
    }
}
