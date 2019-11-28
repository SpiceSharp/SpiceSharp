namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when an instance is not bound to any simulation.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class InstanceNotBoundException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceNotBoundException"/> class.
        /// </summary>
        public InstanceNotBoundException()
            : base(Properties.Resources.InstanceNotBound)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceNotBoundException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        public InstanceNotBoundException(string parameterName)
            : base(Properties.Resources.InstanceNotBoundNamed.FormatString(parameterName))
        {
        }
    }
}
