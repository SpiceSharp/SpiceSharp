using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Interface for an <see cref="IBiasingBehavior"/> that can check for convergence.
    /// </summary>
    /// <seealso cref="IBiasingBehavior" />
    [SimulationBehavior]
    public interface IConvergenceBehavior : IBiasingBehavior
    {
        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IsConvergent();
    }
}
