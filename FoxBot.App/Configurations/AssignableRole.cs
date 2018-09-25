using Discord;
using Discord.Commands;
using FoxBot.Core.Enums;
using FoxBot.Core.Interfaces;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.App.Configurations
{
    public class AssignableRole : IAssignableRole
    {
        /// <summary>
        /// The unique Id number for this role
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// The unique Id of the category this role belongs to.
        /// </summary>
        public ulong CategoryId { get; set; }

        /// <summary>
        /// How this role is assigned
        /// </summary>
        public AssignmentMethod AssignmentMethod { get; set; } = AssignmentMethod.Self;

        /// <summary>
        /// How to handle the prerequisite
        /// </summary>
        public PrerequisiteType PrerequisiteType { get; set; } = PrerequisiteType.None;

        /// <summary>
        /// This prerequite should represent the type selected in PrerequisiteType
        /// </summary>
        [Obsolete("Divided into two sub-types")]
        public object AssignmentPrerequisite { get; set; } = null;

        /// <summary>
        /// A user must have the specified role before they can be given this one.
        /// </summary>
        public ulong RolePrerequisite { get; set; }

        /// <summary>
        /// The amount of time a member must be a member of the server before they can get this role.
        /// </summary>
        public Period MembershipDuration { get; set; }

        /// <summary>
        /// Zero-based order for this role
        /// </summary>
        public int Order { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool Equals(IAssignableRole other)
        {
            return RoleId == other.RoleId;
        }

        public AssignableRole()
        {

        }
        public AssignableRole(IRole role, AssignmentMethod assignmentMethod, PrerequisiteType prerequisiteType, IRole rolePrerequisite = null, Period membershipDuration = null, int order = -1, string description = "")
            : this(role.Id, assignmentMethod, prerequisiteType, rolePrerequisite, membershipDuration, order, description) { }
        public AssignableRole(ulong roleId, AssignmentMethod assignmentMethod, PrerequisiteType prerequisiteType, IRole rolePrerequisite = null, Period membershipDuration = null, int order = -1, string description = "")
        {
            RoleId = roleId;
            AssignmentMethod = assignmentMethod;
            PrerequisiteType = prerequisiteType;
            RolePrerequisite = rolePrerequisite?.Id ?? ulong.MaxValue;
            MembershipDuration = membershipDuration;
            Order = order;
            Description = description;
        }
    }
}
