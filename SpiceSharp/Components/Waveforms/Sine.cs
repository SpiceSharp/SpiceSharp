using System;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A sine waveform
    /// </summary>
    public class Sine : Waveform
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("vo"), ParameterInfo("The offset of the sine wave")]
        public GivenParameter Offset { get; } = new GivenParameter();
        [ParameterName("va"), ParameterInfo("The amplitude of the sine wave")]
        public GivenParameter Amplitude { get; } = new GivenParameter();
        [ParameterName("freq"), ParameterInfo("The frequency in Hz")]
        public GivenParameter Frequency { get; } = new GivenParameter();
        [ParameterName("td"), ParameterInfo("The delay in seconds")]
        public GivenParameter Delay { get; } = new GivenParameter();
        [ParameterName("theta"), ParameterInfo("The damping factor")]
        public GivenParameter Theta { get; } = new GivenParameter();

        /// <summary>
        /// Private variables
        /// </summary>
        private double _vo, _va, _freq, _td, _theta;

        /// <summary>
        /// Constructor
        /// </summary>
        public Sine()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="amplitude">Amplitude</param>
        /// <param name="frequency">Frequency (Hz)</param>
        /// <param name="delay">Delay (s)</param>
        /// <param name="theta">Damping factor</param>
        public Sine(double offset, double amplitude, double frequency, double delay, double theta)
        {
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
            Delay.Value = delay;
            Theta.Value = theta;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="amplitude">Amplitude</param>
        /// <param name="frequency">Frequency (Hz)</param>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
        }

        /// <summary>
        /// Setup the sine wave
        /// </summary>
        public override void Setup()
        {
            // Cache parameter values
            _vo = Offset;
            _va = Amplitude;
            _freq = Frequency * 2.0 * Math.PI;
            _td = Delay;
            _theta = Theta;

            // Some checks
            if (_freq < 0)
                throw new CircuitException("Invalid frequency {0}".FormatString(Frequency.Value));
            if (Theta.Given && _theta <= 0.0)
                throw new CircuitException("Invalid damping factor {0}".FormatString(_theta));
        }

        /// <summary>
        /// Calculate the sine wave at a timepoint
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns></returns>
        public override double At(double time)
        {
            time -= _td;

            // Calculate sine wave result (no offset)
            double result;
            if (time <= 0.0)
                result = 0.0;
            else
                result = _va * Math.Sin(_freq * time);

            // Modify with theta
            if (Theta.Given)
                result *= Math.Exp(-(time - _td) / _theta);

            // Return result (with offset)
            return _vo + result;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Do nothing
        }
    }
}
