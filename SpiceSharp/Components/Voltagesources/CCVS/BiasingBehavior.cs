using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the voltage biasing behavior.
        /// </summary>
        protected VoltageSourceBehaviors.BiasingBehavior VoltageLoad { get; private set; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterInfo("Output current")]
        public double GetCurrent() => _state.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Output voltage")]
        public double GetVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower() => _state.ThrowIfNotBound(this).Solution[BranchEq] * (_state.Solution[PosNode] - _state.Solution[NegNode]);

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the controlling branch equation row.
        /// </summary>
        protected int ContBranchEq { get; private set; }

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
        /// Gets the (controlling branch, branch) element.
        /// </summary>
        protected MatrixElement<double> BranchControlBranchPtr { get; private set; }

        // Cache
        private BaseSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            // Get behaviors
            VoltageLoad = context.GetBehavior<VoltageSourceBehaviors.BiasingBehavior>("control");

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            _state = ((BaseSimulation)simulation).RealState;
            var solver = _state.Solver;
            var variables = simulation.Variables;

            // Create/get nodes
            ContBranchEq = VoltageLoad.BranchEq;
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            BranchControlBranchPtr = solver.GetMatrixElement(BranchEq, ContBranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlBranchPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            PosBranchPtr.Value += 1.0;
            BranchPosPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;
            BranchControlBranchPtr.Value -= BaseParameters.Coefficient;
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
