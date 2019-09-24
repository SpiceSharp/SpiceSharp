using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
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
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            ComplexMatrixElements = new ComplexMatrixElementSet(ComplexState.Solver,
                new MatrixPin(Pos1, Pos1),
                new MatrixPin(Pos1, Internal1),
                new MatrixPin(Neg1, BranchEq1),
                new MatrixPin(Pos2, Pos2),
                new MatrixPin(Neg2, BranchEq2),
                new MatrixPin(Internal1, Pos1),
                new MatrixPin(Internal1, Internal1),
                new MatrixPin(Internal1, BranchEq1),
                new MatrixPin(Internal2, Internal2),
                new MatrixPin(Internal2, BranchEq2),
                new MatrixPin(BranchEq1, Neg1),
                new MatrixPin(BranchEq1, Pos2),
                new MatrixPin(BranchEq1, Neg2),
                new MatrixPin(BranchEq1, Internal1),
                new MatrixPin(BranchEq1, BranchEq2),
                new MatrixPin(BranchEq2, Pos1),
                new MatrixPin(BranchEq2, Neg1),
                new MatrixPin(BranchEq2, Neg2),
                new MatrixPin(BranchEq2, Internal2),
                new MatrixPin(BranchEq2, BranchEq1),
                new MatrixPin(Pos2, Internal2),
                new MatrixPin(Internal2, Pos2));
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
        /// Initializes the parameters.
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
            var factor = Complex.Exp(-laplace * BaseParameters.Delay.Value);
            var y = BaseParameters.Admittance;
            ComplexMatrixElements.Add(
                y, -y, -1, y, -1,
                -y, y, 1, y, 1, -1, -factor,
                factor, 1, -factor * BaseParameters.Impedance,
                -factor, factor, -1, 1, -factor * BaseParameters.Impedance,
                -y, -y);
        }
    }
}
