using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A description of a matrix element.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IMatrixElement<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the matrix element.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        T Value { get; set; }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <value>
        /// The row.
        /// </value>
        int Row { get; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        int Column { get; }

        /// <summary>
        /// Gets the matrix element left of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        IMatrixElement<T> Left { get; }

        /// <summary>
        /// Gets the matrix element right of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        IMatrixElement<T> Right { get; }

        /// <summary>
        /// Gets the matrix element above this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        IMatrixElement<T> Above { get; }

        /// <summary>
        /// Gets the matrix element below this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        IMatrixElement<T> Below { get; }
    }
}
