using System;
using SpiceSharp.Algebra.Solve;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving sets of equations with real numbers.
    /// </summary>
    /// <seealso cref="SpiceSharp.Algebra.Solver{Double}" />
    public class RealSolver : Solver<double>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private double[] _intermediate;
        private MatrixElement<double>[] _dest;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealSolver"/> class.
        /// </summary>
        public RealSolver()
            : base(new Markowitz<double>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealSolver"/> class.
        /// </summary>
        /// <param name="size">The number of equations and variables.</param>
        public RealSolver(int size)
            : base(new Markowitz<double>(), size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealSolver"/> class.
        /// </summary>
        /// <param name="size">The number of equations and variables.</param>
        /// <param name="strategy">The pivot strategy.</param>
        public RealSolver(int size, PivotStrategy<double> strategy)
            : base(strategy, size)
        {
        }

        /// <summary>
        /// Fix the number of equations and variables.
        /// </summary>
        /// <remarks>
        /// This method can be used to make sure that the matrix is fixed during
        /// solving. When fixed, it is impossible to add more elements to the sparse
        /// matrix or vector.
        /// </remarks>
        public override void FixEquations()
        {
            base.FixEquations();
            _intermediate = new double[Order + 1];
            _dest = new MatrixElement<double>[Order + 1];
        }

        /// <summary>
        /// Unfix the number of equations and variables.
        /// </summary>
        public override void UnfixEquations()
        {
            base.UnfixEquations();
            _intermediate = null;
            _dest = null;
        }

        /// <summary>
        /// Factor the matrix.
        /// </summary>
        /// <returns>
        /// True if factoring was successful.
        /// </returns>
        public override bool Factor()
        {
            if (!IsFixed)
                FixEquations();

            // Get the diagonal
            var element = Matrix.GetDiagonalElement(1);
            if (element == null || element.Value.Equals(0))
                return false;

            // pivot = 1 / pivot
            element.Value = 1.0 / element.Value;

            // Start factorization
            for (var step = 2; step <= Matrix.Size; step++)
            {
                // Scatter
                element = Matrix.GetFirstInColumn(step);
                while (element != null)
                {
                    _dest[element.Row] = element;
                    element = element.Below;
                }

                // Update column
                var column = Matrix.GetFirstInColumn(step);
                while (column.Row < step)
                {
                    element = Matrix.GetDiagonalElement(column.Row);

                    // Mult = dest[row] / pivot
                    var mult = _dest[column.Row].Value * element.Value;
                    _dest[column.Row].Value = mult;
                    while ((element = element.Below) != null)
                    {
                        // dest[element.Row] -= mult * element
                        _dest[element.Row].Value -= mult * element.Value;
                    }
                    column = column.Below;
                }

                // Check for a singular matrix
                element = Matrix.GetDiagonalElement(step);
                if (element == null || element.Value.Equals(0.0))
                {
                    IsFactored = false;
                    return false;
                }
                element.Value = 1.0 / element.Value;
            }

            IsFactored = true;
            return true;
        }

        /// <summary>
        /// Solve the system of equations.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the set of equations.</param>
        public override void Solve(Vector<double> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new SparseException("Solver is not yet factored");

            // Scramble
            var rhsElement = Rhs.First;
            var index = 0;
            while (rhsElement != null)
            {
                while (index < rhsElement.Index)
                    _intermediate[index++] = 0.0;
                _intermediate[index++] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }
            while (index <= Order)
                _intermediate[index++] = 0.0;

            // Forward substitution
            for (var i = 1; i <= Matrix.Size; i++)
            {
                var temp = _intermediate[i];

                // This step of the substitution is skipped if temp == 0.0
                if (!temp.Equals(0.0))
                {
                    var pivot = Matrix.GetDiagonalElement(i);

                    // temp = temp / pivot
                    temp *= pivot.Value;
                    _intermediate[i] = temp;
                    var element = pivot.Below;
                    while (element != null)
                    {
                        // intermediate[row] -= temp * element
                        _intermediate[element.Row] -= temp * element.Value;
                        element = element.Below;
                    }
                }
            }

            // Backward substitution
            for (var i = Matrix.Size; i > 0; i--)
            {
                var temp = _intermediate[i];
                var pivot = Matrix.GetDiagonalElement(i);
                var element = pivot.Right;

                while (element != null)
                {
                    // temp -= element * intermediate[column]
                    temp -= element.Value * _intermediate[element.Column];
                    element = element.Right;
                }
                _intermediate[i] = temp;
            }

            // Unscramble
            Column.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Solve the transposed problem.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the transposed set of equations.</param>
        public override void SolveTransposed(Vector<double> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new SparseException("Solver is not yet factored");

            // Scramble
            var rhsElement = Rhs.First;
            for (var i = 0; i <= Order; i++)
                _intermediate[i] = 0.0;
            while (rhsElement != null)
            {
                var newIndex = Column[Row.Reverse(rhsElement.Index)];
                _intermediate[newIndex] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }

            // Forward elimination
            for (var i = 1; i <= Matrix.Size; i++)
            {
                var temp = _intermediate[i];

                // This step of the elimination is skipped if temp equals 0
                if (!temp.Equals(0.0))
                {
                    var element = Matrix.GetDiagonalElement(i).Right;
                    while (element != null)
                    {
                        // intermediate[col] -= temp * element
                        _intermediate[element.Column] -= temp * element.Value;
                        element = element.Right;
                    }
                }
            }

            // Backward substitution
            for (var i = Matrix.Size; i > 0; i--)
            {
                var temp = _intermediate[i];

                var pivot = Matrix.GetDiagonalElement(i);
                var element = pivot.Below;
                while (element != null)
                {
                    // temp -= intermediate[element.row] * element
                    temp -= _intermediate[element.Row] * element.Value;
                    element = element.Below;
                }

                // intermediate = temp / pivot
                _intermediate[i] = temp * pivot.Value;
            }

            // Unscramble
            Row.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Factor while reordering the matrix
        /// </summary>
        public override void OrderAndFactor()
        {
            if (!IsFixed)
                FixEquations();

            var step = 1;
            if (!NeedsReordering)
            {
                // Matrix has been factored before and reordering is not required
                for (step = 1; step <= Matrix.Size; step++)
                {
                    var pivot = Matrix.GetDiagonalElement(step);
                    if (Strategy.IsValidPivot(pivot))
                        Elimination(pivot);
                    else
                    {
                        NeedsReordering = true;
                        break;
                    }
                }

                // Done!
                if (!NeedsReordering)
                {
                    IsFactored = true;
                    return;
                }
            }

            // Setup for reordering
            Strategy.Setup(Matrix, Rhs, step, Math.Abs);

            // Perform reordering and factorization starting from where we stopped last time
            for (; step <= Matrix.Size; step++)
            {
                var pivot = Strategy.FindPivot(Matrix, step);
                if (pivot == null)
                    throw new SparseException("Singular matrix");

                // Move the pivot to the current diagonal
                MovePivot(pivot, step);

                // Elimination
                Elimination(pivot);
            }

            // Flag the solver a sfactored
            IsFactored = true;
            NeedsReordering = false;
        }

        /// <summary>
        /// Eliminate a row.
        /// </summary>
        /// <param name="pivot">The current pivot element.</param>
        private void Elimination(MatrixElement<double> pivot)
        {
            // Test for zero pivot
            if (pivot.Value.Equals(0.0))
                throw new SparseException("Matrix is singular");
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
                    var row = lower.Row;

                    // Find element in row that lines up with the current lower triangular element
                    while (sub != null && sub.Row < row)
                        sub = sub.Below;

                    // Test to see if the desired element was not found, if not, create fill-in
                    if (sub == null || sub.Row > row)
                        sub = CreateFillin(row, upper.Column);

                    // element -= upper * lower
                    sub.Value -= upper.Value * lower.Value;
                    sub = sub.Below;
                    lower = lower.Below;
                }
                upper = upper.Right;
            }
        }
    }
}
