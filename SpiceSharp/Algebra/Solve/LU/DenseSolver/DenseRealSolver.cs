using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving dense sets of equations with real numbers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <seealso cref="DenseLUSolver{M, V, T}" />
    public partial class DenseRealSolver<M, V> : DenseLUSolver<M, V, double>
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
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];

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
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];

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
        /// Eliminates the submatrix right and below the pivot.
        /// </summary>
        /// <param name="step">The current elimination step.</param>
        /// <param name="size">The maximum row/column to be eliminated.</param>
        /// <returns>
        /// <c>true</c> if the elimination was succesful; otherwise <c>false</c>.
        /// </returns>
        protected override bool Elimination(int step, int size)
        {
            var diagonal = Matrix[step, step];
            if (diagonal.Equals(0.0))
                return false;
            diagonal = 1.0 / diagonal;
            Matrix[step, step] = diagonal;

            for (var r = step + 1; r <= size; r++)
            {
                var lead = Matrix[r, step];
                if (lead.Equals(0.0))
                    continue;
                lead *= diagonal;
                Matrix[r, step] = lead;

                for (var c = step + 1; c <= size; c++)
                    Matrix[r, c] -= lead * Matrix[step, c];
            }
            return true;
        }
    }
}
