using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.IO;
using System.Numerics;

namespace SpiceSharpTest.Algebra;

[TestFixture]
public class SparseSolverTests : SolveFramework
{
    [Test]
    public void When_BigMatrix_Expect_NoException()
    {
        // Test factoring a big matrix
        var solver = new SparseRealSolver();
        ReadMatrix(solver, Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "fidapm05")));

        // Order and factor this larger matrix
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
    }

    [Test]
    public void When_Spice3f5Reference01_Expect_NoException()
    {
        // Load a matrix from Spice 3f5
        var solver = ReadSpice3f5File(
            Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_matrix01.dat")),
            Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_vector01.dat")));

        // Order and factor
        ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
        solver.Precondition((matrix, vector) => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, matrix.Size));
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));

        IVector<double> solution = new DenseVector<double>(solver.Size);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
    }

    [Test]
    public void When_SingletonPivoting_Expect_NoException()
    {
        // Build the solver with only the singleton pivoting
        var solver = new SparseRealSolver();
        solver.Parameters.Strategies.Clear();
        solver.Parameters.Strategies.Add(new MarkowitzSingleton<double>());

        // Build the matrix that should be solvable using only the singleton pivoting strategy
        double[][] matrix =
        [
            [0, 0, 1, 0],
            [1, 1, 1, 1],
            [0, 0, 0, 1],
            [1, 0, 0, 0]
        ];
        double[] rhs = [0, 1, 0, 0];
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (!matrix[r][c].Equals(0.0))
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (!rhs[r].Equals(0.0))
                solver.GetElement(r + 1).Value = rhs[r];
        }

        // This should run without throwing an exception
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
    }

    [Test]
    public void When_RhsReachesPastTheMatrix_Expect_Singular()
    {
        // The solver's size is the larger of the matrix and the right-hand side, so an
        // equation that has a right-hand side but no matrix entry at all stretches the
        // system past the matrix. Those rows and columns are empty, which makes the matrix
        // singular, and the pivot search has to be able to walk over them to report that
        // rather than running off the end of its own bookkeeping.
        var solver = new SparseRealSolver();
        solver.GetElement(new MatrixLocation(1, 1)).Value = 2.0;
        solver.GetElement(new MatrixLocation(2, 2)).Value = 3.0;
        solver.GetElement(4).Value = 1.0;

        Assert.That(solver.Size, Is.EqualTo(4));
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));
    }

    [Test]
    public void When_ComplexRhsReachesPastTheMatrix_Expect_Singular()
    {
        var solver = new SparseComplexSolver();
        solver.GetElement(new MatrixLocation(1, 1)).Value = 2.0;
        solver.GetElement(new MatrixLocation(2, 2)).Value = 3.0;
        solver.GetElement(4).Value = 1.0;

        Assert.That(solver.Size, Is.EqualTo(4));
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));
    }

    [Test]
    public void When_RhsShorterThanMatrix_Expect_CorrectSolution()
    {
        // The other way around: nothing drives the last equation, which is ordinary and has
        // to keep working.
        var solver = new SparseRealSolver();
        solver.GetElement(new MatrixLocation(1, 1)).Value = 2.0;
        solver.GetElement(new MatrixLocation(1, 2)).Value = 1.0;
        solver.GetElement(new MatrixLocation(2, 1)).Value = 1.0;
        solver.GetElement(new MatrixLocation(2, 2)).Value = 3.0;
        solver.GetElement(1).Value = 5.0;

        Assert.That(solver.Size, Is.EqualTo(2));
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));

        IVector<double> solution = new DenseVector<double>(2);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        Assert.Multiple(() =>
        {
            Assert.That(solution[1], Is.EqualTo(3.0).Within(1e-12));
            Assert.That(solution[2], Is.EqualTo(-1.0).Within(1e-12));
        });
    }

    [Test]
    public void When_QuickDiagonalPivoting_Expect_NoException()
    {
        // Build the solver with only the quick diagonal pivoting
        var solver = new SparseRealSolver();
        var strategy = solver.Parameters;
        strategy.Strategies.Clear();
        strategy.Strategies.Add(new MarkowitzQuickDiagonal<double>());

        // Build the matrix that should be solvable using only the singleton pivoting strategy
        double[][] matrix =
        [
            [1,    0.5,   0,      0],
            [-0.5,      5,   4,      0],
            [0,      3,   2,    0.1],
            [0,      0,  -0.01,   3]
        ];
        double[] rhs = [0, 0, 0, 0];
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (!matrix[r][c].Equals(0.0))
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (!rhs[r].Equals(0.0))
                solver.GetElement(r + 1).Value = rhs[r];
        }

        // This should run without throwing an exception
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
    }

    [Test]
    public void When_QuickDiagonalPivotingHasTiedCandidates_Expect_BestConditionedChosen()
    {
        // Choosing between diagonals that tie on their Markowitz product, by how far each one
        // is from the largest element in its column, is what this strategy documents itself as
        // doing. For a while it could not: the test that skips worse candidates read
        // 'product >= minMarkowitzProduct', which also skipped the equal ones, so the branch
        // that collects ties was unreachable and only ever one candidate reached the
        // comparison at the end. It then took whichever diagonal happened to reach the lowest
        // product first, as long as it scraped past the relative threshold.
        //
        // Both diagonals below have three entries in their row and three in their column, so
        // both score a Markowitz product of four and neither is a singleton. The first one is
        // a hundred times smaller than the entry under it; the second is the largest in its
        // column. Scanning runs low index first, so the weak one is met first.
        var matrix = new SparseMatrix<double>();
        var rhs = new SparseVector<double>();

        // Columns 1 and 2 hold the two tied diagonals, 3 to 5 pad the rows and columns out so
        // that nothing scores lower than they do.
        double[][] values =
        [
            [1e-2, 0.0,  1.0,  1.0,  0.0],
            [0.0,  1.0,  1.0,  0.0,  1.0],
            [1.0,  1.0,  1.0,  1.0,  1.0],
            [1.0,  0.0,  1.0,  1.0,  1.0],
            [0.0,  1.0,  1.0,  1.0,  1.0]
        ];
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                if (values[r][c] != 0.0)
                    matrix.GetElement(new MatrixLocation(r + 1, c + 1)).Value = values[r][c];
            }
        }

        var markowitz = new Markowitz<double>(Math.Abs);
        markowitz.Setup(matrix, rhs, 1, 5);
        Assert.That(markowitz.Product(1), Is.EqualTo(markowitz.Product(2)),
            "the two diagonals are supposed to tie");

        var strategy = new MarkowitzQuickDiagonal<double>();
        var pivot = strategy.FindPivot(markowitz, matrix, 1, 5);

        Assert.That(pivot.Element, Is.Not.Null);
        Assert.That(pivot.Element.Row, Is.EqualTo(2),
            $"picked the diagonal at {pivot.Element.Row} with value {pivot.Element.Value}, "
            + "which is not the best conditioned of the tied candidates");
    }

    [Test]
    public void When_DiagonalPivoting_Expect_NoException()
    {
        // Build the solver with only the quick diagonal pivoting
        var solver = new SparseRealSolver();
        var strategy = solver.Parameters;
        strategy.Strategies.Clear();
        strategy.Strategies.Add(new MarkowitzDiagonal<double>());

        // Build the matrix that should be solvable using only the singleton pivoting strategy
        double[][] matrix =
        [
            [1,    0.5,   0,      0],
            [0,      5,   4,      0],
            [0,      3,   2,      0],
            [0,      0,  -0.01,   3]
        ];
        double[] rhs = [1, 0, 0, 0];
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (!matrix[r][c].Equals(0.0))
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (!rhs[r].Equals(0.0))
                solver.GetElement(r + 1).Value = rhs[r];
        }

        // This should run without throwing an exception
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
    }

    [Test]
    public void When_EntireMatrixPivoting_Expect_NoException()
    {
        // Build the solver with only the quick diagonal pivoting
        var solver = new SparseRealSolver();
        var strategy = solver.Parameters;
        strategy.Strategies.Clear();
        strategy.Strategies.Add(new MarkowitzEntireMatrix<double>());

        // Build the matrix that should be solvable using only the singleton pivoting strategy
        double[][] matrix =
        [
            [1,    0.5,      0,  2],
            [2,      5,      4,  3],
            [0,      3,      2,  0],
            [4,    1.8,  -0.01,  8]
        ];
        double[] rhs = [1, 2, 3, 4];
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (!matrix[r][c].Equals(0.0))
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
            if (!rhs[r].Equals(0.0))
                solver.GetElement(r + 1).Value = rhs[r];
        }

        // This should run without throwing an exception
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
    }

    [Test]
    public void When_ExampleComplexMatrix1_Expect_MatlabReference()
    {
        // Build the example matrix
        Complex[][] matrix =
        [
            [0, 0, 0, 0, 1, 0, 1, 0],
            [0, 0, 0, 0, -1, 1, 0, 0],
            [0, 0, new Complex(0.0, 0.000628318530717959), 0, 0, 0, -1, 1],
            [0, 0, 0, 0.001, 0, 0, 0, -1],
            [1, -1, 0, 0, 0, 0, 0, 0],
            [0, 1, 0, 0, 0, 0, 0, 0],
            [1, 0, -1, 0, 0, 0, 0, 0],
            [0, 0, 1, -1, 0, 0, 0, new Complex(0.0, -1.5707963267949)]
        ];
        Complex[] rhs = [0, 0, 0, 0, 0, 24.0];
        Complex[] reference =
        [
            new(24, 0),
            new(24, 0),
            new(24, 0),
            new(23.999940782519708, -0.037699018824477),
            new(-0.023999940782520, -0.015041945718407),
            new(-0.023999940782520, -0.015041945718407),
            new(0.023999940782520, 0.015041945718407),
            new(0.023999940782520, -0.000037699018824)
        ];

        // build the matrix
        var solver = new SparseComplexSolver();
        for (int r = 0; r < matrix.Length; r++)
        {
            for (int c = 0; c < matrix[r].Length; c++)
            {
                if (!matrix[r][c].Equals(Complex.Zero))
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
            }
        }

        // Add some zero elements
        solver.GetElement(new MatrixLocation(7, 7));
        solver.GetElement(5);

        // Build the Rhs vector
        for (int r = 0; r < rhs.Length; r++)
        {
            if (!rhs[r].Equals(Complex.Zero))
                solver.GetElement(r + 1).Value = rhs[r];
        }

        // Solver
        Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));
        var solution = new DenseVector<Complex>(solver.Size);
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);

        // Check!
        for (int r = 0; r < reference.Length; r++)
        {
            Assert.That(solution[r + 1].Real, Is.EqualTo(reference[r].Real).Within(1e-12));
            Assert.That(solution[r + 1].Imaginary, Is.EqualTo(reference[r].Imaginary).Within(1e-12));
        }
    }

    [Test]
    public void When_PartialDecomposition_Expect_Reference()
    {
        var solver = new SparseRealSolver
        {
            PivotSearchReduction = 2, // Limit to only the 2 first elements
            Degeneracy = 2 // Only perform elimination on the first two rows
        };

        solver[1, 2] = 2;
        solver[2, 1] = 1;
        solver[1, 3] = 4;
        solver[4, 2] = 4;
        solver[3, 3] = 2;
        solver[3, 4] = 4;
        solver[4, 4] = 1;

        Assert.That(solver.OrderAndFactor(), Is.EqualTo(2));

        // We are testing two things here:
        // - First, the solver should not have chosen a pivot in the lower-right submatrix
        // - Second, the submatrix should be equal to A_cc - A_c1 * A^-1 * A_1c with A the top-left 
        //   matrix, A_cc the bottom-right submatrix, A_1c and A_c1 the off-diagonal matrices
        Assert.That(solver[3, 3], Is.EqualTo(2.0).Within(1e-12));
        Assert.That(solver[3, 4], Is.EqualTo(4.0).Within(1e-12));
        Assert.That(solver[4, 3], Is.EqualTo(-8.0).Within(1e-12));
        Assert.That(solver[4, 4], Is.EqualTo(1.0).Within(1e-12));
    }

    [Test]
    public void When_PartialSolve_Expect_Reference()
    {
        var solver = new SparseRealSolver
        {
            PivotSearchReduction = 2, // Limit to only the 2 first elements
            Degeneracy = 2 // Only perform elimination on the first two rows
        };

        solver[1, 1] = 1;
        solver[1, 3] = 2;
        solver[1, 4] = 3;
        solver[1] = 1;
        solver[2, 2] = 2;
        solver[2, 3] = 4;
        solver[2] = 2;
        solver.Factor();

        // We should now be able to solve for multiple solutions, where the last two elements
        // will determine the result.
        var solution = new DenseVector<double>(4);
        solution[3] = 0;
        solution[4] = 0;
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        Assert.That(solution[1], Is.EqualTo(1.0).Within(1e-12));
        Assert.That(solution[2], Is.EqualTo(1.0).Within(1e-12));
        solution[3] = 1.0;
        solution[4] = 2.0;
        solver.ForwardSubstitute(solution);
        solver.BackwardSubstitute(solution);
        Assert.That(solution[1], Is.EqualTo(-7.0).Within(1e-12));
        Assert.That(solution[2], Is.EqualTo(-1.0).Within(1e-12));
    }
}
