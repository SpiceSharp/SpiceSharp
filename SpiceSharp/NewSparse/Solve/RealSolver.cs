using System;
using System.Diagnostics;
using SpiceSharp.NewSparse.Solve;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Class for solving real matrices
    /// </summary>
    public class RealSolver : Solver<double>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strategy">Strategy</param>
        public RealSolver(PivotStrategy<double> strategy)
            : base(strategy)
        {
        }

        /// <summary>
        /// Factor the matrix
        /// </summary>
        public override void Factor()
        {
            Element<double> element, column;

            // Get the diagonal
            element = Matrix.GetDiagonalElement(1);
            if (element.Value.Equals(0))
                throw new Exception("Zero pivot");

            // pivot = 1 / pivot
            element.Value = 1.0 / element.Value;

            // TODO: maybe we should cache this
            Element<double>[] dest = new Element<double>[Matrix.Size + 1];

            // Start factorization
            double mult;
            for (int step = 2; step <= Matrix.Size; step++)
            {
                // Scatter
                element = Matrix.GetFirstInColumn(step);
                while (element != null)
                {
                    dest[element.Row] = element;
                    element = element.Below;
                }

                // Update column
                column = Matrix.GetFirstInColumn(step);
                while (column.Row < step)
                {
                    element = Matrix.GetDiagonalElement(column.Row);

                    // Mult = dest[row] / pivot
                    mult = dest[column.Row].Value * element.Value;
                    dest[column.Row].Value = mult;
                    element = element.Below;
                    while (element != null)
                    {
                        // dest[element.Row] -= mult * element
                        dest[element.Row].Value -= mult * element.Value;
                        element = element.Below;
                    }
                    column = column.Below;
                }

                // Check for a singular matrix
                element = Matrix.GetDiagonalElement(step);
                if (element == null || element.Value.Equals(0.0))
                    throw new Exception("Zero pivot");
                element.Value = 1.0 / element.Value;
            }
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side
        /// </summary>
        /// <param name="solution">Solution vector</param>
        public override void Solve(Vector<double> solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // TODO: Maybe we should cache intermediate
            // Scramble
            var intermediate = new double[Rhs.Length];
            for (int i = 0; i < Rhs.Length; i++)
            {
                intermediate[i] = Rhs[i];
            }

            // Forward substitution
            for (int i = 1; i <= Matrix.Size; i++)
            {
                double temp = intermediate[i];

                // This step of the substitution is skipped if temp == 0.0
                if (!temp.Equals(0.0))
                {
                    var pivot = Matrix.GetDiagonalElement(i);

                    // temp = temp / pivot
                    temp *= pivot.Value;
                    intermediate[i] = temp;
                    var element = pivot.Below;
                    while (element != null)
                    {
                        // intermediate[row] -= temp * element
                        intermediate[element.Row] -= temp * element.Value;
                        element = element.Below;
                    }
                }
            }

            // Backward substitution
            for (int i = Matrix.Size; i > 0; i--)
            {
                double temp = intermediate[i];
                var pivot = Matrix.GetDiagonalElement(i);
                var element = pivot.Right;

                while (element != null)
                {
                    // temp -= element * intermediate[column]
                    temp -= element.Value * intermediate[element.Column];
                    element = element.Right;
                }
                intermediate[i] = temp;
            }

            // Unscrable
            for (int i = Matrix.Size; i > 0; i--)
                solution[i] = intermediate[i];
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side (transposed)
        /// </summary>
        /// <param name="solution">Solution vector</param>
        public override void SolveTransposed(Vector<double> solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // TODO: Maybe we should cache intermediate
            // Scramble
            var intermediate = new double[Rhs.Length];
            for (int i = 0; i < intermediate.Length; i++)
            {
                intermediate[i] = Rhs[i];
            }

            // Forward elimination
            for (int i = 1; i <= Matrix.Size; i++)
            {
                double temp = intermediate[i];

                // This step of the elimination is skipped if temp equals 0
                if (!temp.Equals(0.0))
                {
                    var element = Matrix.GetDiagonalElement(i).Right;
                    while (element != null)
                    {
                        // intermediate[col] -= temp * element
                        intermediate[element.Column] -= temp * element.Value;
                        element = element.Right;
                    }
                }
            }

            // Backward substitution
            for (int i = Matrix.Size; i > 0; i--)
            {
                double temp = intermediate[i];

                var pivot = Matrix.GetDiagonalElement(i);
                var element = pivot.Below;
                while (element != null)
                {
                    // temp -= intermediate[element.row] * element
                    temp -= intermediate[element.Row] * element.Value;
                    element = element.Below;
                }

                // intermediate = temp / pivot
                intermediate[i] *= pivot.Value;
            }

            // Unscramble
            for (int i = Matrix.Size; i > 0; i--)
                solution[i] = intermediate[i];
        }

        /// <summary>
        /// Factor while reordering the matrix
        /// </summary>
        public override void OrderAndFactor()
        {
            int step = 1;
            if (!NeedsReordering)
            {
                // Matrix has been factored before and reordering is not required
                for (step = 1; step <= Matrix.Size; step++)
                {
                    var pivot = Matrix.GetDiagonalElement(step);
                    double largest = LargestInColumn(pivot.Below);
                    if (largest * RelativePivotThreshold < Math.Abs(pivot.Value))
                        Elimination(pivot);
                    else
                    {
                        NeedsReordering = true;
                        break;
                    }
                }

                // Done!
                if (!NeedsReordering)
                    return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            // Setup for reordering
            Strategy.Setup(Matrix, Rhs, step);
            sw.Stop();
            Console.WriteLine($"Setup: {sw.ElapsedTicks} ticks");

            // Perform reordering and factorization starting from where we stopped last time
            for (; step <= Matrix.Size; step++)
            {
                swPivot.Start();
                var pivot = Strategy.FindPivot(Matrix, step);
                if (pivot == null)
                    throw new Exception("Singular matrix");
                swPivot.Stop();

                // Move the pivot to the current diagonal
                MovePivot(pivot, step);

                swEliminate.Start();
                // Elimination
                Elimination(pivot);
                swEliminate.Stop();
            }

            Console.WriteLine($"Pivot {swPivot.ElapsedTicks} ticks");
            Console.WriteLine($"Exchange {swExchange.ElapsedTicks} ticks");
            Console.WriteLine($"Eliminate {swEliminate.ElapsedTicks} ticks");
        }

        /// <summary>
        /// Eliminate a row
        /// </summary>
        /// <param name="pivot">Current pivot</param>
        void Elimination(Element<double> pivot)
        {
            // Test for zero pivot
            if (pivot.Value.Equals(0.0))
                throw new Exception("Matrix is singular");
            pivot.Value = 1.0 / pivot.Value;

            var upper = pivot.Right;
            while (upper != null)
            {
                // Calculate upper triangular element
                // upper = upper / pivot
                upper.Value *= pivot.Value;

                var sub = upper.Below;
                var lower = pivot.Below;
                while (lower != null)
                {
                    int row = lower.Row;

                    // Find element in row that lines up with the current lower triangular element
                    while (sub != null && sub.Row < row)
                        sub = sub.Below;

                    // Test to see if the desired element was not found, if not, create fill-in
                    if (sub == null || sub.Row > row)
                        sub = base.CreateFillin(row, upper.Column);

                    // element -= upper * lower
                    sub.Value -= upper.Value * lower.Value;
                    sub = sub.Below;
                    lower = lower.Below;
                }
                upper = upper.Right;
            }
        }

        /// <summary>
        /// Find the largest magnitude in the column at or below <paramref name="element"/>
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns></returns>
        double LargestInColumn(Element<double> element)
        {
            double magnitude, largest = 0;

            // Search the column for the largest element beginning at element
            while (element != null)
            {
                magnitude = Math.Abs(element.Value);
                if (magnitude > largest)
                    largest = magnitude;
                element = element.Below;
            }
            return largest;
        }
    }
}
