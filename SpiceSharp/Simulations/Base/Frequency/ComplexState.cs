using System;
using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// State in the complex domain
    /// </summary>
    public class ComplexState : State
    {
        /// <summary>
        /// Did the solution converge?
        /// </summary>
        public bool IsConvergent { get; set; }

        /// <summary>
        /// Sparse matrix flags
        /// </summary>
        [Flags]
        public enum SparseStates
        {
            /// <summary>
            /// Indicates that the matrix should be reordered
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
            ShouldReorder = 0x01,

            /// <summary>
            /// Indicates that the matrix is preordered
            /// </summary>
            /// <remarks>Preordering uses common observations in matrices for Modifed Nodal Analysis (MNA) to reorder the matrix before running any analysis.</remarks>
            DidPreorder = 0x100,

            /// <summary>
            /// Indicates that the matrix should be reordered for AC analysis
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
            AcShouldReorder = 0x10
        }

        /// <summary>
        /// TEMPORARY: Pivot absolute tolerance
        /// </summary>
        public double PivotAbsoluteTolerance { get; set; } = 1e-13;

        /// <summary>
        /// TEMPORARY: Pivot relative tolerance
        /// </summary>
        public double PivotRelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Extra conductance that is added to all nodes to ground to aid convergence
        /// </summary>
        public double DiagonalGmin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the sparse matrix flags
        /// </summary>
        public SparseStates Sparse { get; set; }

        /// <summary>
        /// The complex solver
        /// </summary>
        public ComplexSolver Solver { get; } = new ComplexSolver();

        /// <summary>
        /// Gets the solution
        /// </summary>
        public Vector<Complex> Solution { get; private set; }

        /// <summary>
        /// Gets or sets the current laplace variable
        /// Using a purely imaginary variable here will give you the steady-state frequency response
        /// </summary>
        public Complex Laplace { get; set; } = new Complex();

        /// <summary>
        /// Initialize circuit
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public override void Initialize(UnknownCollection nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            Solution = new DenseVector<Complex>(Solver.Order);
            base.Initialize(nodes);
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public override void Destroy()
        {
            Solution = null;
            base.Destroy();
        }
    }
}
