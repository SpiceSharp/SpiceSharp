using SpiceSharp.Sparse;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Represents the state of a circuit.
    /// </summary>
    public class CircuitStateReal
    {
        /// <summary>
        /// Get the equation matrix
        /// </summary>
        public Matrix Matrix { get; private set; } = null;

        /// <summary>
        /// Get the right-hand side vector
        /// </summary>
        public double[] Rhs { get; private set; } = null;

        /// <summary>
        /// Get the current solution
        /// </summary>
        public double[] Solution { get; private set; } = null;

        /// <summary>
        /// Get the previous solution
        /// Can be used for checking convergence
        /// </summary>
        public double[] OldSolution { get; private set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitStateReal()
        {
            Matrix = spsmp.SMPnewMatrix(); // new SparseMatrix(order);
            // Rhs = new double[order]; // new SparseVector(order);
            // Solution = new double[order]; // new DenseVector(order);
            // OldSolution = new double[order]; // new DenseVector(order);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="order"></param>
        public void Initialize(int order)
        {
            Rhs = new double[order];
            Solution = new double[order];
            OldSolution = new double[order];
        }

        /// <summary>
        /// Clear the Matrix and Rhs vector
        /// </summary>
        public void Clear()
        {
            spbuild.spClear(Matrix);
            for (int i = 0; i < Rhs.Length; i++)
                Rhs[i] = 0.0;
        }

        /// <summary>
        /// Store the current solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = Rhs;
            Rhs = OldSolution;
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
