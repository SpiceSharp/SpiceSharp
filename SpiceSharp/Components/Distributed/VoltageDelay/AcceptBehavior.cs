using System;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    public class AcceptBehavior : BaseAcceptBehavior
    {
        // Necessary behaviors parameters
        private BaseParameters _bp;
        private TransientBehavior _tran;
        private double _oldSlope;
        private bool _wasBreak;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public AcceptBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get behaviors
            _tran = provider.GetBehavior<TransientBehavior>();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            // Force first order interpolation if we are close to a breakpoint
            var breakpoint = _wasBreak;
            if (simulation.Method is IBreakpoints method)
                breakpoint |= method.Break;

            _tran.Signal.Probe(simulation.Method.Time, breakpoint);
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            if (simulation.Method is IBreakpoints method)
            {
                // The integration method supports breakpoints, let's see if we need to add one
                if (_wasBreak || method.Break)
                {
                    // Calculate the slope of the accepted timepoint
                    var slope = method.Time.Equals(0.0)
                        ? 0.0
                        : (_tran.Signal.GetValue(0, 0) - _tran.Signal.GetValue(1, 0)) /
                          (_tran.Signal.GetTime(0) - _tran.Signal.GetTime(1));

                    // The previous point was a breakpoint, let's see if we need to add another breakpoint
                    if (_wasBreak)
                    {
                        var tol = _bp.RelativeTolerance * Math.Max(Math.Abs(_oldSlope), Math.Abs(slope)) + _bp.AbsoluteTolerance;
                        if (Math.Abs(slope - _oldSlope) > tol)
                            method.Breakpoints.SetBreakpoint(_tran.Signal.GetTime(1) + _tran.Signal.Delay);
                    }

                    // Track for the next time
                    _oldSlope = slope;
                    _wasBreak = method.Break;
                }
            }

            // Move to the next probed value
            _tran.Signal.AcceptProbedValues();
        }
    }
}
