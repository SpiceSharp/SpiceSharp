namespace SpiceSharp.Components.Common
{
    /// <summary>
    /// A container for a contribution between two nodes.
    /// </summary>
    /// <remarks>
    /// Please be careful using this struct, as it is mutable. Its main
    /// use-case is to make code more readable by grouping variables
    /// for more complex models, because the conductance and equivalent current
    /// are often evaluated and passed around together.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public struct Contribution<T> where T : struct
    {
        /// <summary>
        /// The equivalent conductance (Y-matrix contribution).
        /// </summary>
        public T G;

        /// <summary>
        /// The equivalent current.
        /// </summary>
        public T C;

        /// <summary>
        /// Initializes a new instance of the <see cref="Contribution{T}"/> struct.
        /// </summary>
        /// <param name="g">The equivalent conductance.</param>
        /// <param name="ceq">The equivalent current.</param>
        public Contribution(T g, T ceq)
        {
            G = g;
            C = ceq;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is Contribution<T> c)
            {
                if (!G.Equals(c.G))
                    return false;
                if (!C.Equals(c.C))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => (13 * G.GetHashCode()) ^ C.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Contribution<T> left, Contribution<T> right)
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
        public static bool operator !=(Contribution<T> left, Contribution<T> right)
        {
            return !(left == right);
        }
    }
}
