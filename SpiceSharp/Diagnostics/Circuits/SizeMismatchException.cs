namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when two objects do not have the same size.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class SizeMismatchException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeMismatchException"/> class.
        /// </summary>
        /// <param name="name1">The first parameter name.</param>
        /// <param name="name2">The second parameter name.</param>
        public SizeMismatchException(string name1, string name2)
            : base(Properties.Resources.SizeMismatch1.FormatString(name1, name2))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeMismatchException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="expected">The expected size.</param>
        public SizeMismatchException(string name, int expected)
            : base(Properties.Resources.SizeMismatch2.FormatString(name, expected))
        {
        }
    }
}
