using SpiceSharp.Circuits;
using SpiceSharp.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Local simulation for a <see cref="ParallelLoader"/>
    /// </summary>
    /// <seealso cref="LocalSimulation" />
    public class ParallelSimulation : LocalSimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSimulation"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation.</param>
        public ParallelSimulation(ISimulation parent)
            : base(parent)
        {
            EntityBehaviors = new LocalBehaviorContainerCollection(parent.EntityBehaviors);
        }
    }
}
