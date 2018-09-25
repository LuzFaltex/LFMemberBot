namespace FoxBot.Core.Enums
{
    public enum AssignmentMethod
    {
        /// <summary>
        /// User is not allowed to self-assign this role for the time being
        /// </summary>
        Locked = 0,
        /// <summary>
        /// User may self-assign this role at any time,
        /// barring any prerequisite conditions
        /// </summary>
        Self = 1
    }
}
