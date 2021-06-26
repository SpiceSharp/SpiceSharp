using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;
using SpiceSharp.Attributes;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(VoltageDelay)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly TwoPort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;

        /// <inheritdoc/>
        public new IVariable<Complex> Branch { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Right.Positive.Value - _variables.Right.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("Complex power")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Branch.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            _variables = new TwoPort<Complex>(_complex, context);
            int posNode = _complex.Map[_variables.Right.Positive];
            int negNode = _complex.Map[_variables.Right.Negative];
            int contPosNode = _complex.Map[_variables.Left.Positive];
            int contNegNode = _complex.Map[_variables.Left.Negative];
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            int branchEq = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                        new MatrixLocation(posNode, branchEq),
                        new MatrixLocation(negNode, branchEq),
                        new MatrixLocation(branchEq, posNode),
                        new MatrixLocation(branchEq, negNode),
                        new MatrixLocation(branchEq, contPosNode),
                        new MatrixLocation(branchEq, contNegNode)
                    });
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var laplace = _complex.Laplace;
            var factor = Complex.Exp(-laplace * Parameters.Delay);

            // Load the Y-matrix and RHS-vector
            _elements.Add(1, -1, 1, -1, -factor, factor);
        }
    }
}