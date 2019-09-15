﻿using System;
using SpiceSharp.Algebra.Solve;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving sparse sets of equations with real numbers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.SparseLUSolver{M, V, T}" />
    public class SparseRealSolver<M, V> : SparseLUSolver<M, V, double>
        where M : IPermutableMatrix<double>, ISparseMatrix<double>, IElementMatrix<double>
        where V : IPermutableVector<double>, ISparseVector<double>, IElementVector<double>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private double[] _intermediate;
        private IMatrixElement<double>[] _dest;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseRealSolver{M, V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        public SparseRealSolver(M matrix, V vector)
            : base(matrix, vector, new Markowitz<double>(Math.Abs))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseRealSolver{M,V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="strategy">The pivot strategy.</param>
        public SparseRealSolver(M matrix, V vector, SparsePivotStrategy<double> strategy)
            : base(matrix, vector, strategy)
        {
        }
        
        /// <summary>
        /// Factor the matrix.
        /// </summary>
        /// <returns>
        /// True if factoring was successful.
        /// </returns>
        public override bool Factor()
        {
            if (_intermediate == null || _intermediate.Length != Size + 1)
            {
                _intermediate = new double[Size + 1];
                _dest = new IMatrixElement<double>[Size + 1];
            }

            // Get the diagonal
            var element = Matrix.FindDiagonalElement(1);
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
                    element = Matrix.FindDiagonalElement(column.Row);

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
                element = Matrix.FindDiagonalElement(step);
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
        public override void Solve(IVector<double> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException("Solver is not yet factored");

            // Scramble
            var rhsElement = Vector.GetFirstInVector();
            var index = 0;
            while (rhsElement != null)
            {
                while (index < rhsElement.Index)
                    _intermediate[index++] = 0.0;
                _intermediate[index++] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }
            while (index <= Size)
                _intermediate[index++] = 0.0;

            // Forward substitution
            for (var i = 1; i <= Matrix.Size; i++)
            {
                var temp = _intermediate[i];

                // This step of the substitution is skipped if temp == 0.0
                if (!temp.Equals(0.0))
                {
                    var pivot = Matrix.FindDiagonalElement(i);

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
                var pivot = Matrix.FindDiagonalElement(i);
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
        public override void SolveTransposed(IVector<double> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException("Solver is not yet factored");

            // Scramble
            var rhsElement = Vector.GetFirstInVector();
            for (var i = 0; i <= Size; i++)
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
                    var element = Matrix.FindDiagonalElement(i).Right;
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

                var pivot = Matrix.FindDiagonalElement(i);
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
            if (_intermediate == null || _intermediate.Length != Size + 1)
            {
                _intermediate = new double[Size + 1];
                _dest = new IMatrixElement<double>[Size + 1];
            }

            var step = 1;
            if (!NeedsReordering)
            {
                // Matrix has been factored before and reordering is not required
                for (step = 1; step <= Matrix.Size; step++)
                {
                    var pivot = Matrix.FindDiagonalElement(step);
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
            Strategy.Setup(Matrix, Vector, step);

            // Perform reordering and factorization starting from where we stopped last time
            for (; step <= Matrix.Size; step++)
            {
                var pivot = Strategy.FindPivot(Matrix, step);
                if (pivot == null)
                    throw new AlgebraException("Singular matrix");

                // Move the pivot to the current diagonal
                MovePivot(pivot, step);

                // Elimination
                Elimination(pivot);
            }

            // Flag the solver as factored
            IsFactored = true;
            NeedsReordering = false;
        }

        /// <summary>
        /// Eliminate a row.
        /// </summary>
        /// <param name="pivot">The current pivot element.</param>
        private void Elimination(ISparseMatrixElement<double> pivot)
        {
            // Test for zero pivot
            if (pivot.Value.Equals(0.0))
                throw new AlgebraException("Matrix is singular");
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
