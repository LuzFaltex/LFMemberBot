using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.Core.Interfaces
{
    [JsonObject]
    public interface IRoleCategory : IEquatable<IRoleCategory>
    {
        /// <summary>
        /// Parent role for this category
        /// </summary>
        IAssignableRole CategoryRole { get; set; }

        /// <summary>
        /// The ID of this category, which is the ID of the Category Role
        /// </summary>
        ulong Id { get; }

        /// <summary>
        /// The order in which this should display in the roles command
        /// </summary>
        int Order { get; }

        /// <summary>
        /// List of child roles for this category
        /// </summary>
        HashSet<IAssignableRole> ChildRoles { get; set; }
    }
}
