using System;
using System.Collections.Generic;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Markowitz
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Markowitz<T> : PivotStrategy<T> where T : IFormattable
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
        /// Gets the Markowitz row counts
        /// </summary>
        public Vector<int> Row { get; private set; }

        /// <summary>
        /// Gets the Markowitz column counts
        /// </summary>
        public Vector<int> Column { get; private set; }

        /// <summary>
        /// Gets the Markowitz products
        /// </summary>
        public Vector<long> Product { get; private set; }

        /// <summary>
        /// Gets the number of singletons
        /// </summary>
        public int Singletons { get; private set; }

        /// <summary>
        /// Gets the magnitude method
        /// </summary>
        public Func<T, double> Magnitude { get; }

        /// <summary>
        /// Private variables
        /// </summary>
        List<MarkowitzSearchStrategy<T>> strategies = new List<MarkowitzSearchStrategy<T>>();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="magnitude">Magnitude</param>
        public Markowitz(Func<T, double> magnitude)
        {
            // Magnitude method
            Magnitude = magnitude;

            // Register default strategies
            strategies.Add(new MarkowitzSingleton<T>());
            strategies.Add(new MarkowitzEntireMatrix<T>());
        }

        /// <summary>
        /// Add a strategy for finding a pivot using Markowitz products
        /// </summary>
        /// <param name="strategy">Strategy</param>
        public void AddStrategy(MarkowitzSearchStrategy<T> strategy) => strategies.Insert(0, strategy);

        /// <summary>
        /// Remove a strategy for find pivots
        /// </summary>
        /// <param name="strategy"></param>
        public void RemoveStrategy(MarkowitzSearchStrategy<T> strategy) => strategies.Remove(strategy);

        /// <summary>
        /// Clear all strategies for pivots
        /// </summary>
        public void ClearStrategies() => strategies.Clear();

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public void Initialize(Matrix<T> matrix)
        {
            // Allocate arrays
            Row = new Vector<int>(matrix.Size + 1);
            Column = new Vector<int>(matrix.Size + 1);
            Product = new Vector<long>(matrix.Size + 2);
        }

        /// <summary>
        /// Count Markowitz numbers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="step">Step</param>
        void Count(Matrix<T> matrix, Vector<T> rhs, int step)
        {
            Element<T> element;

            // Generate Markowitz row count
            for (int i = step; i <= matrix.Size; i++)
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

                // Include nonzero elements in the rhs vector
                if (rhs != null && !rhs[i].Equals(default(T)))
                    count++;
                Row[i] = count;
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
                Column[i] = count;
            }
        }

        /// <summary>
        /// Calculate Markowitz products
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        void Products(Matrix<T> matrix, int step)
        {
            Singletons = 0;
            for (int i = step; i <= matrix.Size; i++)
            {
                UpdateMarkowitzProduct(i);
                if (Product[i] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Setup the pivoting strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="step">Step</param>
        public override void Setup(Matrix<T> matrix, Vector<T> rhs, int step)
        {
            // Initialize Markowitz row, column and product vectors if necessary
            if (Row == null || Row.Length != matrix.Size + 1)
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
        public override void MovePivot(Matrix<T> matrix, Vector<T> rhs, Element<T> pivot, int step)
        {
            int row = pivot.Row;
            int column = pivot.Column;

            // Exchange rows and column
            if (row == column)
            {
                // Swap row values
                var tmp = Product[step];
                Product[step] = Product[row];
                Product[row] = tmp;
            }
            else
            {
                // Exchange rows
                if (pivot.Row != step)
                {
                    // Swap row Markowitz numbers
                    int tmp = Row[row];
                    Row[row] = Row[step];
                    Row[step] = tmp;
                }

                // Exchange columns
                if (column != step)
                {
                    // Swap column Markowitz numbers
                    int tmp = Column[column];
                    Column[column] = Column[step];
                    Column[step] = tmp;
                }

                // We will update the Markowtiz products later after the pivot has moved
            }
        }

        /// <summary>
        /// Update the pivoting strategy
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public override void Update(Matrix<T> matrix, Element<T> pivot, int step)
        {
            // TODO: Check this method because I'm sceptical for its correctness

            // Go through all elements below the pivot. If they exist, then we can subtract 1 from the Markowitz row vector!
            for (Element<T> column = pivot.Below; column != null; column = column.Below)
            {
                int row = column.Row;
                --Row[row];

                // Update the Markowitz product
                UpdateMarkowitzProduct(row);
                if (Row[row] == 0)
                    Singletons++;
            }

            // go through all elements right of the pivot. For every element, we can subtract 1 from the Markowitz column vector!
            for (Element<T> row = pivot.Right; row != null; row = row.Right)
            {
                int column = row.Column;
                --Column[column];

                // Update the Markowitz product
                UpdateMarkowitzProduct(column);
                if (Column[column] == 0 && Row[column] != 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Calculate the Markowitz product for a specific index
        /// </summary>
        /// <param name="index">Index</param>
        void UpdateMarkowitzProduct(int index)
        {
            int rowCount = Row[index];
            int columnCount = Column[index];

            // Make sure there is no overflow
            if ((rowCount > short.MaxValue && columnCount != 0)
                || (columnCount > short.MaxValue && rowCount != 0))
            {
                double product = (double)columnCount * rowCount;
                if (product >= long.MaxValue)
                    Product[index] = long.MaxValue;
                else
                    Product[index] = (long)product;
            }
            else
                Product[index] = rowCount * columnCount;
        }

        /// <summary>
        /// Find a pivot
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        /// <returns></returns>
        public override Element<T> FindPivot(Matrix<T> matrix, int step)
        {
            foreach (var strategy in strategies)
            {
                Element<T> chosen = strategy.FindPivot(this, matrix, step);
                if (chosen != null)
                    return chosen;
            }
            return null;
        }
    }
}
