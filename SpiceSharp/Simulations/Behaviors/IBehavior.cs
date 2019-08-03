using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Contract for a behavior
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        void Setup(Simulation simulation, SetupDataProvider provider);

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        void Unsetup(Simulation simulation);
    }
}
