using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="CurrentControlledCurrentSource" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    public class BiasingBehavior : Behavior,
        IBiasingBehavior,
        IParameterized<BaseParameters>
    {
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _control;
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;

        /// <inheritdoc/>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the instantaneous current through the source.
        /// </summary>
        /// <value>
        /// The instantaneous current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_r"), ParameterInfo("Current")]
        public double Current => _control.Value * Parameters.Coefficient;

        /// <summary>
        /// Gets the instantaneous volage over the source.
        /// </summary>
        /// <value>
        /// The instantaneous voltage.
        /// </value>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// The instantaneous power dissipation by the source.
        /// </summary>
        /// <value>
        /// The instantaneous power dissipation.
        /// </value>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior" /> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The context for the behavior.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name" />, <paramref name="context" />, the current
        /// <see cref="ISolverSimulationState{T}.Solver" /> or <see cref="ISolverSimulationState{T}.Map" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the simulation does not implement an <see cref="IBiasingSimulationState" />, or
        /// if the controlling source does not define an <see cref="IBranchedBehavior{T}" /></exception>
        /// <exception cref="NodeMismatchException">Thrown if <paramref name="context" /> does not define exactly 2 nodes.</exception>
        public BiasingBehavior(string name, ICurrentControlledBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);
            Parameters = context.GetParameterSet<BaseParameters>();
            _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(_biasing, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;

            var pos = _biasing.Map[_variables.Positive];
            var neg = _biasing.Map[_variables.Negative];
            var br = _biasing.Map[_control];
            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br));
        }

        void IBiasingBehavior.Load()
        {
            var value = Parameters.Coefficient;
            _elements.Add(value, -value);
        }
    }
}
