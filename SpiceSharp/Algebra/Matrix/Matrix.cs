// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Base class for matrices
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public abstract class Matrix<T>
    {
        /// <summary>
        /// Gets the size of the matrix
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Gets or sets a value in the matrix
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public abstract T this[int row, int column] { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        protected Matrix(int size)
        {
            if (size < 0)
                throw new SparseException("Invalid matrix size {0}".FormatString(size)); 
            Size = size;
        }
    }
}