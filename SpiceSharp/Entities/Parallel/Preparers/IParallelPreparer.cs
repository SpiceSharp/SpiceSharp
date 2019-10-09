using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// An interface that can prepare simulations for a <see cref="ParallelEntity"/>.
    /// </summary>
    public interface IParallelPreparer
    {
        /// <summary>
        /// Prepares the specified simulation for parallel loading.
        /// </summary>
        /// <param name="simulations">The simulation that will be used for each task.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="parameters">The parameters of the <see cref="ParallelEntity" />.</param>
        /// <param name="entities">The entities that are potentially computed in parallel.</param>
        void Prepare(ISimulation[] simulations, ISimulation parent, ParameterSetDictionary parameters, IEntityCollection entities);
    }
}
