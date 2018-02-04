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
        public Parameter InitialValue { get; } = new Parameter();
        [PropertyName("v2"), PropertyInfo("The peak value")]
        public Parameter PulsedValue { get; } = new Parameter();
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
        /// <param name="initialValue">The initial value</param>
        /// <param name="pulsedValue">The peak value</param>
        /// <param name="delay">The initial delay time in seconds</param>
        /// <param name="riseTime">The rise time in seconds</param>
        /// <param name="fallTime">The fall time in seconds</param>
        /// <param name="pulseWidth">The pulse width in seconds</param>
        /// <param name="period">The period in seconds</param>
        public Pulse(double initialValue, double pulsedValue, double delay, double riseTime, double fallTime, double pulseWidth, double period)
        {
            InitialValue.Set(initialValue);
            PulsedValue.Set(pulsedValue);
            Delay.Set(delay);
            RiseTime.Set(riseTime);
            FallTime.Set(fallTime);
            PulseWidth.Set(pulseWidth);
            Period.Set(period);
        }

        /// <summary>
        /// Setup the pulsed waveform
        /// </summary>
        public override void Setup()
        {
            v1 = InitialValue;
            v2 = PulsedValue;
            td = Delay;
            tr = RiseTime;
            tf = FallTime;
            pw = PulseWidth;
            per = Period;

            // Some checks
            if (per <= tr + pw + tf)
                throw new CircuitException("Invalid pulse specification: Period {0} is too small".FormatString(per));
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
        /// <param name="simulation">Time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Should not be here
            if (simulation.Method == null)
                return;

            // Are we at a breakpoint?
            IntegrationMethod method = simulation.Method;
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
