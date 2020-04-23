namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A matrix element for an <see cref="ISparseMatrix{T}"/>. This element has links
    /// to the surrounding matrix elements.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ISparseMatrixElement<T>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        T Value { get; set; }

        /// <summary>
        /// Gets the row of the matrix element.
        /// </summary>
        /// <value>
        /// The row index.
        /// </value>
        int Row { get; }

        /// <summary>
        /// Gets the column of the matrix element.
        /// </summary>
        /// <value>
        /// The column index.
        /// </value>
        int Column { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> left of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Left { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> right of this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Right { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> above this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Above { get; }

        /// <summary>
        /// Gets the first non-default <see cref="ISparseMatrixElement{T}"/> below this one.
        /// </summary>
        /// <value>
        /// The matrix element.
        /// </value>
        ISparseMatrixElement<T> Below { get; }
    }
}
