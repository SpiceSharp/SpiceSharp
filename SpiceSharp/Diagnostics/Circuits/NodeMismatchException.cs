namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when nodes aren't matched.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class NodeMismatchException : SpiceSharpException
    {
        /// <summary>
        /// Gets the expected number of nodes.
        /// </summary>
        /// <value>
        /// The expected number of nodes.
        /// </value>
        public int Expected { get; }

        /// <summary>
        /// Gets the actual number of nodes.
        /// </summary>
        /// <value>
        /// The actual number of nodes.
        /// </value>
        public int Actual { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected number of nodes.</param>
        /// <param name="actual">The actual number of nodes.</param>
        public NodeMismatchException(int expected, int actual)
            : base(Properties.Resources.NodeMismatch.FormatString(expected, actual))
        {
            Expected = expected;
            Actual = actual;
        }
    }
}
