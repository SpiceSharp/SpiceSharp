// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Base class for square matrices.
    /// </summary>
    /// <remarks>
    /// The elements in row and column with index 0 are considered trashcan elements. They
    /// should all map on the same element.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Matrix<T>
    {
        /// <summary>
        /// Gets the size of the matrix.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix{T}"/> class.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        protected Matrix(int size)
        {
            if (size < 0)
                throw new SparseException("Invalid matrix size {0}".FormatString(size)); 
            Size = size;
        }

        /// <summary>
        /// Gets a value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The value at the specified row and column.</returns>
        public abstract T GetValue(int row, int column);

        /// <summary>
        /// Sets the value in the matrix at a specific row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public abstract void SetValue(int row, int column, T value);
    }
}