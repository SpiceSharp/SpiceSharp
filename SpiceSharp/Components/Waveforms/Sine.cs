using System;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.Waveforms
{
    /// <summary>
    /// This class is capable of generating a sine wave
    /// </summary>
    public class Sine : Waveform
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vo"), SpiceInfo("The offset of the sine wave")]
        public Parameter<double> VO { get; } = new Parameter<double>();
        [SpiceName("va"), SpiceInfo("The amplitude of the sine wave")]
        public Parameter<double> VA { get; } = new Parameter<double>();
        [SpiceName("freq"), SpiceInfo("The frequency in Hz")]
        public Parameter<double> Freq { get; } = new Parameter<double>();
        [SpiceName("td"), SpiceInfo("The delay in seconds")]
        public Parameter<double> Delay { get; } = new Parameter<double>();
        [SpiceName("theta"), SpiceInfo("The damping factor")]
        public Parameter<double> Theta { get; } = new Parameter<double>();

        /// <summary>
        /// Private variables
        /// </summary>
        private double vo, va, freq, td, theta;

        /// <summary>
        /// Constructor
        /// </summary>
        public Sine() : base("SINE") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vo">The offset of the sine wave</param>
        /// <param name="va">The amplitude of the sine wave</param>
        /// <param name="freq">The frequency in Hz</param>
        /// <param name="td">The delay in seconds</param>
        /// <param name="theta">The damping factor</param>
        public Sine(object vo, object va, object freq, object td = null, object theta = null) : base("SINE")
        {
            Set("vo", vo);
            Set("va", va);
            Set("freq", freq);
            if (td != null)
                Set("td", td);
            if (theta != null)
                Set("theta", theta);
        }

        /// <summary>
        /// Setup the sine wave
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
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
                result = vo + va * Math.Sin(freq * time * 2.0 * Circuit.CONSTPI);
            return result;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Do nothing
        }
    }
}
