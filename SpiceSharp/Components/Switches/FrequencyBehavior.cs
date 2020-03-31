using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Frequency behavior for switches.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly int _posNode, _negNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _posNode = state.Map[state.GetSharedVariable(context.Nodes[0])];
            _negNode = state.Map[state.GetSharedVariable(context.Nodes[1])];
            _elements = new ElementSet<Complex>(state.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
            });
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
