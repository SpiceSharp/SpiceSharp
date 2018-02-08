using System;

namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Class for solving matrices
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Solver<T>
    {
        /// <summary>
        /// Get the number of fillins added by the solver
        /// </summary>
        public int Fillins { get; private set; }

        /// <summary>
        /// Pivoting object
        /// </summary>
        Pivoting pivoting = new Pivoting();

        /// <summary>
        /// Gets the matrix
        /// </summary>
        public Matrix<T> Matrix { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Solver(Matrix<T> matrix)
        {
            Matrix = matrix;
        }

        /// <summary>
        /// Factor the matrix
        /// </summary>
        public void Factor()
        {
            MatrixIterator<T> element, column;

            // Get the diagonal
            element = Matrix.GetMatrixIterator(1);
            if (element.Element.Magnitude.Equals(0))
                throw new Exception("Zero pivot");

            // pivot = 1 / pivot
            element.Element.AssignReciprocal(element.Element);

            // TODO: maybe we should cache this
            Element<T>[] dest = new Element<T>[Matrix.Size + 1];

            // Start factorization
            Element<T> mult = ElementFactory.Create<T>();
            for (int step = 2; step <= Matrix.Size; step++)
            {
                // Scatter
                element = Matrix.GetFirstInColumn(step);
                while (element.Element != null)
                {
                    dest[element.Row] = element.Element;
                    element.MoveDown();
                }

                // Update column
                column = Matrix.GetFirstInColumn(step);
                while (column.Row < step)
                {
                    element = Matrix.GetMatrixIterator(column.Row);

                    // Mult = dest[row] / pivot
                    mult.AssignMultiply(dest[column.Row], element.Element);
                    dest[column.Row].CopyFrom(mult);
                    element.MoveDown();
                    while (element.Element != null)
                    {
                        // dest[element.Row] -= mult * element
                        dest[element.Row].SubtractMultiply(mult, element.Element);
                        element.MoveDown();
                    }
                    column.MoveDown();
                }

                // Check for a singular matrix
                element = Matrix.GetMatrixIterator(step);
                if (element.Element == null || element.Element.Magnitude.Equals(0.0))
                    throw new Exception("Zero pivot");
                element.Element.AssignReciprocal(element.Element);
            }
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side
        /// </summary>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="solution">Solution vector</param>
        public void Solve(Vector<T> rhs, Vector<T> solution)
        {
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // TODO: Maybe we should cache intermediate
            // Scramble
            var intermediate = new Element<T>[rhs.Length];
            for (int i = 0; i < rhs.Length; i++)
            {
                intermediate[i] = ElementFactory.Create<T>();
                intermediate[i].Value = rhs[i];
            }

            // Forward substitution
            var temp = ElementFactory.Create<T>();
            for (int i = 1; i <= Matrix.Size; i++)
            {
                temp.CopyFrom(intermediate[i]);

                // This step of the substitution is skipped if temp == 0.0
                if (!temp.EqualsZero())
                {
                    var pivot = Matrix.GetMatrixIterator(i);

                    // temp = temp / pivot
                    temp.Multiply(pivot.Element);
                    intermediate[i].CopyFrom(temp);
                    var element = pivot.BranchDown();
                    while (element.Element != null)
                    {
                        // rhs[row] -= temp * element
                        intermediate[element.Row].SubtractMultiply(temp, element.Element);
                        element.MoveDown();
                    }
                }
            }

            // Backward substitution
            for (int i = Matrix.Size; i > 0; i--)
            {
                temp.CopyFrom(intermediate[i]);
                var pivot = Matrix.GetMatrixIterator(i);
                var element = pivot.BranchRight();

                while (element.Element != null)
                {
                    // temp -= element * intermeidate[column]
                    temp.SubtractMultiply(element.Element, intermediate[element.Column]);
                    element.MoveRight();
                }
                intermediate[i].CopyFrom(temp);
            }

            // Unscrable
            for (int i = Matrix.Size; i > 0; i--)
                solution[i] = intermediate[i].Value;
        }

        /// <summary>
        /// Find the solution for a factored matrix and a right-hand-side (transposed)
        /// </summary>
        /// <param name="rhs"></param>
        /// <param name="solution"></param>
        public void SolveTransposed(Vector<T> rhs, Vector<T> solution)
        {
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            // TODO: Maybe we should cache intermediate
            // Scramble
            var intermediate = new Element<T>[rhs.Length];
            for (int i = 0; i < intermediate.Length; i++)
            {
                intermediate[i] = ElementFactory.Create<T>();
                intermediate[i].Value = rhs[i];
            }

            // Forward elimination
            var temp = ElementFactory.Create<T>();
            for (int i = 1; i <= Matrix.Size; i++)
            {
                temp.CopyFrom(intermediate[i]);

                // This step of the elimination is skipped if temp equals 0
                if (!temp.EqualsZero())
                {
                    var element = Matrix.GetMatrixIterator(i);
                    element.MoveRight();
                    while (element.Element != null)
                    {
                        // intermediate[col] -= temp * element
                        intermediate[element.Column].SubtractMultiply(temp, element.Element);
                        element.MoveRight();
                    }
                }
            }

            // Backward substitution
            for (int i = Matrix.Size; i > 0; i--)
            {
                var pivot = Matrix.GetMatrixIterator(i);
                temp.CopyFrom(intermediate[i]);
                var element = pivot.BranchDown();

                while (element.Element != null)
                {
                    // temp -= intermediate[element.row] * element
                    temp.SubtractMultiply(intermediate[element.Row], element.Element);
                    element.MoveDown();
                }

                // intermediate = temp / pivot
                intermediate[i].AssignMultiply(temp, pivot.Element);
            }

            // Unscramble
            for (int i = Matrix.Size; i > 0; i--)
                solution[i] = intermediate[i].Value;
        }

        /// <summary>
        /// Factor while reordering the matrix
        /// </summary>
        /// <param name="rhs">Right-Hand Side</param>
        /// <param name="diagonalPivoting">Use diagonal pivoting</param>
        public void OrderAndFactor(Vector<T> rhs, bool diagonalPivoting)
        {
            // Matrix has been factored before and reordering is not required
            for (int step = 1; step <= Matrix.Size; step++)
            {
                var pivot = Matrix.GetMatrixIterator(step);
                double largest = pivoting.LargestInColumn(pivot);
                if (largest * pivoting.RelativeThreshold < pivot.Element.Magnitude)
                    Elimination(pivot);
                else
                    throw new Exception("Reordering required");
            }

            // Reorder and stuff...
        }

        /// <summary>
        /// Eliminate a row
        /// </summary>
        /// <param name="pivot">Current pivot</param>
        void Elimination(MatrixIterator<T> pivot)
        {
            // Test for zero pivot
            if (pivot.Element.Magnitude.Equals(0.0))
                throw new Exception("Matrix is singular");
            pivot.Element.AssignReciprocal(pivot.Element);

            var upper = pivot.BranchRight();
            while (upper.Element != null)
            {
                // Calculate upper triangular element
                // upper = upper / pivot
                upper.Element.Multiply(pivot.Element);

                var sub = upper.BranchDown();
                var lower = pivot.BranchDown();
                while (lower.Element != null)
                {
                    int row = lower.Row;

                    // Find element in row that lines up with the current lower triangular element
                    while (sub.Element != null && sub.Row < row)
                        sub.MoveDown();

                    // Test to see if the desired element was not found, if not, create fill-in
                    if (sub == null || sub.Row > row)
                    {
                        Fillins++;
                        sub = Matrix.GetIterator(row, upper.Column);
                    }

                    // element -= upper * lower
                    sub.Element.SubtractMultiply(upper.Element, lower.Element);
                    sub.MoveDown();
                    lower.MoveDown();
                }
                upper.MoveRight();
            }
        }
    }
}
