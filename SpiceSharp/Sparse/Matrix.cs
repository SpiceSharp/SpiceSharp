using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Possible errors for sparse matrices
    /// </summary>
    public enum SparseError
    {
        Okay = 0,
        SmallPivot = 0,
        ZeroDiagonal = 102,
        Singular = 102,
        Panic = 101,
        Fatal = 101
    }

    /// <summary>
    /// Possible partitioning method
    /// </summary>
    public enum SparsePartition
    {
        Default = 0,
        Direct = 1,
        Indirect = 2,
        Auto = 3
    }

    /// <summary>
    /// A sparse matrix representation for SpiceSharp
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Constants
        /// </summary>
        internal const double DEFAULT_THRESHOLD = 1.0e-3;
        internal const bool DIAG_PIVOTING_AS_DEFAULT = true;
        internal const int MINIMUM_ALLOCATED_SIZE = 6;
        internal const float EXPANSION_FACTOR = 1.5f;
        internal const int MAX_MARKOWITZ_TIES = 100;
        internal const int TIES_MULTIPLIER = 5;
        internal const SparsePartition DEFAULT_PARTITION = SparsePartition.Auto;

        /// <summary>
        /// Flag for indicating if he matrix uses complex numbers or not
        /// </summary>
        public bool Complex { get; set; }

        /// <summary>
        /// Gets the external size
        /// </summary>
        public int ExternalSize { get => ExtSize; }

        /// <summary>
        /// Gets the internal size
        /// </summary>
        public int InternalSize { get => Size; }

        /// <summary>
        /// Internal variables
        /// </summary>
        internal double AbsThreshold;
        internal int AllocatedSize;
        internal int AllocatedExtSize;
        internal int CurrentSize;
        internal MatrixElement[] Diag;
        internal bool[] DoCmplxDirect;
        internal bool[] DoRealDirect;
        internal int Elements;
        internal SparseError Error;
        internal int ExtSize;
        internal int[] ExtToIntColMap;
        internal int[] ExtToIntRowMap;
        internal bool Factored;
        internal int Fillins;
        internal MatrixElement[] FirstInCol;
        internal MatrixElement[] FirstInRow;
        internal ElementValue[] Intermediate;
        internal bool InternalVectorsAllocated;
        internal int[] IntToExtColMap;
        internal int[] IntToExtRowMap;
        internal int[] MarkowitzRow;
        internal int[] MarkowitzCol;
        internal long[] MarkowitzProd;
        internal int MaxRowCountInLowerTri;
        internal bool NeedsOrdering;
        internal bool NumberOfInterchangesIsOdd;
        internal bool Partitioned;
        internal int PivotsOriginalCol;
        internal int PivotsOriginalRow;
        internal char PivotSelectionMethod;
        internal bool PreviousMatrixWasComplex;
        internal double RelThreshold;
        internal bool Reordered;
        internal bool RowsLinked;
        internal int SingularCol;
        internal int SingularRow;
        internal int Singletons;
        internal int Size;
        internal MatrixElement TrashCan;

        /// <summary>
        /// Constructor
        /// </summary>
        public Matrix() : this(0, true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Size">Matrix size</param>
        /// <param name="Complex">Is complex</param>
        public Matrix(int Size, bool Complex)
        {
            if (Size < 0)
                throw new SparseException("Invalid size");

            // Create matrix
            int AllocatedSize = Math.Max(Size, MINIMUM_ALLOCATED_SIZE);
            int SizePlusOne = AllocatedSize + 1;

            // Initialize matrix
            this.Complex = Complex;
            PreviousMatrixWasComplex = Complex;
            Factored = false;
            Elements = 0;
            Error = SparseError.Okay;
            Fillins = 0;
            Reordered = false;
            NeedsOrdering = true;
            NumberOfInterchangesIsOdd = false;
            Partitioned = false;
            RowsLinked = false;
            InternalVectorsAllocated = false;
            SingularCol = 0;
            SingularRow = 0;
            this.Size = Size;
            this.AllocatedSize = AllocatedSize;
            ExtSize = Size;
            AllocatedExtSize = AllocatedSize;
            CurrentSize = 0;
            ExtToIntColMap = null;
            ExtToIntRowMap = null;
            IntToExtColMap = null;
            IntToExtRowMap = null;
            MarkowitzRow = null;
            MarkowitzCol = null;
            MarkowitzProd = null;
            DoCmplxDirect = null;
            DoRealDirect = null;
            Intermediate = null;
            RelThreshold = DEFAULT_THRESHOLD;
            AbsThreshold = 0.0;

            // Take out the trash
            TrashCan = new MatrixElement(0, 0);
            TrashCan.Row = 0;
            TrashCan.Col = 0;
            TrashCan.NextInRow = null;
            TrashCan.NextInCol = null;

            // Allocate space in memory for Diag pointer vector
            Diag = new MatrixElement[SizePlusOne];

            // Allocate space in memory for FirstInCol pointer vector
            FirstInCol = new MatrixElement[SizePlusOne];

            // Allocate space in memory for FirstInRow pointer vector
            FirstInRow = new MatrixElement[SizePlusOne];

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

        /// <summary>
        /// Where is matrix singular
        /// </summary>
        /// <param name="pRow">Row</param>
        /// <param name="pCol">Column</param>
        internal void SingularAt(out int pRow, out int pCol)
        {
            if (Error == SparseError.Singular || Error == SparseError.ZeroDiagonal)
            {
                pRow = SingularRow;
                pCol = SingularCol;
            }
            else
            {
                pRow = 0;
                pCol = 0;
            }
            return;
        }

        /// <summary>
        /// Get matrix size
        /// </summary>
        /// <param name="External"></param>
        /// <returns></returns>
        public int GetSize(bool External) => External ? ExtSize : Size;

        /// <summary>
        /// Get the number of fillins
        /// </summary>
        /// <returns></returns>
        public int spFillinCount() => Fillins;

        /// <summary>
        /// Get the number of elements
        /// </summary>
        /// <returns></returns>
        public int spElementCount() => Elements;

        /// <summary>
        /// Convert the matrix to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparseOutput.Print(this, false, true, false);
        }
    }
}
