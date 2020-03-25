using SpiceSharp.Simulations;

namespace SpiceSharp.Diagnostics.Validation
{
    /// <summary>
    /// Exception for validating floating nodes.
    /// </summary>
    /// <seealso cref="ValidationException" />
    public class FloatingNodeException : ValidationException
    {
        /// <summary>
        /// Gets the floating node variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public IVariable Variable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeException"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public FloatingNodeException(IVariable variable)
            : base(Properties.Resources.Validation_FloatingNodeFound.FormatString(variable?.Name))
        {
            Variable = variable;
        }
    }
}
