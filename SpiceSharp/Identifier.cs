using System;

namespace SpiceSharp
{
    /// <summary>
    /// An identifier for a circuit object
    /// </summary>
    [Serializable]
    public abstract class Identifier : IEquatable<Identifier>
    {
        /// <summary>
        /// Is the identifier equal to another identifier?
        /// </summary>
        /// <param name="other">Other identifier</param>
        /// <returns></returns>
        public abstract bool Equals(Identifier other);

        /// <summary>
        /// Clone the identifier
        /// </summary>
        /// <returns></returns>
        public abstract Identifier Clone();

        /// <summary>
        /// Allow implicit conversion to StringIdentifier for strings
        /// This can significantly simplify input
        /// </summary>
        /// <param name="id">Id</param>
        public static implicit operator Identifier(string id)
        {
            return new StringIdentifier(id);
        }
    }
}
