using SpiceSharp.Circuits;

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
        /// <param name="ckt">Circuit</param>
        public abstract void Temperature(Circuit ckt);
    }
}
