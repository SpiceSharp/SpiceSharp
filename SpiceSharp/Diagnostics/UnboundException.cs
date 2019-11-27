namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when an instance is not bound to any simulation.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class UnboundException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundException"/> class.
        /// </summary>
        public UnboundException()
            : base("Instance is not bound to any simulation")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        public UnboundException(string parameterName)
            : base("{0} is not bound to any simulation".FormatString(parameterName))
        {
        }
    }
}
