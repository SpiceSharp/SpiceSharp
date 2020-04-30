using System;
using System.Collections.Generic;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// A class that can map parameter or property names to their <see cref="MemberDescription"/> instances.
    /// </summary>
    public interface IMemberMap
    {
        /// <summary>
        /// Gets the comparer used to compare member names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets the members.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        IEnumerable<MemberDescription> Members { get; }

        /// <summary>
        /// Adds a member description to the map.
        /// </summary>
        /// <param name="member">The member to add.</param>
        /// <exception cref="ArgumentException">Thrown if a member with the same name is already defined.</exception>
        void Add(MemberDescription member);
    }
}
