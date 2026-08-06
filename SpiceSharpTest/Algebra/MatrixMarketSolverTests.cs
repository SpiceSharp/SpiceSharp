using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SpiceSharpTest.Algebra;

/// <summary>
/// Factors and solves published sparse matrices with every solver and checks the answers
/// against a reference.
/// </summary>
/// <remarks>
/// <para>
/// The matrices come from the SuiteSparse Matrix Collection and live in
/// <c>Algebra/Matrices</c>; see the readme there for the sources and the license. Most of them
/// are circuit simulation matrices, which is what this library actually has to cope with, and
/// they bring in the things a hand-written test does not: rows that differ by decades,
/// patterns that are nowhere near symmetric, block structure, and empty diagonals.
/// </para>
/// <para>
/// The reference is a manufactured solution. A known vector <c>x</c> is multiplied by the
/// matrix to get a right-hand side, so the answer is known before anything is factored and
/// does not depend on another implementation being trusted. Two things are then checked:
/// </para>
/// <list type="bullet">
///   <item><description>
///     The solution matches <c>x</c>. How closely it can match is set by the matrix, not by
///     the solver: an error of about <c>condition * epsilon</c> is the best any factorization
///     in double precision can do, so that is what the tolerance is derived from.
///   </description></item>
///   <item><description>
///     The residual is small relative to the size of the terms that make it up. This is the
///     stricter check of the two and the one that actually says the factorization is sound,
///     because it holds no matter how ill-conditioned the matrix is.
///   </description></item>
/// </list>
/// </remarks>
[TestFixture]
public class MatrixMarketSolverTests
{
    /// <summary>
    /// The solvers a matrix is run against. Anything less than <see cref="Both"/> is
    /// explained where it is used.
    /// </summary>
    [Flags]
    public enum Solvers
    {
        /// <summary>The Markowitz solver.</summary>
        Sparse = 1,

        /// <summary>The KLU solver.</summary>
        Klu = 2,

        /// <summary>Both solvers.</summary>
        Both = Sparse | Klu
    }

    /// <summary>
    /// One matrix from <c>Algebra/Matrices</c>, with the properties needed to know what to
    /// expect from it.
    /// </summary>
    /// <param name="Name">The file name, without the <c>.mtx.gz</c> extension.</param>
    /// <param name="Size">The number of equations.</param>
    /// <param name="Count">
    /// The number of entries the reader should produce. For a file that only stores one
    /// triangle this includes the mirrored half, so it also pins down that the file was read
    /// the way its header says.
    /// </param>
    /// <param name="Condition">
    /// An estimate of the 1-norm condition number, computed with SciPy's sparse LU. It only
    /// needs to be right to an order of magnitude; it sets the tolerance.
    /// </param>
    /// <param name="Solvers">The solvers this matrix is run against.</param>
    /// <param name="Refactor">
    /// Whether to also check that refactoring reproduces the factorization. That path does
    /// not depend on the matrix beyond its structure, so only a handful of matrices are
    /// picked for it rather than paying for all of them.
    /// </param>
    public sealed record Case(string Name, int Size, int Count, double Condition,
        Solvers Solvers = Solvers.Both, bool Refactor = false)
    {
        /// <inheritdoc/>
        public override string ToString() => Name;
    }

