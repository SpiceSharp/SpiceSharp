using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected CommonBehaviors.IndependentSourceParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the waveform.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        protected IWaveform Waveform { get; }

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

        private IIntegrationMethod _method;
        private int _posNode, _negNode, _brNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            BaseParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();

            context.TryGetState(out _method);
            if (context.Behaviors.Parameters.TryGetValue(out IWaveformDescription wdesc))
                Waveform = wdesc.Create(_method);
            if (!BaseParameters.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (Waveform != null)
                {
                    SpiceSharpWarning.Warning(this, 
                        Properties.Resources.IndependentSources_NoDcUseWaveform.FormatString(Name));
                    BaseParameters.DcValue.RawValue = Waveform.Value;
                }
                else
                {
                    SpiceSharpWarning.Warning(this, 
                        Properties.Resources.IndependentSources_NoDc.FormatString(Name));
                }
            }

            // Connections
            BiasingState = context.GetState<IBiasingSimulationState>();
            context.TryGetState(out _method);
            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
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
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var state = BiasingState.ThrowIfNotBound(this);
            double value;

            if (_method != null)
            {
                // Use the waveform if possible
                if (Waveform != null)
                    value = Waveform.Value;
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
    }
}
