using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Frequency behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
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
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, IComponentBindingContext context)
            : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            _posNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[0])];
            _negNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[1])];
            _contPosNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[2])];
            _contNegNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[3])];
            Branch = _complex.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            _branchEq = _complex.Map[Branch];

            _elements = new ElementSet<Complex>(this._complex.Solver, new[] {
                        new MatrixLocation(_posNode, _branchEq),
                        new MatrixLocation(_negNode, _branchEq),
                        new MatrixLocation(_branchEq, _posNode),
                        new MatrixLocation(_branchEq, _negNode),
                        new MatrixLocation(_branchEq, _contPosNode),
                        new MatrixLocation(_branchEq, _contNegNode)
                    });
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var laplace = _complex.Laplace;
            var factor = Complex.Exp(-laplace * Parameters.Delay);

            // Load the Y-matrix and RHS-vector
            _elements.Add(1, -1, 1, -1, -factor, factor);
        }
    }
}