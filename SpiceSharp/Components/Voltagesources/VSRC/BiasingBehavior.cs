using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected CommonBehaviors.IndependentSourceParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("i_r"), ParameterInfo("Voltage source current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[_brNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Instantaneous power")]
        public double GetPower() => (BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode]) * -BiasingState.Solution[_brNode];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public Variable Branch { get; private set; }

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
        protected IBiasingSimulationState BiasingState { get; private set; }

        private ITimeSimulationState _timeState;
        private int _posNode, _negNode, _brNode;

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

            BaseParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();
            BaseParameters.Waveform?.Bind(context);

            if (!BaseParameters.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (BaseParameters.Waveform != null)
                {
                    CircuitWarning.Warning(this, "{0}: No DC value, transient time 0 value used".FormatString(Name));
                    BaseParameters.DcValue.RawValue = BaseParameters.Waveform.Value;
                }
                else
                {
                    CircuitWarning.Warning(this, "{0}: No value, DC 0 assumed".FormatString(Name));
                }
            }

            // Connections
            var c = (ComponentBindingContext)context;
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            context.States.TryGetValue(out _timeState);
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _brNode = BiasingState.Map[Branch];
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _brNode),
                new MatrixLocation(_brNode, _posNode),
                new MatrixLocation(_negNode, _brNode),
                new MatrixLocation(_brNode, _negNode)
            }, new[] { _brNode });
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
            var state = BiasingState.ThrowIfNotBound(this);
            double value;

            if (_timeState != null)
            {
                // Use the waveform if possible
                if (BaseParameters.Waveform != null)
                    value = BaseParameters.Waveform.Value;
                else
                    value = BaseParameters.DcValue * state.SourceFactor;
            }
            else
            {
                value = BaseParameters.DcValue * state.SourceFactor;
            }

            Voltage = value;
            Elements.Add(1, 1, -1, -1, value);
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
