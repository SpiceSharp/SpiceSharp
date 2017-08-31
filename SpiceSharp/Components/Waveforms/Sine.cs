using System;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.Waveforms
{
    /// <summary>
    /// This class is capable of generating a sine wave
    /// </summary>
    public class Sine : Waveform<Sine>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vo"), SpiceInfo("The offset of the sine wave")]
        public Parameter VO { get; } = new Parameter();
        [SpiceName("va"), SpiceInfo("The amplitude of the sine wave")]
        public Parameter VA { get; } = new Parameter();
        [SpiceName("freq"), SpiceInfo("The frequency in Hz")]
        public Parameter Freq { get; } = new Parameter();
        [SpiceName("td"), SpiceInfo("The delay in seconds")]
        public Parameter Delay { get; } = new Parameter();
        [SpiceName("theta"), SpiceInfo("The damping factor")]
        public Parameter Theta { get; } = new Parameter();

        /// <summary>
        /// Private variables
        /// </summary>
        private double vo, va, freq, td, theta;

        /// <summary>
        /// Constructor
        /// </summary>
        public Sine() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vo">The offset of the sine wave</param>
        /// <param name="va">The amplitude of the sine wave</param>
        /// <param name="freq">The frequency in Hz</param>
        /// <param name="td">The delay in seconds</param>
        /// <param name="theta">The damping factor</param>
        public Sine(double vo, double va, double freq, double td = double.NaN, double theta = double.NaN) : base()
        {
            VO.Set(vo);
            VA.Set(va);
            Freq.Set(freq);
            if (!double.IsNaN(td))
                Delay.Set(td);
            if (!double.IsNaN(theta))
                Theta.Set(theta);
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
