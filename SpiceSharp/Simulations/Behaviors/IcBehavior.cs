using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for setting initial conditions
    /// </summary>
    public abstract class IcBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public IcBehavior(Identifier name = null) : base(name) { }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void SetIc(Circuit ckt);
    }
}