    // Real matrices, smallest first. The condition numbers span eleven decades, which is
    // deliberate: the well-conditioned ones pin the solution down to a few ulps, and the
    // ill-conditioned ones are where a factorization that is not backward stable shows up.
    private static readonly Case[] _realCases =
    [
        new("west0067", 67, 294, 2.998e+02),
        new("Hamrle1", 32, 98, 1.168e+06),
        new("rajat11", 135, 812, 9.442e+05),
        new("rajat05", 301, 1384, 2.237e+05),
        new("rajat14", 180, 1503, 4.197e+08, Refactor: true),
        new("oscil_dcop_01", 430, 1544, 1.561e+13, Refactor: true),
        new("fs_760_1", 760, 5976, 8.362e+03),
        new("rajat19", 1157, 5399, 9.173e+10, Refactor: true),
        new("adder_dcop_01", 1813, 11156, 1.298e+08),
        new("add20", 2395, 17319, 1.764e+04, Refactor: true),
        new("circuit_2", 4510, 21199, 7.118e+06),
        new("add32", 4960, 23884, 2.136e+02),
        new("circuit_1", 2624, 35823, 3.268e+05, Refactor: true),

        // The Markowitz solver does not reach a backward stable factorization on these two,
        // landing at a residual of 8e-11 and 1e-8 where KLU gets 1e-12 and 2e-13. This is the
        // relative pivot threshold rather than anything going wrong: at its default of 1e-3 a
        // pivot may be a thousand times smaller than the largest entry in its column, and each
        // such pivot is licence for the entries around it to grow by that much. Both of these
        // matrices have a pattern that is almost entirely unsymmetric, and instrumenting the
        // search shows they take pivots right at that limit, worst ratios of 940 and 990. Real
        // circuit matrices never come close - add32 never goes past 0.96, circuit_2 0.98,
        // circuit_1 0.14 - which is why the loose default costs nothing in practice and is
        // kept. Raising it to 0.1 puts both of these at 1e-14 or better, at the price of four
        // times the fill on Hamrle2, so it is available per solver for anyone who needs it:
        //
        //     solver.Parameters.RelativePivotThreshold = 0.1;
        //
        // circuit_3 is a fair bit better than the number above suggests in real use. The
        // simulations precondition with PreorderModifiedNodalAnalysis, which on its own brings
        // it to 9e-13 with a third of the fill; this fixture loads the matrix bare.
        new("Hamrle2", 5952, 22162, 3.544e+05, Solvers.Klu, Refactor: true),
        new("circuit_3", 12127, 48137, 3.512e+10, Solvers.Klu),

        // The largest matrix here. The Markowitz solver does solve it, and accurately, but
        // needs several seconds to order it, which is more than this is worth on every run
        // when add32 and circuit_1 already cover that solver at a few thousand unknowns.
        // KLU is the one that has to stay quick as the matrix grows, so it carries the large
        // end on its own.
        new("memplus", 17758, 126150, 2.666e+05, Solvers.Klu),
    ];

    // Complex matrices. There are no complex circuit matrices in the collection, so these
    // come from electromagnetics and acoustics. young1c is well conditioned enough to hold
    // the solution to a few ulps, and mhd1280b only stores one triangle, so reading it
    // exercises the Hermitian mirror.
    private static readonly Case[] _complexCases =
    [
        new("young1c", 841, 4089, 5.229e+02),
        new("dwg961b", 961, 10591, 3.355e+07),
        new("mhd1280b", 1280, 22778, 5.988e+12),
        new("qc324", 324, 26730, 7.384e+04),
    ];

    private const double Epsilon = 2.220446049250313e-16;

    // How much slack the forward error gets over condition * epsilon. Measured over the
    // matrices above, the worst any solver actually needs is about 20x, so this leaves an
    // order of magnitude before a test starts being fragile.
    private const double Margin = 250.0;

    // The residual is capped independently of the conditioning, because a backward stable
    // factorization keeps it at a small multiple of epsilon whatever the matrix looks like.
    // Nearly every matrix here lands between 1e-17 and 1e-14. The three that do not are
    // circuit_2, circuit_3 and Hamrle2 under KLU, which reach 1e-12 because of the row
    // scaling it applies, so the bound sits above that rather than at the 1e-14 the rest of
    // them would allow. It is still two decades below what the Markowitz solver produces on
    // the two matrices it cannot handle.
    private const double ResidualTolerance = 1e-10;

    /// <summary>
    /// The accuracy the matrix allows. Beyond a point the bound stops saying anything, so it
    /// is capped: a solver that returns nothing but noise still has to fail.
    /// </summary>
    private static double ForwardTolerance(Case matrix)
        => Math.Min(1e-2, Margin * matrix.Condition * Epsilon);

