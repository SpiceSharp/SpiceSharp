using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for setting initial conditions
    /// </summary>
    public abstract class BaseInitialConditionBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected BaseInitialConditionBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        /// <param name="simulation"></param>
        public abstract void SetInitialCondition(Simulation simulation);
    }
}
