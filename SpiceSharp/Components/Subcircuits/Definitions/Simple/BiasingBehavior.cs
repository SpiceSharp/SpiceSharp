using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : SubcircuitBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public BiasingBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Load()
        {
            foreach (var behavior in Behaviors)
                behavior.Load();
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            var result = true;
            foreach (var behavior in Behaviors)
                result &= behavior.IsConvergent();
            return result;
        }
    }
}
