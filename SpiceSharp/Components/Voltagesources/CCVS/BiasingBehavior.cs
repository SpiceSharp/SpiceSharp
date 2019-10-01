using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Output current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double GetPower() => BiasingState.ThrowIfNotBound(this).Solution[BranchEq] * (BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode]);

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
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected BiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var c = (CommonBehaviors.ControlledBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];
            VoltageLoad = c.ControlBehaviors.GetValue<VoltageSourceBehaviors.BiasingBehavior>();
            ContBranchEq = VoltageLoad.BranchEq;
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            BranchEq = context.Variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            // Get matrix elements
            var solver = context.States.GetValue<BiasingSimulationState>().Solver;
            Elements = new ElementSet<double>(solver,
                new MatrixLocation(PosNode, BranchEq),
                new MatrixLocation(NegNode, BranchEq),
                new MatrixLocation(BranchEq, PosNode),
                new MatrixLocation(BranchEq, NegNode),
                new MatrixLocation(BranchEq, ContBranchEq));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BiasingState = null;
            Elements?.Destroy();
            Elements = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            Elements.Add(1, -1, 1, -1, -BaseParameters.Coefficient);
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
