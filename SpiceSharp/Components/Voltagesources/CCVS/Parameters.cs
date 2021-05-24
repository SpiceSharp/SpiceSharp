using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledVoltageSources
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the transresistance gain.
        /// </summary>
        /// <value>
        /// The transresistance gain.
        /// </value>
        [ParameterName("gain"), ParameterInfo("Transresistance (gain)")]
        [Finite]
        private double _coefficient;
    }
}
