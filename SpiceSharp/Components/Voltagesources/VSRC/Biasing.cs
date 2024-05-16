using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.VoltageSources
{
    /// <summary>
    /// Biasing behavior for <see cref="VoltageSource"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="IndependentSourceParameters"/>
    [BehaviorFor(typeof(VoltageSource)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>,
        IParameterized<IndependentSourceParameters>
    {
        private readonly IIntegrationMethod _method;
        private readonly IIterationSimulationState _iteration;
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly IBiasingSimulationState _biasing;

        /// <inheritdoc/>
        public IndependentSourceParameters Parameters { get; }

        /// <summary>
        /// Gets the waveform.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        protected IWaveform Waveform { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Voltage source current")]
        public double Current => Branch.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Instantaneous power")]
        public double Power => Voltage * -Branch.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.TryGetState<IIntegrationMethod>(out _method);
            Parameters = context.GetParameterSet<IndependentSourceParameters>();
            _iteration = context.GetState<IIterationSimulationState>();
            Waveform = Parameters.Waveform?.Create(context);
            if (!Parameters.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (Waveform != null)
                {
                    SpiceSharpWarning.Warning(this,
                        Properties.Resources.IndependentSources_NoDcUseWaveform.FormatString(Name));
                    Parameters.DcValue = new GivenParameter<double>(Waveform.Value, false);
                }
                else
                {
                    SpiceSharpWarning.Warning(this,
                        Properties.Resources.IndependentSources_NoDc.FormatString(Name));
                }
            }

            // Connections
            _biasing = context.GetState<IBiasingSimulationState>();

            _variables = new OnePort<double>(_biasing, context);
            Branch = _biasing.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            int pos = _biasing.Map[_variables.Positive];
            int neg = _biasing.Map[_variables.Negative];
            int br = _biasing.Map[Branch];

            _elements = new ElementSet<double>(_biasing.Solver, [
                        new MatrixLocation(pos, br),
                        new MatrixLocation(br, pos),
                        new MatrixLocation(neg, br),
                        new MatrixLocation(br, neg)
                    ], [br]);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double value;

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
                value = Parameters.DcValue * _iteration.SourceFactor;
            }

            Voltage = value;
            _elements.Add(1, 1, -1, -1, value);
        }
    }
}