using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Frequency behavior for switches.
    /// </summary>
    public class Frequency : Biasing, IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="Frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="Frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent
        {
            get
            {
                var gNow = CurrentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;
                return ComplexVoltage * gNow;
            }
        }

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="Frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("The complex power")]
        public Complex ComplexPower => ComplexPower * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public Frequency(string name, ComponentBindingContext context, Controller controller) : base(name, context, controller)
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
            var gNow = CurrentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;

            // Load the Y-matrix
            _elements.Add(gNow, -gNow, -gNow, gNow);
        }
    }
}
