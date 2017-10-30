using System.Numerics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Represents the state of an electronic circuit in the complex domain.
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
        public Matrix Matrix { get; private set; } = null;

        /// <summary>
        /// Gets the (complex) RHS vector
        /// </summary>
        public double[] Rhs { get; private set; } = null;

        public double[] iRhs { get; private set; } = null;

        /// <summary>
        /// Gets the solution vector
        /// </summary>
        public double[] Solution { get; private set; } = null;

        /// <summary>
        /// Gets the imaginary solution vector
        /// </summary>
        public double[] iSolution { get; private set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitStateComplex()
        {
            Matrix = spsmp.SMPnewMatrix();
            // Matrix =  new SparseMatrix(order);
            // Rhs = new double[order]; // new SparseVector(order);
            // iRhs = new double[order];
            // Solution = new double[order]; // Solution = new DenseVector(order);
            // iSolution = new double[order];
        }

        public void Initialize(int order)
        {
            Rhs = new double[order];
            iRhs = new double[order];
            Solution = new double[order];
            iSolution = new double[order];
        }

        /// <summary>
        /// Clear the matrices
        /// </summary>
        public void Clear()
        {
            spbuild.spClear(Matrix);
            for (int i = 0; i < Rhs.Length; i++)
            {
                Rhs[i] = 0.0;
                iRhs[i] = 0.0;
            }
        }

        /// <summary>
        /// Store the solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = iSolution;
            iSolution = iRhs;
            iRhs = tmp;

            tmp = Solution;
            iSolution = Rhs;
            Rhs = tmp;
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
