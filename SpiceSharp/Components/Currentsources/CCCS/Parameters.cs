using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledCurrentSources
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the current gain of the source.
        /// </summary>
        /// <value>
        /// The current gain.
        /// </value>
        [ParameterName("gain"), ParameterInfo("Gain of the source")]
        [Finite]
        private double _coefficient;

        /// <summary>
        /// Gets or sets the number of resistors in parallel.
        /// </summary>
        /// <value>
        /// The number of resistors in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _parallelMultiplier = 1.0;
    }
}
