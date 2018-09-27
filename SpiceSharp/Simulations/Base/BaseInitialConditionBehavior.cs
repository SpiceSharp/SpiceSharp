using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// This class describes behavior for initial conditions.
    /// </summary>
    /// <seealso cref="Behavior" />
    public abstract class BaseInitialConditionBehavior : Behavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseInitialConditionBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseInitialConditionBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public abstract void SetInitialCondition(Simulation simulation);
    }
}
