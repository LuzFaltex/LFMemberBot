using FoxBot.Core.Interfaces;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.App.Configurations
{
    public class BonkConfiguration : IBonkConfiguration
    {
        public BonkConfiguration()
        {

        }
        public BonkConfiguration(ulong victim, ulong moderator, string reason, Instant? instant = null)
        {
            Victim = victim;
            Moderator = moderator;
            Reason = reason;
            BonkDate = instant ?? Instant.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
        public ulong Victim { get; set; }
        public ulong Moderator { get; set; }
        public string Reason { get; set; }
        public Instant BonkDate { get; set; }
    }
}
