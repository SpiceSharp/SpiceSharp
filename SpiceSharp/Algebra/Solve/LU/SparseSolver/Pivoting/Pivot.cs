using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Describes a pivot.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Pivot<T> : IEquatable<Pivot<T>> where T : IFormattable
    {
        /// <summary>
        /// No pivot.
        /// </summary>
        public static Pivot<T> Empty = new Pivot<T>(default, PivotInfo.None);

        /// <summary>
        /// The pivot.
        /// </summary>
        public readonly ISparseMatrixElement<T> Element;

        /// <summary>
        /// The information about the pivot.
        /// </summary>
        public readonly PivotInfo Info;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pivot{T}"/> struct.
        /// </summary>
        /// <param name="element">The pivot.</param>
        /// <param name="info">The information.</param>
        public Pivot(ISparseMatrixElement<T> element, PivotInfo info)
        {
            Element = element;
            Info = info;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Pivot<T> pivot)
                return Equals(pivot);
            return false;
        }

        /// <summary>
        /// Equalses the specified pivot.
        /// </summary>
        /// <param name="pivot">The pivot.</param>
        /// <returns>
        /// <c>true</c> if the pivots are the equal; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(Pivot<T> pivot)
        {
            if (!Element.Equals(pivot.Element))
                return false;
            if (!Info.Equals(pivot.Info))
                return false;
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (Element?.GetHashCode() ?? 0) ^ Info.GetHashCode();

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Pivot<T> left, Pivot<T> right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Pivot<T> left, Pivot<T> right) => !left.Equals(right);
    }

    /// <summary>
    /// Kinds of pivots.
    /// </summary>
    public enum PivotInfo
    {
        /// <summary>
        /// A good pivot is one that meets the required tolerances.
        /// </summary>
        Good,

        /// <summary>
        /// A suboptimal pivot is one that meets the required tolerances, but may
        /// have some unwanted side-effects such as the creation of fill-ins.
        /// </summary>
        Suboptimal,

        /// <summary>
        /// A bad pivot is one that does not meet the required tolerances.
        /// </summary>
        Bad,

        /// <summary>
        /// No pivot was found at all, not even a bad one.
        /// </summary>
        None
    }
}
