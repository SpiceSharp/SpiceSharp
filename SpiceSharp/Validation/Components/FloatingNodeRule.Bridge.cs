using System;

namespace SpiceSharp.Validation
{
    public partial class FloatingNodeRule
    {
        /// <summary>
        /// A bridge between two groups.
        /// </summary>
        /// <seealso cref="IEquatable{Bridge}" />
        protected struct Path : IEquatable<Path>
        {
            /// <summary>
            /// Gets the first group (the group with the smallest index).
            /// </summary>
            /// <value>
            /// The group1.
            /// </value>
            public int Group1 { get; }

            /// <summary>
            /// Gets the second group (the group with the largest index).
            /// </summary>
            /// <value>
            /// The group2.
            /// </value>
            public int Group2 { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Path"/> struct.
            /// </summary>
            /// <param name="group1">The first group.</param>
            /// <param name="group2">The second group.</param>
            public Path(int group1, int group2)
            {
                if (group1 < group2)
                {
                    Group1 = group1;
                    Group2 = group2;
                }
                else
                {
                    Group2 = group1;
                    Group1 = group2;
                }
            }

            /// <summary>
            /// Checks if the bridge references a specified group.
            /// </summary>
            /// <param name="group">The group.</param>
            /// <returns>
            /// <c>true</c> if the group is part of the path; otherwise <c>false</c>.
            /// </returns>
            public bool References(int group) => Group1 == group || Group2 == group;

            /// <summary>
            /// Determines whether the specified <see cref="Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is Path bridge)
                    return Equals(bridge);
                return false;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
            /// </returns>
            public bool Equals(Path other)
            {
                if (other.Group1 == Group1 && other.Group2 == Group2)
                    return true;
                return false;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                // The group order doesn't matter
                return Group1.GetHashCode() ^ Group2.GetHashCode();
            }

            /// <summary>
            /// Implements the operator ==.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator ==(Path left, Path right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Implements the operator !=.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator !=(Path left, Path right)
            {
                return !left.Equals(right);
            }
        }
    }
}
