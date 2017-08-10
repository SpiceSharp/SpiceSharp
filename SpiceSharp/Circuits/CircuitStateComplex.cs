using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Numerics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// This class represents the state of a circuit when using complex numbers
    /// </summary>
    public class CircuitStateComplex
    {
        /// <summary>
        /// Gets or sets the current laplace variable
        /// Using a purely imaginary variable here will give you the steady-state frequency response
        /// </summary>
        public Complex Laplace = new Complex();

        /// <summary>
        /// Gets the (complex) Yn-matrix
        /// </summary>
        public Matrix<Complex> Matrix { get; private set; } = null;

        /// <summary>
        /// Gets the (complex) RHS vector
        /// </summary>
        public Vector<Complex> Rhs { get; private set; } = null;

        /// <summary>
        /// Gets the (complex) solution vector
        /// </summary>
        public Vector<Complex> Solution { get; private set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitStateComplex(int order)
        {
            Matrix = new SparseMatrix(order);
            Rhs = new SparseVector(order);
            Solution = new DenseVector(order);
        }

        /// <summary>
        /// Solve the current matrix using the Rhs vector
        /// </summary>
        public void Solve()
        {
            // All indices at 0 are the ground node
            // We remove these rows/columns/items because they will lead to a singular matrix
            var m = Matrix.RemoveRow(0).RemoveColumn(0);
            var b = Rhs.SubVector(1, Rhs.Count - 1);

            // Create a new solution vector of the original size
            Solution = new DenseVector(Rhs.Count);

            // Fill the 1-N elements with the solution
            Solution.SetSubVector(1, Solution.Count - 1, m.Solve(b));
        }

        /// <summary>
        /// Solve the transposed matrix using the Rhs vector
        /// </summary>
        public void SolveTransposed()
        {
            // All indices at 0 are the ground node
            // We remove these rows/columns/items because they will lead to singular matrices
            var m = Matrix.RemoveRow(0).RemoveColumn(0);
            var b = Rhs.SubVector(1, Rhs.Count - 1);

            // Solve transposed matrix
            m = m.Transpose();

            // Create a new solution vector of the original size
            Solution = new DenseVector(Rhs.Count);

            // Fill the 1-N elements with the solution
            Solution.SetSubVector(1, Solution.Count - 1, m.Solve(b));
        }

        /// <summary>
        /// Clear the matrices
        /// </summary>
        public void Clear()
        {
            Matrix.Clear();
            Rhs.Clear();
        }

        /// <summary>
        /// Destroy all references
        /// </summary>
        public void Destroy()
        {
            Matrix = null;
            Rhs = null;
            Solution = null;
        }
    }
}
