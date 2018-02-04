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
        protected TemperatureBehavior(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public abstract void Temperature(BaseSimulation simulation);
    }
}
