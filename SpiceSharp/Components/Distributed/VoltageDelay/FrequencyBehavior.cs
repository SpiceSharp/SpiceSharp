using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Binds the specified simulation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            var c = (ComponentBindingContext)context;
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            _contPosNode = BiasingState.Map[c.Nodes[2]];
            _contNegNode = BiasingState.Map[c.Nodes[3]];
            _branchEq = BiasingState.Map[Branch];
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver, new[] {
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            });
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexElements?.Destroy();
            ComplexElements = null;
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
            var laplace = ComplexState.Laplace;
            var factor = Complex.Exp(-laplace * BaseParameters.Delay);

            // Load the Y-matrix and RHS-vector
            ComplexElements.Add(1, -1, 1, -1, -factor, factor);
        }
    }
}
