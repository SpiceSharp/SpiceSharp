using System.Collections.ObjectModel;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="DC"/>
    /// </summary>
    public class DcConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets the list of sweeps that need to be executed
        /// </summary>
        [ParameterName("sweeps"), ParameterInfo("List of sweeps")]
        public Collection<SweepConfiguration> Sweeps { get; } = new Collection<SweepConfiguration>();

        /// <summary>
        /// Number of iterations for DC sweeps
        /// </summary>
        public int SweepMaxIterations { get; set; } = 20;
    }
}
