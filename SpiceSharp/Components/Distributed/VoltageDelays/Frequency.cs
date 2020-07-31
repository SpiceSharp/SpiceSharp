using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(VoltageDelay), typeof(IFrequencyBehavior), 1)]
    public class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <inheritdoc/>
        public new IVariable<Complex> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            _posNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[0])];
            _negNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[1])];
            _contPosNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[2])];
            _contNegNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[3])];
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            _branchEq = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(_complex.Solver, new[] {
                        new MatrixLocation(_posNode, _branchEq),
                        new MatrixLocation(_negNode, _branchEq),
                        new MatrixLocation(_branchEq, _posNode),
                        new MatrixLocation(_branchEq, _negNode),
                        new MatrixLocation(_branchEq, _contPosNode),
                        new MatrixLocation(_branchEq, _contNegNode)
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