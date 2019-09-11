using System;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state using complex numbers.
    /// </summary>
    /// <seealso cref="SimulationState" />
    public class ComplexSimulationState : SimulationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the solution converges.
        /// </summary>
        public bool IsConvergent { get; set; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        public IVector<Complex> Solution { get; protected set; }

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        public Complex Laplace { get; set; } = new Complex();

        /// <summary>
        /// Gets or sets the solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public LUSolver<Complex> Solver
        {
            get => _solver;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _solver = value;
            }
        }
        private LUSolver<Complex> _solver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
        /// </summary>
        public ComplexSimulationState()
        {
            _solver = new ComplexSolver();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public ComplexSimulationState(LUSolver<Complex> solver)
        {
            _solver = solver.ThrowIfNull(nameof(solver));
        }

        /// <summary>
        /// Sets up the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        public override void Setup(IVariableSet nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            Solution = new DenseVector<Complex>(Solver.Size);
            base.Setup(nodes);
        }

        /// <summary>
        /// Unsetup the state.
        /// </summary>
        public override void Unsetup()
        {
            Solution = null;
            base.Unsetup();
        }
    }
}
