namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when a matrix is singular.
    /// </summary>
    /// <seealso cref="AlgebraException" />
    public class SingularException : AlgebraException
    {
        /// <summary>
        /// Gets the index where the first zero-diagonal element was found.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        public SingularException()
            : base(Properties.Resources.SingularMatrix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularException"/> class.
        /// </summary>
        /// <param name="index">The index where the exception occurred.</param>
        public SingularException(int index)
            : base(Properties.Resources.SingularMatrixIndexed.FormatString(index))
        {
            Index = index;
        }
    }
}
