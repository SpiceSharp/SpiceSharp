using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior that can accept a time point.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface IAcceptBehavior : IBehavior
    {
        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void Probe();

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        void Accept();
    }
}
