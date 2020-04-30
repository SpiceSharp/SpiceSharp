using System.Collections.Generic;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// Interface for types that can get or set members.
    /// </summary>
    public interface IMembers
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
    }
}
