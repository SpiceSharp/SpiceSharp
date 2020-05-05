using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.VoltageControlledVoltageSources
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the voltage gain.
        /// </summary>
        /// <value>
        /// The voltage gain.
        /// </value>
        [ParameterName("gain"), ParameterInfo("Voltage gain")]
        public double Coefficient { get; set; }
    }
}
