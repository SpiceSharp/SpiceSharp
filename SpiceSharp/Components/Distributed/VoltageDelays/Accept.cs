using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Behavior for accepting time-points for a <see cref="VoltageDelay"/>.
    /// </summary>
    /// <seealso cref="Time"/>
    /// <seealso cref="IAcceptBehavior"/>
    [BehaviorFor(typeof(VoltageDelay)), AddBehaviorIfNo(typeof(IAcceptBehavior))]
    [GeneratedParameters]
    public partial class Accept : Time,
        IAcceptBehavior
    {
        private double _oldSlope;
        private bool _wasBreak;
        private readonly IIntegrationMethod _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="Accept"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Accept(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _method = context.GetState<IIntegrationMethod>();
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
            // Force first order interpolation if we are close to a breakpoint
            bool breakpoint = _wasBreak;
            if (_method is IBreakpointMethod method)
                breakpoint |= method.Break;
            Signal.Probe(_method.Time, breakpoint);
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            if (_method is IBreakpointMethod method)
            {
                // The integration method supports breakpoints, let's see if we need to add one
                if (_wasBreak || method.Break)
                {
                    // Calculate the slope of the accepted timepoint
                    double slope = method.Time.Equals(0.0)
                        ? 0.0
                        : (Signal.GetValue(0, 0) - Signal.GetValue(1, 0)) /
                          (Signal.GetTime(0) - Signal.GetTime(1));

                    // The previous point was a breakpoint, let's see if we need to add another breakpoint
                    if (_wasBreak)
                    {
                        double tol = Parameters.RelativeTolerance * Math.Max(Math.Abs(_oldSlope), Math.Abs(slope)) + Parameters.AbsoluteTolerance;
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