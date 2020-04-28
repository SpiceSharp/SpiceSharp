using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    public partial class Inductor
    {
        /// <summary>
        /// DC biasing behavior for an <see cref="Inductor" />
        /// </summary>
        /// <seealso cref="TemperatureBehavior"/>
        /// <seealso cref="IBiasingBehavior" />
        /// <seealso cref="IBranchedBehavior{T}" />
        protected class BiasingBehavior : TemperatureBehavior,
            IBiasingBehavior, 
            IBranchedBehavior<double>
        {
            private readonly ElementSet<double> _elements;
            private readonly OnePort<double> _variables;

            /// <inheritdoc/>
            public IVariable<double> Branch { get; }

            /// <summary>
            /// Gets the instantaneous current.
            /// </summary>
            /// <value>
            /// The instantaneous current.
            /// </value>
            [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
            public double Current => Branch.Value;

            /// <summary>
            /// Gets the instantaneous voltage.
            /// </summary>
            /// <value>
            /// The instantaneous voltage.
            /// </value>
            [ParameterName("v"), ParameterInfo("Voltage")]
            public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

            /// <summary>
            /// Gets the power dissipated by the inductor.
            /// </summary>
            /// <value>
            /// The instantaneous power.
            /// </value>
            [ParameterName("p"), ParameterInfo("Power")]
            public double Power => -Voltage * Current;

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
                Branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

                var pos = state.Map[_variables.Positive];
                var neg = state.Map[_variables.Negative];
                var br = state.Map[Branch];
                _elements = new ElementSet<double>(state.Solver,
                    new MatrixLocation(pos, br),
                    new MatrixLocation(neg, br),
                    new MatrixLocation(br, neg),
                    new MatrixLocation(br, pos));
            }

            /// <summary>
            /// Loads the Y-matrix and right hand side vector.
            /// </summary>
            public virtual void Load()
            {
                _elements.Add(1, -1, -1, 1);
            }
        }
    }
}