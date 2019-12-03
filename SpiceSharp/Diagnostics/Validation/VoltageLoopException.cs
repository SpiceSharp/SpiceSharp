using SpiceSharp.Components;

namespace SpiceSharp.Diagnostics.Validation
{
    /// <summary>
    /// Exception for when a voltage loop is detected.
    /// </summary>
    /// <seealso cref="ValidationException" />
    public class VoltageLoopException : ValidationException
    {
        /// <summary>
        /// Gets the component that closes the loop.
        /// </summary>
        /// <value>
        /// The component.
        /// </value>
        public IComponent Component { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageLoopException"/> class.
        /// </summary>
        /// <param name="component">The component.</param>
        public VoltageLoopException(IComponent component)
            : base(Properties.Resources.Validation_ShortCircuitFixedVoltage.FormatString(component?.Name))
        {
            Component = component;
        }
    }
}
