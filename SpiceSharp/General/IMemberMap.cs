using System;

namespace SpiceSharp.General
{
    /// <summary>
    /// A class that contains the members of a certain type.
    /// </summary>
    /// <seealso cref="IMembers" />
    public interface IMemberMap : IMembers
    {
        /// <summary>
        /// Adds a member description to the map.
        /// </summary>
        /// <param name="member">The member to add.</param>
        /// <exception cref="ArgumentException">Thrown if a member with the same name is already defined.</exception>
        void Add(MemberDescription member);
    }
}
