using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IComplexSimulationState"/> for <see cref="FrequencyBehavior"/> capable of solving in parallel.
    /// </summary>
    /// <seealso cref="ParallelSolveSolverState{T}" />
    /// <seealso cref="ISubcircuitComplexSimulationState" />
    public class SolveComplexState : ParallelSolveSolverState<Complex>, ISubcircuitComplexSimulationState
    {
        private IComplexSimulationState _parent;
        private bool _isConvergent;

        /// <summary>
        /// Gets the laplace.
        /// </summary>
        /// <value>
        /// The laplace.
        /// </value>
        public Complex Laplace => _parent.Laplace;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is convergent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </value>
        public bool IsConvergent
        {
            get => _isConvergent && _parent.IsConvergent;
            set => _isConvergent = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveComplexState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public SolveComplexState(IComplexSimulationState parent)
            : base(parent, LUHelper.CreateSparseComplexSolver())
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }
    }
}
