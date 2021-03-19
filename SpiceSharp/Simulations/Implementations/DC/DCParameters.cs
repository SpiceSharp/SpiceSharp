using SpiceSharp.ParameterSets;
using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="DC" /> simulation.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class DCParameters : ParameterSet, ICloneable<DCParameters>
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

        /// <inheritdoc/>
        public DCParameters Clone()
        {
            var clone = new DCParameters();
            clone.SweepMaxIterations = SweepMaxIterations;
            foreach (var sweep in Sweeps)
                clone.Sweeps.Add(sweep.Clone());
            return clone;
        }
    }
}
