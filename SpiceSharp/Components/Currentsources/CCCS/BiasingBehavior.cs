using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// The <see cref="VoltageSourceBehaviors.BiasingBehavior"/> that handles the controlling voltage source current.
        /// </summary>
        protected VoltageSourceBehaviors.BiasingBehavior VoltageLoad { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public int ControlBranchEq { get; protected set; }

        /// <summary>
        /// The positive node index.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// The negative node index.
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
        /// Device methods and properties
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[ControlBranchEq] * BaseParameters.Coefficient;

        /// <summary>
        /// Gets the volage over the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

        /// <summary>
        /// The power dissipation by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            return (BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode]) * BiasingState.Solution[ControlBranchEq] * BaseParameters.Coefficient;
        }

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
        /// Bind behavior.
        /// </summary>
        /// <param name="context">Data provider</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();

            var c = (CommonBehaviors.ControlledBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];
            VoltageLoad = c.ControlBehaviors.GetValue<VoltageSourceBehaviors.BiasingBehavior>();
            ControlBranchEq = VoltageLoad.BranchEq;

            // Get matrix elements
            BiasingState = context.States.GetValue<BiasingSimulationState>();
            MatrixElements = new RealMatrixElementSet(BiasingState.Solver,
                new MatrixPin(PosNode, ControlBranchEq),
                new MatrixPin(NegNode, ControlBranchEq)
            );
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
            var value = BaseParameters.Coefficient.Value;
            MatrixElements.Add(value, -value);
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent() => true;
    }
}
