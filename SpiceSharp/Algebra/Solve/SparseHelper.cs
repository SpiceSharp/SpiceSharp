using System.Numerics;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Helper methods for sparse solvers.
    /// </summary>
    public static class SparseHelper
    {
        /// <summary>
        /// Creates a solver for real numbers.
        /// </summary>
        /// <returns>A default <see cref="ISolver{T}"/>.</returns>
        public static ISolver<double> CreateDoubleSolver()
        {
            return new RealSolver<SparseMatrix< double >, SparseVector<double>>(
                new SparseMatrix<double>(),
                new SparseVector<double>());
        }

        /// <summary>
        /// Creates a solver for complex numbers.
        /// </summary>
        /// <returns>A default <see cref="ISolver{T}"/>.</returns>
        public static ISolver<Complex> CreateComplexSolver()
        {
            return new ComplexSolver<SparseMatrix<Complex>, SparseVector<Complex>>(
                new SparseMatrix<Complex>(),
                new SparseVector<Complex>());
        }
    }
}
