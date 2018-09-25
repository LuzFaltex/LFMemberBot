using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.Core.Interfaces
{
    public interface IBonkConfiguration
    {
        /// <summary>
        /// Gets or sets the ID of the bonked user
        /// </summary>
        ulong Victim { get; set; }

        /// <summary>
        /// Gets or sets the ID of the responsible moderator
        /// </summary>
        ulong Moderator { get; set; }

        /// <summary>
        /// The reason for the bonk
        /// </summary>
        string Reason { get; set; }

        /// <summary>
        /// When the bonk took place
        /// </summary>
        Instant BonkDate { get; set; }
    }
}
