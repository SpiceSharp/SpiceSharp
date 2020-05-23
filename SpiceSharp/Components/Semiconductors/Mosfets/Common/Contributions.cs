using SpiceSharp.Components.Common;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Contributions for loading a mosfet.
    /// </summary>
    /// <remarks>
    /// Please be careful using this struct, as it is mutable. It was created to be
    /// used to group the contribution variables for a mosfet with 4 terminals.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public struct Contributions<T> where T : struct
    {
        /// <summary>
        /// The gate-drain contribution.
        /// </summary>
        public Contribution<T> Gd;

        /// <summary>
        /// The gate-source contribution.
        /// </summary>
        public Contribution<T> Gs;

        /// <summary>
        /// The gate-bulk contribution.
        /// </summary>
        public Contribution<T> Gb;

        /// <summary>
        /// The bulk-drain contribution.
        /// </summary>
        public Contribution<T> Bd;

        /// <summary>
        /// The bulk-source contribution.
        /// </summary>
        public Contribution<T> Bs;

        /// <summary>
        /// The drain-source contribution.
        /// </summary>
        public Contribution<T> Ds;

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Contributions<T> c)
            {
                if (!Bd.Equals(c.Bd))
                    return false;
                if (!Bs.Equals(c.Bs))
                    return false;
                if (!Ds.Equals(c.Ds))
                    return false;
                if (!Gb.Equals(c.Gb))
                    return false;
                if (!Gd.Equals(c.Gd))
                    return false;
                if (!Gs.Equals(c.Gs))
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
        public override int GetHashCode()
        {
            var hash = Bd.GetHashCode();
            hash = (hash * 13) ^ Bs.GetHashCode();
            hash = (hash * 13) ^ Ds.GetHashCode();
            hash = (hash * 13) ^ Gb.GetHashCode();
            hash = (hash * 13) ^ Gs.GetHashCode();
            hash = (hash * 13) ^ Gd.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Contributions<T> left, Contributions<T> right)
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
        public static bool operator !=(Contributions<T> left, Contributions<T> right)
        {
            return !(left == right);
        }
    }
}
