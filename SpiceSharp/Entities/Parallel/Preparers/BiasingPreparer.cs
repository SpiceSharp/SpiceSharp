using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for the <see cref="IBiasingBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class BiasingPreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulations">The simulation that will be used for each task.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters of the <see cref="ParallelLoader" />.</param>
        /// <param name="entities">The entities that are potentially computed in parallel.</param>
        public void Prepare(ISimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters, IEntityCollection entities)
        {
            for (var i = 0; i < simulations.Length; i++)
            {
                var psim = (ParallelSimulation)simulations[i];
                var state = psim.Parent.States.GetValue<IBiasingSimulationState>();
                psim.States.Add<IBiasingSimulationState>(new BiasingSimulationState(state));
            }
        }
    }
}
