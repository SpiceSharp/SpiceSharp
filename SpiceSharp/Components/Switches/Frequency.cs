using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Frequency behavior for switches.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IFrequencyBehavior" />
    [BehaviorFor(typeof(CurrentSwitch))]
    [BehaviorFor(typeof(VoltageSwitch))]
    [AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <include file='../Common/docs.xml' path='docs/members[@name="Frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='../Common/docs.xml' path='docs/members[@name="Frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent
        {
            get
            {
                double gNow = CurrentState ? ModelTemperature.OnConductance : ModelTemperature.OffConductance;
                return ComplexVoltage * gNow;
            }
        }

        /// <include file='../Common/docs.xml' path='docs/members[@name="Frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("The complex power")]
        public Complex ComplexPower => ComplexPower * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(ISwitchBindingContext context)
            : base(context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(state.GetSharedVariable(context.Nodes[0]), state.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<Complex>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            // Get the current state
            double gNow = CurrentState ? ModelTemperature.OnConductance : ModelTemperature.OffConductance;
            gNow *= Parameters.ParallelMultiplier;

            // Load the Y-matrix
            _elements.Add(gNow, -gNow, -gNow, gNow);
        }
    }
}
