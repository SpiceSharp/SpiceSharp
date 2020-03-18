using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// Base parameters for an independent source.
    /// </summary>
    public class IndependentSourceParameters : ParameterSet
    {
        /// <summary>
        /// The DC value of the source.
        /// </summary>
        [ParameterName("dc"), ParameterInfo("D.C. source value")]
        public GivenParameter<double> DcValue { get; set; }

        /// <summary>
        /// Gets or sets the waveform.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        [ParameterName("waveform"), ParameterInfo("The waveform")]
        public IWaveformDescription Waveform { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndependentSourceParameters"/> class.
        /// </summary>
        public IndependentSourceParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndependentSourceParameters"/> class.
        /// </summary>
        /// <param name="dc">DC value</param>
        public IndependentSourceParameters(double dc)
        {
            DcValue = dc;
        }
    }
}
