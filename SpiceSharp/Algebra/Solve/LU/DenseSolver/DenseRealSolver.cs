using SpiceSharp.Algebra.Solve;
using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving dense sets of equations with real numbers.
    /// </summary>
    /// <seealso cref="DenseLUSolver{T}" />
    public partial class DenseRealSolver : DenseLUSolver<double>
    {
        private double[] _intermediate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseRealSolver"/> class.
        /// </summary>
        public DenseRealSolver()
            : base(Math.Abs)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseRealSolver"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public DenseRealSolver(int size)
            : base(size, Math.Abs)
        {
        }

        /// <inheritdoc/>
        public override void Solve(IVector<double> solution)
            => Solve(solution, Size);

        /// <summary>
        /// Solves the system of equations with a matrix that was factored for a number of steps.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="size">The size of the submatrix to be solved.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution"/> is <c>null</c>.</exception>
        /// <exception cref="AlgebraException">Thrown if the solver is not factored yet.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="solution"/> does not have <see cref="ISolver{T}.Size"/> elements.
        /// </exception>
        public void Solve(IVector<double> solution, int size)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored.FormatString(nameof(Solve)));
            if (solution.Length != Size)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));

            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];
            size = Math.Min(size, Size);
            var order = Math.Min(size, Size - Degeneracy);

            // Fill in the values from the solution for degenerate cases
            for (var i = order + 1; i <= Size; i++)
                _intermediate[i] = solution[Column.Reverse(i)];

            // Forward substitution
            for (var i = 1; i <= order; i++)
            {
                _intermediate[i] = Vector[i];
                for (var j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
            }

            // Backward substitution
            for (var i = order; i >= 1; i--)
            {
                for (var j = i + 1; j <= size; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Column.Unscramble(_intermediate, solution);
        }

        /// <inheritdoc/>
        public override void SolveTransposed(IVector<double> solution)
            => SolveTransposed(solution, Size);

        /// <summary>
        /// Solves the transposed.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="steps">The steps.</param>
        /// <exception cref="AlgebraException">Thrown if the solver is not factored yet.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="solution" /> does not have <see cref="ISolver{T}.Size" /> elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution" /> is <c>null</c>.</exception>
        public void SolveTransposed(IVector<double> solution, int steps)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored);
            if (solution.Length != Size)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));

            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];
            steps = Math.Max(steps, Size);

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

        /// <inheritdoc/>
        protected override void Eliminate(int step, int size)
        {
            var diagonal = Matrix[step, step];
            if (diagonal.Equals(0.0))
                throw new AlgebraException(Properties.Resources.Algebra_InvalidPivot.FormatString(step));
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
        }
    }
}
