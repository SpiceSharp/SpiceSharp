using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="VoltageSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("waveform"), ParameterInfo("Waveform shape")]
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
