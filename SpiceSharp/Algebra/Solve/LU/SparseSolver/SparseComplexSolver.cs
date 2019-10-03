using System;
using System.Numerics;
using SpiceSharp.Algebra.Solve;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving real matrices
    /// </summary>
    public partial class SparseComplexSolver<M, V> : SparseLUSolver<M, V, Complex>
        where M : IPermutableMatrix<Complex>, ISparseMatrix<Complex>
        where V : IPermutableVector<Complex>, ISparseVector<Complex>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Complex[] _intermediate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseComplexSolver{M,V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        public SparseComplexSolver(M matrix, V vector)
            : base(matrix, vector, new Markowitz<Complex>(Magnitude))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseComplexSolver{M,V}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="strategy">The pivoting strategy.</param>
        public SparseComplexSolver(M matrix, V vector, SparsePivotStrategy<Complex> strategy)
            : base(matrix, vector, strategy)
        {
        }

        /// <summary>
        /// Finds the element at the specified position in the matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public override ISolverElement<Complex> FindElement(int row, int column)
        {
            var element = FindMatrixElement(row, column);
            if (element == null)
                return null;
            return new ComplexMatrixSolverElement(element);
        }

        /// <summary>
        /// Finds the element at the specified position in the right-hand side vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public override ISolverElement<Complex> FindElement(int row)
        {
            var element = FindVectorElement(row);
            if (element == null)
                return null;
            return new ComplexVectorSolverElement(element);
        }

        /// <summary>
        /// Gets the element at the specified position in the matrix. A new element is
        /// created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public override ISolverElement<Complex> GetElement(int row, int column)
        {
            var element = GetMatrixElement(row, column);
            return new ComplexMatrixSolverElement(element);
        }

        /// <summary>
        /// Gets the element at the specified position in the right-hand side vector.
        /// A new element is created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        public override ISolverElement<Complex> GetElement(int row)
        {
            var element = GetVectorElement(row);
            return new ComplexVectorSolverElement(element);
        }

        /// <summary>
        /// Solves the equations using the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public override void Solve(IVector<Complex> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException("Solver is not factored yet");
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new Complex[Size + 1];

            var ea = new SolveEventArgs<Complex>(solution);

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
            for (var i = 1; i <= Order; i++)
            {
                var temp = _intermediate[i];
                if (!temp.Equals(0.0))
                {
                    var pivot = Matrix.FindDiagonalElement(i);
                    temp *= pivot.Value;
                    _intermediate[i] = temp;
                    var element = pivot.Below;
                    while (element != null && element.Row <= Order)
                    {
                        _intermediate[element.Row] -= temp * element.Value;
                        element = element.Below;
                    }
                }
            }

            // Backward substitution
            for (var i = Order; i > 0; i--)
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

            // Unscrable
            Column.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Solves the equations using the transposed Y-matrix.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public override void SolveTransposed(IVector<Complex> solution)
        {
            solution.ThrowIfNull(nameof(solution));
            if (!IsFactored)
                throw new AlgebraException("Solver is not factored yet");
            if (_intermediate == null || _intermediate.Length != Size + 1)
                _intermediate = new Complex[Size + 1];

            var ea = new SolveEventArgs<Complex>(solution);

            // Scramble
            for (var i = 0; i <= Size; i++)
                _intermediate[i] = 0.0;
            var rhsElement = Vector.GetFirstInVector();
            while (rhsElement != null)
            {
                var newIndex = Column[Row.Reverse(rhsElement.Index)];
                _intermediate[newIndex] = rhsElement.Value;
                rhsElement = rhsElement.Below;
            }

            // Forward elimination
            for (var i = 1; i <= Order; i++)
            {
                var temp = _intermediate[i];
                if (!temp.Equals(0.0))
                {
                    var element = Matrix.FindDiagonalElement(i).Right;
                    while (element != null && element.Column <= Order)
                    {
                        _intermediate[element.Column] -= temp * element.Value;
                        element = element.Right;
                    }
                }
            }

            // Backward substitution
            for (var i = Order; i > 0; i--)
            {
                var temp = _intermediate[i];
                var pivot = Matrix.FindDiagonalElement(i);
                var element = pivot.Below;
                while (element != null && element.Row <= Order)
                {
                    temp -= _intermediate[element.Row] * element.Value;
                    element = element.Below;
                }
                _intermediate[i] = temp * pivot.Value;
            }

            // Unscramble
            Row.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Eliminate the matrix right and below the pivot.
        /// </summary>
        /// <param name="pivot">The pivot element.</param>
        /// <returns>
        /// <c>true</c> if the elimination was successful; otherwise <c>false</c>.
        /// </returns>
        protected override bool Elimination(ISparseMatrixElement<Complex> pivot)
        {
            // Test for zero pivot
            if (pivot == null || pivot.Value.Equals(0.0))
                return false;
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
            return true;
        }

        /// <summary>
        /// Method for finding the magnitude of a complex value.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>A scalar indicating the magnitude of the complex value.</returns>
        private static double Magnitude(Complex value) => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

        /// <summary>
        /// Calculates the inverse of a complex number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The inverse value.</returns>
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
