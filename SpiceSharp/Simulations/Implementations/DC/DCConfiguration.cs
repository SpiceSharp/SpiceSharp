using System.Collections.ObjectModel;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="DC" /> simulation.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class DCConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets the list of sweeps that need to be executed.
        /// </summary>
        [ParameterName("sweeps"), ParameterInfo("List of sweeps")]
        public Collection<SweepConfiguration> Sweeps { get; } = new Collection<SweepConfiguration>();

        /// <summary>
        /// Gets the maximum number of iterations allowed for DC sweeps.
        /// </summary>
        public int SweepMaxIterations { get; set; } = 20;
    }
}
