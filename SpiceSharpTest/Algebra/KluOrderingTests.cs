using NUnit.Framework;
using SpiceSharp.Algebra.Solve;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpTest.Algebra;

[TestFixture]
public class KluOrderingTests
{
    /// <summary>
    /// Builds a compressed-column pattern from a list of (row, column) pairs.
    /// </summary>
    private static (int[] Columns, int[] Rows) Pattern(int size, IEnumerable<(int Row, int Column)> entries)
    {
        var perColumn = new List<int>[size];
        for (int j = 0; j < size; j++)
            perColumn[j] = [];
        foreach (var (row, column) in entries)
        {
            if (!perColumn[column].Contains(row))
                perColumn[column].Add(row);
        }

        int[] columns = new int[size + 1];
        var rows = new List<int>();
        for (int j = 0; j < size; j++)
        {
            columns[j] = rows.Count;
            perColumn[j].Sort();
            rows.AddRange(perColumn[j]);
        }
        columns[size] = rows.Count;
        return (columns, [.. rows]);
    }

    private static (int[] Columns, int[] Rows) RandomPattern(int size, double density, int seed, bool forceDiagonal)
    {
        var random = new Random(seed);
        var entries = new List<(int, int)>();
        for (int j = 0; j < size; j++)
        {
            if (forceDiagonal)
                entries.Add((j, j));
            for (int i = 0; i < size; i++)
            {
                if (random.NextDouble() < density)
                    entries.Add((i, j));
            }
        }
        return Pattern(size, entries);
    }

    private static void AssertIsPermutation(int[] permutation, int size)
    {
        Assert.That(permutation.Length, Is.GreaterThanOrEqualTo(size));
        bool[] seen = new bool[size];
        for (int k = 0; k < size; k++)
        {
            int value = permutation[k];
            Assert.That(value, Is.InRange(0, size - 1));
            Assert.That(seen[value], Is.False, $"Index {value} appears twice");
            seen[value] = true;
        }
    }

    [Test]
    public void When_MaximumTransversalOnFullDiagonal_Expect_PerfectMatching()
    {
        const int size = 40;
        var (columns, rows) = RandomPattern(size, 0.05, 1234, true);
        int[] match = new int[size];
        int matched = MaximumTransversal.Find(size, columns, rows, match);

        Assert.That(matched, Is.EqualTo(size));
        AssertIsPermutation(match, size);

        // Every matched pair has to be an actual nonzero entry.
        for (int i = 0; i < size; i++)
        {
            int j = match[i];
            bool exists = false;
            for (int p = columns[j]; p < columns[j + 1]; p++)
            {
                if (rows[p] == i)
                    exists = true;
            }
            Assert.That(exists, Is.True, $"({i},{j}) is not a nonzero entry");
        }
    }

    [Test]
    public void When_MaximumTransversalOnEmptyColumn_Expect_StructurallySingular()
    {
        // Column 2 is empty, so rows cannot all be matched.
        var (columns, rows) = Pattern(4, [(0, 0), (1, 1), (2, 1), (3, 3)]);
        int[] match = new int[4];
        Assert.That(MaximumTransversal.Find(4, columns, rows, match), Is.EqualTo(3));
    }

    [Test]
    public void When_MaximumTransversalNeedsAugmenting_Expect_PerfectMatching()
    {
        // A greedy first pass matches row 0 to column 0 and then gets stuck on column 1,
        // so this only succeeds if the search actually re-routes an existing match.
        var (columns, rows) = Pattern(3, [(0, 0), (1, 0), (0, 1), (2, 2)]);
        int[] match = new int[3];
        Assert.That(MaximumTransversal.Find(3, columns, rows, match), Is.EqualTo(3));
        AssertIsPermutation(match, 3);
    }

