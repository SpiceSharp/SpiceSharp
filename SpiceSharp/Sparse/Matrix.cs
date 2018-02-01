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
        internal const int TIES_MULTIPLIER = 5;
        internal const SparsePartition DEFAULT_PARTITION = SparsePartition.Auto;

        /// <summary>
        /// Flag for indicating if he matrix uses complex numbers or not
        /// </summary>
        public bool Complex { get; set; }

        /// <summary>
        /// Gets the number of fillins
        /// </summary>
        public int Fillins { get; internal set; }

        /// <summary>
        /// Gets the number of elements
        /// </summary>
        public int Elements { get; internal set; }

        /// <summary>
        /// Gets the public size of the matrix
        /// </summary>
        public int Size { get; internal set; }

        /// <summary>
        /// Internal variables
        /// </summary>
        internal double AbsThreshold;
        internal double RelThreshold;

        internal int IntSize;
        internal int AllocatedSize;
        internal int AllocatedExtSize;
        internal int CurrentSize;

        internal MatrixElement[] Diag;
        internal MatrixElement[] FirstInCol;
        internal MatrixElement[] FirstInRow;
        internal MatrixElement TrashCan;

        internal bool Factored;

        internal int MaxRowCountInLowerTri;
        internal bool NeedsOrdering;
        internal bool NumberOfInterchangesIsOdd;


        internal bool Reordered;
        internal bool RowsLinked;


        internal SparseError Error;
        internal int SingularCol;
        internal int SingularRow;

        /// <summary>
        /// Translation for indices
        /// </summary>
        public SparseTranslation Translation { get; }

        /// <summary>
        /// Pivoting
        /// </summary>
        public SparsePivoting Pivoting { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Matrix() : this(0, true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Matrix size</param>
        /// <param name="complex">Is complex</param>
        public Matrix(int size, bool complex)
        {
            if (size < 0)
                throw new SparseException("Invalid size");

            // Create matrix
            int allocated = Math.Max(size, MINIMUM_ALLOCATED_SIZE);
            int sizeplusone = allocated + 1;

            // Initialize matrix
            Complex = complex;
            Factored = false;
            Elements = 0;
            Error = SparseError.Okay;
            Fillins = 0;
            Reordered = false;
            NeedsOrdering = true;
            NumberOfInterchangesIsOdd = false;
            RowsLinked = false;
            SingularCol = 0;
            SingularRow = 0;
            IntSize = size;
            AllocatedSize = allocated;
            Size = size;
            AllocatedExtSize = allocated;
            CurrentSize = 0;
            RelThreshold = DEFAULT_THRESHOLD;
            AbsThreshold = 0.0;

            // Take out the trash
            TrashCan = new MatrixElement(0, 0);

            // Allocate space in memory for Diag pointer vector
            Diag = new MatrixElement[sizeplusone];

            // Allocate space in memory for FirstInRow/Col pointer vectors
            FirstInCol = new MatrixElement[sizeplusone];
            FirstInRow = new MatrixElement[sizeplusone];

            Translation = new SparseTranslation(sizeplusone);

            Pivoting = new SparsePivoting();

        }

        /// <summary>
        /// Clear all elements of a matrix
        /// </summary>
        public void Clear()
        {
            for (int i = 1; i <= IntSize; i++)
            {
                MatrixElement elt = FirstInCol[i];
                while (elt != null)
                {
                    elt.Value.Complex = 0.0;
                    elt = elt.NextInColumn;
                }
            }

            // Empty the trash
            TrashCan.Value.Complex = 0.0;

            // Reset flags
            Error = SparseError.Okay;
            Factored = false;
            SingularCol = 0;
            SingularRow = 0;
        }

        /// <summary>
        /// Get an element from the matrix
        /// If it does not find the element in the matrix, it will create it!
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        public ElementValue GetElement(int row, int col)
        {
            if (row < 0 || col < 0)
                throw new SparseException("Index out of bounds");

            // Trash
            if (row == 0 || col == 0)
                return TrashCan;

            // Translate external indices to internal indices
            Translation.Translate(this, ref row, ref col);

            // Quickly access diagonal
            MatrixElement elt;
            if (row != col || (elt = Diag[row]) == null)
            {
                // We have to find the element or create it!
                elt = CreateElement(row, col);
            }
            return elt;
        }

        /// <summary>
        /// Find an element in the matrix without creating it
        /// This method works on the reordered matrix!
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        public MatrixElement FindReorderedElement(int row, int col)
        {
            if (row < 0 || col < 0)
                throw new SparseException("Index out of bounds");

            if (row == 0 || col == 0)
                return TrashCan;

            // Find the element at the right place
            MatrixElement elt = FirstInCol[col];
            while (elt != null)
            {
                if (elt.Row < row)
                {
                    // Next one maybe?
                    elt = elt.NextInColumn;
                }
                else if (elt.Row == row)
                {
                    // Found it!
                    return elt;
                }
                else
                {
                    // Aww...
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Find an element in the matrix without creating it
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        public MatrixElement FindElement(int row, int col)
        {
            Translation.Translate(this, ref row, ref col);
            return FindReorderedElement(row, col);
        }

        /// <summary>
        /// Create a new element in the matrix if it doesn't exist
        /// Only used internally!
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <returns></returns>
        private MatrixElement CreateElement(int row, int col)
        {
            MatrixElement elt = FirstInCol[col], last = null;

            // Splice into the column vector while also searching for an existing element
            if (elt == null || elt.Row > row)
            {
                // There are no elements yet in the column
                elt = new MatrixElement(row, col);
                elt.NextInColumn = FirstInCol[col];
                FirstInCol[col] = elt;
                if (row == col)
                    Diag[row] = elt;

                if (RowsLinked)
                    SpliceInRows(elt);
            }
            else
            {
                // Find the insert point
                while (elt != null && elt.Row < row)
                {
                    last = elt;
                    elt = elt.NextInColumn;
                }

                // If the element does not exist yet, create it
                if (elt == null || elt.Row != row)
                {
                    elt = new MatrixElement(row, col);
                    elt.NextInColumn = last.NextInColumn;
                    last.NextInColumn = elt;

                    if (row == col)
                        Diag[row] = elt;

                    // Splice it in the row vector
                    if (RowsLinked)
                        SpliceInRows(elt);
                }
            }
            return elt;
        }

        /// <summary>
        /// Create a fillin matrix element
        /// </summary>
        /// <param name="Row">Row</param>
        /// <param name="Col">Column</param>
        /// <returns></returns>
        internal MatrixElement CreateFillin(int Row, int Col)
        {
            // End of search, create the element. 
            MatrixElement pElement = CreateElement(Row, Col);

            // Update Markowitz counts and products
            Pivoting.MarkowitzProd[Row] = ++Pivoting.MarkowitzRow[Row] * Pivoting.MarkowitzCol[Row];
            if ((Pivoting.MarkowitzRow[Row] == 1) && (Pivoting.MarkowitzCol[Row] != 0))
                Pivoting.Singletons--;
            Pivoting.MarkowitzProd[Col] = ++Pivoting.MarkowitzCol[Col] * Pivoting.MarkowitzRow[Col];
            if ((Pivoting.MarkowitzRow[Col] != 0) && (Pivoting.MarkowitzCol[Col] == 1))
                Pivoting.Singletons--;
            return pElement;
        }

        /// <summary>
        /// Build the row links
        /// </summary>
        public void LinkRows()
        {
            for (int Col = IntSize; Col >= 1; Col--)
            {
                // Generate row links for the elements in the Col'th column
                MatrixElement pElement = FirstInCol[Col];

                while (pElement != null)
                {
                    pElement.Column = Col;
                    pElement.NextInRow = FirstInRow[pElement.Row];
                    FirstInRow[pElement.Row] = pElement;
                    pElement = pElement.NextInColumn;
                }
            }
            RowsLinked = true;
            return;
        }

        /// <summary>
        /// Splice a matrix element in the row vectors
        /// </summary>
        /// <param name="elt">Element</param>
        private void SpliceInRows(MatrixElement elt)
        {
            int row = elt.Row;
            int col = elt.Column;

            MatrixElement splice = FirstInRow[row];
            if (splice == null || splice.Column > col)
            {
                elt.NextInRow = FirstInRow[row];
                FirstInRow[row] = elt;
            }
            else
            {
                while (splice.NextInRow != null && splice.NextInRow.Column < col)
                    splice = splice.NextInRow;
                elt.NextInRow = splice.NextInRow;
                splice.NextInRow = elt;
            }
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
        /// Convert the matrix to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparseOutput.Print(this, false, true, false);
        }
    }
}
