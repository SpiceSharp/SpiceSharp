using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        public Variable ControlBranch { get; protected set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[_brNode] * BaseParameters.Coefficient;

        /// <summary>
        /// Gets the volage over the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// The power dissipation by the source.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            return (BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode]) * BiasingState.Solution[_brNode] * BaseParameters.Coefficient;
        }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _posNode, _negNode, _brNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
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

            // Connections
            var c = (CommonBehaviors.ControlledBindingContext)context;
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            c.Nodes.ThrowIfNot("nodes", 2);
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            VoltageLoad = c.ControlBehaviors.GetValue<VoltageSourceBehaviors.BiasingBehavior>();
            ControlBranch = VoltageLoad.Branch;
            _brNode = BiasingState.Map[ControlBranch];
            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode));
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
            var value = BaseParameters.Coefficient.Value;
            Elements.Add(value, -value);
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
