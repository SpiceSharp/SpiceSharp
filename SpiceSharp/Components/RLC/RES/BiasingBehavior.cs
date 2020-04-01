using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Resistor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;

        /// <summary>
        /// Gets the voltage across the resistor.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the current through the resistor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Current")]
        public double Current => Voltage * Conductance;

        /// <summary>
        /// Gets the power dissipated by the resistor.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(2);
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state, context);
            _elements = new ElementSet<double>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            _elements.Add(Conductance, -Conductance, -Conductance, Conductance);
        }
    }
}
