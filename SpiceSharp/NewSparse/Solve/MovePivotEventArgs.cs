using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Event arguments for moving a pivot
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public class MovePivotEventArgs<T> : EventArgs where T : IFormattable
    {
        /// <summary>
        /// Gets the pivot
        /// </summary>
        public Element<T> Pivot { get; }

        /// <summary>
        /// Gets the matrix
        /// </summary>
        public Matrix<T> Matrix { get; }

        /// <summary>
        /// Gets the right-hand side vector
        /// </summary>
        public Vector<T> Rhs { get; }

        /// <summary>
        /// Gets the current step in the elimination process
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Right-hand side</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="step">Step</param>
        public MovePivotEventArgs(Matrix<T> matrix, Vector<T> rhs, Element<T> pivot, int step)
        {
            Matrix = matrix;
            Rhs = rhs;
            Pivot = pivot;
            Step = step;
        }
    }
}
