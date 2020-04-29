using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Biasing behavior for <see cref="Resistor"/>.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    public class Biasing : Temperature, 
        IBiasingBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterInfo("Current")]
        public double Current => Voltage * Conductance;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("Power")]
        public double Power
        {
            get
            {
                var v = Voltage;
                return v * v * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Biasing(string name, ComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(2);
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state, context);
            _elements = new ElementSet<double>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        void IBiasingBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
