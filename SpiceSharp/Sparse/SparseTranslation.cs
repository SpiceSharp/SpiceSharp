using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A class for dealing with translation
    /// </summary>
    internal class SparseTranslation
    {
        /// <summary>
        /// External to internal column index map
        /// </summary>
        internal int[] ExtToIntColMap;

        /// <summary>
        /// External to internal row index map
        /// </summary>
        internal int[] ExtToIntRowMap;

        /// <summary>
        /// Internal to external column index map
        /// </summary>
        internal int[] IntToExtColMap;

        /// <summary>
        /// Internal to external row index map
        /// </summary>
        internal int[] IntToExtRowMap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Size"></param>
        public SparseTranslation(int AllocatedSize)
        {
            int SizePlusOne = AllocatedSize + 1;

            // Allocate space in memory for IntToExtColMap vector
            IntToExtColMap = new int[SizePlusOne];

            // Allocate space in memory for IntToExtRowMap vector
            IntToExtRowMap = new int[SizePlusOne];

            // Initialize MapIntToExt vectors
            for (int I = 1; I <= AllocatedSize; I++)
            {
                IntToExtRowMap[I] = I;
                IntToExtColMap[I] = I;
            }

            // Allocate space in memory for ExtToIntColMap vector
            ExtToIntColMap = new int[SizePlusOne];

            // Allocate space in memory for ExtToIntRowMap vector
            ExtToIntRowMap = new int[SizePlusOne];

            // Initialize MapExtToInt vectors
            for (int I = 1; I <= AllocatedSize; I++)
            {
                ExtToIntColMap[I] = -1;
                ExtToIntRowMap[I] = -1;
            }
            ExtToIntColMap[0] = 0;
            ExtToIntRowMap[0] = 0;
        }
    }
}
