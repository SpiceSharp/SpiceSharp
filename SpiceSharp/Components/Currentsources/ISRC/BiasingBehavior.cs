using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentSource" />.
    /// </summary>
    /// <remarks>
    /// This behavior also includes transient behavior logic. When transient analysis is
    /// performed, then waveforms need to be used to calculate the operating point anyway.
    /// </remarks>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected CommonBehaviors.IndependentSourceParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage accross the supply")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power supplied by the source")]
        public double GetPower() => (BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_posNode]) * -Current;

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterName("i"), ParameterName("i_r"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Gets the vector elements.
        /// </summary>
        /// <value>
        /// The vector elements.
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
        private int _posNode, _negNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            BiasingState = context.GetState<IBiasingSimulationState>();
            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];

            BaseParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();
            context.TryGetState(out _timeState);
            BaseParameters.Waveform?.Bind(context);

            // Give some warnings if no value is given
            if (!BaseParameters.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                SpiceSharpWarning.Warning(this,
                    BaseParameters.Waveform != null
                        ? Properties.Resources.IndependentSources_NoDcUseWaveform.FormatString(Name)
                        : Properties.Resources.IndependentSources_NoDc.FormatString(Name));
            }

            Elements = new ElementSet<double>(BiasingState.Solver, null, new[] { _posNode, _negNode });
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            double value;

            // Time domain analysis
            if (_timeState != null)
            {
                // Use the waveform if possible
                if (BaseParameters.Waveform != null)
                    value = BaseParameters.Waveform.Value;
                else
                    value = BaseParameters.DcValue * BiasingState.SourceFactor;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = BaseParameters.DcValue * BiasingState.SourceFactor;
            }

            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            Elements.Add(-value, value);
            Current = value;
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
