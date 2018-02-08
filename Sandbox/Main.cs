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
            SpiceSharp.NewSparse.Matrix<double> nmatrix = new SpiceSharp.NewSparse.Matrix<double>();
            SpiceSharp.NewSparse.Vector<double> nrhs = new SpiceSharp.NewSparse.Vector<double>(4);
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    nmatrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];
            for (int i = 1; i < nrhs.Length; i++)
                nrhs[i] = rhs[i - 1];
            sw.Stop();

            Console.WriteLine($"New matrix setup: {sw.ElapsedTicks} ticks");
            Console.WriteLine(nmatrix);
            SpiceSharp.NewSparse.Vector<double> nsolution = new SpiceSharp.NewSparse.Vector<double>(4);

            sw.Restart();
            Solver<double> solver = new Solver<double>(nmatrix);
            // solver.Factor();
            // solver.SolveTransposed(nrhs, nsolution);
            sw.Stop();

            Console.WriteLine($"New matrix solve: {sw.ElapsedTicks} ticks");
            Console.WriteLine(nmatrix);
            Console.WriteLine(nsolution);
            Console.WriteLine();

            // Build the matrix for old sparse
            sw.Restart();
            SpiceSharp.Sparse.Matrix<double> omatrix = new SpiceSharp.Sparse.Matrix<double>();
            SpiceSharp.Sparse.Vector<double> orhs = new SpiceSharp.Sparse.Vector<double>(4);
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    omatrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];
            for (int i = 1; i < orhs.Length; i++)
                orhs[i] = rhs[i - 1];
            sw.Stop();

            Console.WriteLine($"Old matrix setup: {sw.ElapsedTicks} ticks");
            Console.WriteLine(omatrix);
            SpiceSharp.Sparse.Vector<double> osolution = new SpiceSharp.Sparse.Vector<double>(4);

            sw.Restart();
            omatrix.OrderAndFactor(orhs, true);
            omatrix.SolveTransposed(orhs, osolution);
            sw.Stop();

            Console.WriteLine($"Old matrix solve: {sw.ElapsedTicks} ticks");
            Console.WriteLine(omatrix);
            Console.WriteLine(osolution);
        }
    }
}