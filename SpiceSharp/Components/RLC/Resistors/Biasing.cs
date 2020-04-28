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

        /// <summary>
        /// Gets the instantaneous voltage across the resistor.
        /// </summary>
        /// <value>
        /// The instantaneous voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the instantaneous current through the resistor.
        /// </summary>
        /// <value>
        /// The instantaneous current.
        /// </value>
        [ParameterName("i"), ParameterInfo("Current")]
        public double Current => Voltage * Conductance;

        /// <summary>
        /// Gets the instantaneous power dissipated by the resistor.
        /// </summary>
        /// <value>
        /// The instantaneous power dissipation.
        /// </value>
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
