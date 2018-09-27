using System;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state using complex numbers.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.State" />
    public class ComplexState : State
    {
        /// <summary>
        /// Gets or sets a value indicating whether the solution converges.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the solution converges; otherwise, <c>false</c>.
        /// </value>
        public bool IsConvergent { get; set; }

        // TODO: This should probably be separated.
        /// <summary>
        /// Flags for sparse matrices.
        /// </summary>
        [Flags]
        public enum SparseStates
        {
            /// <summary>
            /// Indicates that the matrix should be reordered.
            /// </summary>
            /// <remarks>
            /// Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.
            /// </remarks>
            ShouldReorder = 0x01,

            /// <summary>
            /// Indicates that the matrix is preordered.
            /// </summary>
            /// <remarks>
            /// Preordering uses common observations in matrices for Modifed Nodal Analysis (MNA) to reorder the matrix before running any analysis.
            /// </remarks>
            DidPreorder = 0x100,

            /// <summary>
            /// Indicates that the matrix should be reordered for AC analysis.
            /// </summary>
            /// <remarks>
            /// Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.
            /// </remarks>
            AcShouldReorder = 0x10
        }

        /// <summary>
        /// TEMPORARY: Pivot absolute tolerance
        /// </summary>
        /// <value>
        /// The pivot absolute tolerance.
        /// </value>
        public double PivotAbsoluteTolerance { get; set; } = 1e-13;

        /// <summary>
        /// TEMPORARY: Pivot relative tolerance
        /// </summary>
        /// <value>
        /// The pivot relative tolerance.
        /// </value>
        public double PivotRelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// An extra conductance that can be added to all nodes to ground to aid convergence.
        /// </summary>
        /// <value>
        /// The conductance added on the diagonal.
        /// </value>
        public double DiagonalGmin { get; set; }

        /// <summary>
        /// Gets or sets flags for solving using sparse matrices.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public SparseStates Sparse { get; set; }

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
        public Vector<Complex> Solution { get; private set; }

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
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            Solution = new DenseVector<Complex>(Solver.Order);
            base.Setup(nodes);
        }

        /// <summary>
        /// Unsetup the state.
        /// </summary>
        public override void Destroy()
        {
            Solution = null;
            base.Destroy();
        }
    }
}
