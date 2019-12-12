using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Pulse waveform description.
    /// </summary>
    public partial class Pulse
    {
        /// <summary>
        /// The instance for a pulsed waveform.
        /// </summary>
        /// <seealso cref="IWaveform" />
        protected class Instance : IWaveform
        {
            /// <summary>
            /// Private variables
            /// </summary>
            private double _v1, _v2, _td, _tr, _tf, _pw, _per;
            private ITimeSimulationState _timeState;

            /// <summary>
            /// Gets the value that is currently being probed.
            /// </summary>
            /// <value>
            /// The value at the probed timepoint.
            /// </value>
            public double Value { get; private set; }

            /// <summary>
            /// Sets up the waveform.
            /// </summary>
            public Instance(ITimeSimulationState state,
                double v1, double v2, double td, double tr, double tf, double pw, double per)
            {
                _timeState = state;

                // Cache parameter values
                _v1 = v1;
                _v2 = v2;
                _td = td;
                _tr = tr;
                _tf = tf;
                _pw = pw;
                _per = per;

                // Some checks
                if (_tr < 0.0)
                    throw new BadParameterException(nameof(tr), tr,
                        Properties.Resources.Waveforms_Pulse_RiseTimeTooSmall);
                if (_tf < 0.0)
                    throw new BadParameterException(nameof(tf), tf,
                        Properties.Resources.Waveforms_Pulse_FallTimeTooSmall);
                if (_pw < 0.0)
                    throw new BadParameterException(nameof(pw), pw,
                        Properties.Resources.Waveforms_Pulse_PulseWidthTooSmall);
                if (_per < _tr + _pw + _tf)
                    throw new BadParameterException(nameof(per), per,
                        Properties.Resources.Waveforms_Pulse_PeriodTooSmall);

                // Initialize the value
                At(0.0);
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
            /// Probes a new timepoint.
            /// </summary>
            public void Probe()
            {
                var time = _timeState?.Method?.Time ?? 0.0;
                At(time);
            }

            /// <summary>
            /// Accepts the current timepoint.
            /// </summary>
            public void Accept()
            {
                _timeState.ThrowIfNull("time state");

                // Initialize the pulse
                if (_timeState.Method.Time.Equals(0.0))
                    Value = _v1;

                // Are we at a breakpoint?
                if (_timeState.Method is IBreakpoints method)
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
}
