using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// A sparse matrix representation for SpiceSharp
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Constants
        /// </summary>
        internal const int OK = 0;
        internal const int E_PANIC = 1;
        internal const int E_INTERN = E_PANIC;
        internal const int E_PRIVATE = 100;
        internal const int E_BADMATRIX = (E_PRIVATE + 1); /* ill-formed matrix can't be decomposed */
        internal const int E_SINGULAR = (E_PRIVATE + 2); /* matrix is singular */
        internal const int E_ITERLIM = (E_PRIVATE + 3);  /* iteration limit reached,operation aborted */
        internal const int E_ORDER = (E_PRIVATE + 4);    /* integration order not supported */
        internal const int E_METHOD = (E_PRIVATE + 5);   /* integration method not supported */
        internal const int E_TIMESTEP = (E_PRIVATE + 6); /* timestep too small */
        internal const int E_XMISSIONLINE = (E_PRIVATE + 7);    /* transmission line in pz analysis */
        internal const int E_MAGEXCEEDED = (E_PRIVATE + 8); /* pole-zero magnitude too large */
        internal const int E_SHORT = (E_PRIVATE + 9);   /* pole-zero input or output shorted */
        internal const int E_INISOUT = (E_PRIVATE + 10);    /* pole-zero input is output */
        internal const int E_ASKCURRENT = (E_PRIVATE + 11); /* ac currents cannot be ASKed */
        internal const int E_ASKPOWER = (E_PRIVATE + 12);   /* ac powers cannot be ASKed */
        internal const int E_NODUNDEF = (E_PRIVATE + 13); /* node not defined in noise anal */
        internal const int E_NOACINPUT = (E_PRIVATE + 14); /* no ac input src specified for noise */
        internal const int E_NOF2SRC = (E_PRIVATE + 15); /* no source at F2 for IM disto analysis */
        internal const int E_NODISTO = (E_PRIVATE + 16); /* no distortion analysis - NODISTO defined */
        internal const int E_NONOISE = (E_PRIVATE + 17); /* no noise analysis - NONOISE defined */
        internal const int spOKAY = OK;
        internal const int spSMALL_PIVOT = OK;
        internal const int spZERO_DIAG = E_SINGULAR;
        internal const int spSINGULAR = E_SINGULAR;
        internal const int spPANIC = E_BADMATRIX;

        internal const int spFATAL = E_BADMATRIX;

        internal const int spDEFAULT_PARTITION = 0;
        internal const int spDIRECT_PARTITION = 1;
        internal const int spINDIRECT_PARTITION = 2;
        internal const int spAUTO_PARTITION = 3;

        internal const double DEFAULT_THRESHOLD = 1.0e-3;
        internal const bool DIAG_PIVOTING_AS_DEFAULT = true;
        internal const int SPACE_FOR_ELEMENTS = 6;
        internal const int SPACE_FOR_FILL_INS = 4;
        internal const int ELEMENTS_PER_ALLOCATION = 31;
        internal const int MINIMUM_ALLOCATED_SIZE = 6;
        internal const float EXPANSION_FACTOR = 1.5f;
        internal const int MAX_MARKOWITZ_TIES = 100;
        internal const int TIES_MULTIPLIER = 5;
        internal const int DEFAULT_PARTITION = spAUTO_PARTITION;

        internal const int SPARSE_ID = 0x772773;

        internal double AbsThreshold;
        internal int AllocatedSize;
        internal int AllocatedExtSize;
        internal bool Complex;
        internal int CurrentSize;
        internal MatrixElement[] Diag;
        internal bool[] DoCmplxDirect;
        internal bool[] DoRealDirect;
        internal int Elements;
        internal int Error;
        internal int ExtSize;
        internal int[] ExtToIntColMap;
        internal int[] ExtToIntRowMap;
        internal bool Factored;
        internal int Fillins;
        internal MatrixElement[] FirstInCol;
        internal MatrixElement[] FirstInRow;
        internal ulong ID;
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

        internal int RecordsRemaining;
        internal MatrixElement[] NextAvailElement;
        internal int ElementsRemaining;
        internal MatrixElement[] NextAvailFillin;
        internal int FillinsRemaining;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Size">Matrix size</param>
        /// <param name="Complex">Is complex</param>
        public Matrix(int Size, bool Complex)
        {
            int SizePlusOne;
            int AllocatedSize;

            if (Size < 0)
                throw new SparseException("Invalid size");

            // Create matrix
            AllocatedSize = Math.Max(Size, MINIMUM_ALLOCATED_SIZE);
            SizePlusOne = AllocatedSize + 1;

            // Initialize matrix
            ID = SPARSE_ID;
            this.Complex = Complex;
            PreviousMatrixWasComplex = Complex;
            Factored = false;
            Elements = 0;
            Error = spOKAY;
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

            RecordsRemaining = 0;
            ElementsRemaining = 0;
            FillinsRemaining = 0;

            // Take out the trash
            TrashCan = new MatrixElement();
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

            // Allocate space for fill-ins and initial set of elements
            InitializeElementBlocks(SPACE_FOR_ELEMENTS * AllocatedSize,
                                             SPACE_FOR_FILL_INS * AllocatedSize);
        }

        /// <summary>
        /// Element allocation
        /// </summary>
        /// <returns></returns>
        internal MatrixElement spcGetElement() => new MatrixElement();

        /// <summary>
        /// Get fill-in
        /// </summary>
        /// <returns></returns>
        internal MatrixElement spcGetFillin() => new MatrixElement();

        private void InitializeElementBlocks(int InitialNumberOfElements, int NumberOfFillinsExpected)
        {
            MatrixElement[] pElement;

            // Allocate block of MatrixElements for elements
            pElement = new MatrixElement[InitialNumberOfElements];
            ElementsRemaining = InitialNumberOfElements;
            NextAvailElement = pElement;

            // Allocate block of MatrixElements for fill-ins
            pElement = new MatrixElement[NumberOfFillinsExpected];
            FillinsRemaining = NumberOfFillinsExpected;
            NextAvailFillin = pElement;
        }

        /// <summary>
        /// Where is matrix singular
        /// </summary>
        /// <param name="pRow">Row</param>
        /// <param name="pCol">Column</param>
        internal void spWhereSingular(out int pRow, out int pCol)
        {
            if (Error == spSINGULAR || Error == spZERO_DIAG)
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
        internal int spGetSize(bool External) => External ? ExtSize : Size;

        internal void spSetReal() => Complex = false;
        internal void spSetComplex() => Complex = true;
        internal int spFillinCount() => Fillins;
        internal int spElementCount() => Elements;
}
}
