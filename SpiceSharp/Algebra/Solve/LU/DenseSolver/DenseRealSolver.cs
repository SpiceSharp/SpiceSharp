using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving dense sets of equations with real numbers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.DenseLUSolver{M, V, T}" />
    public class DenseRealSolver<M, V> : DenseLUSolver<M, V, double>
        where M : IPermutableMatrix<double>
        where V : IPermutableVector<double>
    {
        private double[] _intermediate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseRealSolver{M, V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        public DenseRealSolver(M matrix, V vector)
            : base(matrix, vector, new RookPivoting<double>(Math.Abs))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseRealSolver{M, V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="strategy">The strategy.</param>
        public DenseRealSolver(M matrix, V vector, DensePivotStrategy<double> strategy)
            : base(matrix, vector, strategy)
        {
        }

        /// <summary>
        /// Factors the matrix.
        /// </summary>
        /// <returns>True if factoring was successful.</returns>
        public override bool Factor()
        {
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];

            // Start factorization
            for (var step = 1; step <= Size; step++)
            {
                // Check for a singular matrix
                if (Matrix[step, step].Equals(0.0))
                {
                    IsFactored = false;
                    return false;
                }
                var diagonal = 1.0 / Matrix[step, step];
                Matrix[step, step] = diagonal;

                // Doolittle algorithm
                for (var r = step + 1; r <= Size; r++)
                {
                    Matrix[r, step] *= diagonal;
                    for (var c = step + 1; c <= Size; c++)
                        Matrix[r, c] -= Matrix[r, step] * Matrix[step, c];
                }
            }

            IsFactored = true;
            return true;
        }

        /// <summary>
        /// Solves the system of equations.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the set of equations.</param>
        public override void Solve(IVector<double> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException("Solver is not yet factored");

            // Scramble
            for (var i = 1; i <= Size; i++)
                _intermediate[i] = Vector[i];

            // Forward substitution
            for (var i = 1; i <= Matrix.Size; i++)
            {
                _intermediate[i] = Vector[i];
                for (var j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * Vector[j];
            }

            // Backward substitution
            _intermediate[Size] *= Matrix[Size, Size];
            for (var i = Size - 1; i >= 1; i--)
            {
                for (var j = i + 1; j <= Size; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Column.Unscramble(_intermediate, solution);
        }

        public override void SolveTransposed(IVector<double> solution)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Orders and factors the matrix.
        /// </summary>
        public override void OrderAndFactor()
        {
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];

            var step = 1;
            if (!NeedsReordering)
            {
                // Matrix has been factored before and reordering is not required
                for (step = 1; step <= Matrix.Size; step++)
                {
                    if (Strategy.IsValidPivot(Matrix, step))
                        Elimination(step);
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

            // Setup reordering
            Strategy.Setup(Matrix, Vector, step);

            // Perform reordering and factorization starting from where we stopped last
            for (; step <= Matrix.Size; step++)
            {
                if (!Strategy.FindPivot(Matrix, step, out int row, out int column))
                    throw new AlgebraException("Singular matrix");

                // Move the pivot to the current diagonal
                SwapRows(row, step);
                SwapColumns(column, step);

                // Elimination
                Elimination(step);
            }

            // Flag the solver as factored
            IsFactored = true;
            NeedsReordering = false;
        }

        /// <summary>
        /// Eliminate a row.
        /// </summary>
        /// <param name="step">The current elimination step.</param>
        private void Elimination(int step)
        {
            var diagonal = Matrix[step, step];
            if (diagonal.Equals(0.0))
                throw new AlgebraException("Matrix is singular");
            diagonal = 1.0 / diagonal;
            Matrix[step, step] = diagonal;

            for (var r = step + 1; r <= Size; r++)
            {
                Matrix[r, step] *= diagonal;
                for (var c = step + 1; c <= Size; c++)
                    Matrix[r, c] -= Matrix[r, step] * Matrix[step, c];
            }
        }
    }
}
