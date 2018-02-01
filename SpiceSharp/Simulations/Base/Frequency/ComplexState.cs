using System;
using System.Numerics;
using SpiceSharp.Sparse;

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
        public bool IsCon { get; set; }

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
            ACShouldReorder = 0x10
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
        /// Get the complex right-hand-side vector
        /// </summary>
        public ComplexSolution Rhs { get; private set; } = null;

        /// <summary>
        /// Get the complex solution vector
        /// </summary>
        public ComplexSolution Solution { get; private set; } = null;

        /// <summary>
        /// Gets or sets the current laplace variable
        /// Using a purely imaginary variable here will give you the steady-state frequency response
        /// </summary>
        public Complex Laplace { get; set; } = new Complex();

        /// <summary>
        /// Get the equation matrix
        /// </summary>
        public Matrix Matrix { get; private set; } = null;

        /// <summary>
        /// Gets the order
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// True if already initialized
        /// </summary>
        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ComplexState()
        {
            Matrix = new Matrix();
        }

        /// <summary>
        /// Constructor
        /// Can be used to share a matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public ComplexState(Matrix matrix)
        {
            Matrix = matrix;
        }

        /// <summary>
        /// Initialize circuit
        /// </summary>
        /// <param name="circuit"></param>
        public void Initialize(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            Order = circuit.Nodes.Count + 1;
            Rhs = new ComplexSolution(Order);
            Solution = new ComplexSolution(Order);
            Initialized = true;
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public void Destroy()
        {
            Order = 0;
            Initialized = false;
            Rhs = null;
            Solution = null;
            Matrix = null;
        }

        /// <summary>
        /// Store the solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = Rhs;
            Rhs = Solution;
            Solution = tmp;
        }

        /// <summary>
        /// Clear the matrix and Rhs vector
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Order; i++)
                Rhs[i] = 0;
            Matrix.Clear();
        }
    }
}
