﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.VoltageControlledVoltageSources
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    public class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly TwoPort<Complex> _variables;
        private readonly IComplexSimulationState _complex;
        private readonly ElementSet<Complex> _elements;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

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
            _variables = new TwoPort<Complex>(_complex, context);
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            var pos = _complex.Map[_variables.Right.Positive];
            var neg = _complex.Map[_variables.Right.Negative];
            var contPos = _complex.Map[_variables.Left.Positive];
            var contNeg = _complex.Map[_variables.Left.Negative];
            var br = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var value = Parameters.Coefficient;
            _elements.Add(1, -1, 1, -1, -value, value);
        }
    }
}
