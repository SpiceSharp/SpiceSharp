using System;
using System.Numerics;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Algebra.Solve.Markowitz;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving real matrices
    /// </summary>
    public class ComplexSolver : Solver<Complex>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        Complex[] _intermediate;
        MatrixElement<Complex>[] _dest;

        /// <summary>
        /// Constructor
        /// </summary>
        public ComplexSolver()
            : base(new Markowitz<Complex>())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Matrix size</param>
        public ComplexSolver(int size)
            : base(new Markowitz<Complex>(), size)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="strategy">Strategy</param>
        public ComplexSolver(int size, PivotStrategy<Complex> strategy)
            : base(strategy, size)
        {
        }

        /// <summary>
        /// Fix the number of equations and variables
        /// </summary>
        public override void FixEquations()
        {
            base.FixEquations();
            _intermediate = new Complex[Order + 1];
            _dest = new MatrixElement<Complex>[Order + 1];
        }

        /// <summary>
        /// Unfix the number of equations and variables
        /// </summary>
        public override void UnfixEquations()
        {
            base.UnfixEquations();
            _intermediate = null;
            _dest = null;
        }

        /// <summary>
        /// Factor the matrix
        /// </summary>
        public override bool Factor()
        {
            if (!IsFixed)
                FixEquations();
            MatrixElement<Complex> element, column;

            // Get the diagonal
            element = Matrix.GetDiagonalElement(1);
            if (element.Value.Equals(0.0))
                return false;

            // pivot = 1 / pivot
            element.Value = 1.0 / element.Value; // Inverse(element.Value);

            // Start factorization
            Complex mult;
            for (int step = 2; step <= Matrix.Size; step++)
            {
                // Scatter
                element = Matrix.GetFirstInColumn(step);
                while (element != null)
                {
                    _dest[element.Row] = element;
                    element = element.Below;
                }

                // Update column
                column = Matrix.GetFirstInColumn(step);
                while (column.Row < step)
                {
                    element = Matrix.GetDiagonalElement(column.Row);

                    // Mult = dest[row] / pivot
                    mult = _dest[column.Row].Value * element.Value;
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
                element.Value = Inverse(element.Value);
            }

            IsFactored = true;
            return true;
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side
        /// </summary>
        /// <param name="solution">Solution vector</param>
        public override void Solve(Vector<Complex> solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));
            if (!IsFactored)
                throw new SparseException("Solver is not factored yet");

            // Scramble
            var rhsElement = Rhs.First;
            int index = 0;
            while (rhsElement != null)
            {
                while (index < rhsElement.Index)
                    _intermediate[index++] = 0.0;
                _intermediate[index++] = rhsElement.Value;
                rhsElement = rhsElement.Next;
            }
            while (index <= Order)
                _intermediate[index++] = 0.0;

            // Forward substitution
            for (int i = 1; i <= Matrix.Size; i++)
            {
                Complex temp = _intermediate[i];

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
            for (int i = Matrix.Size; i > 0; i--)
            {
                Complex temp = _intermediate[i];
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

            // Unscrable
            Column.Unscramble(_intermediate, solution);
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side (transposed)
        /// </summary>
        /// <param name="solution">Solution vector</param>
        public override void SolveTransposed(Vector<Complex> solution)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));
            if (!IsFactored)
                throw new SparseException("Solver is not factored yet");

            // Scramble
            for (int i = 0; i <= Order; i++)
                _intermediate[i] = 0.0;
            var rhsElement = Rhs.First;
            while (rhsElement != null)
            {
                int newIndex = Column[Row.Reverse(rhsElement.Index)];
                _intermediate[newIndex] = rhsElement.Value;
                rhsElement = rhsElement.Next;
            }

            // Forward elimination
            for (int i = 1; i <= Matrix.Size; i++)
            {
                Complex temp = _intermediate[i];

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
            for (int i = Matrix.Size; i > 0; i--)
            {
                Complex temp = _intermediate[i];
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

            int step = 1;
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
            Strategy.Setup(Matrix, Rhs, step, Magnitude);

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

            // Flag the solver as factored
            IsFactored = true;
        }

        /// <summary>
        /// Eliminate a row
        /// </summary>
        /// <param name="pivot">Current pivot</param>
        void Elimination(MatrixElement<Complex> pivot)
        {
            // Test for zero pivot
            if (pivot.Value.Equals(0.0))
                throw new SparseException("Matrix is singular");
            pivot.Value = 1.0 / pivot.Value; // Inverse(pivot.Value);

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
        /// Magnitude method
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        static double Magnitude(Complex value) => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

        /// <summary>
        /// Calculate inverse of a complex number
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        static Complex Inverse(Complex value)
        {
            double real, imaginary;
            double r;
            if ((value.Real >= value.Imaginary && value.Real > -value.Imaginary) ||
                (value.Real < value.Imaginary && value.Real <= -value.Imaginary))
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
