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
        [PropertyName("vo"), PropertyInfo("The offset of the sine wave")]
        public Parameter Offset { get; } = new Parameter();
        [PropertyName("va"), PropertyInfo("The amplitude of the sine wave")]
        public Parameter Amplitude { get; } = new Parameter();
        [PropertyName("freq"), PropertyInfo("The frequency in Hz")]
        public Parameter Frequency { get; } = new Parameter();
        [PropertyName("td"), PropertyInfo("The delay in seconds")]
        public Parameter Delay { get; } = new Parameter();
        [PropertyName("theta"), PropertyInfo("The damping factor")]
        public Parameter Theta { get; } = new Parameter();

        /// <summary>
        /// Private variables
        /// </summary>
        double _vo, _va, _freq, _td, _theta;

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
            Offset.Set(offset);
            Amplitude.Set(amplitude);
            Frequency.Set(frequency);
            Delay.Set(delay);
            Theta.Set(theta);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="amplitude">Amplitude</param>
        /// <param name="frequency">Frequency (Hz)</param>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset.Set(offset);
            Amplitude.Set(amplitude);
            Frequency.Set(frequency);
        }

        /// <summary>
        /// Setup the sine wave
        /// </summary>
        public override void Setup()
        {
            _vo = Offset;
            _va = Amplitude;
            _freq = Frequency;
            _td = Delay;
            _theta = Theta;
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
            double result = 0.0;
            if (time <= 0.0)
                result = 0.0;
            else
                result = _va * Math.Sin(_freq * time * 2.0 * Math.PI);

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
