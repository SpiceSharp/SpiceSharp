using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("waveform"), ParameterInfo("The waveform object for this source")]
        public Waveform Waveform { get; set; }
        [ParameterName("dc"), ParameterInfo("D.C. source value")]
        public GivenParameter<double> DcValue { get; } = new GivenParameter<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">DC value</param>
        public BaseParameters(double dc)
        {
            DcValue.Value = dc;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="waveform">Waveform</param>
        public BaseParameters(Waveform waveform)
        {
            Waveform = waveform;
        }
    }
}
