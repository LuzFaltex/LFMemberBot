using Discord.Commands;
using FoxBot.Core.Enums;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.Core.Interfaces
{
    [JsonObject]
    public interface IAssignableRole : IEquatable<IAssignableRole>
    {
        /// <summary>
        /// The unique Id number for this role
        /// </summary>
        ulong RoleId { get; set; }

        /// <summary>
        /// The unique Id of the category this role belongs to.
        /// </summary>
        ulong CategoryId { get; set; }

        /// <summary>
        /// How this role is assigned
        /// </summary>
        AssignmentMethod AssignmentMethod { get; set; }
        
        /// <summary>
        /// How to handle the prerequisite
        /// </summary>
        PrerequisiteType PrerequisiteType { get; set; }

        /// <summary>
        /// This prerequite should represent the type selected in PrerequisiteType
        /// </summary>
        [Obsolete("Use RolePrerequisite and MembershipDuration instead")]
        object AssignmentPrerequisite { get; set; }

        /// <summary>
        /// A user must have the specified role before they can be given this one.
        /// </summary>
        ulong RolePrerequisite { get; set; }

        /// <summary>
        /// The amount of time a member must be a member of the server before they can get this role.
        /// </summary>
        Period MembershipDuration { get; set; }

        /// <summary>
        /// Zero-based order for this role
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// A description of the role
        /// </summary>
        string Description { get; set; }
    }
}
