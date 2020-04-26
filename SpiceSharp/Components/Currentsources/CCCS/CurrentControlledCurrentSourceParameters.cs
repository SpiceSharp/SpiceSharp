using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class CurrentControlledCurrentSourceParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the current gain of the source.
        /// </summary>
        /// <value>
        /// The current gain.
        /// </value>
        [ParameterName("gain"), ParameterInfo("Gain of the source")]
        public double Coefficient { get; set; }
    }
}
