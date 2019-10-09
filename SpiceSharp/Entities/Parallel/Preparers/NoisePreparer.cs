using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Noise preparer for a <see cref="ParallelEntity"/>
    /// </summary>
    /// <seealso cref="IParallelPreparer" />
    public class NoisePreparer : IParallelPreparer
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
                psim.States.Add(psim.Parent.States.GetValue<INoiseSimulationState>());
            }
        }
    }
}
