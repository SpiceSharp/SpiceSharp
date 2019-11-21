namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A <see cref="IBiasingBehavior"/> that can update after solving an iteration of a <see cref="Simulations.IBiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="IBiasingBehavior" />
    public interface IBiasingUpdateBehavior : IBehavior
    {
        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void Update();
    }
}
