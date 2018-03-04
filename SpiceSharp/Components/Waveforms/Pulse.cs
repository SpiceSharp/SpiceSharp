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
        double _v1, _v2, _td, _tr, _tf, _pw, _per;

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
            _v1 = InitialValue;
            _v2 = PulsedValue;
            _td = Delay;
            _tr = RiseTime;
            _tf = FallTime;
            _pw = PulseWidth;
            _per = Period;

            // Some checks
            if (_per <= _tr + _pw + _tf)
                throw new CircuitException("Invalid pulse specification: Period {0} is too small".FormatString(_per));
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
            time -= _td;
            if (time > _per)
            {
                basetime = _per * Math.Floor(time / _per);
                time -= basetime;
            }

            if (time <= 0.0 || time >= _tr + _pw + _tf)
                return _v1;
            if (time >= _tr && time <= _tr + _pw)
                return _v2;
            if (time > 0 && time < _tr)
                return _v1 + (_v2 - _v1) * time / _tr;
            return _v2 + (_v1 - _v2) * (time - _tr - _pw) / _tf;
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
            double time = method.Time - _td;
            double basetime = 0.0;
            if (time >= _per)
            {
                basetime = _per * Math.Floor(time / _per);
                time -= basetime;
            }
            double tol = 1e-7 * _pw;

            // Are we at the start of a breakpoint?
            if (time <= 0 || time >= _tr + _pw + _tf)
            {
                if (Math.Abs(time - 0) <= tol)
                    breaks.SetBreakpoint(basetime + _tr + _td);
                else if (Math.Abs(_tr + _pw + _tf - time) <= tol)
                    breaks.SetBreakpoint(basetime + _per + _td);
                else if ((time <= -_td))
                    breaks.SetBreakpoint(basetime + _td);
                else if (Math.Abs(_per - time) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr + _per);
            }
            else if (time >= _tr && time <= _tr + _pw)
            {
                if (Math.Abs(time - _tr) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr + _pw);
                else if (Math.Abs(_tr + _pw - time) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr + _pw + _tf);
            }
            else if (time > 0 && time < _tr)
            {
                if (Math.Abs(time - 0) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr);
                else if (Math.Abs(time - _tr) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr + _pw);
            }
            else
            {
                if (Math.Abs(_tr + _pw - time) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _tr + _pw + _tf);
                else if (Math.Abs(_tr + _pw + _tf - time) <= tol)
                    breaks.SetBreakpoint(basetime + _td + _per);
            }
        }
    }
}
