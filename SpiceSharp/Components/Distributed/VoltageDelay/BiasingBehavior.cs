using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the controlling positive node.
        /// </summary>
        protected int ContPosNode { get; private set; }

        /// <summary>
        /// Gets the controlling negative node.
        /// </summary>
        protected int ContNegNode { get; private set; }

        /// <summary>
        /// Gets the branch equation row.
        /// </summary>
        public int BranchEq { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected RealMatrixElementSet MatrixElements { get; private set; }

        /// <summary>
        /// Gets the real state.
        /// </summary>
        protected BiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public BiasingBehavior(string name)
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
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            var c = (ComponentBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];
            ContPosNode = c.Pins[2];
            ContNegNode = c.Pins[3];
            BranchEq = context.Variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            BiasingState = context.States.GetValue<BiasingSimulationState>();
            MatrixElements = new RealMatrixElementSet(BiasingState.Solver,
                new MatrixPin(PosNode, BranchEq),
                new MatrixPin(NegNode, BranchEq),
                new MatrixPin(BranchEq, PosNode),
                new MatrixPin(BranchEq, NegNode),
                new MatrixPin(BranchEq, ContPosNode),
                new MatrixPin(BranchEq, ContNegNode));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BiasingState = null;
            MatrixElements?.Destroy();
            MatrixElements = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (BiasingState.UseDc)
                MatrixElements.Add(1, -1, 1, -1, -1, 1);
            else
                MatrixElements.Add(1, -1, 1, -1);
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
