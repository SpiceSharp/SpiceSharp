using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SpiceSharpTest.Algebra;

[TestFixture]
public class KluBenchmark : SolveFramework
{
    private static List<(int Row, int Col)> Ladder(int stages)
    {
        // A chain of RC stages: the classic sparse circuit matrix shape.
        var e = new List<(int, int)>();
        for (int i = 1; i <= stages; i++)
        {
            e.Add((i, i));
            if (i > 1) { e.Add((i, i - 1)); e.Add((i - 1, i)); }
        }
        return e;
    }

    private static List<(int Row, int Col)> Mesh(int side)
    {
        var e = new List<(int, int)>();
        for (int y = 0; y < side; y++)
            for (int x = 0; x < side; x++)
            {
                int n = (y * side) + x + 1;
                e.Add((n, n));
                if (x > 0) { e.Add((n, n - 1)); e.Add((n - 1, n)); }
                if (y > 0) { e.Add((n, n - side)); e.Add((n - side, n)); }
            }
        return e;
    }

    private static (double Order, double Refactor, int Fill, int Fallbacks) Measure(
        ISparsePivotingSolver<double> solver, List<(int Row, int Col)> pattern, int size, int repeats)
    {
        var random = new Random(42);
        var elements = new List<(Element<double> E, double Base)>();
        foreach (var (r, c) in pattern)
        {
            double v = r == c ? 4.0 + random.NextDouble() : -1.0 - (random.NextDouble() * 0.5);
            elements.Add((solver.GetElement(new MatrixLocation(r, c)), v));
        }
        var rhs = new List<(Element<double> E, double Base)>();
        for (int i = 1; i <= size; i++)
            rhs.Add((solver.GetElement(i), random.NextDouble()));

        void LoadValues(double scale)
        {
            solver.Reset();
            foreach (var (e, b) in elements)
                e.Value = b * scale;
            foreach (var (e, b) in rhs)
                e.Value = b;
        }

        IVector<double> solution = new DenseVector<double>(size);

        LoadValues(1.0);
        var sw = Stopwatch.StartNew();
        if (solver.OrderAndFactor() != size)
            throw new Exception("singular");
        sw.Stop();
        double orderMs = sw.Elapsed.TotalMilliseconds;

        // Warm up
        for (int i = 0; i < 5; i++)
        {
            LoadValues(1.0 + (i * 1e-4));
            if (!solver.Factor() && solver.OrderAndFactor() != size) throw new Exception("singular in warmup");
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
        }

        int fallbacks = 0;
        sw.Restart();
        for (int i = 0; i < repeats; i++)
        {
            LoadValues(1.0 + (i * 1e-6));
            if (!solver.Factor())
            {
                fallbacks++;
                if (solver.OrderAndFactor() != size)
                    throw new Exception("singular");
            }
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
        }
        sw.Stop();

        int fill = solver is KluRealSolver klu
            ? klu.FactorNonZeroCount - klu.MatrixNonZeroCount
            : ((SparseRealSolver)solver).Fillins;
        return (orderMs, sw.Elapsed.TotalMilliseconds, fill, fallbacks);
    }

    [Test, Explicit("Timing comparison, run on demand rather than as part of the suite.")]
    public void Benchmark()
    {
        var report = new StringBuilder();
        var cases = new List<(string Name, List<(int, int)> Pattern, int Size, int Repeats)>
        {
            ("ladder-200", Ladder(200), 200, 3000),
            ("ladder-2000", Ladder(2000), 2000, 500),
            ("mesh-20x20", Mesh(20), 400, 2000),
            ("mesh-50x50", Mesh(50), 2500, 300),
        };

        // A throwaway pass so that everything is jitted before anything is timed.
        foreach (var (_, pattern, size, _) in cases)
        {
            Measure(new SparseRealSolver(), pattern, size, 200);
            Measure(new KluRealSolver(), pattern, size, 200);
        }

        foreach (var (name, pattern, size, repeats) in cases)
        {
            var s = Measure(new SparseRealSolver(), pattern, size, repeats);
            var k = Measure(new KluRealSolver(), pattern, size, repeats);
            var kn = new KluRealSolver(); kn.Parameters.Scaling = KluScaling.None;
            var k2 = Measure(kn, pattern, size, repeats);
            report.AppendLine($"{name} n={size} repeats={repeats}");
            report.AppendLine($"   sparse: order={s.Order:F2}ms  {repeats}x(factor+solve)={s.Refactor:F1}ms  fill={s.Fill} fallbacks={s.Fallbacks}");
            report.AppendLine($"   klu:    order={k.Order:F2}ms  {repeats}x(factor+solve)={k.Refactor:F1}ms  fill={k.Fill} fallbacks={k.Fallbacks}");
            report.AppendLine($"   klu-noscale: {repeats}x(factor+solve)={k2.Refactor:F1}ms");
            report.AppendLine($"   speedup={s.Refactor / k.Refactor:F2}x  noscale={s.Refactor / k2.Refactor:F2}x");
        }
        TestContext.Out.Write(report.ToString());
    }
}
