using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Temperature-dependent behavior for circuit objects
    /// </summary>
    public abstract class TemperatureBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TemperatureBehavior(Identifier name = null) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public abstract void Temperature(BaseSimulation sim);
    }
}
