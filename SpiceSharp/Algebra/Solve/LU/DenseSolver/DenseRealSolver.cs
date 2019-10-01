﻿using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Class for solving dense sets of equations with real numbers.
    /// </summary>
    /// <typeparam name="M">The matrix type.</typeparam>
    /// <typeparam name="V">The vector type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.DenseLUSolver{M, V, T}" />
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
        /// Finds the element at the specified position in the matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public override ISolverElement<double> FindElement(int row, int column)
            => new RealSolverMatrixElement(this, row, column);

        /// <summary>
        /// Finds the element at the specified position in the right-hand side vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element if it exists; otherwise <c>null</c>.
        /// </returns>
        public override ISolverElement<double> FindElement(int row)
            => new RealSolverVectorElement(this, row);

        /// <summary>
        /// Gets the element at the specified position in the matrix. A new element is
        /// created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        public override ISolverElement<double> GetElement(int row, int column)
            => new RealSolverMatrixElement(this, row, column);

        /// <summary>
        /// Gets the element at the specified position in the right-hand side vector.
        /// A new element is created if it doesn't exist yet.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The vector element.
        /// </returns>
        public override ISolverElement<double> GetElement(int row)
            => new RealSolverVectorElement(this, row);

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

            var ea = new SolveEventArgs<double>(solution);
            OnBeforeSolve(ea);

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

            OnAfterSolve(ea);
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

            var ea = new SolveEventArgs<double>(solution);
            OnBeforeSolveTransposed(ea);

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

            OnAfterSolveTransposed(ea);
        }

        /// <summary>
        /// Eliminate the submatrix right and below the pivot.
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
