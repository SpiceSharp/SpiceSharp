using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.MutualInductances
{
    /// <summary>
    /// Transient behavior for a <see cref="MutualInductance"/>
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(MutualInductance)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    public class Time : Temperature,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private double _conductance;
        private readonly IVariable<double> _branch1, _branch2;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(BindingContext context) : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();
            var state = context.GetState<IBiasingSimulationState>();
            var load1 = context.Inductor1Behaviors.GetValue<Inductors.Time>();
            _branch1 = load1.Branch;
            var load2 = context.Inductor2Behaviors.GetValue<Inductors.Time>();
            _branch2 = load2.Branch;

            // Register events for modifying the flux through the inductors
            load1.UpdateFlux += UpdateFlux1;
            load2.UpdateFlux += UpdateFlux2;

            int br1 = state.Map[_branch1];
            int br2 = state.Map[_branch2];
            _elements = new ElementSet<double>(state.Solver,
                new MatrixLocation(br1, br2),
                new MatrixLocation(br2, br1));
        }

        /// <summary>
        /// Update the flux through the secondary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux2(object sender, Inductors.UpdateFluxEventArgs args)
        {
            args.Flux.Value += Factor * _branch1.Value;
        }

        /// <summary>
        /// Update the flux through the primary inductor.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void UpdateFlux1(object sender, Inductors.UpdateFluxEventArgs args)
        {
            _conductance = args.Flux.GetContributions(Factor).Jacobian;
            args.Flux.Value += Factor * _branch2.Value;
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            // Load Y-matrix
            _elements.Add(-_conductance, -_conductance);
        }
    }
}