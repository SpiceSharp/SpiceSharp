using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

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
        /// Nodes
        /// </summary>
        public Variable ControlBranch { get; private set; }

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
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ControlledBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.ThrowIfNot("nodes", 2);

            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
            var load = context.ControlBehaviors.GetValue<VoltageSourceBehaviors.BiasingBehavior>();
            ControlBranch = load.Branch;
            _brNode = BiasingState.Map[ControlBranch];
            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_negNode, _brNode));
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
