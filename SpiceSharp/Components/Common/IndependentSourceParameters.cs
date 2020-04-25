using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// Base parameters for an independent source.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class IndependentSourceParameters : ParameterSet
    {
        /// <summary>
        /// The DC value of the source.
        /// </summary>
        /// <value>
        /// The DC value.
        /// </value>
        [ParameterName("dc"), ParameterInfo("D.C. source value")]
        public GivenParameter<double> DcValue { get; set; }

        /// <summary>
        /// Gets or sets the waveform description.
        /// </summary>
        /// <value>
        /// The waveform description.
        /// </value>
        [ParameterName("waveform"), ParameterInfo("The waveform")]
        public IWaveformDescription Waveform { get; set; }
    }
}
