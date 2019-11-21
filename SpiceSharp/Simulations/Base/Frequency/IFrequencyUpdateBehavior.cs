namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An <see cref="IFrequencyBehavior"/> that can update after solving an iteration of a <see cref="Simulations.FrequencySimulation"/>.
    /// </summary>
    /// <seealso cref="IBehavior" />
    public interface IFrequencyUpdateBehavior : IBehavior
    {
        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void Update();
    }
}
