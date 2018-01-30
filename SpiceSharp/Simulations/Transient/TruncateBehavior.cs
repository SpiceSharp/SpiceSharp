using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for truncating the current timeStep
    /// </summary>
    public abstract class TruncateBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected TruncateBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Truncate the current timeStep
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="timeStep">TimeStep</param>
        public abstract void Truncate(TimeSimulation simulation, ref double timeStep);
    }
}
