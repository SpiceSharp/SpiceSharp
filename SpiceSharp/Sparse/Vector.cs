namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A vector with real values
    /// </summary>
    public class Vector<T> where T : struct
    {
        /// <summary>
        /// Gets or sets a value
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new SparseException("Invalid index {0}".FormatString(index));
                return values[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new SparseException("Invalid index {0}".FormatString(index));
                values[index] = value;
            }
        }

        /// <summary>
        /// Gets the length of the vector
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Values
        /// </summary>
        T[] values;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public Vector(int length)
        {
            if (length < 1)
                throw new SparseException("Invalid vector length {0}".FormatString(length));
            Length = length;
            values = new T[length];
        }
    }
}
