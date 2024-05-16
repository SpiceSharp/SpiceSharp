using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// Describes a bridge between the local and global circuit.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IEquatable{T}" />
    public readonly struct Bridge<T> : IEquatable<Bridge<T>>
    {
        /// <summary>
        /// The local/internal instance.
        /// </summary>
        public readonly T Local;

        /// <summary>
        /// The global/external instance.
        /// </summary>
        public readonly T Global;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bridge{T}"/> struct.
        /// </summary>
        /// <param name="local">The local instance.</param>
        /// <param name="global">The global instance.</param>
        public Bridge(T local, T global)
        {
            Local = local;
            Global = global;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Bridge<T> bridge)
                return Equals(bridge);
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => (Local.GetHashCode() * 13) ^ Global.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Bridge<T> left, Bridge<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Bridge<T> left, Bridge<T> right) => !left.Equals(right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public readonly bool Equals(Bridge<T> other)
        {
            if (!Local.Equals(other.Local))
                return false;
            if (!Global.Equals(other.Global))
                return false;
            return true;
        }
    }
}
