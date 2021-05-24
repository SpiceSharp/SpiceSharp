using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledCurrentSources
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the transconductance gain.
        /// </summary>
        /// <value>
        /// The transconductance gain..
        /// </value>
        [ParameterName("gain"), ParameterInfo("Transconductance of the source (gain)", Units = "\u03a9^-1")]
        [Finite]
        private double _transconductance;

        /// <summary>
        /// Gets or sets the number of current sources in parallel.
        /// </summary>
        /// <value>
        /// The number of current sources in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _parallelMultiplier = 1.0;
    }
}
