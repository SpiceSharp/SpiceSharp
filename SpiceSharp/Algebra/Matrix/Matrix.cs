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
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        protected Matrix(int size)
        {
            if (size < 0)
                throw new SparseException("Invalid matrix size {0}".FormatString(size)); 
            Size = size;
        }

        /// <summary>
        /// Gets a value in the matrix at a specific row and column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        public abstract T GetValue(int row, int column);

        /// <summary>
        /// Sets the value in the matrix at a specific row and column
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public abstract void SetValue(int row, int column, T value);
    }
}