    /// <summary>
    /// Test sources. NUnit needs these to be public.
    /// </summary>
    public static IEnumerable<Case> RealMatrices() => _realCases;

    /// <summary>
    /// Test sources. NUnit needs these to be public.
    /// </summary>
    public static IEnumerable<Case> RealMatricesForSparseSolver()
    {
        foreach (var matrix in _realCases)
        {
            if (matrix.Solvers.HasFlag(Solvers.Sparse))
                yield return matrix;
        }
    }

    /// <summary>
    /// Test sources. NUnit needs these to be public.
    /// </summary>
    public static IEnumerable<Case> RealMatricesForRefactoring()
    {
        foreach (var matrix in _realCases)
        {
            if (matrix.Refactor)
                yield return matrix;
        }
    }

    /// <summary>
    /// Test sources. NUnit needs these to be public.
    /// </summary>
    public static IEnumerable<Case> ComplexMatrices() => _complexCases;

    private static MatrixMarketFile Read(Case matrix)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Algebra", "Matrices", matrix.Name + ".mtx.gz");
        var file = MatrixMarketFile.Read(path);
        Assert.Multiple(() =>
        {
            Assert.That(file.Size, Is.EqualTo(matrix.Size), $"{matrix.Name}: size");
            Assert.That(file.Count, Is.EqualTo(matrix.Count), $"{matrix.Name}: entry count");
        });
        return file;
    }

    /// <summary>
    /// The vector that the right-hand side is built from. The entries vary but stay the same
    /// order of magnitude, so no part of the answer is lost next to another.
    /// </summary>
    private static double[] Reference(int size)
    {
        double[] x = new double[size + 1];
        for (int i = 1; i <= size; i++)
            x[i] = (i % 2 == 0 ? 1.0 : -1.0) * (1.0 + ((i - 1) % 7 / 7.0));
        return x;
    }

    private static Complex[] ReferenceComplex(int size)
    {
        var x = new Complex[size + 1];
        for (int i = 1; i <= size; i++)
        {
            x[i] = new Complex(
                (i % 2 == 0 ? 1.0 : -1.0) * (1.0 + ((i - 1) % 7 / 7.0)),
                (i % 3 == 0 ? -1.0 : 1.0) * (0.5 + ((i - 1) % 5 / 5.0)));
        }
        return x;
    }

    /// <summary>
    /// The backward error of a solution: the largest residual of any equation, relative to
    /// the size of the system it came out of.
    /// </summary>
    /// <remarks>
    /// This is the usual measure of whether an LU factorization did its job. Unlike the
    /// forward error it does not grow with the conditioning of the matrix, so a
    /// factorization that chose its pivots well keeps it near <c>epsilon</c> no matter what
    /// it was handed. Scaling by the norms rather than per row keeps it from being swamped
    /// by a single row whose terms happen to cancel almost completely, which in a circuit
    /// matrix is normal rather than a sign of trouble.
    /// </remarks>
    private static double ScaledResidual(MatrixMarketFile file, double[] b,
        IVector<double> solution, bool transposed)
    {
        int size = file.Size;
        double[] x = new double[size + 1];
        for (int i = 1; i <= size; i++)
            x[i] = solution[i];

        double[] product = file.Multiply(x, transposed);
        double residual = 0.0, largestX = 0.0, largestB = 0.0;
        for (int i = 1; i <= size; i++)
        {
            residual = Math.Max(residual, Math.Abs(product[i] - b[i]));
            largestX = Math.Max(largestX, Math.Abs(x[i]));
            largestB = Math.Max(largestB, Math.Abs(b[i]));
        }
        return residual / ((file.InfinityNorm(transposed) * largestX) + largestB);
    }

    private static double ScaledResidual(MatrixMarketFile file, Complex[] b,
        IVector<Complex> solution, bool transposed)
    {
        int size = file.Size;
        var x = new Complex[size + 1];
        for (int i = 1; i <= size; i++)
            x[i] = solution[i];

        Complex[] product = file.Multiply(x, transposed);
        double residual = 0.0, largestX = 0.0, largestB = 0.0;
        for (int i = 1; i <= size; i++)
        {
            residual = Math.Max(residual, (product[i] - b[i]).Magnitude);
            largestX = Math.Max(largestX, x[i].Magnitude);
            largestB = Math.Max(largestB, b[i].Magnitude);
        }
        return residual / ((file.InfinityNorm(transposed) * largestX) + largestB);
    }

    private static void AssertForwardError(Case matrix, double[] expected,
        IVector<double> actual)
    {
        double worst = 0.0, scale = 0.0;
        int where = 0;
        for (int i = 1; i <= matrix.Size; i++)
        {
            double error = Math.Abs(actual[i] - expected[i]);
            if (error > worst)
            {
                worst = error;
                where = i;
            }
            scale = Math.Max(scale, Math.Abs(expected[i]));
        }
        double tolerance = ForwardTolerance(matrix);
        Assert.That(worst / scale, Is.LessThan(tolerance),
            $"{matrix.Name}: worst relative error {worst / scale:e2} at unknown {where}, "
            + $"condition {matrix.Condition:e2} allows {tolerance:e2}");
    }

    private static void AssertForwardError(Case matrix, Complex[] expected,
        IVector<Complex> actual)
    {
        double worst = 0.0, scale = 0.0;
        int where = 0;
        for (int i = 1; i <= matrix.Size; i++)
        {
            double error = (actual[i] - expected[i]).Magnitude;
            if (error > worst)
            {
                worst = error;
                where = i;
            }
            scale = Math.Max(scale, expected[i].Magnitude);
        }
        double tolerance = ForwardTolerance(matrix);
        Assert.That(worst / scale, Is.LessThan(tolerance),
            $"{matrix.Name}: worst relative error {worst / scale:e2} at unknown {where}, "
            + $"condition {matrix.Condition:e2} allows {tolerance:e2}");
    }

    /// <summary>
    /// Every entry of the matrix ends up in the factors somewhere, so the factors can never
    /// hold fewer entries than the matrix did. Several of the circuit matrices here break
    /// into hundreds of blocks, which is what makes them worth checking: the entries above
    /// the diagonal blocks are the ones easily left out of the count.
    /// </summary>
    private static void AssertFillIsNotNegative<T>(Case matrix, ISparsePivotingSolver<T> solver)
    {
        if (solver is not KluSolver<T> klu)
            return;
        Assert.That(klu.FactorNonZeroCount, Is.GreaterThanOrEqualTo(klu.MatrixNonZeroCount),
            $"{matrix.Name}: the factors hold {klu.FactorNonZeroCount} entries but the matrix "
            + $"had {klu.MatrixNonZeroCount}, which is a fill of "
            + $"{klu.FactorNonZeroCount - klu.MatrixNonZeroCount}");
    }

    private static void RunReal(Case matrix, ISparsePivotingSolver<double> solver)
    {
        var file = Read(matrix);
        file.LoadInto(solver);

        double[] expected = Reference(file.Size);
        double[] b = file.Multiply(expected);
        for (int i = 1; i <= file.Size; i++)
        {
            if (b[i] != 0.0)
                solver.GetElement(i).Value = b[i];
        }

        Assert.That(solver.OrderAndFactor(), Is.EqualTo(file.Size),
            $"{matrix.Name}: the factorization did not get through every equation");
        AssertFillIsNotNegative(matrix, solver);

        IVector<double> solution = new DenseVector<double>(file.Size);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);

        AssertForwardError(matrix, expected, solution);
        Assert.That(ScaledResidual(file, b, solution, false), Is.LessThan(ResidualTolerance),
            $"{matrix.Name}: residual");

        // The transposed solve uses the same right-hand side and the same factors, so its
        // answer is a different one and there is nothing to compare it to except the system
        // it is supposed to satisfy.
        solver.ForwardSubstituteTransposed(solution);
        solver.BackwardSubstituteTransposed(solution);
        Assert.That(ScaledResidual(file, b, solution, true), Is.LessThan(ResidualTolerance),
            $"{matrix.Name}: transposed residual");
    }

    private static void RunComplex(Case matrix, ISparsePivotingSolver<Complex> solver)
    {
        var file = Read(matrix);
        file.LoadInto(solver);

        Complex[] expected = ReferenceComplex(file.Size);
        Complex[] b = file.Multiply(expected);
        for (int i = 1; i <= file.Size; i++)
        {
            if (b[i] != Complex.Zero)
                solver.GetElement(i).Value = b[i];
        }

        Assert.That(solver.OrderAndFactor(), Is.EqualTo(file.Size),
            $"{matrix.Name}: the factorization did not get through every equation");
        AssertFillIsNotNegative(matrix, solver);

        IVector<Complex> solution = new DenseVector<Complex>(file.Size);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);

        AssertForwardError(matrix, expected, solution);
        Assert.That(ScaledResidual(file, b, solution, false), Is.LessThan(ResidualTolerance),
            $"{matrix.Name}: residual");

        solver.ForwardSubstituteTransposed(solution);
        solver.BackwardSubstituteTransposed(solution);
        Assert.That(ScaledResidual(file, b, solution, true), Is.LessThan(ResidualTolerance),
            $"{matrix.Name}: transposed residual");
    }

    [Test]
    [TestCaseSource(nameof(RealMatricesForSparseSolver))]
    public void When_RealMatrixFactoredBySparseSolver_Expect_ReferenceSolution(Case matrix)
        => RunReal(matrix, new SparseRealSolver());

    [Test]
    [TestCaseSource(nameof(RealMatrices))]
    public void When_RealMatrixFactoredByKluSolver_Expect_ReferenceSolution(Case matrix)
        => RunReal(matrix, new KluRealSolver());

    [Test]
    [TestCaseSource(nameof(ComplexMatrices))]
    public void When_ComplexMatrixFactoredBySparseSolver_Expect_ReferenceSolution(Case matrix)
        => RunComplex(matrix, new SparseComplexSolver());

    [Test]
    [TestCaseSource(nameof(ComplexMatrices))]
    public void When_ComplexMatrixFactoredByKluSolver_Expect_ReferenceSolution(Case matrix)
        => RunComplex(matrix, new KluComplexSolver());

    [Test]
    [TestCaseSource(nameof(RealMatricesForRefactoring))]
    public void When_RealMatrixRefactoredByKluSolver_Expect_SameSolution(Case matrix)
    {
        // Refactoring reuses the pivots that OrderAndFactor picked. Feeding it the same
        // numbers again has to give the same answer, which is what every simulation relies
        // on between iterations.
        var solver = new KluRealSolver();
        var file = Read(matrix);
        file.LoadInto(solver);

        double[] expected = Reference(file.Size);
        double[] b = file.Multiply(expected);
        var rhs = new List<(int Row, double Value)>();
        for (int i = 1; i <= file.Size; i++)
        {
            if (b[i] != 0.0)
                rhs.Add((i, b[i]));
        }
        foreach (var (row, value) in rhs)
            solver.GetElement(row).Value = value;

        Assert.That(solver.OrderAndFactor(), Is.EqualTo(file.Size), matrix.Name);
        IVector<double> first = new DenseVector<double>(file.Size);
        solver.ForwardSubstitute(first);
        solver.BackwardSubstitute(first);

        solver.Reset();
        file.LoadInto(solver);
        foreach (var (row, value) in rhs)
            solver.GetElement(row).Value = value;
        Assert.That(solver.Factor(), Is.True, $"{matrix.Name}: refactoring failed");

        IVector<double> second = new DenseVector<double>(file.Size);
        solver.ForwardSubstitute(second);
        solver.BackwardSubstitute(second);

        for (int i = 1; i <= file.Size; i++)
            Assert.That(second[i], Is.EqualTo(first[i]), $"{matrix.Name}: unknown {i}");
    }
}
