using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

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
        protected ComplexMatrixElementSet ComplexMatrixElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            ComplexMatrixElements = new ComplexMatrixElementSet(ComplexState.Solver,
                new MatrixPin(PosNode, BranchEq),
                new MatrixPin(NegNode, BranchEq),
                new MatrixPin(BranchEq, NegNode),
                new MatrixPin(BranchEq, PosNode),
                new MatrixPin(BranchEq, BranchEq));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexMatrixElements?.Destroy();
            ComplexMatrixElements = null;
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
            var val = ComplexState.Laplace * BaseParameters.Inductance.Value;
            ComplexMatrixElements.Add(1, -1, -1, 1, -val);
        }
    }
}
