using System;
using System.Windows.Forms;
using SpiceSharp.Sparse;
using SpiceSharp.NewSparse;
using System.Diagnostics;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
            Stopwatch sw = new Stopwatch();

            // Reference
            double[][] matrixElements =
            {
                new double[] { 1, 1, 1 },
                new double[] { 0, 0, 3 },
                new double[] { 0, 2, 3 }
            };
            double[] rhs = { 1, 1, 1 };

            // Build the matrix for new sparse
            sw.Start();
            var markowitz = new SpiceSharp.NewSparse.Solve.Markowitz<double>(Math.Abs);
            Solver<double> solver = new RealSolver(markowitz);
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                {
                    if (matrixElements[r][c] != 0.0)
                        solver.Matrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];
                }
            for (int i = 0; i < rhs.Length; i++)
                solver.Rhs[i + 1] = rhs[i];
            sw.Stop();

            Console.WriteLine($"New matrix setup: {sw.ElapsedTicks} ticks");
            Console.WriteLine(solver.Matrix);
            SpiceSharp.NewSparse.Vector<double> nsolution = new SpiceSharp.NewSparse.Vector<double>(4);

            sw.Restart();
            solver.OrderAndFactor();
            sw.Stop();

            Console.WriteLine($"New matrix solve: {sw.ElapsedTicks} ticks");
            Console.WriteLine(solver.Matrix);
            Console.WriteLine(nsolution);
            Console.WriteLine();

            // Build the matrix for old sparse
            sw.Restart();
            SpiceSharp.Sparse.Matrix<double> omatrix = new SpiceSharp.Sparse.Matrix<double>();
            SpiceSharp.Sparse.Vector<double> orhs = new SpiceSharp.Sparse.Vector<double>(4);
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                {
                    if (matrixElements[r][c] != 0.0)
                        omatrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];
                }
            for (int i = 1; i < orhs.Length; i++)
                orhs[i] = rhs[i - 1];
            sw.Stop();

            Console.WriteLine($"Old matrix setup: {sw.ElapsedTicks} ticks");
            Console.WriteLine(omatrix);
            SpiceSharp.Sparse.Vector<double> osolution = new SpiceSharp.Sparse.Vector<double>(4);

            sw.Restart();
            omatrix.OrderAndFactor(orhs, false);
            sw.Stop();

            Console.WriteLine($"Old matrix solve: {sw.ElapsedTicks} ticks");
            Console.WriteLine(omatrix);
            Console.WriteLine(osolution);
        }
    }
}