﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Frequency behavior for <see cref="Inductor"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    public class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;
        private readonly OnePort<Complex> _variables;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterName("The complex power")]
        public Complex ComplexPower => -Branch.Value * (_variables.Positive.Value - _variables.Negative.Value);

        /// <inheritdoc/>
        public new IVariable<Complex> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, IComponentBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            var pos = _complex.Map[_variables.Positive];
            var neg = _complex.Map[_variables.Negative];
            var br = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, br));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var val = _complex.Laplace * Inductance;
            _elements.Add(1, -1, -1, 1, -val);
        }
    }
}