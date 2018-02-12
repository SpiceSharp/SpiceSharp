using System;

namespace SpiceSharp.NewSparse.Solve
{
    /// <summary>
    /// Event arguments when a new pivot has to be found
    /// </summary>
    /// <typeparam name="T">Base type</typeparam>
    public class PivotSearchEventArgs<T> : EventArgs where T : IFormattable
    {
        /// <summary>
        /// Gets the matrix
        /// </summary>
        public Matrix<T> Matrix { get; }

        /// <summary>
        /// Gets the current step
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Gets or sets the chosen pivot
        /// </summary>
        public Element<T> Pivot { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="step">Step</param>
        public PivotSearchEventArgs(Matrix<T> matrix, int step)
        {
            Matrix = matrix;
            Step = step;
        }
    }
}
