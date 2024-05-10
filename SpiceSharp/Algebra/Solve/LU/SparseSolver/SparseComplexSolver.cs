using SpiceSharp.Algebra.Solve;
using System;
using System.Numerics;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving real matrices
    /// </summary>
    /// <seealso cref="SparseLUSolver{T}"/>
    public partial class SparseComplexSolver : SparseLUSolver<Complex>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Complex[] _intermediate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseComplexSolver"/> class.
        /// </summary>
        public SparseComplexSolver()
            : base(Magnitude)
        {
        }

        /// <inheritdoc />
        public override void ForwardSubstitute(IVector<Complex> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored.FormatString(nameof(Solve)));
            if (solution.Length != Size)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new Complex[Size + 1];
            int order = Size - Degeneracy;

            // Scramble
            var rhsElement = Vector.GetFirstInVector();
            int index = 0;
            while (rhsElement != null && rhsElement.Index <= order)
            {
                while (index < rhsElement.Index)
                    _intermediate[index++] = 0.0;
                _intermediate[index++] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }
            while (index <= order)
                _intermediate[index++] = 0.0;
            while (index <= Size)
            {
                _intermediate[index] = solution[Column.Reverse(index)];
                index++;
            }

            // Forward substitution
            for (int i = 1; i <= order; i++)
            {
                var temp = _intermediate[i];
                if (!temp.Equals(0.0))
                {
                    var pivot = Matrix.FindDiagonalElement(i);
                    temp *= pivot.Value;
                    _intermediate[i] = temp;
                    var element = pivot.Below;
                    while (element != null && element.Row <= order)
                    {
                        _intermediate[element.Row] -= temp * element.Value;
                        element = element.Below;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void BackwardSubstitute(IVector<Complex> solution)
        {
            int order = Size - Degeneracy;

            // Copy the degenerate solution to the intermediate vector
            for (int i = order + 1; i <= Size; i++)
                _intermediate[i] = solution[Row.Reverse(i)];

            // Backward substitution
            for (int i = order; i > 0; i--)
            {
                var temp = _intermediate[i];
                var pivot = Matrix.FindDiagonalElement(i);
                var element = pivot.Right;
                while (element != null)
                {
                    temp -= element.Value * _intermediate[element.Column];
                    element = element.Right;
                }
                _intermediate[i] = temp;
            }

            // Unscramble
            Column.Unscramble(_intermediate, solution);
        }

        /// <inheritdoc />
        public override Complex ComputeDegenerateContribution(int index)
        {
            Complex result = 0.0;
            var elt = Matrix.GetFirstInRow(Row[index]);
            int order = Size - Degeneracy;
            while (elt != null && elt.Column <= order)
            {
                result += elt.Value * _intermediate[elt.Column];
                elt = elt.Right;
            }
            return result;
        }

        /// <inheritdoc />
        public override void ForwardSubstituteTransposed(IVector<Complex> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored);
            if (solution.Length != Size)
                throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new Complex[Size + 1];
            int order = Size - Degeneracy;

            // Scramble
            for (int i = 0; i <= order; i++)
                _intermediate[i] = 0.0;
            for (int i = order + 1; i <= Size; i++)
                _intermediate[i] = solution[Column.Reverse(i)];
            var rhsElement = Vector.GetFirstInVector();
            while (rhsElement != null && rhsElement.Index <= order)
            {
                int newIndex = Column[Row.Reverse(rhsElement.Index)];
                _intermediate[newIndex] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }

            // Forward substitution
            for (int i = 1; i <= order; i++)
            {
                var temp = _intermediate[i];
                if (!temp.Equals(0.0))
                {
                    var element = Matrix.FindDiagonalElement(i).Right;
                    while (element != null && element.Column <= order)
                    {
                        _intermediate[element.Column] -= temp * element.Value;
                        element = element.Right;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void BackwardSubstituteTransposed(IVector<Complex> solution)
        {
            int order = Size - Degeneracy;

            // Copy the degenerate solution to the intermediate vector
            for (int i = order + 1; i <= Size; i++)
                _intermediate[i] = solution[Row.Reverse(i)];

            // Backward substitution
            for (int i = order; i > 0; i--)
            {
                var temp = _intermediate[i];
                var pivot = Matrix.FindDiagonalElement(i);
                var element = pivot.Below;
                while (element != null)
                {
                    temp -= _intermediate[element.Row] * element.Value;
                    element = element.Below;
                }
                _intermediate[i] = temp * pivot.Value;
            }

            // Unscramble
            Row.Unscramble(_intermediate, solution);
        }

        /// <inheritdoc />
        public override Complex ComputeDegenerateContributionTransposed(int index)
        {
            Complex result = 0.0;
            var elt = Matrix.GetFirstInColumn(Column[index]);
            int order = Size - Degeneracy;
            while (elt != null && elt.Row <= order)
            {
                result += elt.Value * _intermediate[elt.Row];
                elt = elt.Below;
            }
            return result;
        }

        /// <inheritdoc/>
        protected override void Eliminate(ISparseMatrixElement<Complex> pivot)
        {
            // Test for zero pivot
            if (pivot == null || pivot.Value.Equals(0.0))
                throw new AlgebraException(Properties.Resources.Algebra_InvalidPivot.FormatString(pivot.Row));
            pivot.Value = Inverse(pivot.Value);

            var upper = pivot.Right;
            while (upper != null)
            {
                // Calculate upper triangular element
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
                        sub = CreateFillin(new MatrixLocation(row, upper.Column));

                    // element -= upper * lower
                    sub.Value -= upper.Value * lower.Value;
                    sub = sub.Below;
                    lower = lower.Below;
                }
                upper = upper.Right;
            }
        }

        /// <summary>
        /// Method for finding the magnitude of a complex value.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>
        /// A scalar indicating the magnitude of the complex value.
        /// </returns>
        private static double Magnitude(Complex value) => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

        /// <summary>
        /// Calculates the inverse of a complex number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>
        /// The inverse value.
        /// </returns>
        private static Complex Inverse(Complex value)
        {
            double real, imaginary;
            double r;
            if (value.Real >= value.Imaginary && value.Real > -value.Imaginary ||
                value.Real < value.Imaginary && value.Real <= -value.Imaginary)
            {
                r = value.Imaginary / value.Real;
                real = 1.0 / (value.Real + r * value.Imaginary);
                imaginary = -r * real;
            }
            else
            {
                r = value.Real / value.Imaginary;
                imaginary = -1.0 / (value.Imaginary + r * value.Real);
                real = -r * imaginary;
            }
            return new Complex(real, imaginary);
        }
    }
}
