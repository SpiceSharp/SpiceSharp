using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltagesourceBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="VoltageSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("waveform"), PropertyInfo("Waveform shape")]
        public Waveform Waveform { get; set; }
        [PropertyName("dc"), PropertyInfo("D.C. source value")]
        public Parameter DcValue { get; } = new Parameter();

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
            DcValue.Set(dc);
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
