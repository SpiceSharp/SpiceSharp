using SpiceSharp.Attributes;
using SpiceSharp.Entities;
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
        public GivenParameter<double> Offset { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the amplitude of the sine wave.
        /// </summary>
        [ParameterName("va"), ParameterInfo("The amplitude of the sine wave")]
        public GivenParameter<double> Amplitude { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the frequency of the sine wave in Hertz (Hz).
        /// </summary>
        [ParameterName("freq"), ParameterInfo("The frequency in Hz")]
        public GivenParameter<double> Frequency { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the delay of the sine wave in seconds.
        /// </summary>
        [ParameterName("td"), ParameterInfo("The delay in seconds")]
        public GivenParameter<double> Delay { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the damping factor theta of the sinewave.
        /// </summary>
        [ParameterName("theta"), ParameterInfo("The damping factor")]
        public GivenParameter<double> Theta { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the phase of the sinewave.
        /// </summary>
        [ParameterName("phase"), ParameterInfo("The phase")]
        public GivenParameter<double> Phase { get; } = new GivenParameter<double>();

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
                    Phase.Value = parameters[5];
                    goto case 5;
                case 5:
                    Theta.Value = parameters[4];
                    goto case 4;
                case 4:
                    Delay.Value = parameters[3];
                    goto case 3;
                case 3:
                    Frequency.Value = parameters[2];
                    goto case 2;
                case 2:
                    Amplitude.Value = parameters[1];
                    goto case 1;
                case 1:
                    Offset.Value = parameters[0];
                    break;
                default:
                    throw new BadParameterException(nameof(parameters));
            }
        }

        /// <summary>
        /// Creates a waveform instance for the specified simulation and entity.
        /// </summary>
        /// <param name="state">The time simulation state.</param>
        /// <returns>
        /// A waveform instance.
        /// </returns>
        public IWaveform Create(ITimeSimulationState state)
            => new Instance(state, Offset, Amplitude, Frequency, Delay, Theta, Phase);

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
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
            Delay.Value = delay;
            Theta.Value = theta;
            Phase.Value = phase;
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
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
            Delay.Value = delay;
            Theta.Value = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
        }
    }
}
