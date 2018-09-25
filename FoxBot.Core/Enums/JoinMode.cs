namespace FoxBot.Core.Enums
{
    public enum JoinMode
    {
        /// <summary>
        /// Allows all users to join the server. If a Member role is specified, it will automatically be granted on join.
        /// </summary>
        /// <remarks>Default</remarks>
        All,

        /// <summary>
        /// Requires that users upvote a welcome message using the :thumbsup: emoji
        /// </summary>
        /// <remarks>Requires Member role to be defined.</remarks>
        Vote,

        /// <summary>
        /// A staff member must manually approve all new users. Similar to All, but does not automatically grant the Member role.
        /// </summary>
        Manual
    }
}
