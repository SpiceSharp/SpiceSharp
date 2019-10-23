using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A <see cref="IComplexSimulationState"/> for a <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <seealso cref="IComplexSimulationState" />
    public class LoadComplexState : IComplexSimulationState
    {
        private IComplexSimulationState _parent;

        /// <summary>
        /// Gets or sets a value indicating whether the solution converges.
        /// </summary>
        public bool IsConvergent 
        {
            get => _parent.IsConvergent; 
            set => _parent.IsConvergent = value; 
        }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        public IVector<Complex> Solution => _parent.Solution;

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        public Complex Laplace => _parent.Laplace;

        /// <summary>
        /// Gets the solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparseSolver<Complex> Solver => _solver;
        private SolverElementProvider<Complex> _solver;

        /// <summary>
        /// Gets the variable to index map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map => _parent.Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadComplexState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LoadComplexState(IComplexSimulationState parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _solver = new SolverElementProvider<Complex>(parent.Solver);
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Setup(ISimulation simulation)
        {
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public void Unsetup()
        {
        }
    }
}
