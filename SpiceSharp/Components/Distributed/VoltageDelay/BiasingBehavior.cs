using SpiceSharp.Algebra;
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
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<double> PosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> NegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<double> BranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> BranchNegPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, ctrlpos) element.
        /// </summary>
        protected MatrixElement<double> BranchControlPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, ctrlneg) element.
        /// </summary>
        protected MatrixElement<double> BranchControlNegPtr { get; private set; }

        /// <summary>
        /// Gets the real state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

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
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The provider.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
                ContPosNode = cc.Pins[2];
                ContNegNode = cc.Pins[3];
            }

            State = ((BaseSimulation)simulation).RealState;
            var solver = State.Solver;
            var variables = simulation.Variables;
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            BranchControlPosPtr = solver.GetMatrixElement(BranchEq, ContPosNode);
            BranchControlNegPtr = solver.GetMatrixElement(BranchEq, ContNegNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            State = null;
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlPosPtr = null;
            BranchControlNegPtr = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            // In DC, the delay should just copy input to output
            if (State.UseDc)
            {
                BranchControlPosPtr.Value -= 1.0;
                BranchControlNegPtr.Value += 1.0;
            }
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
