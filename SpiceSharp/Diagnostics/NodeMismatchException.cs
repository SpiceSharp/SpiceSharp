namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when nodes aren't matched.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class NodeMismatchException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected number of nodes.</param>
        /// <param name="actual">The actual number of nodes.</param>
        public NodeMismatchException(int expected, int actual)
            : base("Node mismatch: {0} nodes expected, but {1} were given.".FormatString(expected, actual))
        {
        }
    }
}
