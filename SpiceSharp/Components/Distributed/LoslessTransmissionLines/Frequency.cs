using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    [BehaviorFor(typeof(LosslessTransmissionLine)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly int _pos1, _neg1, _pos2, _neg2, _int1, _int2, _br1, _br2;
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;

        /// <summary>
        /// Gets the left-side internal node.
        /// </summary>
        /// <value>
        /// The left internal node.
        /// </value>
        protected new IVariable<Complex> Internal1 { get; }

        /// <summary>
        /// Gets the right-side internal node.
        /// </summary>
        /// <value>
        /// The right internal node.
        /// </value>
        protected new IVariable<Complex> Internal2 { get; }

        /// <summary>
        /// Gets the left-side branch.
        /// </summary>
        /// <value>
        /// The left branch.
        /// </value>
        protected new IVariable<Complex> Branch1 { get; }

        /// <summary>
        /// Gets the right-side branch.
        /// </summary>
        /// <value>
        /// The right branch.
        /// </value>
        protected new IVariable<Complex> Branch2 { get; }

        /// <summary>
        /// Gets the voltage on side 1.
        /// </summary>
        /// <value>
        /// The voltage on side 1.
        /// </value>
        [ParameterName("v1"), ParameterName("v1_r"), ParameterInfo("Voltage 1")]
        public Complex ComplexVoltage1 => _complex.Solution[_pos1] - _complex.Solution[_neg1];

        /// <summary>
        /// Gets the voltage on side 2.
        /// </summary>
        /// <value>
        /// The voltage on side 2.
        /// </value>
        [ParameterName("v2"), ParameterName("v2_r"), ParameterInfo("Voltage 2")]
        public Complex ComplexVoltage2 => _complex.Solution[_pos2] - _complex.Solution[_neg2];

        /// <summary>
        /// Gets the current on side 1.
        /// </summary>
        /// <value>
        /// The current on side 1.
        /// </value>
        [ParameterName("i1"), ParameterName("c1"), ParameterName("i1_r"), ParameterInfo("Current 1")]
        public Complex ComplexCurrent1 => _complex.Solution[_br1];

        /// <summary>
        /// Gets the current on side 2.
        /// </summary>
        /// <value>
        /// The current on side 2.
        /// </value>
        [ParameterName("i2"), ParameterName("c2"), ParameterName("i2_r"), ParameterInfo("Current 2")]
        public Complex ComplexCurrent2 => _complex.Solution[_br2];

        /// <summary>
        /// Gets the power on side 1.
        /// </summary>
        /// <value>
        /// The power on side 1.
        /// </value>
        [ParameterName("p1"), ParameterName("p1_r"), ParameterInfo("Power 1")]
        public Complex ComplexPower1 => -Voltage1 * Current1;

        /// <summary>
        /// Gets the power on side 2.
        /// </summary>
        /// <value>
        /// The power on side 2.
        /// </value>
        [ParameterName("p2"), ParameterName("p2_r"), ParameterInfo("Power 2")]
        public Complex ComplexPower2 => -Voltage1 * Current1;


        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            _pos1 = _complex.Map[_complex.GetSharedVariable(context.Nodes[0])];
            _neg1 = _complex.Map[_complex.GetSharedVariable(context.Nodes[1])];
            _pos2 = _complex.Map[_complex.GetSharedVariable(context.Nodes[2])];
            _neg2 = _complex.Map[_complex.GetSharedVariable(context.Nodes[3])];

            Internal1 = _complex.CreatePrivateVariable(Name.Combine("int1"), Units.Volt);
            _int1 = _complex.Map[Internal1];
            Internal2 = _complex.CreatePrivateVariable(Name.Combine("int2"), Units.Volt);
            _int2 = _complex.Map[Internal2];
            Branch1 = _complex.CreatePrivateVariable(Name.Combine("branch1"), Units.Ampere);
            _br1 = _complex.Map[Branch1];
            Branch2 = _complex.CreatePrivateVariable(Name.Combine("branch2"), Units.Ampere);
            _br2 = _complex.Map[Branch2];

            _elements = new ElementSet<Complex>(_complex.Solver,
                new MatrixLocation(_pos1, _pos1),
                new MatrixLocation(_pos1, _int1),
                new MatrixLocation(_neg1, _br1),
                new MatrixLocation(_pos2, _pos2),
                new MatrixLocation(_neg2, _br2),
                new MatrixLocation(_int1, _pos1),
                new MatrixLocation(_int1, _int1),
                new MatrixLocation(_int1, _br1),
                new MatrixLocation(_int2, _int2),
                new MatrixLocation(_int2, _br2),
                new MatrixLocation(_br1, _neg1),
                new MatrixLocation(_br1, _pos2),
                new MatrixLocation(_br1, _neg2),
                new MatrixLocation(_br1, _int1),
                new MatrixLocation(_br1, _br2),
                new MatrixLocation(_br2, _pos1),
                new MatrixLocation(_br2, _neg1),
                new MatrixLocation(_br2, _neg2),
                new MatrixLocation(_br2, _int2),
                new MatrixLocation(_br2, _br1),
                new MatrixLocation(_pos2, _int2),
                new MatrixLocation(_int2, _pos2));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var laplace = _complex.Laplace;
            var factor = Complex.Exp(-laplace * Parameters.Delay.Value);
            double y = Parameters.Admittance * Parameters.ParallelMultiplier;
            double z = Parameters.Impedance / Parameters.ParallelMultiplier;
            _elements.Add(
                y, -y, -1, y, -1,
                -y, y, 1, y, 1, -1, -factor,
                factor, 1, -factor * z,
                -factor, factor, -1, 1, -factor * z,
                -y, -y);
        }
    }
}