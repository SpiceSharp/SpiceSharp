namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Helper methods for Sparse matrices
    /// </summary>
    public static class SparseHelper
    {
        /// <summary>
        /// Constants
        /// </summary>
        public const double M_LN2 = 0.69314718055994530942;
        public const double M_LN10 = 2.30258509299404568402;

        /// <summary>
        /// SMPcLUfac
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <returns></returns>
        public static SparseError SMPcLUfac(this Matrix matrix)
        {
            // matrix.Complex = true;
            return matrix.Factor();
        }

        /// <summary>
        /// SMPluFac
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="Gmin">Value added on the diagonal</param>
        /// <returns></returns>
        public static SparseError SMPluFac(this Matrix matrix, double Gmin)
        {
            // matrix.Complex = false;
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
        public static SparseError SMPcReorder(this Matrix matrix, double PivTol, double PivRel)
        {
            matrix.Complex = true;
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
        public static SparseError SMPreorder(this Matrix matrix, double PivTol, double PivRel, double Gmin)
        {
            matrix.Complex = false;
            matrix.LoadGmin(Gmin);
            return matrix.OrderAndFactor(null, PivRel, PivTol, true);
        }

        /// <summary>
        /// SMPcaSolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">The right hand side</param>
        /// <param name="iRHS">The imaginary values of the right hand side</param>
        public static void SMPcaSolve(this Matrix matrix, double[] RHS, double[] iRHS)
        {
            SparseSolve.spSolveTransposed(matrix, RHS, RHS, iRHS, iRHS);
        }

        /// <summary>
        /// SMPcSolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">Right hand side</param>
        /// <param name="iRHS">Imaginary values of the right hand side</param>
        public static void SMPcSolve(this Matrix matrix, double[] RHS, double[] iRHS)
        {
            SparseSolve.spSolve(matrix, RHS, RHS, iRHS, iRHS);
        }

        /// <summary>
        /// SMPsolve
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="RHS">Right hand side</param>
        /// <param name="Spare">Imaginary values of the right hand side</param>
        public static void SMPsolve(this Matrix matrix, double[] RHS, double[] Spare)
        {
            SparseSolve.spSolve(matrix, RHS, RHS, null, null);
        }
        
        /// <summary>
        /// SMPpreOrder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <returns></returns>
        public static SparseError SMPpreOrder(this Matrix matrix)
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
        public static void LoadGmin(this Matrix matrix, double Gmin)
        {
            MatrixElement[] Diag = matrix.Diag;

            if (Gmin != 0.0)
            {
                for (int i = 1; i < matrix.Size; i++)
                {
                    if (Diag[i] != null)
                        Diag[i].Value.Real += Gmin;
                }
            }
            return;
        }
    }
}
