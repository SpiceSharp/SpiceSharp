using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Frequency behavior for switches.
    /// </summary>
    public class FrequencyBehavior : Biasing, IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context, Controller controller) : base(name, context, controller)
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(state.GetSharedVariable(context.Nodes[0]), state.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<Complex>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            // Get the current state
            var currentState = CurrentState;
            var gNow = currentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;

            // Load the Y-matrix
            _elements.Add(gNow, -gNow, -gNow, gNow);
        }
    }
}
