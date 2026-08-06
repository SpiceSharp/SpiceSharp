using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Algebra;

[TestFixture]
public class KluComplexSolverTests : SolveFramework
{
    private static void Load(ISparseSolver<Complex> solver, Complex[][] matrix, Complex[] rhs)
    {
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (matrix[r][c] != Complex.Zero)
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (rhs[r] != Complex.Zero)
                solver.GetElement(r + 1).Value = rhs[r];
        }
    }

    private static void AssertSolves(Complex[][] matrix, Complex[] rhs, IVector<Complex> solution, double tolerance)
    {
        for (int r = 0; r < matrix.Length; r++)
        {
            Complex sum = Complex.Zero;
            for (int c = 0; c < matrix[r].Length; c++)
                sum += matrix[r][c] * solution[c + 1];
            Assert.That((sum - rhs[r]).Magnitude, Is.LessThan(tolerance), $"Equation {r + 1} is not satisfied");
        }
    }

    private static void AssertSolvesTransposed(Complex[][] matrix, Complex[] rhs, IVector<Complex> solution, double tolerance)
    {
        int size = matrix.Length;
        for (int c = 0; c < size; c++)
        {
            Complex sum = Complex.Zero;
            for (int r = 0; r < size; r++)
                sum += matrix[r][c] * solution[r + 1];
            Assert.That((sum - rhs[c]).Magnitude, Is.LessThan(tolerance), $"Transposed equation {c + 1} is not satisfied");
        }
    }

    private static Complex[][] RandomMatrix(int size, double density, Random random, bool dominant)
    {
        Complex[][] matrix = new Complex[size][];
        for (int r = 0; r < size; r++)
        {
            matrix[r] = new Complex[size];
            if (dominant)
                matrix[r][r] = new Complex((size * 2.0) + random.NextDouble(), random.NextDouble());
            for (int c = 0; c < size; c++)
            {
                if ((r != c || !dominant) && random.NextDouble() < density)
                    matrix[r][c] = new Complex((random.NextDouble() * 2.0) - 1.0, (random.NextDouble() * 2.0) - 1.0);
            }
        }
        return matrix;
    }

    private static Complex[] RandomVector(int size, Random random)
    {
        Complex[] rhs = new Complex[size];
        for (int r = 0; r < size; r++)
            rhs[r] = new Complex((random.NextDouble() * 20.0) - 10.0, (random.NextDouble() * 20.0) - 10.0);
        return rhs;
    }

    [Test]
    public void When_SmallSystem_Expect_CorrectSolution()
    {
        Complex[][] matrix =
        [
            [new Complex(2.0, 1.0), new Complex(1.0, 0.0), Complex.Zero],
            [new Complex(1.0, 0.0), new Complex(3.0, -2.0), new Complex(1.0, 1.0)],
            [Complex.Zero, new Complex(1.0, 1.0), new Complex(4.0, 0.5)]
        ];
        Complex[] rhs = [new Complex(1.0, 1.0), new Complex(2.0, -1.0), new Complex(3.0, 0.0)];

        var solver = new KluComplexSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(3));

        IVector<Complex> solution = new DenseVector<Complex>(3);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);
    }

    [Test]
    public void When_RandomSystems_Expect_SameSolutionAsSparseSolver()
    {
        for (int seed = 0; seed < 30; seed++)
        {
            var random = new Random(seed);
            int size = 3 + (seed % 25);
            Complex[][] matrix = RandomMatrix(size, 3.0 / size, random, true);
            Complex[] rhs = RandomVector(size, random);

            var klu = new KluComplexSolver();
            Load(klu, matrix, rhs);
            Assert.That(klu.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");
            IVector<Complex> solution = new DenseVector<Complex>(size);
            klu.ForwardSubstitute(solution);
            klu.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-9);

            var sparse = new SparseComplexSolver();
            Load(sparse, matrix, rhs);
            Assert.That(sparse.OrderAndFactor(), Is.EqualTo(size));
            IVector<Complex> reference = new DenseVector<Complex>(size);
            sparse.ForwardSubstitute(reference);
            sparse.BackwardSubstitute(reference);

            for (int i = 1; i <= size; i++)
                Assert.That((solution[i] - reference[i]).Magnitude, Is.LessThan(1e-9), $"seed {seed}, unknown {i}");
        }
    }

    [Test]
    public void When_RandomSystemsNeedingPivoting_Expect_CorrectSolution()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var random = new Random(7000 + seed);
            int size = 4 + (seed % 25);
            Complex[][] matrix = RandomMatrix(size, 4.0 / size, random, false);
            Complex[] rhs = RandomVector(size, random);

            var sparse = new SparseComplexSolver();
            Load(sparse, matrix, rhs);
            if (sparse.OrderAndFactor() != size)
                continue;

            var klu = new KluComplexSolver();
            Load(klu, matrix, rhs);
            Assert.That(klu.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");

            IVector<Complex> solution = new DenseVector<Complex>(size);
            klu.ForwardSubstitute(solution);
            klu.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-8);

            klu.ForwardSubstituteTransposed(solution);
            klu.BackwardSubstituteTransposed(solution);
            AssertSolvesTransposed(matrix, rhs, solution, 1e-8);
        }
    }

    [Test]
    public void When_RefactoredWithNewValues_Expect_CorrectSolution()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            var random = new Random(2000 + seed);
            int size = 4 + (seed % 20);
            Complex[][] matrix = RandomMatrix(size, 3.0 / size, random, true);
            Complex[] rhs = RandomVector(size, random);

            var solver = new KluComplexSolver();
            Load(solver, matrix, rhs);
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(size));

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (matrix[r][c] == Complex.Zero)
                        continue;
                    matrix[r][c] = r == c
                        ? new Complex((size * 3.0) + random.NextDouble(), random.NextDouble())
                        : new Complex((random.NextDouble() * 2.0) - 1.0, (random.NextDouble() * 2.0) - 1.0);
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                rhs[r] = new Complex((random.NextDouble() * 20.0) - 10.0, (random.NextDouble() * 20.0) - 10.0);
                solver.GetElement(r + 1).Value = rhs[r];
            }

            Assert.That(solver.Factor(), Is.True, $"seed {seed}");
            IVector<Complex> solution = new DenseVector<Complex>(size);
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-9);
        }
    }

    [Test]
    public void When_RefactoringRepeatedlyOnIllScaledSystems_Expect_AccurateSolutions()
    {
        // The loop a frequency sweep runs: reuse the route while it holds, fall back to a full
        // factorization when it does not. Rows spanning many decades are what makes a stored
        // pivot go stale, so the answer has to stay accurate whichever path each step takes.
        for (int seed = 0; seed < 25; seed++)
        {
            var random = new Random(9000 + seed);
            int size = 6 + (seed % 15);

            var pattern = new List<(int Row, int Column)>();
            for (int r = 0; r < size; r++)
            {
                pattern.Add((r, r));
                for (int c = 0; c < size; c++)
                {
                    if (c != r && random.NextDouble() < 3.0 / size)
                        pattern.Add((r, c));
                }
            }

            var solver = new KluComplexSolver();
            var elements = new Dictionary<(int, int), Element<Complex>>();
            foreach (var (r, c) in pattern)
                elements[(r, c)] = solver.GetElement(new MatrixLocation(r + 1, c + 1));
            var rhsElements = new Element<Complex>[size];
            for (int r = 0; r < size; r++)
                rhsElements[r] = solver.GetElement(r + 1);

            Complex[][] matrix = new Complex[size][];
            for (int r = 0; r < size; r++)
                matrix[r] = new Complex[size];
            Complex[] rhs = new Complex[size];
            IVector<Complex> solution = new DenseVector<Complex>(size);

            bool factored = false;
            for (int step = 0; step < 12; step++)
            {
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                        matrix[r][c] = Complex.Zero;
                }
                for (int r = 0; r < size; r++)
                {
                    double rowScale = Math.Pow(10.0, (random.NextDouble() * 12.0) - 6.0);
                    foreach (var (rr, cc) in pattern)
                    {
                        if (rr != r)
                            continue;
                        matrix[rr][cc] = new Complex(
                            rowScale * ((random.NextDouble() * 2.0) - 1.0),
                            rowScale * ((random.NextDouble() * 2.0) - 1.0));
                    }
                    if (matrix[r][r] == Complex.Zero)
                        matrix[r][r] = new Complex(rowScale, 0.0);
                    rhs[r] = new Complex((random.NextDouble() * 2.0) - 1.0, (random.NextDouble() * 2.0) - 1.0);
                }
                foreach (var (r, c) in pattern)
                    elements[(r, c)].Value = matrix[r][c];
                for (int r = 0; r < size; r++)
                    rhsElements[r].Value = rhs[r];

                if (!factored || !solver.Factor())
                {
                    if (solver.OrderAndFactor() != size)
                    {
                        factored = false;
                        continue;
                    }
                }
                factored = true;

                solver.ForwardSubstitute(solution);
                solver.BackwardSubstitute(solution);

                for (int r = 0; r < size; r++)
                {
                    Complex sum = Complex.Zero;
                    double norm = 0.0;
                    for (int c = 0; c < size; c++)
                    {
                        Complex term = matrix[r][c] * solution[c + 1];
                        sum += term;
                        norm += term.Magnitude;
                    }
                    double denominator = Math.Max(norm, rhs[r].Magnitude);
                    if (denominator > 0.0)
                    {
                        Assert.That((sum - rhs[r]).Magnitude / denominator, Is.LessThan(1e-8),
                            $"seed {seed}, step {step}, equation {r + 1}");
                    }
                }
            }
        }
    }

    [Test]
    public void When_BlockTriangularSystem_Expect_CorrectSolution()
    {
        Complex[][] matrix =
        [
            [new Complex(2.0, 1.0), new Complex(1.0, 0.0), new Complex(3.0, 1.0), Complex.Zero],
            [new Complex(1.0, 0.0), new Complex(2.0, -1.0), Complex.Zero, new Complex(5.0, 2.0)],
            [Complex.Zero, Complex.Zero, new Complex(4.0, 0.0), new Complex(1.0, 1.0)],
            [Complex.Zero, Complex.Zero, new Complex(1.0, -1.0), new Complex(3.0, 0.0)]
        ];
        Complex[] rhs = [new Complex(1.0, 0.0), new Complex(-2.0, 1.0), new Complex(3.0, 0.0), new Complex(4.0, -1.0)];

        var solver = new KluComplexSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(4));
        Assert.That(solver.Symbolic.Blocks, Is.GreaterThan(1));

        IVector<Complex> solution = new DenseVector<Complex>(4);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);

        solver.ForwardSubstituteTransposed(solution);
        solver.BackwardSubstituteTransposed(solution);
        AssertSolvesTransposed(matrix, rhs, solution, 1e-12);
    }

    [Test]
    [TestCase(KluOrdering.ApproximateMinimumDegree, KluScaling.Sum, true)]
    [TestCase(KluOrdering.ColumnApproximateMinimumDegree, KluScaling.Maximum, true)]
    [TestCase(KluOrdering.Natural, KluScaling.None, false)]
    public void When_ParametersVaried_Expect_CorrectSolution(KluOrdering ordering, KluScaling scaling, bool blockTriangular)
    {
        for (int seed = 0; seed < 10; seed++)
        {
            var random = new Random(3000 + seed);
            int size = 8 + seed;
            Complex[][] matrix = RandomMatrix(size, 3.0 / size, random, true);
            Complex[] rhs = RandomVector(size, random);

            var solver = new KluComplexSolver();
            solver.Parameters.Ordering = ordering;
            solver.Parameters.Scaling = scaling;
            solver.Parameters.UseBlockTriangularForm = blockTriangular;
            Load(solver, matrix, rhs);
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");

            IVector<Complex> solution = new DenseVector<Complex>(size);
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-9);

            solver.ForwardSubstituteTransposed(solution);
            solver.BackwardSubstituteTransposed(solution);
            AssertSolvesTransposed(matrix, rhs, solution, 1e-9);
        }
    }

    [Test]
    public void When_SingularSystem_Expect_FailureAtRow()
    {
        Complex[][] matrix =
        [
            [new Complex(1.0, 0.0), new Complex(1.0, 0.0), Complex.Zero],
            [Complex.Zero, new Complex(1.0, 0.0), new Complex(1.0, 0.0)],
            [new Complex(1.0, 0.0), new Complex(2.0, 0.0), new Complex(1.0, 0.0)]
        ];
        Complex[] rhs = [Complex.One, new Complex(2.0, 0.0), new Complex(3.0, 0.0)];

        var solver = new KluComplexSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.LessThan(3));
    }
}
