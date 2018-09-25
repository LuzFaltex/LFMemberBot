using Discord;
using FoxBot.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxBot.App.Configurations
{
    [JsonObject]
    public class RoleCategory : IRoleCategory
    {
        /// <summary>
        /// Parent role for this category
        /// </summary>
        public IAssignableRole CategoryRole { get; set; }

        /// <summary>
        /// The ID of this category, which is the ID of the Category Role
        /// </summary>
        public ulong Id { get { return CategoryRole.RoleId; } }

        /// <summary>
        /// The order in which this should display in the roles command
        /// </summary>
        public int Order
        {
            get
            {
                return CategoryRole.Order;
            }
        }

        /// <summary>
        /// List of child roles for this category
        /// </summary>
        public HashSet<IAssignableRole> ChildRoles { get; set; }

        public bool Equals(IRoleCategory other)
        {
            return Id == other.Id;
        }

        public RoleCategory() : this(null) { }
        public RoleCategory(IAssignableRole categoryRole)
        {
            CategoryRole = categoryRole;
            ChildRoles = new HashSet<IAssignableRole>();
        }
    }
}
