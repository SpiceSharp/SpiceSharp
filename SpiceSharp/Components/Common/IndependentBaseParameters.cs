using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// Base parameters for an independent source.
    /// </summary>
    public class IndependentBaseParameters : ParameterSet
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
        public IndependentBaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">DC value</param>
        public IndependentBaseParameters(double dc)
        {
            DcValue.Value = dc;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="waveform">Waveform</param>
        public IndependentBaseParameters(Waveform waveform)
        {
            Waveform = waveform;
        }
    }
}
