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

        /// <inheritdoc />
        public override void ForwardSubstitute(IVector<double> solution)
            => ForwardSubstitute(solution, Size);

        /// <summary>
        /// Applies forward substitution, but limits to the given size.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="size">The size.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution"/> is <c>null</c>.</exception>
        /// <exception cref="AlgebraException">Thrown if the matrix is not yet factored.</exception>
        /// <exception cref="ArgumentException">Thrown if the solution vector is not of size <see cref="PivotingSolver{M, V, T}.Size"/>.</exception>
        public void ForwardSubstitute(IVector<double> solution, int size)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored.FormatString(nameof(Solve)));
            if (solution.Length != Size)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new double[Size + 1];
            size = Math.Min(size, Size);
            int order = Math.Min(size, Size - Degeneracy);

            // Fill in the values from the solution for degenerate cases
            for (int i = order + 1; i <= Size; i++)
                _intermediate[i] = solution[Column.Reverse(i)];

            // Forward substitution
            for (int i = 1; i <= order; i++)
            {
                _intermediate[i] = Vector[i];
                for (int j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
            }
        }

        /// <inheritdoc />
        public override void BackwardSubstitute(IVector<double> solution)
            => BackwardSubstitute(solution, Size);

        /// <summary>
        /// Applies backward substitution.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="size">The maximum size.</param>
        public void BackwardSubstitute(IVector<double> solution, int size)
        {
            solution.ThrowIfNull(nameof(solution));
            size = Math.Min(size, Size);
            int order = Math.Min(size, Size - Degeneracy);

            // Backward substitution
            for (int i = order; i >= 1; i--)
            {
                for (int j = i + 1; j <= size; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Column.Unscramble(_intermediate, solution);
        }

        /// <inheritdoc />
        public override double ComputeDegenerateContribution(int index)
        {
            double result = 0.0;
            for (int i = 1; i <= Degeneracy; i++)
                result += Matrix[index, i] * _intermediate[i];
            return result;
        }

        /// <inheritdoc />
        public override void ForwardSubstituteTransposed(IVector<double> solution)
            => ForwardSubstituteTransposed(solution, Size);

        /// <summary>
        /// Applies forward substitution of the adjoint matrix, but limits to the given size.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        /// <param name="steps">The steps to substitute.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="solution"/> is <c>null</c>.</exception>
        /// <exception cref="AlgebraException">Thrown if the matrix is not yet factored.</exception>
        /// <exception cref="ArgumentException">Thrown if the solution vector is not of size <see cref="PivotingSolver{M, V, T}.Size"/>.</exception>
        public void ForwardSubstituteTransposed(IVector<double> solution, int steps)
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
            for (int i = 1; i <= steps; i++)
            {
                int newIndex = Column[Row.Reverse(i)];
                _intermediate[newIndex] = Vector[i];
            }

            // Forward substitution
            for (int i = 1; i <= steps; i++)
            {
                for (int j = 1; j < i; j++)
                    _intermediate[i] -= Matrix[i, j] * Vector[j];
            }
        }

        /// <inheritdoc />
        public override void BackwardSubstituteTransposed(IVector<double> solution)
            => BackwardSubstituteTransposed(solution, Size);

        /// <summary>
        /// Applies backward substitution on the adjoint matrix, but limits to the given size.
        /// </summary>
        /// <param name="solution">The solution vector.</param>
        /// <param name="steps">The steps to substitute.</param>
        public void BackwardSubstituteTransposed(IVector<double> solution, int steps)
        {
            // Backward substitution
            _intermediate[steps] *= Matrix[steps, steps];
            for (int i = steps - 1; i >= 1; i--)
            {
                for (int j = i + 1; j <= steps; j++)
                    _intermediate[i] -= Matrix[i, j] * _intermediate[j];
                _intermediate[i] *= Matrix[i, i];
            }

            Row.Unscramble(_intermediate, solution);
        }

        /// <inheritdoc />
        public override double ComputeDegenerateContributionTransposed(int index)
        {
            double result = 0.0;
            for (int i = 1; i <= Degeneracy; i++)
                result += Matrix[i, index] * _intermediate[i];
            return result;
        }

        /// <inheritdoc/>
        protected override void Eliminate(int step, int size)
        {
            double diagonal = Matrix[step, step];
            if (diagonal.Equals(0.0))
                throw new AlgebraException(Properties.Resources.Algebra_InvalidPivot.FormatString(step));
            diagonal = 1.0 / diagonal;
            Matrix[step, step] = diagonal;

            for (int r = step + 1; r <= size; r++)
            {
                double lead = Matrix[r, step];
                if (lead.Equals(0.0))
                    continue;
                lead *= diagonal;
                Matrix[r, step] = lead;

                for (int c = step + 1; c <= size; c++)
                    Matrix[r, c] -= lead * Matrix[step, c];
            }
        }
    }
}
