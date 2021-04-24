using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes a sweep of simulation properties.
    /// </summary>
    public interface ISweep : IParameterSet, ICloneable<ISweep>
    {
        /// <summary>
        /// Gets the name of the sweep.
        /// </summary>
        /// <value>
        /// The name of the sweep.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Creates an enumerable that can sweep properties of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to create the points for.</param>
        /// <returns>
        /// The created sweep points.
        /// </returns>
        IEnumerator<double> CreatePoints(IBiasingSimulation simulation);
    }
}
