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
        /// Gets the solver for complex linear systems of equations.
        /// </summary>
        public ComplexSolver Solver { get; } = new ComplexSolver();

        /// <summary>
        /// Gets the solution.
        /// </summary>
        public SpiceSharp.Algebra.Vector<Complex> Solution { get; private set; }

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        public Complex Laplace { get; set; } = new Complex();

        /// <summary>
        /// Setup the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        public override void Setup(VariableSet nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));
            Solution = new DenseVector<Complex>(Solver.Order);
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
