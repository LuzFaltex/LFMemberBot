using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.Core.Structs
{ 
    [JsonObject]
    public struct BanConfiguration : IEquatable<BanConfiguration>
    {
        public BanConfiguration(ulong victim, ulong moderator, string reason, Instant unbanDate)
        {
            Victim = victim;
            Moderator = moderator;
            Reason = reason;
            BanDate = Instant.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            UnbanDate = unbanDate;
        }

        /// <summary>
        /// Gets or sets the ID of the bonked user
        /// </summary>
        public ulong Victim { get; set; }

        /// <summary>
        /// Gets or sets the ID of the responsible moderator
        /// </summary>
        public ulong Moderator { get; set; }

        /// <summary>
        /// The reason for the bonk
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// When the ban took place
        /// </summary>
        public Instant BanDate { get; set; }

        /// <summary>
        /// When the user should be unbanned
        /// </summary>
        /// <remarks>Use the Unix epoch to do a permaban</remarks>
        public Instant UnbanDate { get; set; }

        /// <summary>
        /// Return whether the ban is expired
        /// </summary>
        [JsonIgnore]
        public bool Expired => IsExpired();

        public bool Equals(BanConfiguration other)
        {
            return Victim == other.Victim;
        }

        private bool IsExpired()
        {
            if (UnbanDate.Equals(Instant.FromUnixTimeMilliseconds(0)))
            {
                return false;
            }

            Instant Now = Instant.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            return Now >= UnbanDate;
        }
    }
}
