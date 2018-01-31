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
        /// <param name="gmin">Value added on the diagonal</param>
        /// <returns></returns>
        public static SparseError Factor(this Matrix matrix, double gmin)
        {
            matrix.LoadGmin(gmin);
            return matrix.Factor();
        }

        /// <summary>
        /// SMPcReorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="pivotTolerance">Pivot tolerance</param>
        /// <param name="pivotRelativeTolerance">Pivot relative tolerance</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double pivotTolerance, double pivotRelativeTolerance)
        {
            return matrix.OrderAndFactor(null, pivotRelativeTolerance, pivotTolerance, true);
        }

        /// <summary>
        /// SMPreorder
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="pivotTolerance">Pivot tolerance</param>
        /// <param name="pivotRelativeTolerance">Pivot relative tolerance</param>
        /// <param name="gmin">Minimum conductance on the diagonal</param>
        /// <returns></returns>
        public static SparseError Reorder(this Matrix matrix, double pivotTolerance, double pivotRelativeTolerance, double gmin)
        {
            matrix.LoadGmin(gmin);
            return matrix.OrderAndFactor(null, pivotRelativeTolerance, pivotTolerance, true);
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
