using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a sine wave.
    /// </summary>
    /// <seealso cref="IWaveformDescription" />
    public partial class Sine : ParameterSet, IWaveformDescription
    {
        /// <summary>
        /// Gets the offset.
        /// </summary>
        [ParameterName("vo"), ParameterInfo("The offset of the sine wave")]
        public double Offset { get; set; }

        /// <summary>
        /// Gets the amplitude of the sine wave.
        /// </summary>
        [ParameterName("va"), ParameterInfo("The amplitude of the sine wave")]
        public double Amplitude { get; set; }

        /// <summary>
        /// Gets the frequency of the sine wave in Hertz (Hz).
        /// </summary>
        [ParameterName("freq"), ParameterInfo("The frequency in Hz")]
        public double Frequency { get; set; }

        /// <summary>
        /// Gets the delay of the sine wave in seconds.
        /// </summary>
        [ParameterName("td"), ParameterInfo("The delay in seconds")]
        public double Delay { get; set; }

        /// <summary>
        /// Gets the damping factor theta of the sinewave.
        /// </summary>
        [ParameterName("theta"), ParameterInfo("The damping factor")]
        public double Theta { get; set; }

        /// <summary>
        /// Gets the phase of the sinewave.
        /// </summary>
        [ParameterName("phase"), ParameterInfo("The phase")]
        public double Phase { get; set; }

        /// <summary>
        /// Sets all the sine parameters.
        /// </summary>
        /// <param name="parameters"></param>
        [ParameterName("sine"), ParameterInfo("A vector of all sine waveform parameters")]
        public void SetSine(double[] parameters)
        {
            parameters.ThrowIfNull(nameof(parameters));
            switch (parameters.Length)
            {
                case 6:
                    Phase = parameters[5];
                    goto case 5;
                case 5:
                    Theta = parameters[4];
                    goto case 4;
                case 4:
                    Delay = parameters[3];
                    goto case 3;
                case 3:
                    Frequency = parameters[2];
                    goto case 2;
                case 2:
                    Amplitude = parameters[1];
                    goto case 1;
                case 1:
                    Offset = parameters[0];
                    break;
                default:
                    throw new BadParameterException(nameof(parameters));
            }
        }

        /// <summary>
        /// Creates a waveform instance for the specified simulation and entity.
        /// </summary>
        /// <param name="method">The integration method.</param>
        /// <returns>
        /// A waveform instance.
        /// </returns>
        public IWaveform Create(IIntegrationMethod method)
            => new Instance(method, Offset, Amplitude, Frequency, Delay, Theta, Phase);

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        public Sine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="theta">The theta.</param>
        /// <param name="phase">The phase.</param>
        public Sine(double offset, double amplitude, double frequency, double delay, double theta, double phase)
        {
            Offset = offset;
            Amplitude = amplitude;
            Frequency = frequency;
            Delay = delay;
            Theta = theta;
            Phase = phase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="theta">The theta.</param>
        public Sine(double offset, double amplitude, double frequency, double delay, double theta)
        {
            Offset = offset;
            Amplitude = amplitude;
            Frequency = frequency;
            Delay = delay;
            Theta = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset = offset;
            Amplitude = amplitude;
            Frequency = frequency;
        }
    }
}
