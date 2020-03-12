using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using IndependentSourceParameters = SpiceSharp.Components.CommonBehaviors.IndependentSourceParameters;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentSource" />.
    /// </summary>
    /// <remarks>
    /// This behavior also includes transient behavior logic. When transient analysis is
    /// performed, then waveforms need to be used to calculate the operating point anyway.
    /// </remarks>
    public class BiasingBehavior : Behavior, IBiasingBehavior,
        IParameterized<IndependentSourceParameters>
    {
        private readonly IBiasingSimulationState _biasing;
        private readonly IIntegrationMethod _method;
        private readonly IIterationSimulationState _iteration;
        private readonly int _posNode, _negNode;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public IndependentSourceParameters Parameters { get; }

        /// <summary>
        /// Gets the waveform.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        public IWaveform Waveform { get; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage accross the supply")]
        public double GetVoltage() => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power supplied by the source")]
        public double GetPower() => (_biasing.Solution[_posNode] - _biasing.Solution[_posNode]) * -Current;

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterName("i"), ParameterName("i_r"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            Parameters = context.GetParameterSet<IndependentSourceParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _iteration = context.GetState<IIterationSimulationState>();
            _posNode = _biasing.Map[context.Nodes[0]];
            _negNode = _biasing.Map[context.Nodes[1]];

            context.TryGetState(out _method);
            Waveform = Parameters.Waveform?.Create(_method);

            // Give some warnings if no value is given
            if (!Parameters.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                SpiceSharpWarning.Warning(this,
                    Waveform != null
                        ? Properties.Resources.IndependentSources_NoDcUseWaveform.FormatString(Name)
                        : Properties.Resources.IndependentSources_NoDc.FormatString(Name));
            }

            _elements = new ElementSet<double>(_biasing.Solver, null, new[] { _posNode, _negNode });
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            double value;

            // Time domain analysis
            if (_method != null)
            {
                // Use the waveform if possible
                if (Waveform != null)
                    value = Waveform.Value;
                else
                    value = Parameters.DcValue * _iteration.SourceFactor;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = Parameters.DcValue * _iteration.SourceFactor;
            }

            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            _elements.Add(-value, value);
            Current = value;
        }
    }
}
