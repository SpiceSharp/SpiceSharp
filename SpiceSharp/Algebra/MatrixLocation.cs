using System;
using System.Collections.Generic;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A simple struct for describing a matrix row/column location.
    /// </summary>
    public struct MatrixLocation
    {
        /// <summary>
        /// An <see cref="IEqualityComparer{T}"/> for <see cref="MatrixLocation"/>.
        /// </summary>
        /// <seealso cref="IEqualityComparer{T}" />
        public class Comparer : IEqualityComparer<MatrixLocation>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(MatrixLocation x, MatrixLocation y)
            {
                if (x.Row != y.Row)
                    return false;
                if (x.Column != y.Column)
                    return false;
                return true;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(MatrixLocation obj)
            {
                return (obj.Row.GetHashCode() * 13) ^ obj.Column.GetHashCode();
            }
        }

        /// <summary>
        /// The row index.
        /// </summary>
        public int Row;

        /// <summary>
        /// The column index.
        /// </summary>
        public int Column;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixLocation"/> struct.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public MatrixLocation(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Row.GetHashCode() * 13) ^ Column.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is MatrixLocation ml)
            {
                if (Row != ml.Row)
                    return false;
                if (Column != ml.Column)
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "({0},{1})".FormatString(Row, Column);
        }
    }
}
