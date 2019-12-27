using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context)
            : base(name, context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            _posNode = state.Map[context.Nodes[0]];
            _negNode = state.Map[context.Nodes[1]];
            _contPosNode = state.Map[context.Nodes[2]];
            _contNegNode = state.Map[context.Nodes[3]];
            _branchEq = state.Map[Branch];
            _complex = context.GetState<IComplexSimulationState>();
            _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            });
        }

        /// <summary>
        /// Initializes the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var laplace = _complex.Laplace;
            var factor = Complex.Exp(-laplace * Parameters.Delay);

            // Load the Y-matrix and RHS-vector
            _elements.Add(1, -1, 1, -1, -factor, factor);
        }
    }
}
