using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A class that handles index translation
    /// </summary>
    public class SparseTranslation
    {
        /// <summary>
        /// Internal to external column index map
        /// </summary>
        internal int[] IntToExtColMap = null;

        /// <summary>
        /// Internal to external row index map
        /// </summary>
        internal int[] IntToExtRowMap = null;

        /// <summary>
        /// External to internal column map
        /// </summary>
        internal int[] ExtToIntColMap = null;

        /// <summary>
        /// External to internal row map
        /// </summary>
        internal int[] ExtToIntRowMap = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allocated">Initial size of the vectors</param>
        public SparseTranslation(int allocated)
        {
            // Allocate vectors
            IntToExtColMap = new int[allocated];
            IntToExtRowMap = new int[allocated];
            ExtToIntColMap = new int[allocated];
            ExtToIntRowMap = new int[allocated];

            // Initialize MapIntToExt vectors
            for (int i = 1; i < allocated; i++)
            {
                IntToExtRowMap[i] = i;
                IntToExtColMap[i] = i;
                ExtToIntColMap[i] = -1;
                ExtToIntRowMap[i] = -1;
            }
            ExtToIntColMap[0] = 0;
            ExtToIntRowMap[0] = 0;
        }

        /// <summary>
        /// Translate external indices to internal indices
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="row">Row index</param>
        /// <param name="col">Column index</param>
        public void Translate(Matrix matrix, ref int row, ref int col)
        {
            int IntRow, IntCol, ExtRow, ExtCol;

            // Begin `Translate'
            ExtRow = row;
            ExtCol = col;

            // Expand translation arrays if necessary
            if ((ExtRow > matrix.AllocatedExtSize) || (ExtCol > matrix.AllocatedExtSize))
                ExpandTranslationArrays(matrix, Math.Max(ExtRow, ExtCol));

            // Set Size if necessary
            if ((ExtRow > matrix.Size) || (ExtCol > matrix.Size))
                matrix.Size = Math.Max(ExtRow, ExtCol);

            // Translate external row or node number to internal row or node number
            IntRow = ExtToIntRowMap[ExtRow];
            if (IntRow == -1)
            {
                // We don't have an internal row yet!
                matrix.CurrentSize++;
                ExtToIntRowMap[ExtRow] = matrix.CurrentSize;
                ExtToIntColMap[ExtRow] = matrix.CurrentSize;
                IntRow = matrix.CurrentSize;

                // Re-size Matrix if necessary
                if (IntRow > matrix.IntSize)
                    EnlargeMatrix(matrix, IntRow);

                IntToExtRowMap[IntRow] = ExtRow;
                IntToExtColMap[IntRow] = ExtRow;
            }

            // Translate external column or node number to internal column or node number
            if ((IntCol = ExtToIntColMap[ExtCol]) == -1)
            {
                matrix.CurrentSize++;
                ExtToIntRowMap[ExtCol] = matrix.CurrentSize;
                ExtToIntColMap[ExtCol] = matrix.CurrentSize;
                IntCol = matrix.CurrentSize;

                // Re-size Matrix if necessary
                if (IntCol > matrix.IntSize)
                    EnlargeMatrix(matrix, IntCol);

                IntToExtRowMap[IntCol] = ExtCol;
                IntToExtColMap[IntCol] = ExtCol;
            }

            row = IntRow;
            col = IntCol;
        }

        /// <summary>
        /// Expand the translation vectors
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="newsize">Matrix size</param>
        private void ExpandTranslationArrays(Matrix matrix, int newsize)
        {
            int OldAllocatedSize = matrix.AllocatedExtSize;
            matrix.Size = newsize;

            if (newsize <= OldAllocatedSize)
                return;

            // Expand the translation arrays ExtToIntRowMap and ExtToIntColMap
            newsize = Math.Max(newsize, (int)(Matrix.EXPANSION_FACTOR * OldAllocatedSize));
            matrix.AllocatedExtSize = newsize;

            Array.Resize(ref ExtToIntRowMap, newsize + 1);
            Array.Resize(ref ExtToIntColMap, newsize + 1);

            // Initialize the new portion of the vectors
            for (int i = OldAllocatedSize + 1; i <= newsize; i++)
            {
                ExtToIntRowMap[i] = -1;
                ExtToIntColMap[i] = -1;
            }

            return;
        }

        /// <summary>
        /// Allocate memory when making a matrix bigger
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="newsize">Matrix size</param>
        private void EnlargeMatrix(Matrix matrix, int newsize)
        {
            int OldAllocatedSize = matrix.AllocatedSize;
            matrix.IntSize = newsize;

            if (newsize <= OldAllocatedSize)
                return;

            // Expand the matrix frame
            newsize = Math.Max(newsize, (int)(Matrix.EXPANSION_FACTOR * OldAllocatedSize));
            matrix.AllocatedSize = newsize;

            Array.Resize(ref IntToExtColMap, newsize + 1);
            Array.Resize(ref IntToExtRowMap, newsize + 1);
            Array.Resize(ref matrix.Diag, newsize + 1);
            Array.Resize(ref matrix.FirstInCol, newsize + 1);
            Array.Resize(ref matrix.FirstInRow, newsize + 1);

            // Destroy the Markowitz and Intermediate vectors, they will be recreated
            // in spOrderAndFactor().
            matrix.Pivoting.Clear();

            // Initialize the new portion of the vectors
            for (int i = OldAllocatedSize + 1; i <= newsize; i++)
            {
                IntToExtColMap[i] = i;
                IntToExtRowMap[i] = i;
                matrix.Diag[i] = null;
                matrix.FirstInRow[i] = null;
                matrix.FirstInCol[i] = null;
            }
        }
    }
}