    [Test]
    public void When_BlockTriangularFormOnLowerTriangular_Expect_AllSingletonBlocks()
    {
        // A triangular matrix has no cycles at all, so every block is a single entry.
        const int size = 8;
        var entries = new List<(int, int)>();
        for (int j = 0; j < size; j++)
        {
            for (int i = j; i < size; i++)
                entries.Add((i, j));
        }
        var (columns, rows) = Pattern(size, entries);

        int[] p = new int[size], q = new int[size], r = new int[size + 1];
        int blocks = BlockTriangularForm.Order(size, columns, rows, p, q, r, out bool singular);

        Assert.That(singular, Is.False);
        Assert.That(blocks, Is.EqualTo(size));
        AssertIsPermutation(p, size);
        AssertIsPermutation(q, size);
        AssertBlockUpperTriangular(size, columns, rows, p, q, r, blocks);
    }

    [Test]
    public void When_BlockTriangularFormOnIrreducible_Expect_SingleBlock()
    {
        // One big cycle through every node cannot be broken up.
        const int size = 10;
        var entries = new List<(int, int)>();
        for (int j = 0; j < size; j++)
        {
            entries.Add((j, j));
            entries.Add(((j + 1) % size, j));
        }
        var (columns, rows) = Pattern(size, entries);

        int[] p = new int[size], q = new int[size], r = new int[size + 1];
        int blocks = BlockTriangularForm.Order(size, columns, rows, p, q, r, out bool singular);

        Assert.That(singular, Is.False);
        Assert.That(blocks, Is.EqualTo(1));
        AssertBlockUpperTriangular(size, columns, rows, p, q, r, blocks);
    }

    [Test]
    public void When_BlockTriangularFormOnTwoCycles_Expect_TwoBlocks()
    {
        // Two independent 3-cycles, with a coupling that only runs one way.
        var entries = new List<(int, int)>
        {
            (0, 0), (1, 0), (1, 1), (2, 1), (2, 2), (0, 2),
            (3, 3), (4, 3), (4, 4), (5, 4), (5, 5), (3, 5),
            (0, 3)
        };
        var (columns, rows) = Pattern(6, entries);

        int[] p = new int[6], q = new int[6], r = new int[7];
        int blocks = BlockTriangularForm.Order(6, columns, rows, p, q, r, out bool singular);

        Assert.That(singular, Is.False);
        Assert.That(blocks, Is.EqualTo(2));
        Assert.That(r[1] - r[0], Is.EqualTo(3));
        Assert.That(r[2] - r[1], Is.EqualTo(3));
        AssertBlockUpperTriangular(6, columns, rows, p, q, r, blocks);
    }

    [Test]
    public void When_BlockTriangularFormOnRandomPatterns_Expect_BlockUpperTriangular()
    {
        for (int seed = 0; seed < 25; seed++)
        {
            int size = 5 + (seed % 30);
            var (columns, rows) = RandomPattern(size, 2.0 / size, seed, true);

            int[] p = new int[size], q = new int[size], r = new int[size + 1];
            int blocks = BlockTriangularForm.Order(size, columns, rows, p, q, r, out bool singular);

            Assert.That(singular, Is.False, $"seed {seed}");
            AssertIsPermutation(p, size);
            AssertIsPermutation(q, size);
            Assert.That(r[0], Is.EqualTo(0));
            Assert.That(r[blocks], Is.EqualTo(size));
            AssertBlockUpperTriangular(size, columns, rows, p, q, r, blocks);
        }
    }

    [Test]
    public void When_BlockTriangularFormOnSingularPattern_Expect_ValidPermutation()
    {
        // Two identical columns leave a column with nothing to match.
        var (columns, rows) = Pattern(3, [(0, 0), (0, 1), (2, 2)]);
        int[] p = new int[3], q = new int[3], r = new int[4];
        int blocks = BlockTriangularForm.Order(3, columns, rows, p, q, r, out bool singular);

        Assert.That(singular, Is.True);
        AssertIsPermutation(p, 3);
        AssertIsPermutation(q, 3);
        Assert.That(r[blocks], Is.EqualTo(3));
    }

