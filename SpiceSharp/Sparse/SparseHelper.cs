using System;

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
        /// SMPpreOrder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <returns></returns>
        public static SparseError Preorder(this Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            SparseUtilities.PreorderMNA(matrix);
            return matrix.Error;
        }

        /// <summary>
        /// Load GMIN
        /// Adds diagonal element contributions for increased convergence.
        /// Not recommended
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="gmin">The conductance to be added on the diagonal</param>
        public static void LoadGmin(this Matrix matrix, double gmin)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

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
