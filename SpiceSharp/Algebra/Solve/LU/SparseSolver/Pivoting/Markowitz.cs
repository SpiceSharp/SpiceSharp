using SpiceSharp.ParameterSets;
using System;
using System.Collections.ObjectModel;
using SpiceSharp.Attributes;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A search strategy based on methods outlined by Markowitz.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public partial class Markowitz<T> : ParameterSet, ICloneable<Markowitz<T>>
    {
        private int[] _markowitzRow;
        private int[] _markowitzColumn;
        private int[] _markowitzProduct;

        /// <summary>
        /// The maximum Markowitz count that will not result in Int32 overflow when squared
        /// Markowitz counts are capped at this quantity.
        /// </summary>
        /// <remarks>
        /// To reach this quantity, a variable would have to be connected to this amount of
        /// other varibles. We could say that this is highly unlikely. In the event that this
        /// amount does get reached, we would probably need to do a sanity check.
        /// </remarks>
        private const int _maxMarkowitzCount = 46340;

        /// <summary>
        /// Gets the Markowitz row counts.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="row"/> is negative or greater than the number of Markowitz rows.
        /// </exception>
        public int RowCount(int row) => _markowitzRow[row];

        /// <summary>
        /// Gets the Markowitz column counts.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="column"/> is negative or greater than the number of Markowitz columns.
        /// </exception>
        public int ColumnCount(int column) => _markowitzColumn[column];

        /// <summary>
        /// Gets the Markowitz products.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if <paramref name="index"/> is negative or greater than the number of Markowitz size.
        /// </exception>
        public int Product(int index) => _markowitzProduct[index];

        /// <summary>
        /// Gets the magnitude.
        /// </summary>
        /// <value>
        /// The magnitude.
        /// </value>
        public Func<T, double> Magnitude { get; private set; }

        /// <summary>
        /// Gets the number of singletons.
        /// </summary>
        public int Singletons { get; private set; }

        /// <summary>
        /// Gets or sets the relative threshold for choosing a pivot.
        /// </summary>
        /// <value>
        /// The relative pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value is not greater than 0.
        /// </exception>
        [ParameterName("pivrel"), ParameterInfo("The relative threshold for validating pivots")]
        [GreaterThan(0), Finite]
        private double _relativePivotThreshold = 1e-3;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing a pivot.
        /// </summary>
        /// <value>
        /// The absolute pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value is negative.
        /// </exception>
        [ParameterName("pivtol"), ParameterInfo("The absolute threshold for validating pivots")]
        [GreaterThanOrEquals(0), Finite]
        private double _absolutePivotThreshold = 1e-13;

        /// <summary>
        /// Gets the strategies used for finding a pivot.
        /// </summary>
        /// <value>
        /// The strategies.
        /// </value>
        public Collection<MarkowitzSearchStrategy<T>> Strategies { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="Markowitz{T}" /> class.
        /// </summary>
        /// <param name="magnitude">The function for turning elements into a scalar.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="magnitude"/> is <c>null</c>.</exception>
        public Markowitz(Func<T, double> magnitude)
        {
            Magnitude = magnitude.ThrowIfNull(nameof(magnitude));

            // Register default strategies
            Strategies.Add(new MarkowitzSingleton<T>());
            Strategies.Add(new MarkowitzQuickDiagonal<T>());
            Strategies.Add(new MarkowitzDiagonal<T>());
            Strategies.Add(new MarkowitzEntireMatrix<T>());
        }

        private Markowitz()
        {
        }

        /// <summary>
        /// This method will check whether or not a pivot element is valid or not.
        /// It checks for the submatrix right/below of the pivot.
        /// </summary>
        /// <param name="pivot">The pivot candidate.</param>
        /// <param name="max">The maximum index that a pivot can have.</param>
        /// <returns>
        /// True if the pivot can be used.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pivot"/> is <c>null</c>.</exception>
        public bool IsValidPivot(ISparseMatrixElement<T> pivot, int max)
        {
            pivot.ThrowIfNull(nameof(pivot));
            if (pivot.Row > max || pivot.Column > max)
                return false;

            // Get the magnitude of the current pivot
            double magnitude = Magnitude(pivot.Value);

            // Search for the largest element below the pivot
            var element = pivot.Below;
            double largest = 0.0;
            while (element != null && element.Row <= max)
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
        /// Initializes the pivot searching algorithm.
        /// </summary>
        /// <param name="matrix">The matrix to use for initialization.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="matrix"/> is <c>null</c>.</exception>
        public void Initialize(IMatrix<T> matrix)
        {
            matrix.ThrowIfNull(nameof(matrix));

            // Allocate arrays
            _markowitzRow = new int[matrix.Size + 1];
            _markowitzColumn = new int[matrix.Size + 1];
            _markowitzProduct = new int[matrix.Size + 2];
        }

        /// <summary>
        /// Clears the pivot strategy.
        /// </summary>
        public void Clear()
        {
            _markowitzRow = null;
            _markowitzColumn = null;
            _markowitzProduct = null;
        }

        private void Count(ISparseMatrix<T> matrix, ISparseVector<T> rhs, int step, int max)
        {
            ISparseMatrixElement<T> element;

            // Get the first element in the vector
            var rhsElement = rhs.GetFirstInVector();

            // Generate Markowitz row count
            for (int i = max; i >= step; i--)
            {
                // Set count to -1 initially to remove count due to pivot element
                int count = -1;
                element = matrix.GetFirstInRow(i);
                while (element != null && element.Column < step)
                    element = element.Right;
                while (element != null) // We want to count the elements outside the limit as well
                {
                    count++;
                    element = element.Right;
                }

                // Include elements on the Rhs vector
                while (rhsElement != null && rhsElement.Index < step)
                    rhsElement = rhsElement.Below;
                if (rhsElement != null && rhsElement.Index == i)
                    count++;

                _markowitzRow[i] = Math.Min(count, _maxMarkowitzCount);
            }

            // Generate Markowitz column count
            for (int i = step; i <= max; i++)
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
                _markowitzColumn[i] = Math.Min(count, _maxMarkowitzCount);
            }
        }

        private void Products(int step, int max)
        {
            Singletons = 0;
            for (int i = step; i <= max; i++)
            {
                // UpdateMarkowitzProduct(i);
                _markowitzProduct[i] = _markowitzRow[i] * _markowitzColumn[i];
                if (_markowitzProduct[i] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Setup the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <param name="max">The maximum row/column index.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="matrix"/> or <paramref name="rhs"/> is <c>null</c>.
        /// </exception>
        public void Setup(ISparseMatrix<T> matrix, ISparseVector<T> rhs, int eliminationStep, int max)
        {
            matrix.ThrowIfNull(nameof(matrix));
            rhs.ThrowIfNull(nameof(rhs));

            // Initialize Markowitz row, column and product vectors if necessary
            if (_markowitzRow == null || _markowitzRow.Length != matrix.Size + 1)
                Initialize(matrix);
            Count(matrix, rhs, eliminationStep, max);
            Products(eliminationStep, max);
        }

        /// <summary>
        /// Move the pivot to the diagonal for this elimination step.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <remarks>
        /// This is done by swapping the rows and columns of the diagonal and that of the pivot.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="matrix"/>, <paramref name="rhs"/> or <paramref name="pivot"/> is <c>null</c>.
        /// </exception>
        public void MovePivot(ISparseMatrix<T> matrix, ISparseVector<T> rhs, ISparseMatrixElement<T> pivot, int eliminationStep)
        {
            matrix.ThrowIfNull(nameof(matrix));
            rhs.ThrowIfNull(nameof(rhs));
            pivot.ThrowIfNull(nameof(pivot));

            // If we haven't setup, just skip
            if (_markowitzProduct == null)
                return;
            int oldProduct;

            int row = pivot.Row;
            int col = pivot.Column;

            // If the pivot is a singleton, then we just consumed it
            if (_markowitzProduct[row] == 0 || _markowitzProduct[col] == 0)
                Singletons--;

            // Exchange rows
            if (row != eliminationStep)
            {
                // Swap row Markowitz numbers
                (_markowitzRow[eliminationStep], _markowitzRow[row]) = (_markowitzRow[row], _markowitzRow[eliminationStep]);

                // Update the Markowitz product
                oldProduct = _markowitzProduct[row];
                _markowitzProduct[row] = _markowitzRow[row] * _markowitzColumn[row];
                if (oldProduct == 0)
                {
                    if (_markowitzProduct[row] != 0)
                        Singletons--;
                }
                else
                {
                    if (_markowitzProduct[row] == 0)
                        Singletons++;
                }
            }

            // Exchange columns
            if (col != eliminationStep)
            {
                // Swap column Markowitz numbers
                (_markowitzColumn[eliminationStep], _markowitzColumn[col]) = (_markowitzColumn[col], _markowitzColumn[eliminationStep]);

                // Update the Markowitz product
                oldProduct = _markowitzProduct[col];
                _markowitzProduct[col] = _markowitzRow[col] * _markowitzColumn[col];
                if (oldProduct == 0)
                {
                    if (_markowitzProduct[col] != 0)
                        Singletons--;
                }
                else
                {
                    if (_markowitzProduct[col] == 0)
                        Singletons++;
                }
            }

            // Also update the moved pivot
            oldProduct = _markowitzProduct[eliminationStep];
            _markowitzProduct[eliminationStep] = _markowitzRow[eliminationStep] * _markowitzColumn[eliminationStep];
            if (oldProduct == 0)
            {
                if (_markowitzProduct[eliminationStep] != 0)
                    Singletons--;
            }
            else
            {
                if (_markowitzProduct[eliminationStep] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Update the strategy after the pivot was moved.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="limit">The maximum row/column for pivots.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="matrix"/> or <paramref name="pivot"/> is <c>null</c>.
        /// </exception>
        public void Update(ISparseMatrix<T> matrix, ISparseMatrixElement<T> pivot, int limit)
        {
            matrix.ThrowIfNull(nameof(matrix));
            pivot.ThrowIfNull(nameof(pivot));

            // If we haven't setup, just skip
            if (_markowitzProduct == null)
                return;

            // Go through all elements below the pivot. If they exist, then we can subtract 1 from the Markowitz row vector!
            for (var column = pivot.Below; column != null && column.Row <= limit; column = column.Below)
            {
                int row = column.Row;

                // Update the Markowitz product
                _markowitzProduct[row] -= _markowitzColumn[row];
                --_markowitzRow[row];

                // If we reached 0, then the row just turned to a singleton row
                if (_markowitzRow[row] == 0)
                    Singletons++;
            }

            // go through all elements right of the pivot. For every element, we can subtract 1 from the Markowitz column vector!
            for (var row = pivot.Right; row != null && row.Column <= limit; row = row.Right)
            {
                int column = row.Column;

                // Update the Markowitz product
                _markowitzProduct[column] -= _markowitzRow[column];
                --_markowitzColumn[column];

                // If we reached 0, then the column just turned to a singleton column
                // This only adds a singleton if the row wasn't detected as a singleton row first
                if (_markowitzColumn[column] == 0 && _markowitzRow[column] != 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Notifies the strategy that a fill-in has been created
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="fillin">The fill-in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="matrix"/> or <paramref name="fillin"/> is <c>null</c>.
        /// </exception>
        public void CreateFillin(ISparseMatrix<T> matrix, ISparseMatrixElement<T> fillin)
        {
            matrix.ThrowIfNull(nameof(matrix));
            fillin.ThrowIfNull(nameof(fillin));

            if (_markowitzProduct == null)
                return;

            // Update the markowitz row count
            int index = fillin.Row;
            _markowitzRow[index]++;
            _markowitzProduct[index] =
                Math.Min(_markowitzRow[index] * _markowitzColumn[index], _maxMarkowitzCount);
            if (_markowitzRow[index] == 1 && _markowitzColumn[index] != 0)
                Singletons--;

            // Update the markowitz column count
            index = fillin.Column;
            _markowitzColumn[index]++;
            _markowitzProduct[index] =
                Math.Min(_markowitzRow[index] * _markowitzColumn[index], _maxMarkowitzCount);
            if (_markowitzRow[index] != 0 && _markowitzColumn[index] == 1)
                Singletons--;
        }

        /// <summary>
        /// Find a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <param name="max">The maximum row/column index of any pivot.</param>
        /// <returns>The pivot information.</returns>
        /// <remarks>
        /// The pivot should be searched for in the submatrix towards the right and down of the
        /// current diagonal at row/column <paramref name="eliminationStep" />. This pivot element
        /// will be moved to the diagonal for this elimination step.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="eliminationStep"/> or <paramref name="max"/> not 1 or higher, 
        /// or <paramref name="eliminationStep"/> is higher than <paramref name="max"/>.
        /// </exception>
        public Pivot<ISparseMatrixElement<T>> FindPivot(ISparseMatrix<T> matrix, int eliminationStep, int max)
        {
            matrix.ThrowIfNull(nameof(matrix));
            eliminationStep.GreaterThan(nameof(eliminationStep), 0);
            max.GreaterThan(nameof(max), 0);

            // No pivot possible if we're already eliminating outside of our bounds
            if (eliminationStep > max)
                return Pivot<ISparseMatrixElement<T>>.Empty;

            // Fix the search limit to allow our strategies to work
            foreach (var strategy in Strategies)
            {
                var chosen = strategy.FindPivot(this, matrix, eliminationStep, max);
                if (chosen.Info != PivotInfo.None)
                    return chosen;
            }
            return Pivot<ISparseMatrixElement<T>>.Empty;
        }

        /// <inheritdoc/>
        public Markowitz<T> Clone()
        {
            var clone = new Markowitz<T>
            {
                Magnitude = Magnitude,
                _absolutePivotThreshold = _absolutePivotThreshold,
                _relativePivotThreshold = _relativePivotThreshold,
                Singletons = Singletons,
                _markowitzRow = (int[])_markowitzRow.Clone(),
                _markowitzColumn = (int[])_markowitzColumn.Clone(),
                _markowitzProduct = (int[])_markowitzProduct.Clone()
            };
            foreach (var strategy in Strategies)
                clone.Strategies.Add(strategy);
            return clone;
        }
    }
}
