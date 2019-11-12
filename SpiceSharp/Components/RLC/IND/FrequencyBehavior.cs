using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Frequency behavior for <see cref="Inductor"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            _posNode = ComplexState.Map[context.Nodes[0]];
            _negNode = ComplexState.Map[context.Nodes[1]];
            _branchEq = ComplexState.Map[Branch];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _branchEq));
        }

        /// <summary>
        /// Initialize the small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var val = ComplexState.Laplace * Inductance;
            ComplexElements.Add(1, -1, -1, 1, -val);
        }
    }
}
