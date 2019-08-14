using System;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a sine wave.
    /// </summary>
    /// <seealso cref="Waveform" />
    public class Sine : Waveform
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
            parameters.ThrowIfEmpty(nameof(parameters));

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
        /// Private variables
        /// </summary>
        private double _vo, _va, _freq, _td, _theta, _phase;

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

        /// <summary>
        /// Sets up the waveform.
        /// </summary>
        public override void Setup()
        {
            // Cache parameter values
            _vo = Offset;
            _va = Amplitude;
            _freq = Frequency * 2.0 * Math.PI;
            _td = Delay;
            _theta = Theta;
            _phase = 2 * Math.PI * Phase / 360.0;

            Value = _vo;

            // Some checks
            if (_freq < 0)
                throw new CircuitException("Invalid frequency {0}".FormatString(Frequency.Value));
        }

        /// <summary>
        /// Indicates a new timepoint is being probed.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            var time = simulation.Method.Time;
            time -= _td;

            // Calculate sine wave result (no offset)
            double result;
            if (time <= 0.0)
                result = 0.0;
            else
                result = _va * Math.Sin(_freq * time + _phase);

            // Modify with theta
            if (Theta.Given)
                result *= Math.Exp(-time * _theta);

            // Return result (with offset)
            Value = _vo + result;
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Do nothing
            simulation.ThrowIfNull(nameof(simulation));

            // Initialize the sinewave
            if (simulation.Method.Time.Equals(0.0))
                Value = _vo;
        }
    }
}
