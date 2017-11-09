namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Helper methods for Sparse matrices
    /// </summary>
    public static class SparseHelper
    {
        /// <summary>
        /// SMPluFac
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Gmin">Value added on the diagonal</param>
        /// <returns></returns>
        public static SparseError Factor(this Matrix matrix, double Gmin)
        {
            matrix.LoadGmin(Gmin);
            return matrix.Factor();
        }

        /// <summary>
        /// SMPcReorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="PivTol">Pivot tolerance</param>
        /// <param name="PivRel">Pivot relative tolerance</param>
        /// <param name="NumSwaps">The number of swaps performed</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double PivTol, double PivRel)
        {
            return matrix.OrderAndFactor(null, PivRel, PivTol, true);
        }

        /// <summary>
        /// SMPreorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="PivTol">Pivot tolerance</param>
        /// <param name="PivRel">Pivot relative tolerance</param>
        /// <param name="Gmin">Minimum conductance on the diagonal</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double PivTol, double PivRel, double Gmin)
        {
            matrix.LoadGmin(Gmin);
            return matrix.OrderAndFactor(null, PivRel, PivTol, true);
        }

        /// <summary>
        /// SMPcaSolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">The right hand side</param>
        /// <param name="iRHS">The imaginary values of the right hand side</param>
        public static void SolveTransposed(this Matrix matrix, double[] RHS, double[] iRHS)
        {
            SparseSolve.SolveTransposed(matrix, RHS, RHS, iRHS, iRHS);
        }

        /// <summary>
        /// SMPcSolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">Right hand side</param>
        /// <param name="iRHS">Imaginary values of the right hand side</param>
        public static void Solve(this Matrix matrix, double[] RHS, double[] iRHS)
        {
            SparseSolve.Solve(matrix, RHS, RHS, iRHS, iRHS);
        }

        /// <summary>
        /// SMPsolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">Right hand side</param>
        /// <param name="Spare">Imaginary values of the right hand side</param>
        public static void Solve(this Matrix matrix, double[] RHS)
        {
            SparseSolve.Solve(matrix, RHS, RHS, null, null);
        }
        
        /// <summary>
        /// SMPpreOrder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <returns></returns>
        public static SparseError PreOrder(this Matrix matrix)
        {
            SparseUtilities.PreorderMNA(matrix);
            return matrix.Error;
        }

        /// <summary>
        /// Load GMIN
        /// Adds diagonal element contributions for increased convergence.
        /// Not recommended
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Gmin">The conductance to be added on the diagonal</param>
        public static void LoadGmin(this Matrix matrix, double gmin)
        {
            MatrixElement[] Diag = matrix.Diag;

            if (gmin != 0.0)
            {
                for (int i = 1; i < matrix.IntSize; i++)
                {
                    if (Diag[i] != null)
                        Diag[i].Value.Real += gmin;
                }
            }
            return;
        }
    }
}
