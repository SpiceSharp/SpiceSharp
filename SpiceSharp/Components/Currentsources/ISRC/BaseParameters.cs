using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ISRC
{
    /// <summary>
    /// Base parameters for a <see cref="Currentsource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("waveform"), InfoAttribute("The waveform object for this source")]
        public Waveform ISRCwaveform { get; set; } = null;
        [NameAttribute("dc"), InfoAttribute("D.C. source value")]
        public Parameter ISRCdcValue { get; } = new Parameter();

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
            ISRCdcValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="w">Waveform</param>
        public BaseParameters(Waveform w)
        {
            ISRCwaveform = w;
        }
    }
}