    /// <summary>
    /// Checks that every nonzero entry of the permuted matrix lands on or above the
    /// diagonal blocks, and that the diagonal itself is nonzero.
    /// </summary>
    private static void AssertBlockUpperTriangular(int size, int[] columns, int[] rows,
        int[] p, int[] q, int[] r, int blocks)
    {
        int[] rowPosition = new int[size];
        for (int k = 0; k < size; k++)
            rowPosition[p[k]] = k;

        int[] blockOf = new int[size];
        for (int b = 0; b < blocks; b++)
        {
            for (int k = r[b]; k < r[b + 1]; k++)
                blockOf[k] = b;
        }

        bool[] hasDiagonal = new bool[size];
        for (int k = 0; k < size; k++)
        {
            int column = q[k];
            for (int pos = columns[column]; pos < columns[column + 1]; pos++)
            {
                int row = rowPosition[rows[pos]];
                Assert.That(blockOf[row], Is.LessThanOrEqualTo(blockOf[k]),
                    $"Entry at position ({row},{k}) sits below the diagonal blocks");
                if (row == k)
                    hasDiagonal[k] = true;
            }
        }
        for (int k = 0; k < size; k++)
            Assert.That(hasDiagonal[k], Is.True, $"Diagonal position {k} is zero");
    }

