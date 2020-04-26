using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.General;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly OnePort<Complex> _variables;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly IVariable<Complex> _control;

        /// <summary>
        /// Get the complex voltage over the source.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage =>_variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Get the complex current through the source.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => _control.Value * Parameters.Coefficient;

        /// <summary>
        /// Get the complex power dissipated by the source.
        /// </summary>
        /// <value>
        /// The complex power.
        /// </value>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the simulation does not use an <see cref="IComplexSimulationState"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the controlling entity does not have a behavior of type <see cref="IBranchedBehavior{T}"/>.</exception>
        /// <exception cref="AmbiguousTypeException">Thrown if another's entity behavior could not be resolved unambiguously.</exception>
        public FrequencyBehavior(string name, CurrentControlledBindingContext context) 
            : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            _variables = new OnePort<Complex>(_complex, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Branch;

            var pos = _complex.Map[_variables.Positive];
            var neg = _complex.Map[_variables.Negative];
            var br = _complex.Map[_control];
            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var value = Parameters.Coefficient;
            _elements.Add(value, -value);
        }
    }
}
