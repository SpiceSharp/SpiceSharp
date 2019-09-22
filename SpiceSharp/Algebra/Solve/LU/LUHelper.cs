using System.Numerics;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Helper class for creating solvers defined in the package.
    /// </summary>
    public static class LUHelper
    {
        /// <summary>
        /// Creates a solver for the specified matrix and vector.
        /// </summary>
        /// <typeparam name="M">The matrix type.</typeparam>
        /// <typeparam name="V">The vector type.</typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <returns>
        /// The solver using the specified matrix and vector.
        /// </returns>
        public static SparseRealSolver<M, V> CreateSparseRealSolver<M, V>(M matrix, V vector)
            where M : ISparseMatrix<double>, IPermutableMatrix<double>
            where V : ISparseVector<double>, IPermutableVector<double>
        {
            return new SparseRealSolver<M, V>(matrix, vector);
        }

        /// <summary>
        /// Creates a default sparse solver.
        /// </summary>
        /// <param name="size">The initial size.</param>
        /// <returns></returns>
        public static SparseRealSolver<SparseMatrix<double>, SparseVector<double>> CreateSparseRealSolver(int size = 0)
        {
            return new SparseRealSolver<SparseMatrix<double>, SparseVector<double>>(
                new SparseMatrix<double>(size),
                new SparseVector<double>(size));
        }

        /// <summary>
        /// Creates a default sparse solver.
        /// </summary>
        /// <param name="size">The initial size.</param>
        /// <returns></returns>
        public static SparseComplexSolver<SparseMatrix<Complex>, SparseVector<Complex>> CreateSparseComplexSolver(int size = 0)
        {
            return new SparseComplexSolver<SparseMatrix<Complex>, SparseVector<Complex>>(
                new SparseMatrix<Complex>(size),
                new SparseVector<Complex>(size));
        }

        /// <summary>
        /// Creates a solver for the specified matrix and vector.
        /// </summary>
        /// <typeparam name="M">The matrix type.</typeparam>
        /// <typeparam name="V">The vector type.</typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <returns></returns>
        public static ISolver<Complex> CreateSparseComplexSolver<M, V>(M matrix, V vector)
            where M : ISparseMatrix<Complex>, IPermutableMatrix<Complex>
            where V : ISparseVector<Complex>, IPermutableVector<Complex>
        {
            return new SparseComplexSolver<M, V>(matrix, vector);
        }

        /// <summary>
        /// Creates a solver for the specified matrix and vector.
        /// </summary>
        /// <typeparam name="M">The matrix type.</typeparam>
        /// <typeparam name="V">The vector type.</typeparam>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <returns></returns>
        public static DenseRealSolver<M, V> CreateDenseRealSolver<M, V>(M matrix, V vector)
            where M : IPermutableMatrix<double>
            where V : IPermutableVector<double>
        {
            return new DenseRealSolver<M, V>(matrix, vector);
        }

        /// <summary>
        /// Creates a default dense solver.
        /// </summary>
        /// <param name="size">The matrix size.</param>
        /// <returns></returns>
        public static DenseRealSolver<DenseMatrix<double>, DenseVector<double>> CreateDenseRealSolver(int size = 0)
        {
            return new DenseRealSolver<DenseMatrix<double>, DenseVector<double>>(
                new DenseMatrix<double>(size),
                new DenseVector<double>(size));
        }
    }
}
