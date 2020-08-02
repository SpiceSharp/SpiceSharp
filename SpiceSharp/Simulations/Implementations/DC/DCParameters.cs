using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="DC" /> simulation.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class DCParameters : ParameterSet
    {
        /// <summary>
        /// Gets the list of sweeps that need to be executed.
        /// </summary>
        [ParameterName("sweeps"), ParameterInfo("List of sweeps")]
        public ICollection<ISweep> Sweeps { get; } = new List<ISweep>();

        /// <summary>
        /// Gets the maximum number of iterations allowed for DC sweeps.
        /// </summary>
        public int SweepMaxIterations { get; set; } = 20;
    }
}
