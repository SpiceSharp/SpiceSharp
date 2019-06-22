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
        /// <value>
        ///   <c>true</c> if the solution converges; otherwise, <c>false</c>.
        /// </value>
        public bool IsConvergent { get; set; }

        /// <summary>
        /// Gets the solver for complex linear systems of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ComplexSolver Solver { get; } = new ComplexSolver();

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public SpiceSharp.Algebra.Vector<Complex> Solution { get; private set; }

        /// <summary>
        /// Gets or sets the current laplace variable.
        /// </summary>
        /// <value>
        /// The laplace variable value.
        /// </value>
        public Complex Laplace { get; set; } = new Complex();

        /// <summary>
        /// Setup the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        /// <exception cref="ArgumentNullException">nodes</exception>
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
