using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes temperature-dependent behavior.
    /// </summary>
    public abstract class BaseTemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseTemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public abstract void Temperature(BaseSimulation simulation);
    }
}
