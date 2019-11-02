using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IComplexSimulationState"/> for a <see cref="FrequencyBehavior"/> that can load in parallel.
    /// </summary>
    /// <seealso cref="ParallelLoadSolverState{T}" />
    /// <seealso cref="ISubcircuitComplexSimulationState" />
    public class LoadComplexState : ParallelLoadSolverState<Complex>, ISubcircuitComplexSimulationState
    {
        /// <summary>
        /// The parent simulation state.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="LoadComplexState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LoadComplexState(IComplexSimulationState parent)
            : base(parent, LUHelper.CreateSparseComplexSolver())
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }
    }
}
