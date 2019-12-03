using SpiceSharp.Components;

namespace SpiceSharp.Diagnostics.Validation
{
    /// <summary>
    /// An exception for when an <see cref="IComponent"/> has been short-circuited.
    /// </summary>
    /// <seealso cref="ValidationException" />
    public class ShortCircuitComponentException : ValidationException
    {
        /// <summary>
        /// Gets the component that was short-circuited.
        /// </summary>
        /// <value>
        /// The component.
        /// </value>
        public IComponent Component { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortCircuitComponentException"/> class.
        /// </summary>
        /// <param name="component">The component.</param>
        public ShortCircuitComponentException(IComponent component)
            : base(Properties.Resources.Validation_ShortCircuitComponent)
        {
            Component = component;
        }
    }
}
