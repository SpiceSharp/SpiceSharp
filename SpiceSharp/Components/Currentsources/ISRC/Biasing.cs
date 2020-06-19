﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using IndependentSourceParameters = SpiceSharp.Components.CommonBehaviors.IndependentSourceParameters;

namespace SpiceSharp.Components.CurrentSources
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentSource" />.
    /// </summary>
    /// <remarks>
    /// This behavior also includes transient behavior logic. When transient analysis is
    /// performed, then waveforms need to be used to calculate the operating point anyway.
    /// </remarks>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="IndependentSourceParameters"/>
    public class Biasing : Behavior,
        IBiasingBehavior,
        IParameterized<IndependentSourceParameters>
    {
        private readonly IBiasingSimulationState _biasing;
        private readonly IIntegrationMethod _method;
        private readonly IIterationSimulationState _iteration;
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;

        /// <inheritdoc/>
        public IndependentSourceParameters Parameters { get; }

        /// <summary>
        /// Gets the waveform.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        public IWaveform Waveform { get; }

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage accross the supply")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power supplied by the source")]
        public double Power => (_variables.Positive.Value - _variables.Negative.Value) * -Current;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("c"), ParameterName("i"), ParameterName("i_r"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Biasing(string name, IComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<IndependentSourceParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _iteration = context.GetState<IIterationSimulationState>();
            _variables = new OnePort<double>(_biasing, context);

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

            _elements = new ElementSet<double>(_biasing.Solver, null, _variables.GetRhsIndices(_biasing.Map));
        }

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