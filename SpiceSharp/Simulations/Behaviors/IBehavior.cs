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
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets a flag indicating whether this behavior has been setup.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this behavior is setup; otherwise, <c>false</c>.
        /// </value>
        bool IsSetup { get; }

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
