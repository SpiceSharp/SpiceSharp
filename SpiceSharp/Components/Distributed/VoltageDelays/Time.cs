using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Time behavior for a <see cref="VoltageDelay"/>.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(VoltageDelay)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    public class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly int _contPosNode, _contNegNode, _branchEq;
        private readonly ElementSet<double> _elements;
        private readonly ITimeSimulationState _time;
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Gets the delayed signal.
        /// </summary>
        /// <value>
        /// The signal.
        /// </value>
        protected DelayedSignal Signal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context)
            : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _contPosNode = _biasing.Map[_biasing.GetSharedVariable(context.Nodes[2])];
            _contNegNode = _biasing.Map[_biasing.GetSharedVariable(context.Nodes[3])];
            _branchEq = _biasing.Map[Branch];
            _elements = new ElementSet<double>(_biasing.Solver, [
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            ], [_branchEq]);
            Signal = new DelayedSignal(1, Parameters.Delay);
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            var sol = _biasing.Solution;
            double input = sol[_contPosNode] - sol[_contNegNode];
            Signal.SetProbedValues(input);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            var sol = _biasing.Solution;
            double input = sol[_contPosNode] - sol[_contNegNode];
            Signal.SetProbedValues(input);

            if (_time.UseDc)
                BiasingElements.Add(1, -1, 1, -1, -1, 1);
            else
            {
                BiasingElements.Add(1, -1, 1, -1);
                double c = Signal.InputDerivative;
                _elements.Add(-c, c, Signal.Values[0]);
            }
        }
    }
}