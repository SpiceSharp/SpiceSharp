using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpiceSharpTest.Algebra;

[TestFixture]
public class KluSolverTests : SolveFramework
{
    /// <summary>
    /// Loads a dense array of coefficients and a right hand side into a solver.
    /// </summary>
    private static void Load(ISparseSolver<double> solver, double[][] matrix, double[] rhs)
    {
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (matrix[r][c] != 0.0)
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (rhs[r] != 0.0)
                solver.GetElement(r + 1).Value = rhs[r];
        }
    }

    /// <summary>
    /// Checks that a solution really satisfies the original system.
    /// </summary>
    private static void AssertSolves(double[][] matrix, double[] rhs, IVector<double> solution, double tolerance)
    {
        for (int r = 0; r < matrix.Length; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < matrix[r].Length; c++)
                sum += matrix[r][c] * solution[c + 1];
            Assert.That(sum, Is.EqualTo(rhs[r]).Within(tolerance), $"Equation {r + 1} is not satisfied");
        }
    }

    /// <summary>
    /// Checks that a solution satisfies the transposed system.
    /// </summary>
    private static void AssertSolvesTransposed(double[][] matrix, double[] rhs, IVector<double> solution, double tolerance)
    {
        int size = matrix.Length;
        for (int c = 0; c < size; c++)
        {
            double sum = 0.0;
            for (int r = 0; r < size; r++)
                sum += matrix[r][c] * solution[r + 1];
            Assert.That(sum, Is.EqualTo(rhs[c]).Within(tolerance), $"Transposed equation {c + 1} is not satisfied");
        }
    }

    private static double[][] RandomMatrix(int size, double density, Random random)
    {
        double[][] matrix = new double[size][];
        for (int r = 0; r < size; r++)
            matrix[r] = new double[size];
        for (int r = 0; r < size; r++)
        {
            // A nonzero diagonal that dominates keeps the system well away from singular.
            matrix[r][r] = (size * 2.0) + random.NextDouble();
            for (int c = 0; c < size; c++)
            {
                if (r != c && random.NextDouble() < density)
                    matrix[r][c] = (random.NextDouble() * 2.0) - 1.0;
            }
        }
        return matrix;
    }

    private static double[] RandomVector(int size, Random random)
    {
        double[] rhs = new double[size];
        for (int r = 0; r < size; r++)
            rhs[r] = (random.NextDouble() * 20.0) - 10.0;
        return rhs;
    }

    /// <summary>
    /// The largest residual of any equation, measured against the size of the terms that went
    /// into it. That is the only meaningful scale when rows differ by many decades.
    /// </summary>
    private static double Residual(double[][] matrix, double[] rhs, IVector<double> solution, int size)
    {
        double worst = 0.0;
        for (int r = 0; r < size; r++)
        {
            double sum = 0.0, norm = 0.0;
            for (int c = 0; c < size; c++)
            {
                double term = matrix[r][c] * solution[c + 1];
                sum += term;
                norm += Math.Abs(term);
            }
            double denominator = Math.Max(norm, Math.Abs(rhs[r]));
            if (denominator > 0.0)
                worst = Math.Max(worst, Math.Abs(sum - rhs[r]) / denominator);
        }
        return worst;
    }

    [Test]
    public void When_SmallSystem_Expect_CorrectSolution()
    {
        double[][] matrix =
        [
            [2.0, 1.0, 0.0],
            [1.0, 3.0, 1.0],
            [0.0, 1.0, 4.0]
        ];
        double[] rhs = [1.0, 2.0, 3.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(3));

        IVector<double> solution = new DenseVector<double>(3);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);
    }

    [Test]
    public void When_BlockTriangularSystem_Expect_CorrectSolution()
    {
        // Two independent 2x2 blocks with a one-way coupling, so the block triangular form
        // really has something to find.
        double[][] matrix =
        [
            [2.0, 1.0, 3.0, 0.0],
            [1.0, 2.0, 0.0, 5.0],
            [0.0, 0.0, 4.0, 1.0],
            [0.0, 0.0, 1.0, 3.0]
        ];
        double[] rhs = [1.0, -2.0, 3.0, 4.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(4));
        Assert.That(solver.Symbolic.Blocks, Is.GreaterThan(1));

        IVector<double> solution = new DenseVector<double>(4);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);
    }

    [Test]
    public void When_TriangularSystem_Expect_AllSingletonBlocks()
    {
        const int size = 6;
        double[][] matrix = new double[size][];
        for (int r = 0; r < size; r++)
        {
            matrix[r] = new double[size];
            for (int c = 0; c <= r; c++)
                matrix[r][c] = r == c ? r + 2.0 : 1.0;
        }
        double[] rhs = [1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(size));
        Assert.That(solver.Symbolic.Blocks, Is.EqualTo(size));

        // Nothing has to be factored at all here, so there should be no fill whatsoever.
        // Every entry is either the diagonal of a singleton block or sits above the blocks.
        Assert.Multiple(() =>
        {
            Assert.That(solver.MatrixNonZeroCount, Is.EqualTo(size * (size + 1) / 2));
            Assert.That(solver.FactorNonZeroCount, Is.EqualTo(solver.MatrixNonZeroCount));
        });

        IVector<double> solution = new DenseVector<double>(size);
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
            double[][] matrix = RandomMatrix(size, 3.0 / size, random);
            double[] rhs = RandomVector(size, random);

            var klu = new KluRealSolver();
            Load(klu, matrix, rhs);
            Assert.That(klu.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");
            IVector<double> kluSolution = new DenseVector<double>(size);
            klu.ForwardSubstitute(kluSolution);
            klu.BackwardSubstitute(kluSolution);
            AssertSolves(matrix, rhs, kluSolution, 1e-9);

            var sparse = new SparseRealSolver();
            Load(sparse, matrix, rhs);
            Assert.That(sparse.OrderAndFactor(), Is.EqualTo(size));
            IVector<double> reference = new DenseVector<double>(size);
            sparse.ForwardSubstitute(reference);
            sparse.BackwardSubstitute(reference);

            for (int i = 1; i <= size; i++)
                Assert.That(kluSolution[i], Is.EqualTo(reference[i]).Within(1e-9), $"seed {seed}, unknown {i}");
        }
    }

    [Test]
    public void When_RandomSystemsNeedingPivoting_Expect_SameSolutionAsSparseSolver()
    {
        // No diagonal dominance here, and zeros scattered along the diagonal, so the pivot
        // search actually has to do something.
        for (int seed = 0; seed < 40; seed++)
        {
            var random = new Random(7000 + seed);
            int size = 4 + (seed % 25);
            double[][] matrix = new double[size][];
            for (int r = 0; r < size; r++)
            {
                matrix[r] = new double[size];
                for (int c = 0; c < size; c++)
                {
                    if (random.NextDouble() < 4.0 / size)
                        matrix[r][c] = (random.NextDouble() * 2.0) - 1.0;
                }
            }
            double[] rhs = RandomVector(size, random);

            var sparse = new SparseRealSolver();
            Load(sparse, matrix, rhs);
            if (sparse.OrderAndFactor() != size)
                continue;   // genuinely singular, nothing to compare against
            IVector<double> reference = new DenseVector<double>(size);
            sparse.ForwardSubstitute(reference);
            sparse.BackwardSubstitute(reference);

            var klu = new KluRealSolver();
            Load(klu, matrix, rhs);
            Assert.That(klu.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");
            IVector<double> solution = new DenseVector<double>(size);
            klu.ForwardSubstitute(solution);
            klu.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-8);

            klu.ForwardSubstituteTransposed(solution);
            klu.BackwardSubstituteTransposed(solution);
            AssertSolvesTransposed(matrix, rhs, solution, 1e-8);
        }
    }

    [Test]
    public void When_RandomSystemsTransposed_Expect_CorrectSolution()
    {
        for (int seed = 0; seed < 30; seed++)
        {
            var random = new Random(1000 + seed);
            int size = 3 + (seed % 25);
            double[][] matrix = RandomMatrix(size, 3.0 / size, random);
            double[] rhs = RandomVector(size, random);

            var klu = new KluRealSolver();
            Load(klu, matrix, rhs);
            Assert.That(klu.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");
            IVector<double> solution = new DenseVector<double>(size);
            klu.ForwardSubstituteTransposed(solution);
            klu.BackwardSubstituteTransposed(solution);
            AssertSolvesTransposed(matrix, rhs, solution, 1e-9);
        }
    }

    [Test]
    public void When_RefactoredWithNewValues_Expect_CorrectSolution()
    {
        // This is the case the whole design is about: same pattern, different numbers.
        for (int seed = 0; seed < 20; seed++)
        {
            var random = new Random(2000 + seed);
            int size = 4 + (seed % 20);
            double[][] matrix = RandomMatrix(size, 3.0 / size, random);
            double[] rhs = RandomVector(size, random);

            var solver = new KluRealSolver();
            Load(solver, matrix, rhs);
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(size));

            // Change every value that already exists, leaving the pattern alone.
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (matrix[r][c] == 0.0)
                        continue;
                    matrix[r][c] = r == c
                        ? (size * 3.0) + random.NextDouble()
                        : (random.NextDouble() * 2.0) - 1.0;
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                rhs[r] = (random.NextDouble() * 20.0) - 10.0;
                solver.GetElement(r + 1).Value = rhs[r];
            }

            Assert.That(solver.Factor(), Is.True, $"seed {seed}");
            IVector<double> solution = new DenseVector<double>(size);
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-9);
        }
    }

    [Test]
    public void When_ReusedPivotDegrades_Expect_RefactorRefusesRatherThanReturnGarbage()
    {
        // Reusing the pivots from an earlier factorization is only safe while those pivots are
        // still good ones. Collapsing an entry that a pivot depended on has to make refactoring
        // report failure, because the alternative - a factorization that is numerically useless
        // but reports success - leaves the caller with a wrong answer and no way to tell.
        int refusals = 0;
        for (int seed = 0; seed < 200; seed++)
        {
            var random = new Random(11000 + seed);
            int size = 3 + (seed % 6);

            double[][] matrix = new double[size][];
            var pattern = new List<(int Row, int Column)>();
            for (int r = 0; r < size; r++)
            {
                matrix[r] = new double[size];
                for (int c = 0; c < size; c++)
                {
                    if (c == r || random.NextDouble() < 0.5)
                    {
                        matrix[r][c] = (random.NextDouble() * 2.0) - 1.0 + (c == r ? 1.5 : 0.0);
                        pattern.Add((r, c));
                    }
                }
            }
            double[] rhs = RandomVector(size, random);

            var solver = new KluRealSolver();
            var elements = new Dictionary<(int, int), Element<double>>();
            foreach (var (r, c) in pattern)
                elements[(r, c)] = solver.GetElement(new MatrixLocation(r + 1, c + 1));
            for (int r = 0; r < size; r++)
                solver.GetElement(r + 1).Value = rhs[r];
            foreach (var (r, c) in pattern)
                elements[(r, c)].Value = matrix[r][c];

            if (solver.OrderAndFactor() != size)
                continue;

            // Collapse the diagonal, which is what the pivots were most likely built on.
            for (int r = 0; r < size; r++)
            {
                matrix[r][r] *= 1e-12;
                elements[(r, r)].Value = matrix[r][r];
            }

            if (!solver.Factor())
            {
                refusals++;
                if (solver.OrderAndFactor() != size)
                    continue;
            }
            IVector<double> reused = new DenseVector<double>(size);
            solver.ForwardSubstitute(reused);
            solver.BackwardSubstitute(reused);

            // Collapsing the diagonal can leave a system that is genuinely hard to solve, so
            // the bar is not an absolute accuracy but that reusing the route is no worse than
            // factoring the same matrix from scratch.
            var fresh = new KluRealSolver();
            Load(fresh, matrix, rhs);
            if (fresh.OrderAndFactor() != size)
                continue;
            IVector<double> reference = new DenseVector<double>(size);
            fresh.ForwardSubstitute(reference);
            fresh.BackwardSubstitute(reference);

            double reusedResidual = Residual(matrix, rhs, reused, size);
            double freshResidual = Residual(matrix, rhs, reference, size);
            Assert.That(reusedResidual, Is.LessThanOrEqualTo(Math.Max(freshResidual * 100.0, 1e-8)),
                $"seed {seed}: reused route gave residual {reusedResidual:E2}, a fresh factorization {freshResidual:E2}");
        }

        // If nothing was ever refused, the check being tested here is not doing anything.
        Assert.That(refusals, Is.GreaterThan(0));
    }

    [Test]
    public void When_RefactoringRepeatedlyOnIllScaledSystems_Expect_AccurateSolutions()
    {
        // This is the loop every simulation runs: reuse the route if it still holds, fall back
        // to a full factorization if it does not. Rows spanning many decades are what makes a
        // stored pivot go stale, so the answer has to stay accurate across the whole sequence
        // no matter which of the two paths each step takes.
        for (int seed = 0; seed < 30; seed++)
        {
            var random = new Random(9000 + seed);
            int size = 6 + (seed % 20);

            double[][] matrix = new double[size][];
            for (int r = 0; r < size; r++)
                matrix[r] = new double[size];
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

            var solver = new KluRealSolver();
            var elements = new Dictionary<(int, int), Element<double>>();
            foreach (var (r, c) in pattern)
                elements[(r, c)] = solver.GetElement(new MatrixLocation(r + 1, c + 1));
            var rhsElements = new Element<double>[size];
            for (int r = 0; r < size; r++)
                rhsElements[r] = solver.GetElement(r + 1);
            double[] rhs = new double[size];
            IVector<double> solution = new DenseVector<double>(size);

            bool factored = false;
            for (int step = 0; step < 15; step++)
            {
                // Re-roll every value, keeping the pattern, with row scales spanning decades.
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                        matrix[r][c] = 0.0;
                }
                for (int r = 0; r < size; r++)
                {
                    double rowScale = Math.Pow(10.0, (random.NextDouble() * 12.0) - 6.0);
                    foreach (var (rr, cc) in pattern)
                    {
                        if (rr != r)
                            continue;
                        matrix[rr][cc] = rowScale * ((random.NextDouble() * 2.0) - 1.0);
                    }
                    if (matrix[r][r] == 0.0)
                        matrix[r][r] = rowScale;
                    rhs[r] = (random.NextDouble() * 2.0) - 1.0;
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
                        continue;   // genuinely singular draw
                    }
                }
                factored = true;

                solver.ForwardSubstitute(solution);
                solver.BackwardSubstitute(solution);

                // Check the residual relative to the size of the terms involved, which is the
                // only meaningful measure when the rows differ by many decades.
                for (int r = 0; r < size; r++)
                {
                    double sum = 0.0, norm = 0.0;
                    for (int c = 0; c < size; c++)
                    {
                        double term = matrix[r][c] * solution[c + 1];
                        sum += term;
                        norm += Math.Abs(term);
                    }
                    double denominator = Math.Max(norm, Math.Abs(rhs[r]));
                    if (denominator > 0.0)
                    {
                        Assert.That(Math.Abs(sum - rhs[r]) / denominator, Is.LessThan(1e-8),
                            $"seed {seed}, step {step}, equation {r + 1}");
                    }
                }
            }
        }
    }

    [Test]
    public void When_PatternGrowsAfterFactoring_Expect_Reanalysis()
    {
        double[][] matrix =
        [
            [2.0, 1.0, 0.0],
            [1.0, 3.0, 0.0],
            [0.0, 0.0, 4.0]
        ];
        double[] rhs = [1.0, 2.0, 3.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(3));

        // Adding an entry invalidates the analysis, so refactoring has to refuse.
        matrix[2][0] = 1.5;
        solver.GetElement(new MatrixLocation(3, 1)).Value = 1.5;
        Assert.That(solver.Factor(), Is.False);

        Assert.That(solver.OrderAndFactor(), Is.EqualTo(3));
        IVector<double> solution = new DenseVector<double>(3);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);
    }

    [Test]
    public void When_SingularSystem_Expect_FailureAtRow()
    {
        // The third equation is the sum of the first two.
        double[][] matrix =
        [
            [1.0, 1.0, 0.0],
            [0.0, 1.0, 1.0],
            [1.0, 2.0, 1.0]
        ];
        double[] rhs = [1.0, 2.0, 3.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.LessThan(3));
    }

    [Test]
    public void When_StructurallySingularSystem_Expect_Failure()
    {
        // The second column is empty.
        var solver = new KluRealSolver();
        solver.GetElement(new MatrixLocation(1, 1)).Value = 1.0;
        solver.GetElement(new MatrixLocation(2, 1)).Value = 1.0;
        solver.GetElement(new MatrixLocation(3, 3)).Value = 1.0;
        solver.GetElement(1).Value = 1.0;

        Assert.That(solver.OrderAndFactor(), Is.LessThan(3));
    }

    [Test]
    public void When_DegeneracySet_Expect_Exception()
    {
        var solver = new KluRealSolver { Degeneracy = 1 };
        solver.GetElement(new MatrixLocation(1, 1)).Value = 1.0;
        Assert.Throws<AlgebraException>(() => solver.OrderAndFactor());
    }

    [Test]
    public void When_ClearedAndReused_Expect_CorrectSolution()
    {
        double[][] matrix =
        [
            [2.0, 1.0],
            [1.0, 3.0]
        ];
        double[] rhs = [1.0, 2.0];

        var solver = new KluRealSolver();
        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));
        solver.Clear();

        Load(solver, matrix, rhs);
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));
        IVector<double> solution = new DenseVector<double>(2);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        AssertSolves(matrix, rhs, solution, 1e-12);
    }

    [Test]
    [TestCase(KluOrdering.ApproximateMinimumDegree, KluScaling.Sum, true)]
    [TestCase(KluOrdering.ApproximateMinimumDegree, KluScaling.None, true)]
    [TestCase(KluOrdering.ApproximateMinimumDegree, KluScaling.Maximum, true)]
    [TestCase(KluOrdering.ColumnApproximateMinimumDegree, KluScaling.Sum, true)]
    [TestCase(KluOrdering.Natural, KluScaling.Sum, true)]
    [TestCase(KluOrdering.ApproximateMinimumDegree, KluScaling.Sum, false)]
    [TestCase(KluOrdering.Natural, KluScaling.None, false)]
    public void When_ParametersVaried_Expect_CorrectSolution(KluOrdering ordering, KluScaling scaling, bool blockTriangular)
    {
        for (int seed = 0; seed < 10; seed++)
        {
            var random = new Random(3000 + seed);
            int size = 8 + seed;
            double[][] matrix = RandomMatrix(size, 3.0 / size, random);
            double[] rhs = RandomVector(size, random);

            var solver = new KluRealSolver();
            solver.Parameters.Ordering = ordering;
            solver.Parameters.Scaling = scaling;
            solver.Parameters.UseBlockTriangularForm = blockTriangular;
            Load(solver, matrix, rhs);
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(size), $"seed {seed}");

            IVector<double> solution = new DenseVector<double>(size);
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);
            AssertSolves(matrix, rhs, solution, 1e-9);

            solver.ForwardSubstituteTransposed(solution);
            solver.BackwardSubstituteTransposed(solution);
            AssertSolvesTransposed(matrix, rhs, solution, 1e-9);
        }
    }

    [Test]
    public void When_BigMatrix_Expect_CorrectSolutionAndLessFillThanSparseSolver()
    {
        // fidapm05 on its own is numerically singular, so a diagonal shift makes it into a
        // system that has a meaningful answer to compare against. The pattern, which is what
        // the ordering has to cope with, is untouched.
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Algebra", "Matrices", "fidapm05");

        // The sparse solver factors in place, so both solvers get loaded before either runs.
        var reference = new SparseRealSolver();
        ReadMatrix(reference, path);
        var klu = new KluRealSolver();
        ReadMatrix(klu, path);

        int size = reference.Size;
        Assert.That(klu.Size, Is.EqualTo(size));
        for (int i = 1; i <= size; i++)
        {
            reference.GetElement(new MatrixLocation(i, i)).Add(100.0);
            klu.GetElement(new MatrixLocation(i, i)).Add(100.0);
            reference.GetElement(i).Value = i % 7;
            klu.GetElement(i).Value = i % 7;
        }

        Assert.That(reference.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> expected = new DenseVector<double>(size);
        reference.ForwardSubstitute(expected);
        reference.BackwardSubstitute(expected);

        Assert.That(klu.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> actual = new DenseVector<double>(size);
        klu.ForwardSubstitute(actual);
        klu.BackwardSubstitute(actual);

        for (int i = 1; i <= size; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(1e-9), $"unknown {i}");

        // The ordering should not be causing more fill than the incremental Markowitz search
        // the other solver does.
        int fill = klu.FactorNonZeroCount - klu.MatrixNonZeroCount;
        Assert.That(fill, Is.LessThanOrEqualTo(reference.Fillins),
            $"KLU caused {fill} fill-ins, the sparse solver {reference.Fillins}");
    }

    [Test]
    public void When_Spice3f5Matrix_Expect_SameSolutionAsSparseSolver()
    {
        string matrix = Path.Combine(TestContext.CurrentContext.TestDirectory, "Algebra", "Matrices", "spice3f5_matrix01.dat");
        string vector = Path.Combine(TestContext.CurrentContext.TestDirectory, "Algebra", "Matrices", "spice3f5_vector01.dat");

        var reference = ReadSpice3f5File(matrix, vector);
        var klu = new KluRealSolver();
        LoadSpice3f5File(klu, matrix, vector);

        int size = reference.Size;
        Assert.That(klu.Size, Is.EqualTo(size));

        ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
        reference.Precondition((m, v) => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(m, m.Size));
        Assert.That(reference.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> expected = new DenseVector<double>(size);
        reference.ForwardSubstitute(expected);
        reference.BackwardSubstitute(expected);

        Assert.That(klu.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> actual = new DenseVector<double>(size);
        klu.ForwardSubstitute(actual);
        klu.BackwardSubstitute(actual);

        for (int i = 1; i <= size; i++)
        {
            double tolerance = Math.Max(Math.Abs(expected[i]) * 1e-6, 1e-9);
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(tolerance), $"unknown {i}");
        }

        // A real circuit matrix should break into blocks and need very little fill.
        Assert.That(klu.Symbolic.Blocks, Is.GreaterThan(1));
    }

    [Test]
    public void When_Spice3f5MatrixPreordered_Expect_SameSolution()
    {
        // The simulations precondition the solver before factoring, which swaps columns
        // around. That has to leave the analysis in a consistent state.
        string matrix = Path.Combine(TestContext.CurrentContext.TestDirectory, "Algebra", "Matrices", "spice3f5_matrix01.dat");
        string vector = Path.Combine(TestContext.CurrentContext.TestDirectory, "Algebra", "Matrices", "spice3f5_vector01.dat");

        var plain = new KluRealSolver();
        LoadSpice3f5File(plain, matrix, vector);
        int size = plain.Size;
        Assert.That(plain.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> expected = new DenseVector<double>(size);
        plain.ForwardSubstitute(expected);
        plain.BackwardSubstitute(expected);

        var preordered = new KluRealSolver();
        LoadSpice3f5File(preordered, matrix, vector);
        ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
        preordered.Precondition((m, v) => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(m, m.Size));
        Assert.That(preordered.OrderAndFactor(), Is.EqualTo(size));
        IVector<double> actual = new DenseVector<double>(size);
        preordered.ForwardSubstitute(actual);
        preordered.BackwardSubstitute(actual);

        for (int i = 1; i <= size; i++)
        {
            double tolerance = Math.Max(Math.Abs(expected[i]) * 1e-6, 1e-9);
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(tolerance), $"unknown {i}");
        }
    }
}
