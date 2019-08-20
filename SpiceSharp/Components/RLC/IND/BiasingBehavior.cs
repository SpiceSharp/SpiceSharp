using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Inductor"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the branch equation index.
        /// </summary>
        public int BranchEq { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent() => State.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => State.ThrowIfNotBound(this).Solution[PosNode] - State.Solution[NegNode];

        /// <summary>
        /// Gets the power dissipated by the inductor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower()
        {
            State.ThrowIfNotBound(this);
            var v = State.Solution[PosNode] - State.Solution[NegNode];
            return v * State.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<double> PosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> NegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> BranchNegPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<double> BranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The provider.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters.
            BaseParameters = context.GetParameterSet<BaseParameters>();

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            State = ((BaseSimulation)simulation).RealState;
            var solver = State.Solver;
            var variables = simulation.Variables;
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
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
            BranchNegPtr = null;
            BranchPosPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            PosBranchPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchPosPtr.Value += 1;
            BranchNegPtr.Value -= 1;
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
