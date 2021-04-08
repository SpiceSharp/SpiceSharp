using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using SpiceSharp.Attributes;
using SpiceSharp.Entities;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a sine wave.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    /// <seealso cref="IWaveformDescription" />
    [GeneratedParameters]
    public partial class Sine : ParameterSet<IWaveformDescription>,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>
        /// The offset of the sine wave.
        /// </value>
        [ParameterName("vo"), ParameterInfo("The offset of the sine wave")]
        public double Offset { get; set; }

        /// <summary>
        /// Gets or sets the amplitude of the sine wave.
        /// </summary>
        /// <value>
        /// The amplitude of the sine wave.
        /// </value>
        [ParameterName("va"), ParameterInfo("The amplitude of the sine wave")]
        public double Amplitude { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the sine wave in Hertz (Hz).
        /// </summary>
        /// <value>
        /// The frequency of the sine wave.
        /// </value>
        [ParameterName("freq"), ParameterInfo("The frequency in Hz", Units = "Hz")]
        [GreaterThanOrEquals(0)]
        private GivenParameter<double> _frequency;

        /// <summary>
        /// Gets or sets the delay of the sine wave in seconds.
        /// </summary>
        /// <value>
        /// The delay of the sine wave.
        /// </value>
        [ParameterName("td"), ParameterInfo("The delay", Units = "s")]
        public double Delay { get; set; }

        /// <summary>
        /// Gets or sets the damping factor theta of the sine wave.
        /// </summary>
        /// <value>
        /// The damping factor theta.
        /// </value>
        [ParameterName("theta"), ParameterInfo("The damping factor")]
        public double Theta { get; set; }

        /// <summary>
        /// Gets or sets the phase of the sine wave.
        /// </summary>
        /// <value>
        /// The phase.
        /// </value>
        [ParameterName("phase"), ParameterInfo("The phase", Units = "\u00b0")]
        public double Phase { get; set; }

        /// <summary>
        /// Sets all the sine parameters.
        /// </summary>
        /// <param name="sine">The sine parameters.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sine"/> does not have 1 to 6 arguments.</exception>
        [ParameterName("sine"), ParameterInfo("A vector of all sine waveform parameters")]
        public void SetSine(double[] sine)
        {
            sine.ThrowIfNotLength(nameof(sine), 1, 6);
            switch (sine.Length)
            {
                case 6:
                    Phase = sine[5];
                    goto case 5;
                case 5:
                    Theta = sine[4];
                    goto case 4;
                case 4:
                    Delay = sine[3];
                    goto case 3;
                case 3:
                    Frequency = sine[2];
                    goto case 2;
                case 2:
                    Amplitude = sine[1];
                    goto case 1;
                case 1:
                    Offset = sine[0];
                    break;
            }
        }

        /// <inheritdoc/>
        public IWaveform Create(IBindingContext context)
        {
            IIntegrationMethod method = null;
            TimeParameters tp = null;
            context?.TryGetState<IIntegrationMethod>(out method);
            context?.TryGetSimulationParameterSet<TimeParameters>(out tp);
            return new Instance(method,
                Offset,
                Amplitude,
                Frequency.Given ? Frequency.Value : 1.0 / (tp?.StopTime ?? 1.0),
                Delay,
                Theta,
                Phase);
        }

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
        /// <exception cref="ArgumentException">Thrown if <paramref name="frequency"/> is negative.</exception>
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
        /// <exception cref="ArgumentException">Thrown if <paramref name="frequency"/> is negative.</exception>
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
        /// <exception cref="ArgumentException">Thrown if <paramref name="frequency"/> is negative.</exception>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset = offset;
            Amplitude = amplitude;
            Frequency = frequency;
        }

        /// <summary>
        /// Returns a string that represents the current sine waveform.
        /// </summary>
        /// <returns>
        /// A string that represents the current sine waveform.
        /// </returns>
        public override string ToString()
        {
            return "sine({0} {1} {2} {3} {4} {5})".FormatString(
                Offset,
                Amplitude,
                Frequency.Value,
                Delay,
                Theta,
                Phase);
        }
    }
}
