using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An <see cref="IParallelPreparer"/> for <see cref="IFrequencyBehavior"/>.
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class FrequencyPreparer : IParallelPreparer
    {
        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulations">The simulation that will be used for each task.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters of the <see cref="ParallelEntity" />.</param>
        /// <param name="entities">The entities that are potentially computed in parallel.</param>
        public void Prepare(ISimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters, IEntityCollection entities)
        {
            for (var i = 0; i < simulations.Length; i++)
            {
                var psim = (ParallelSimulation)simulations[i];
                var state = psim.Parent.States.GetValue<IComplexSimulationState>();
                psim.States.Add<IComplexSimulationState>(new ComplexSimulationState(state));
            }
        }
    }
}
