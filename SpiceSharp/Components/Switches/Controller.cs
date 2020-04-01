using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// This class can calculate the controlling input of a switch.
    /// </summary>
    public abstract class Controller
    {
        /// <summary>
        /// Gets the value of the controlling value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public abstract double Value { get; }
    }
}
