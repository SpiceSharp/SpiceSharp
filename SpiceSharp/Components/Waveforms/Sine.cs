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
        public Parameter VO { get; } = new Parameter();
        [PropertyName("va"), PropertyInfo("The amplitude of the sine wave")]
        public Parameter VA { get; } = new Parameter();
        [PropertyName("freq"), PropertyInfo("The frequency in Hz")]
        public Parameter Freq { get; } = new Parameter();
        [PropertyName("td"), PropertyInfo("The delay in seconds")]
        public Parameter Delay { get; } = new Parameter();
        [PropertyName("theta"), PropertyInfo("The damping factor")]
        public Parameter Theta { get; } = new Parameter();

        /// <summary>
        /// Private variables
        /// </summary>
        double vo, va, freq, td, theta;

        /// <summary>
        /// Constructor
        /// </summary>
        public Sine()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vo">Offset</param>
        /// <param name="va">Amplitude</param>
        /// <param name="freq">Frequency (Hz)</param>
        /// <param name="td">Delay (s)</param>
        /// <param name="theta">Damping factor</param>
        public Sine(double vo, double va, double freq, double td, double theta)
        {
            VO.Set(vo);
            VA.Set(va);
            Freq.Set(freq);
            Delay.Set(td);
            Theta.Set(theta);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vo">Offset</param>
        /// <param name="va">Amplitude</param>
        /// <param name="freq">Frequency (Hz)</param>
        public Sine(double vo, double va, double freq)
        {
            VO.Set(vo);
            VA.Set(va);
            Freq.Set(freq);
        }

        /// <summary>
        /// Setup the sine wave
        /// </summary>
        public override void Setup()
        {
            vo = VO;
            va = VA;
            freq = Freq;
            td = Delay;
            theta = Theta;
        }

        /// <summary>
        /// Calculate the sine wave at a timepoint
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns></returns>
        public override double At(double time)
        {
            time -= td;
            double result = 0.0;
            if (time <= 0.0)
                result = vo;
            else
                result = vo + va * Math.Sin(freq * time * 2.0 * Math.PI);
            return result;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Accept(TimeSimulation sim)
        {
            // Do nothing
        }
    }
}
