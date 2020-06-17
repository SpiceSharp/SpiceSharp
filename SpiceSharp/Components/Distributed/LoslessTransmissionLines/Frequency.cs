﻿using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Biasing,
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
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, IComponentBindingContext context)
            : base(name, context)
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

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var laplace = _complex.Laplace;
            var factor = Complex.Exp(-laplace * Parameters.Delay.Value);
            var y = Parameters.Admittance;
            _elements.Add(
                y, -y, -1, y, -1,
                -y, y, 1, y, 1, -1, -factor,
                factor, 1, -factor * Parameters.Impedance,
                -factor, factor, -1, 1, -factor * Parameters.Impedance,
                -y, -y);
        }
    }
}