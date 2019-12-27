using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Behavior for accepting time-points for a <see cref="VoltageDelay"/>.
    /// </summary>
    public class AcceptBehavior : TimeBehavior, IAcceptBehavior
    {
        private double _oldSlope;
        private bool _wasBreak;
        private readonly IIntegrationMethod _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public AcceptBehavior(string name, ComponentBindingContext context)
            : base(name, context)
        {
            context.ThrowIfNull(nameof(context));
            _method = context.GetState<IIntegrationMethod>();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void IAcceptBehavior.Probe()
        {
            // Force first order interpolation if we are close to a breakpoint
            var breakpoint = _wasBreak;
            if (_method is IBreakpointMethod method)
                breakpoint |= method.Break;
            Signal.Probe(_method.Time, breakpoint);
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        void IAcceptBehavior.Accept()
        {
            if (_method is IBreakpointMethod method)
            {
                // The integration method supports breakpoints, let's see if we need to add one
                if (_wasBreak || method.Break)
                {
                    // Calculate the slope of the accepted timepoint
                    var slope = method.Time.Equals(0.0)
                        ? 0.0
                        : (Signal.GetValue(0, 0) - Signal.GetValue(1, 0)) /
                          (Signal.GetTime(0) - Signal.GetTime(1));

                    // The previous point was a breakpoint, let's see if we need to add another breakpoint
                    if (_wasBreak)
                    {
                        var tol = Parameters.RelativeTolerance * Math.Max(Math.Abs(_oldSlope), Math.Abs(slope)) + Parameters.AbsoluteTolerance;
                        if (Math.Abs(slope - _oldSlope) > tol)
                            method.Breakpoints.SetBreakpoint(Signal.GetTime(1) + Signal.Delay);
                    }

                    // Track for the next time
                    _oldSlope = slope;
                    _wasBreak = method.Break;
                }
            }

            // Move to the next probed value
            Signal.AcceptProbedValues();
        }
    }
}
