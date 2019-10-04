using SpiceSharp.Entities;
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
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power supplied by the source")]
        public double GetPower() => (BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[PosNode]) * -Current;

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("c"), ParameterName("i"), ParameterName("i_r"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// The positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// The negative index.
        /// </summary>
        protected int NegNode { get; private set; }

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

            var c = (ComponentBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];

            BaseParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();
            context.States.TryGetValue(out _timeState);
            BaseParameters.Waveform?.Bind(context);

            // Give some warnings if no value is given
            if (!BaseParameters.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                CircuitWarning.Warning(this,
                    BaseParameters.Waveform != null
                        ? "{0} has no DC value, transient time 0 value used".FormatString(Name)
                        : "{0} has no value, DC 0 assumed".FormatString(Name));
            }

            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            Elements = new ElementSet<double>(BiasingState.Solver, null, new[] { PosNode, NegNode });
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
