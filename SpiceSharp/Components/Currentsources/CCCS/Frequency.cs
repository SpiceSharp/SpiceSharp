﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.CurrentControlledCurrentSources
{
    /// <summary>
    /// Frequency behavior for <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly OnePort<Complex> _variables;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly IVariable<Complex> _control;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => _control.Value * Parameters.Coefficient;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, CurrentControlledBindingContext context)
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