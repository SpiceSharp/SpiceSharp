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
        /// <returns>
        /// <c>true</c> if the matrix was succesfully factored; otherwise <c>false</c>.
        /// </returns>
        public override bool Factor() => Factor(Size);

        /// <summary>
        /// Factors the matrix for a number of steps.
        /// </summary>
        /// <remarks>
        /// This method can be used as an optimization. Instead of reallocating a new matrix for varying
        /// sizes, you can use this method to only factorize part of the matrix inexpensively. Beware,
        /// when you have used <see cref="OrderAndFactor"/> the first steps may not coincide with the
        /// first variables in the vector!
        /// </remarks>
        /// <param name="steps">The number of elimination steps.</param>
        /// <returns>
        /// <c>true</c> if the matrix was succesfully factored; otherwise <c>false</c>.
        /// </returns>
        public bool Factor(int steps)
        {
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];
            if (steps > Size)
                throw new AlgebraException("Cannot factorize more than {0} steps".FormatString(Size));

            // Start factorization
            for (var step = 1; step <= steps; step++)
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
                for (var r = step + 1; r <= steps; r++)
                {
                    Matrix[r, step] *= diagonal;
                    for (var c = step + 1; c <= steps; c++)
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
            => Solve(solution, Size);

        /// <summary>
        /// Solves the system of equations with a matrix that was factored for a number of steps.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="steps">The steps.</param>
        public void Solve(IVector<double> solution, int steps)
        {
            solution.ThrowIfNull(nameof(solution));
            if (steps > Size || steps > solution.Length)
                throw new AlgebraException("Cannot solve for more than {0} elements".FormatString(Math.Max(Size, solution.Length)));
            if (!IsFactored)
                throw new AlgebraException("Solver is not yet factored");
            if (solution.Length != Size)
                throw new AlgebraException("Solution vector and solver order does not match");

            // Forward substitution
            for (var i = 1; i <= steps; i++)
            {
                _intermediate[i] = Vector[i];
                for (var j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * Vector[j];
            }

            // Backward substitution
            _intermediate[steps] *= Matrix[steps, steps];
            for (var i = steps - 1; i >= 1; i--)
            {
                for (var j = i + 1; j <= steps; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Column.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Solves the transposed.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public override void SolveTransposed(IVector<double> solution)
            => SolveTransposed(solution, Size);

        /// <summary>
        /// Solves the transposed.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="steps">The steps.</param>
        public void SolveTransposed(IVector<double> solution, int steps)
        {
            solution.ThrowIfNull(nameof(solution));
            if (steps > Size || steps > solution.Length)
                throw new AlgebraException("Cannot solve for more than {0} elements".FormatString(Math.Max(Size, solution.Length)));
            if (!IsFactored)
                throw new AlgebraException("Solver is not yet factored");
            if (solution.Length != Size)
                throw new AlgebraException("Solution vector and solver order does not match");

            // Scramble
            for (var i = 1; i <= steps; i++)
            {
                var newIndex = Column[Row.Reverse(i)];
                _intermediate[newIndex] = Vector[i];
            }

            // Forward substitution
            for (var i = 1; i <= steps; i++)
            {
                for (var j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * Vector[j];
            }

            // Backward substitution
            _intermediate[steps] *= Matrix[steps, steps];
            for (var i = steps - 1; i >= 1; i--)
            {
                for (var j = i + 1; j <= steps; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Row.Unscramble(_intermediate, solution);
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
