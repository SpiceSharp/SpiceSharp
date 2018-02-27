using System;
using System.Collections.ObjectModel;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Markowitz
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Markowitz<T> : PivotStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets the relative pivot threshold
        /// </summary>
        public double RelativePivotThreshold { get; private set; } = 1e-3;

        /// <summary>
        /// Gets the absolute pivot threshold
        /// </summary>
        public double AbsolutePivotThreshold { get; private set; } = 0;

        /// <summary>
        /// The maximum Markowitz count that will not result in Int32 overflow when squared
        /// Markowitz counts are capped at this quantity
        /// </summary>
        const int MaxMarkowitzCount = 46340;

        /// <summary>
        /// Gets the Markowitz row counts
        /// </summary>
        public int RowCount(int row) => markowitzRow[row];
        int[] markowitzRow;

        /// <summary>
        /// Gets the Markowitz column counts
        /// </summary>
        public int ColumnCount(int column) => markowitzColumn[column];
        int[] markowitzColumn;

        /// <summary>
        /// Gets the Markowitz products
        /// </summary>
        public int Product(int index) => markowitzProduct[index];
        int[] markowitzProduct;

        /// <summary>
        /// Gets the number of singletons
        /// </summary>
        public int Singletons { get; private set; }

        /// <summary>
        /// Gets the magnitude method
        /// </summary>
        public Func<T, double> Magnitude { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        public Collection<MarkowitzSearchStrategy<T>> Strategies { get; } = new Collection<MarkowitzSearchStrategy<T>>();
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Markowitz()
        {
            // Register default strategies
            Strategies.Add(new MarkowitzSingleton<T>());
            Strategies.Add(new MarkowitzQuickDiagonal<T>());
            Strategies.Add(new MarkowitzDiagonal<T>());
            Strategies.Add(new MarkowitzEntireMatrix<T>());
        }

        /// <summary>
        /// Check that the pivot is valid
        /// </summary>
        /// <param name="pivot">Pivot</param>
        /// <returns></returns>
        public override bool IsValidPivot(MatrixElement<T> pivot)
        {
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));

            // Get the magnitude of the current pivot
            double magnitude = Magnitude(pivot.Value);

            // Search for the largest element below the pivot
            var element = pivot.Below;
            double largest = 0.0;
            while (element != null)
            {
                largest = Math.Max(largest, Magnitude(element.Value));
                element = element.Below;
            }

            // Check the validity
            if (largest * RelativePivotThreshold < magnitude)
                return true;
            return false;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public void Initialize(SparseMatrix<T> matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Allocate arrays
            markowitzRow = new int[matrix.Size + 1];
            markowitzColumn = new int[matrix.Size + 1];
            markowitzProduct = new int[matrix.Size + 2];
        }

        /// <summary>
        /// Count Markowitz numbers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="step">Step</param>
        void Count(SparseMatrix<T> matrix, SparseVector<T> rhs, int step)
        {
            MatrixElement<T> element;

            // Get the first element in the vector
            var rhsElement = rhs.First;

            // Generate Markowitz row count
            for (int i = matrix.Size; i >= step; i--)
            {
                // Set count to -1 initially to remove count due to pivot element
                int count = -1;
                element = matrix.GetFirstInRow(i);
                while (element != null && element.Column < step)
                    element = element.Right;
                while (element != null)
                {
                    count++;
                    element = element.Right;
                }

                // Include elements on the Rhs vector
                while (rhsElement != null && rhsElement.Index < step)
                    rhsElement = rhsElement.Next;
                if (rhsElement != null && rhsElement.Index == i)
                    count++;
                
                markowitzRow[i] = Math.Min(count, MaxMarkowitzCount);
            }
            
            // Generate Markowitz column count
            for (int i = step; i <= matrix.Size; i++)
            {
                // Set count to -1 initially to remove count due to pivot element
                int count = -1;
                element = matrix.GetFirstInColumn(i);
                while (element != null && element.Row < step)
                    element = element.Below;
                while (element != null)
                {
                    count++;
                    element = element.Below;
                }
                markowitzColumn[i] = Math.Min(count, MaxMarkowitzCount);
            }
        }

        /// <summary>
        /// Calculate Markowitz products
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        void Products(SparseMatrix<T> matrix, int step)
        {
            Singletons = 0;
            var size = matrix.Size;
            for (int i = step; i <= size; i++)
            {
                // UpdateMarkowitzProduct(i);
                markowitzProduct[i] = markowitzRow[i] * markowitzColumn[i];
                if (markowitzProduct[i] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Setup the pivoting strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="step">Step</param>
        public override void Setup(SparseMatrix<T> matrix, SparseVector<T> rhs, int step, Func<T, double> magnitude)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            Magnitude = magnitude;

            // Initialize Markowitz row, column and product vectors if necessary
            if (markowitzRow == null || markowitzRow.Length != matrix.Size + 1)
                Initialize(matrix);

            Count(matrix, rhs, step);
            Products(matrix, step);
        }

        /// <summary>
        /// Update pivoting strategy before moving the pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public override void MovePivot(SparseMatrix<T> matrix, SparseVector<T> rhs, MatrixElement<T> pivot, int step)
        {
            // If we haven't setup, just skip
            if (markowitzProduct == null)
                return;
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));

            int row = pivot.Row;
            int column = pivot.Column;

            // Decrease singletons if we are using one as a pivot!
            if (markowitzRow[row] == 0 || markowitzColumn[column] == 0)
                Singletons--;

            // Exchange rows
            if (pivot.Row != step)
            {
                // Swap row Markowitz numbers
                int tmp = markowitzRow[row];
                markowitzRow[row] = markowitzRow[step];
                markowitzRow[step] = tmp;

                // Update the Markowitz product
                int oldProduct = markowitzProduct[row];
                markowitzProduct[row] = markowitzRow[row] * markowitzColumn[row];
                if (oldProduct == 0)
                {
                    if (markowitzProduct[row] != 0)
                        Singletons--;
                }
                else
                {
                    if (markowitzProduct[row] == 0)
                        Singletons++;
                }
            }

            // Exchange columns
            if (column != step)
            {
                // Swap column Markowitz numbers
                int tmp = markowitzColumn[column];
                markowitzColumn[column] = markowitzColumn[step];
                markowitzColumn[step] = tmp;

                // Update the Markowitz product
                int oldProduct = markowitzProduct[column];
                markowitzProduct[column] = markowitzRow[column] * markowitzColumn[column];
                if (oldProduct == 0)
                {
                    if (markowitzProduct[column] != 0)
                        Singletons--;
                }
                else
                {
                    if (markowitzProduct[column] == 0)
                        Singletons++;
                }
            }
        }

        /// <summary>
        /// Update the pivoting strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public override void Update(SparseMatrix<T> matrix, MatrixElement<T> pivot, int step)
        {
            // If we haven't setup, just skip
            if (markowitzProduct == null)
                return;
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));

            // Go through all elements below the pivot. If they exist, then we can subtract 1 from the Markowitz row vector!
            for (MatrixElement<T> column = pivot.Below; column != null; column = column.Below)
            {
                int row = column.Row;
                
                // Update the Markowitz product
                markowitzProduct[row] -= markowitzColumn[row];
                --markowitzRow[row];

                // If we reached 0, then the row just turned to a singleton row
                if (markowitzRow[row] == 0)
                    Singletons++;
            }

            // go through all elements right of the pivot. For every element, we can subtract 1 from the Markowitz column vector!
            for (MatrixElement<T> row = pivot.Right; row != null; row = row.Right)
            {
                int column = row.Column;
                
                // Update the Markowitz product
                markowitzProduct[column] -= markowitzRow[column];
                --markowitzColumn[column];

                // If we reached 0, then the column just turned to a singleton column
                // This only adds a singleton if the row wasn't detected as a singleton row first
                if (markowitzColumn[column] == 0 && markowitzRow[column] != 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public override MatrixElement<T> FindPivot(SparseMatrix<T> matrix, int step)
        {
            foreach (var strategy in Strategies)
            {
                MatrixElement<T> chosen = strategy.FindPivot(this, matrix, step);
                if (chosen != null)
                    return chosen;
            }
            return null;
        }
    }
}
