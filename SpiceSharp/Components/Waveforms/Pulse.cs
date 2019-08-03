using System;
using SpiceSharp.Attributes;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class implements a pulse waveform.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Waveform" />
    public class Pulse : Waveform
    {
        /// <summary>
        /// Gets the initial value.
        /// </summary>
        [ParameterName("v1"), ParameterInfo("The initial value")]
        public GivenParameter<double> InitialValue { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the pulsed value.
        /// </summary>
        [ParameterName("v2"), ParameterInfo("The peak value")]
        public GivenParameter<double> PulsedValue { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the delay of the waveform in seconds.
        /// </summary>
        [ParameterName("td"), ParameterInfo("The initial delay time in seconds")]
        public GivenParameter<double> Delay { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the rise time in seconds.
        /// </summary>
        [ParameterName("tr"), ParameterInfo("The rise time in seconds")]
        public GivenParameter<double> RiseTime { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the fall time in seconds.
        /// </summary>
        [ParameterName("tf"), ParameterInfo("The fall time in seconds")]
        public GivenParameter<double> FallTime { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the width of the pulse in seconds.
        /// </summary>
        [ParameterName("pw"), ParameterInfo("The pulse width in seconds")]
        public GivenParameter<double> PulseWidth { get; } = new GivenParameter<double>(double.PositiveInfinity);

        /// <summary>
        /// Gets the period in seconds.
        /// </summary>
        [ParameterName("per"), ParameterInfo("The period in seconds")]
        public GivenParameter<double> Period { get; } = new GivenParameter<double>(double.PositiveInfinity);

        /// <summary>
        /// Sets all the pulse parameters.
        /// </summary>
        /// <param name="parameters">The pulse parameters</param>
        [ParameterName("pulse"), ParameterInfo("A vector of all pulse waveform parameters")]
        public void SetPulse(double[] parameters)
        {
            parameters.ThrowIfEmpty(nameof(parameters));
            switch (parameters.Length)
            {
                case 7:
                    Period.Value = parameters[6];
                    goto case 6;
                case 6:
                    PulseWidth.Value = parameters[5];
                    goto case 5;
                case 5:
                    FallTime.Value = parameters[4];
                    goto case 4;
                case 4:
                    RiseTime.Value = parameters[3];
                    goto case 3;
                case 3:
                    Delay.Value = parameters[2];
                    goto case 2;
                case 2:
                    PulsedValue.Value = parameters[1];
                    goto case 1;
                case 1:
                    InitialValue.Value = parameters[0];
                    break;
                default:
                    throw new BadParameterException(nameof(parameters));
            }
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private double _v1, _v2, _td, _tr, _tf, _pw, _per;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulse"/> class.
        /// </summary>
        public Pulse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pulse"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="pulsedValue">The peak value.</param>
        /// <param name="delay">The initial delay time in seconds.</param>
        /// <param name="riseTime">The rise time in seconds.</param>
        /// <param name="fallTime">The fall time in seconds.</param>
        /// <param name="pulseWidth">The pulse width in seconds.</param>
        /// <param name="period">The period in seconds.</param>
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
        /// Sets up the waveform.
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

            // Initialize the value
            At(0.0);
        }

        /// <summary>
        /// Indicates a new timepoint is being probed.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            At(simulation.Method.Time);
        }

        /// <summary>
        /// Calculate the pulse value at the designated timepoint
        /// </summary>
        /// <param name="time">The time.</param>
        private void At(double time)
        {
            // Get a relative time variable
            time -= _td;
            if (time > _per)
            {
                var basetime = _per * Math.Floor(time / _per);
                time -= basetime;
            }

            if (time <= 0.0 || time >= _tr + _pw + _tf)
                Value = _v1;
            else if (time >= _tr && time <= _tr + _pw)
                Value = _v2;
            else if (time > 0 && time < _tr)
                Value = _v1 + (_v2 - _v1) * time / _tr;
            else
                Value = _v2 + (_v1 - _v2) * (time - _tr - _pw) / _tf;
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Initialize the pulse
            if (simulation.Method.Time.Equals(0.0))
                Value = _v1;

            // Are we at a breakpoint?
            if (simulation.Method is IBreakpoints method)
            {
                var breaks = method.Breakpoints;
                if (!method.Break)
                    return;

                // Find the time relative to the first period
                var time = method.Time - _td;
                var basetime = 0.0;
                if (time >= _per)
                {
                    basetime = _per * Math.Floor(time / _per);
                    time -= basetime;
                }

                var tol = 1e-7 * _pw;

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
}
