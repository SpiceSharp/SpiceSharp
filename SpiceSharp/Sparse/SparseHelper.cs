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
        /// <param name="gMin">Value added on the diagonal</param>
        /// <returns></returns>
        public static SparseError Factor(this Matrix matrix, double gMin)
        {
            matrix.LoadGmin(gMin);
            return matrix.Factor();
        }

        /// <summary>
        /// SMPcReorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="pivTol">Pivot tolerance</param>
        /// <param name="pivRel">Pivot relative tolerance</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double pivTol, double pivRel)
        {
            return matrix.OrderAndFactor(null, pivRel, pivTol, true);
        }

        /// <summary>
        /// SMPreorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="pivTol">Pivot tolerance</param>
        /// <param name="pivRel">Pivot relative tolerance</param>
        /// <param name="gMin">Minimum conductance on the diagonal</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double pivTol, double pivRel, double gMin)
        {
            matrix.LoadGmin(gMin);
            return matrix.OrderAndFactor(null, pivRel, pivTol, true);
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

            SparseUtilities.PreorderModifiedNodalAnalysis(matrix);
            return matrix.Error;
        }

        /// <summary>
        /// Load GMIN
        /// Adds diagonal element contributions for increased convergence.
        /// Not recommended
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="gMin">The conductance to be added on the diagonal</param>
        public static void LoadGmin(this Matrix matrix, double gMin)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            MatrixElement[] Diag = matrix.Diag;

            if (gMin != 0.0)
            {
                for (int i = 1; i < matrix.IntSize; i++)
                {
                    if (Diag[i] != null)
                        Diag[i].Value.Real += gMin;
                }
            }
            return;
        }
    }
}
