using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// This class represents the state of a circuit when using real numbers
    /// </summary>
    public class CircuitStateReal
    {
        /// <summary>
        /// Get the equation matrix
        /// </summary>
        public Matrix<double> Matrix { get; private set; } = null;

        /// <summary>
        /// Get the right-hand side vector
        /// </summary>
        public Vector<double> Rhs { get; private set; } = null;

        /// <summary>
        /// Get the current solution
        /// </summary>
        public Vector<double> Solution { get; private set; } = null;

        /// <summary>
        /// Get the previous solution
        /// Can be used for checking convergence
        /// </summary>
        public Vector<double> OldSolution { get; private set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitStateReal(int order)
        {
            Matrix = new SparseMatrix(order);
            Rhs = new SparseVector(order);
            Solution = new DenseVector(order);
            OldSolution = new DenseVector(order);
        }

        /// <summary>
        /// Solve the matrix equations
        /// </summary>
        public void Solve()
        {
            // All indices at 0 are the ground node
            // We remove these rows/columns because they will lead to a singular matrix
            var m = Matrix.RemoveRow(0).RemoveColumn(0);
            var b = Rhs.SubVector(1, Rhs.Count - 1);

            // Create a new solution vector of the original size
            if (Solution == null)
                Solution = new DenseVector(Rhs.Count);

            // Fill the 1-N elements with the solution
            var sol = m.Solve(b);
            Solution[0] = 0.0;
            Solution.SetSubVector(1, Solution.Count - 1, sol);
        }

        /// <summary>
        /// Clear the Matrix and Rhs vector
        /// </summary>
        public void Clear()
        {
            Matrix.Clear();
            Rhs.Clear();
        }

        /// <summary>
        /// Store the current solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = OldSolution;
            OldSolution = Solution;
            Solution = tmp;
        }

        /// <summary>
        /// Destroy all references
        /// </summary>
        public void Destroy()
        {
            // Remove all matrices
            Matrix = null;
            Rhs = null;
            Solution = null;
            OldSolution = null;
        }
    }
}