    [Test]
    public void When_AmdOnRandomPatterns_Expect_ValidPermutation()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            int size = 1 + (seed % 60);
            var (columns, rows) = RandomPattern(size, 3.0 / Math.Max(size, 1), seed, true);
            int[] permutation = new int[size];
            ApproximateMinimumDegree.Order(size, columns, rows, permutation);
            AssertIsPermutation(permutation, size);
        }
    }

    [Test]
    public void When_AmdOnDisconnectedPattern_Expect_ValidPermutation()
    {
        // Nothing but a diagonal: every node is isolated.
        var (columns, rows) = Pattern(6, [(0, 0), (1, 1), (2, 2), (3, 3), (4, 4), (5, 5)]);
        int[] permutation = new int[6];
        ApproximateMinimumDegree.Order(6, columns, rows, permutation);
        AssertIsPermutation(permutation, 6);
    }

    [Test]
    public void When_AmdOnArrowPattern_Expect_DenseRowLast()
    {
        // A matrix whose last row and column are full is the classic case where the natural
        // ordering fills in completely and minimum degree does not: the hub has to go last.
        const int size = 30;
        var entries = new List<(int, int)>();
        for (int i = 0; i < size; i++)
        {
            entries.Add((i, i));
            entries.Add((i, size - 1));
            entries.Add((size - 1, i));
        }
        var (columns, rows) = Pattern(size, entries);

        int[] permutation = new int[size];
        ApproximateMinimumDegree.Order(size, columns, rows, permutation);
        AssertIsPermutation(permutation, size);
        Assert.That(permutation[size - 1], Is.EqualTo(size - 1));
        Assert.That(CountFill(size, columns, rows, permutation), Is.EqualTo(0));
    }

    [Test]
    public void When_AmdOnGridPattern_Expect_LessFillThanNaturalOrder()
    {
        // A 2D grid is the standard benchmark for a fill-reducing ordering.
        const int side = 14;
        int size = side * side;
        var entries = new List<(int, int)>();
        for (int y = 0; y < side; y++)
        {
            for (int x = 0; x < side; x++)
            {
                int node = (y * side) + x;
                entries.Add((node, node));
                if (x > 0)
                {
                    entries.Add((node, node - 1));
                    entries.Add((node - 1, node));
                }
                if (y > 0)
                {
                    entries.Add((node, node - side));
                    entries.Add((node - side, node));
                }
            }
        }
        var (columns, rows) = Pattern(size, entries);

        int[] natural = new int[size];
        for (int i = 0; i < size; i++)
            natural[i] = i;
        int[] permutation = new int[size];
        ApproximateMinimumDegree.Order(size, columns, rows, permutation);
        AssertIsPermutation(permutation, size);

        int naturalFill = CountFill(size, columns, rows, natural);
        int amdFill = CountFill(size, columns, rows, permutation);
        Assert.That(amdFill, Is.LessThan(naturalFill),
            $"AMD produced {amdFill} fill-ins, natural ordering {naturalFill}");

        // On a grid this size a good ordering lands well under half the natural fill.
        Assert.That(amdFill, Is.LessThan(naturalFill / 2));
    }

    [Test]
    public void When_ColamdOnRandomPatterns_Expect_ValidPermutation()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            int size = 1 + (seed % 60);
            var (columns, rows) = RandomPattern(size, 3.0 / Math.Max(size, 1), seed, true);
            int[] permutation = new int[size];
            ColumnApproximateMinimumDegree.Order(size, columns, rows, permutation);
            AssertIsPermutation(permutation, size);
        }
    }

    [Test]
    public void When_ColamdOnDenserPatterns_Expect_ValidPermutation()
    {
        // Denser patterns force row merging to grow the storage and compact it.
        for (int seed = 0; seed < 10; seed++)
        {
            int size = 40 + seed;
            var (columns, rows) = RandomPattern(size, 0.15, 500 + seed, true);
            int[] permutation = new int[size];
            ColumnApproximateMinimumDegree.Order(size, columns, rows, permutation);
            AssertIsPermutation(permutation, size);
        }
    }

    [Test]
    public void When_ColamdOnDisconnectedPattern_Expect_ValidPermutation()
    {
        var (columns, rows) = Pattern(6, [(0, 0), (1, 1), (2, 2), (3, 3), (4, 4), (5, 5)]);
        int[] permutation = new int[6];
        ColumnApproximateMinimumDegree.Order(6, columns, rows, permutation);
        AssertIsPermutation(permutation, 6);
    }

    [Test]
    public void When_ColamdOnArrowPattern_Expect_DenseColumnLast()
    {
        // The full row and column have to go last here as well.
        const int size = 30;
        var entries = new List<(int, int)>();
        for (int i = 0; i < size; i++)
        {
            entries.Add((i, i));
            entries.Add((i, size - 1));
            entries.Add((size - 1, i));
        }
        var (columns, rows) = Pattern(size, entries);

        int[] permutation = new int[size];
        ColumnApproximateMinimumDegree.Order(size, columns, rows, permutation);
        AssertIsPermutation(permutation, size);
        Assert.That(permutation[size - 1], Is.EqualTo(size - 1));
    }

    [Test]
    public void When_ColamdOnGridPattern_Expect_LessFillThanNaturalOrder()
    {
        const int side = 14;
        int size = side * side;
        var entries = new List<(int, int)>();
        for (int y = 0; y < side; y++)
        {
            for (int x = 0; x < side; x++)
            {
                int node = (y * side) + x;
                entries.Add((node, node));
                if (x > 0)
                {
                    entries.Add((node, node - 1));
                    entries.Add((node - 1, node));
                }
                if (y > 0)
                {
                    entries.Add((node, node - side));
                    entries.Add((node - side, node));
                }
            }
        }
        var (columns, rows) = Pattern(size, entries);

        int[] natural = new int[size];
        for (int i = 0; i < size; i++)
            natural[i] = i;
        int[] permutation = new int[size];
        ColumnApproximateMinimumDegree.Order(size, columns, rows, permutation);
        AssertIsPermutation(permutation, size);

        Assert.That(CountFill(size, columns, rows, permutation),
            Is.LessThan(CountFill(size, columns, rows, natural)));
    }

    /// <summary>
    /// Counts the fill-ins that symmetric elimination in the given order produces on the
    /// symmetrized pattern. Straightforward elimination-graph bookkeeping, meant for
    /// checking the ordering rather than for speed.
    /// </summary>
    private static int CountFill(int size, int[] columns, int[] rows, int[] permutation)
    {
        var pattern = SymmetricPattern.Create(size, columns, rows);
        int[] position = new int[size];
        for (int k = 0; k < size; k++)
            position[permutation[k]] = k;

        var neighbours = new HashSet<int>[size];
        for (int k = 0; k < size; k++)
            neighbours[k] = [];
        for (int node = 0; node < size; node++)
        {
            for (int p = pattern.Pointers[node]; p < pattern.Pointers[node + 1]; p++)
                neighbours[position[node]].Add(position[pattern.Indices[p]]);
        }

        int original = neighbours.Sum(n => n.Count);
        for (int k = 0; k < size; k++)
        {
            var later = neighbours[k].Where(v => v > k).ToArray();
            foreach (int a in later)
            {
                foreach (int b in later)
                {
                    if (a != b)
                        neighbours[a].Add(b);
                }
            }
        }
        return (neighbours.Sum(n => n.Count) - original) / 2;
    }
}
