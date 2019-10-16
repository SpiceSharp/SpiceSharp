using SpiceSharp.Entities;
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
        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        private int _posNode, _negNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public FrequencyBehavior(string name, Controller method) : base(name, method)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            var c = (ComponentBindingContext)context;
            var state = context.States.GetValue<IComplexSimulationState>();
            _posNode = state.Map[c.Nodes[0]];
            _negNode = state.Map[c.Nodes[1]];
            ComplexElements = new ElementSet<Complex>(state.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
            });
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexElements?.Destroy();
            ComplexElements = null;
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
            ComplexElements.ThrowIfNotBound(this);

            // Get the current state
            var currentState = CurrentState;
            var gNow = currentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;

            // Load the Y-matrix
            ComplexElements.Add(gNow, -gNow, -gNow, gNow);
        }
    }
}
