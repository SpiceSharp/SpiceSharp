using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
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
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

        /// <summary>
        /// Gets the power dissipated by the inductor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            var v = BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode];
            return v * BiasingState.Solution[BranchEq];
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
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected RealMatrixElementSet MatrixElements { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
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

            // Get parameters.
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();

            var c = (ComponentBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];
            BranchEq = context.Variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            BiasingState = context.States.GetValue<BiasingSimulationState>();
            MatrixElements = new RealMatrixElementSet(BiasingState.Solver,
                new MatrixPin(PosNode, BranchEq),
                new MatrixPin(NegNode, BranchEq),
                new MatrixPin(BranchEq, NegNode),
                new MatrixPin(BranchEq, PosNode));
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
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            MatrixElements.Add(1, -1, -1, 1);
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
