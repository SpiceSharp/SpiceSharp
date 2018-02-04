using System.Collections.ObjectModel;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="DC"/>
    /// </summary>
    public class DCConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets the list of sweeps that need to be executed
        /// </summary>
        [PropertyName("sweeps"), PropertyInfo("List of sweeps")]
        public Collection<SweepConfiguration> Sweeps { get; } = new Collection<SweepConfiguration>();

        /// <summary>
        /// Number of iterations for DC sweeps
        /// </summary>
        public int SweepMaxIterations { get; set; } = 20;
    }
}
