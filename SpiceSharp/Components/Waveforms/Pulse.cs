using System;
using SpiceSharp.Attributes;
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
        [ParameterName("v1"), ParameterInfo("The initial value")]
        public GivenParameter InitialValue { get; } = new GivenParameter();
        [ParameterName("v2"), ParameterInfo("The peak value")]
        public GivenParameter PulsedValue { get; } = new GivenParameter();
        [ParameterName("td"), ParameterInfo("The initial delay time in seconds")]
        public GivenParameter Delay { get; } = new GivenParameter();
        [ParameterName("tr"), ParameterInfo("The rise time in seconds")]
        public GivenParameter RiseTime { get; } = new GivenParameter();
        [ParameterName("tf"), ParameterInfo("The fall time in seconds")]
        public GivenParameter FallTime { get; } = new GivenParameter();
        [ParameterName("pw"), ParameterInfo("The pulse width in seconds")]
        public GivenParameter PulseWidth { get; } = new GivenParameter();
        [ParameterName("per"), ParameterInfo("The period in seconds")]
        public GivenParameter Period { get; } = new GivenParameter();

        /// <summary>
        /// Private variables
        /// </summary>
        private double _v1, _v2, _td, _tr, _tf, _pw, _per;

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
            InitialValue.Value = initialValue;
            PulsedValue.Value = pulsedValue;
            Delay.Value = delay;
            RiseTime.Value = riseTime;
            FallTime.Value = fallTime;
            PulseWidth.Value = pulseWidth;
            Period.Value = period;
        }

        /// <summary>
        /// Setup the pulsed waveform
        /// </summary>
        public override void Setup()
        {
            // Cache parameter values
            _v1 = InitialValue;
            _v2 = PulsedValue;
            _td = Delay;
            _tr = RiseTime;
            _tf = FallTime;
            _pw = PulseWidth;
            _per = Period;

            // Some checks
            if (_tr < 0.0)
                throw new CircuitException("Invalid rise time {0}".FormatString(_tr));
            if (_tf < 0.0)
                throw new CircuitException("Invalid fall time {0}".FormatString(_tf));
            if (_pw < 0.0)
                throw new CircuitException("Invalid pulse width {0}".FormatString(_pw));
            if (_per < 0.0)
                throw new CircuitException("Invalid period {0}".FormatString(_per));
            if (_per < _tr + _pw + _tf)
                throw new CircuitException("Invalid pulse specification: Period {0} is too small".FormatString(_per));
        }

        /// <summary>
        /// Calculate the pulse at a timepoint
        /// </summary>
        /// <param name="time">Timepoint</param>
        /// <returns></returns>
        public override double At(double time)
        {
            // Get a relative time variable
            time -= _td;
            if (time > _per)
            {
                var basetime = _per * Math.Floor(time / _per);
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
                else if (time <= -_td)
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
