namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when two objects do not have the same size.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class SizeMismatchException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeMismatchException"/> class.
        /// </summary>
        /// <param name="name1">The first parameter name.</param>
        /// <param name="name2">The second parameter name.</param>
        public SizeMismatchException(string name1, string name2)
            : base("Size mismatch of {0} and {1}".FormatString(name1, name2))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeMismatchException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SizeMismatchException(string name)
            : base("Size mismatch of {0}".FormatString(name))
        {
        }
    }
}
