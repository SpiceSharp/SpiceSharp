using System;

namespace SpiceSharp
{
    /// <summary>
    /// An identifier that can be used for a variety of applications.
    /// </summary>
    /// <seealso cref="IEquatable{Identifier}" />
    public abstract class Identifier : IEquatable<Identifier>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Equals(Identifier other);

        /// <summary>
        /// Clones this identifier.
        /// </summary>
        /// <returns>The cloned identifier.</returns>
        public abstract Identifier Clone();

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="Identifier"/>.
        /// </summary>
        /// <param name="id">The string identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Identifier(string id)
        {
            return new StringIdentifier(id);
        }
    }
}
