using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Describes a class that is capable of truncating a timestep.
    /// </summary>
    [SimulationBehavior]
    public interface ITruncatingBehavior : IBehavior
    {
        /// <summary>
        /// Calculate the maximum timestep that the behavior allows for our next time point.
        /// </summary>
        /// <returns>The timestep.</returns>
        double Prepare();

        /// <summary>
        /// Evaluate the currently calculated solution and return the maximum timestep
        /// that this behavior allows.
        /// </summary>
        /// <returns>The timestep.</returns>
        double Evaluate();
    }
}
