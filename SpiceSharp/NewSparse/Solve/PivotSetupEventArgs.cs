using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Event arguments before reordering is executed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PivotSetupEventArgs<T> : EventArgs where T : IFormattable
    {
        /// <summary>
        /// Matrix
        /// </summary>
        public Matrix<T> Matrix { get; }

        /// <summary>
        /// Right-hand side
        /// </summary>
        public Vector<T> Rhs { get; }

        /// <summary>
        /// Current step
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="rhs">Rhs</param>
        /// <param name="step">Step</param>
        public PivotSetupEventArgs(Matrix<T> matrix, Vector<T> rhs, int step)
        {
            Matrix = matrix;
            Rhs = rhs;
            Step = step;
        }
    }
}
