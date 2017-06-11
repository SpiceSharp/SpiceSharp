using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.Waveforms
{
    /// <summary>
    /// A class that represents a pulsed waveform
    /// </summary>
    public class Pulse : Waveform
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("v1"), SpiceInfo("The initial value")]
        public Parameter<double> V1 { get; } = new Parameter<double>();
        [SpiceName("v2"), SpiceInfo("The peak value")]
        public Parameter<double> V2 { get; } = new Parameter<double>();
        [SpiceName("td"), SpiceInfo("The initial delay time in seconds")]
        public Parameter<double> Delay { get; } = new Parameter<double>();
        [SpiceName("tr"), SpiceInfo("The rise time in seconds")]
        public Parameter<double> RiseTime { get; } = new Parameter<double>();
        [SpiceName("tf"), SpiceInfo("The fall time in seconds")]
        public Parameter<double> FallTime { get; } = new Parameter<double>();
        [SpiceName("pw"), SpiceInfo("The pulse width in seconds")]
        public Parameter<double> PulseWidth { get; } = new Parameter<double>();
        [SpiceName("per"), SpiceInfo("The period in seconds")]
        public Parameter<double> Period { get; } = new Parameter<double>();

        /// <summary>
        /// Private variables
        /// </summary>
        private double v1, v2, td, tr, tf, pw, per;

        /// <summary>
        /// Constructor
        /// </summary>
        public Pulse() : base("PULSE")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v1">The initial value</param>
        /// <param name="v2">The peak value</param>
        /// <param name="td">The initial delay time in seconds</param>
        /// <param name="tr">The rise time in seconds</param>
        /// <param name="tf">The fall time in seconds</param>
        /// <param name="pw">The pulse width in seconds</param>
        /// <param name="per">The period in seconds</param>
        public Pulse(double v1, double v2, double td, double tr, double tf, double pw, double per) : base("PULSE")
        {
            V1.Set(v1);
            V2.Set(v2);
            Delay.Set(td);
            RiseTime.Set(tr);
            FallTime.Set(tf);
            PulseWidth.Set(pw);
            Period.Set(per);
        }

        /// <summary>
        /// Setup the pulsed
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            v1 = V1;
            v2 = V2;
            td = Delay;
            tr = RiseTime;
            tf = FallTime;
            pw = PulseWidth;
            per = Period;
        }

        /// <summary>
        /// Calculate the pulse at a timepoint
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override double At(double time)
        {
            double basetime = 0.0;

            // Get a relative time variable
            time -= td;
            if (time > per)
            {
                basetime = per * Math.Floor(time / per);
                time -= basetime;
            }

            if (time <= 0.0 || time >= tr + pw + tf)
                return v1;
            else if (time >= tr && time <= tr + pw)
                return v2;
            else if (time > 0 && time < tr)
                return v1 + (v2 - v1) * time / tr;
            else
                return v2 + (v1 - v2) * (time - tr - pw) / tf;
        }

        /// <summary>
        /// Accept the current time point
        /// </summary>
        /// <param name="ckt"></param>
        public override void Accept(Circuit ckt)
        {
            // Should not be here
            if (ckt.Method == null)
                return;

            IntegrationMethod method = ckt.Method;
            var breaks = method.Breaks;

            double time = method.Time - td;
            double basetime = 0.0;
            if (time >= per)
            {
                basetime = per * Math.Floor(time / per);
                time -= basetime;
            }

            double tol = 1e-7 * pw;
            if (time <= 0 || time >= tr + pw + tf)
            {
                if (method.Break && Math.Abs(time) < tol)
                    breaks.SetBreakpoint(basetime + tr + td);
                else if (method.Break && Math.Abs(time - tr - pw - tf) < tol)
                    breaks.SetBreakpoint(basetime + per + td);
                else if (method.Break && time == -td)
                    breaks.SetBreakpoint(basetime + td);
                else if (method.Break && Math.Abs(time - per) < tol)
                    breaks.SetBreakpoint(basetime + td + tr + per);
            }
            else if (time >= tr && time <= tr + pw)
            {
                if (method.Break && Math.Abs(time - tr) < tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw);
                else if (method.Break && Math.Abs(time - tr - pw) < tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw + tf);
            }
            else if (time > 0 && time < tr)
            {
                if (method.Break && Math.Abs(time) < tol)
                    breaks.SetBreakpoint(basetime + td + tr);
                else if (method.Break && Math.Abs(time - tr) < tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw);
            }
            else
            {
                if (method.Break && Math.Abs(time - tr - pw) < tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw + tf);
                else if (method.Break && Math.Abs(time - tr - pw - tf) < tol)
                    breaks.SetBreakpoint(basetime + td + per);
            }
        }
    }
}
