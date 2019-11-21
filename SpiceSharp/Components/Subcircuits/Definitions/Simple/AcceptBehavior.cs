using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// An <see cref="IAcceptBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class AcceptBehavior : SubcircuitBehavior<IAcceptBehavior>, IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="simulation">The simulation.</param>
        public AcceptBehavior(string name, SubcircuitSimulation simulation)
            : base(name, simulation)
        {
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public void Accept()
        {
            foreach (var behavior in Behaviors)
                behavior.Accept();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            foreach (var behavior in Behaviors)
                behavior.Probe();
        }
    }
}
