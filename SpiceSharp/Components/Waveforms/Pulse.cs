using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A pulse waveform
    /// </summary>
    public class Pulse : Waveform
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("v1"), PropertyInfo("The initial value")]
        public Parameter V1 { get; } = new Parameter();
        [PropertyName("v2"), PropertyInfo("The peak value")]
        public Parameter V2 { get; } = new Parameter();
        [PropertyName("td"), PropertyInfo("The initial delay time in seconds")]
        public Parameter Delay { get; } = new Parameter();
        [PropertyName("tr"), PropertyInfo("The rise time in seconds")]
        public Parameter RiseTime { get; } = new Parameter();
        [PropertyName("tf"), PropertyInfo("The fall time in seconds")]
        public Parameter FallTime { get; } = new Parameter();
        [PropertyName("pw"), PropertyInfo("The pulse width in seconds")]
        public Parameter PulseWidth { get; } = new Parameter();
        [PropertyName("per"), PropertyInfo("The period in seconds")]
        public Parameter Period { get; } = new Parameter();

        /// <summary>
        /// Private variables
        /// </summary>
        double v1, v2, td, tr, tf, pw, per;

        /// <summary>
        /// Constructor
        /// </summary>
        public Pulse()
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
        public Pulse(double v1, double v2, double td, double tr, double tf, double pw, double per) : base()
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
        /// Setup the pulsed waveform
        /// </summary>
        public override void Setup()
        {
            v1 = V1;
            v2 = V2;
            td = Delay;
            tr = RiseTime;
            tf = FallTime;
            pw = PulseWidth;
            per = Period;

            // Some checks
            if (per <= tr + pw + tf)
                throw new CircuitException($"Invalid pulse specification: Period {per} is too small");
        }

        /// <summary>
        /// Calculate the pulse at a timepoint
        /// </summary>
        /// <param name="time">Timepoint</param>
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
            if (time >= tr && time <= tr + pw)
                return v2;
            if (time > 0 && time < tr)
                return v1 + (v2 - v1) * time / tr;
            return v2 + (v1 - v2) * (time - tr - pw) / tf;
        }

        /// <summary>
        /// Accept the current time point
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Accept(TimeSimulation sim)
        {
            // Should not be here
            if (sim.Method == null)
                return;

            // Are we at a breakpoint?
            IntegrationMethod method = sim.Method;
            var breaks = method.Breaks;
            if (!method.Break)
                return;

            // Find the time relative to the first period
            double time = method.Time - td;
            double basetime = 0.0;
            if (time >= per)
            {
                basetime = per * Math.Floor(time / per);
                time -= basetime;
            }
            double tol = 1e-7 * pw;

            // Are we at the start of a breakpoint?
            if (time <= 0 || time >= tr + pw + tf)
            {
                if (Math.Abs(time - 0) <= tol)
                    breaks.SetBreakpoint(basetime + tr + td);
                else if (Math.Abs(tr + pw + tf - time) <= tol)
                    breaks.SetBreakpoint(basetime + per + td);
                else if ((time <= -td))
                    breaks.SetBreakpoint(basetime + td);
                else if (Math.Abs(per - time) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr + per);
            }
            else if (time >= tr && time <= tr + pw)
            {
                if (Math.Abs(time - tr) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw);
                else if (Math.Abs(tr + pw - time) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw + tf);
            }
            else if (time > 0 && time < tr)
            {
                if (Math.Abs(time - 0) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr);
                else if (Math.Abs(time - tr) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw);
            }
            else
            {
                if (Math.Abs(tr + pw - time) <= tol)
                    breaks.SetBreakpoint(basetime + td + tr + pw + tf);
                else if (Math.Abs(tr + pw + tf - time) <= tol)
                    breaks.SetBreakpoint(basetime + td + per);
            }
        }
    }
}
