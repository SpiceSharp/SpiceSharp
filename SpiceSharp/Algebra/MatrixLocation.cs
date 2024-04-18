using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A simple struct for describing a matrix row/column location.
    /// </summary>
    public readonly struct MatrixLocation : IEquatable<MatrixLocation>
    {
        /// <summary>
        /// The row index.
        /// </summary>
        public readonly int Row;

        /// <summary>
        /// The column index.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixLocation"/> struct.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="row"/> or <paramref name="column"/> is not positive.
        /// </exception>
        public MatrixLocation(int row, int column)
        {
            row.GreaterThanOrEquals(nameof(row), 0);
            column.GreaterThanOrEquals(nameof(column), 0);
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
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
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
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(MatrixLocation other)
        {
            if (Row != other.Row)
                return false;
            if (Column != other.Column)
                return false;
            return true;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => "({0},{1})".FormatString(Row, Column);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(MatrixLocation left, MatrixLocation right)
            => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(MatrixLocation left, MatrixLocation right)
            => !left.Equals(right);
    }
}
