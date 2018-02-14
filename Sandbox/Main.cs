using System;
using System.Windows.Forms;
using SpiceSharp.Sparse;
using SpiceSharp.NewSparse;
using SpiceSharp.NewSparse.Solve;
using System.Diagnostics;

namespace Sandbox
{
    public partial class Main : Form
    {
        // Reference
        double[][] matrixElements =
        {
                new double[] { 1, 1, 1 },
                new double[] { 0, 0, 3 },
                new double[] { 0, 2, 3 }
            };
        double[] rhs = { 1, 0, 3 };

        Stopwatch sw = new Stopwatch();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            int count = 500000;

            SolveOldSpaceMatrix();
            sw.Reset();
            for (int i = 0; i < count; i++)
                SolveOldSpaceMatrix();
            sw.Stop();
            Console.WriteLine($"OLD: {sw.ElapsedMilliseconds} ms");

            SolveNewSpaceMatrix();
            sw.Reset();
            for (int i = 0; i < count; i++)
                SolveNewSpaceMatrix();
            sw.Stop();
            Console.WriteLine($"NEW: {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// New sparse matrix
        /// </summary>
        void SolveNewSpaceMatrix()
        {
            // Create the solver
            var solver = new RealSolver();

            // Setup the matrix
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    if (!matrixElements[r][c].Equals(0.0))
                        solver.Matrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];
            for (int r = 0; r < rhs.Length; r++)
                solver.Rhs[r + 1] = rhs[r];

            var solution = new DenseVector<double>(solver.Rhs.Length);

            sw.Start();
            solver.OrderAndFactor();
            sw.Stop();
        }

        /// <summary>
        /// Old sparse matrix
        /// </summary>
        void SolveOldSpaceMatrix()
        {
            // Create the solver
            var omatrix = new SpiceSharp.Sparse.Matrix<double>();
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    if (!matrixElements[r][c].Equals(0.0))
                        omatrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];

            var orhs = new SpiceSharp.Sparse.Vector<double>(matrixElements.Length + 1);
            for (int i = 0; i < rhs.Length; i++)
                orhs[i + 1] = rhs[i];

            var solution = new SpiceSharp.Sparse.Vector<double>(4);

            sw.Start();
            omatrix.OrderAndFactor(orhs, false);
            sw.Stop();
        }
    }
}