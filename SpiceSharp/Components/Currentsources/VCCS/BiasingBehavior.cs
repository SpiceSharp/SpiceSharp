using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="VoltageControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double GetCurrent() => (BiasingState.ThrowIfNotBound(this).Solution[_contPosNode] - BiasingState.Solution[_contNegNode]) * BaseParameters.Coefficient;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            var v = BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];
            var i = (BiasingState.Solution[_contPosNode] - BiasingState.Solution[_contNegNode]) * BaseParameters.Coefficient;
            return -v * i;
        }

        /// <summary>
        /// Gets the state of the biasing.
        /// </summary>
        /// <value>
        /// The state of the biasing.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _posNode, _negNode, _contPosNode, _contNegNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
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
            
            // Connections
            var c = (ComponentBindingContext)context;
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            c.Nodes.ThrowIfNot("nodes", 4);
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            _contPosNode = BiasingState.Map[c.Nodes[2]];
            _contNegNode = BiasingState.Map[c.Nodes[3]];
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _contPosNode),
                new MatrixLocation(_posNode, _contNegNode),
                new MatrixLocation(_negNode, _contPosNode),
                new MatrixLocation(_negNode, _contNegNode)
            });
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
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var value = BaseParameters.Coefficient.Value;
            Elements.Add(value, -value, -value, value);
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
