using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// The result of analysing a sparsity pattern: how to permute the matrix so that it falls
/// apart into diagonal blocks, and in what order to eliminate within each block. This
/// depends only on which entries exist, not on their values, so it survives for as long as
/// the pattern does and every subsequent factorization can reuse it.
/// </summary>
public sealed class KluSymbolic
{
    /// <summary>
    /// Gets the number of rows and columns.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the number of diagonal blocks.
    /// </summary>
    public int Blocks { get; }

    /// <summary>
    /// Gets the row that starts out at each diagonal position. Partial pivoting may still
    /// move rows around inside a block while factoring.
    /// </summary>
    public int[] RowPermutation { get; }

    /// <summary>
    /// Gets the column that sits at each diagonal position. This one is final.
    /// </summary>
    public int[] ColumnPermutation { get; }

    /// <summary>
    /// Gets the diagonal position at which each block starts, terminated by <see cref="Size"/>.
    /// </summary>
    public int[] BlockStarts { get; }

    /// <summary>
    /// Gets the size of the largest diagonal block, which is what bounds the work a
    /// factorization has to do.
    /// </summary>
    public int LargestBlock { get; }

    /// <summary>
    /// Gets a value indicating whether no nonzero diagonal could be found, which means the
    /// matrix is singular no matter what values it holds.
    /// </summary>
    public bool StructurallySingular { get; }

    private KluSymbolic(int size, int blocks, int[] rowPermutation, int[] columnPermutation,
        int[] blockStarts, int largestBlock, bool structurallySingular)
    {
        Size = size;
        Blocks = blocks;
        RowPermutation = rowPermutation;
        ColumnPermutation = columnPermutation;
        BlockStarts = blockStarts;
        LargestBlock = largestBlock;
        StructurallySingular = structurallySingular;
    }

    /// <summary>
    /// Analyses a sparsity pattern.
    /// </summary>
    /// <param name="size">The number of rows and columns.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices, ascending within each column.</param>
    /// <param name="parameters">The parameters deciding which ordering to use.</param>
    /// <returns>The analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    public static KluSymbolic Analyze(int size, int[] columns, int[] rows, KluParameters parameters)
    {
        columns.ThrowIfNull(nameof(columns));
        rows.ThrowIfNull(nameof(rows));
        parameters.ThrowIfNull(nameof(parameters));

        int[] rowPermutation = new int[Math.Max(size, 1)];
        int[] columnPermutation = new int[Math.Max(size, 1)];
        int[] blockStarts = new int[size + 1];
        int blocks;
        bool singular = false;

        if (size == 0)
        {
            blockStarts[0] = 0;
            return new KluSymbolic(0, 0, rowPermutation, columnPermutation, blockStarts, 0, false);
        }

        if (parameters.UseBlockTriangularForm)
        {
            blocks = BlockTriangularForm.Order(size, columns, rows,
                rowPermutation, columnPermutation, blockStarts, out singular);
            if (singular)
            {
                // Without a nonzero diagonal to build on, the block boundaries carry no
                // guarantee that nothing sits below them, so treating the matrix as one
                // block is the only safe thing to do. Factoring will fail on it and report
                // which equation is to blame.
                for (int i = 0; i < size; i++)
                {
                    rowPermutation[i] = i;
                    columnPermutation[i] = i;
                }
                blockStarts[0] = 0;
                blockStarts[1] = size;
                blocks = 1;
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                rowPermutation[i] = i;
                columnPermutation[i] = i;
            }
            blockStarts[0] = 0;
            blockStarts[1] = size;
            blocks = 1;
        }

        // Reduce fill inside each block. The same permutation goes to the rows and the
        // columns, which keeps the nonzero diagonal the block triangular form established
        // and leaves the block structure itself untouched.
        int largest = 0;
        if (parameters.Ordering != KluOrdering.Natural)
        {
            int[] rowPosition = new int[size];
            for (int k = 0; k < size; k++)
                rowPosition[rowPermutation[k]] = k;

            int[] blockColumns = null;
            int[] blockRows = null;
            int[] localPermutation = null;
            int[] scratchRows = null;
            int[] scratchColumns = null;

            for (int block = 0; block < blocks; block++)
            {
                int start = blockStarts[block];
                int count = blockStarts[block + 1] - start;
                if (count > largest)
                    largest = count;
                if (count <= 2)
                    continue;

                if (blockColumns == null || blockColumns.Length < count + 1)
                {
                    blockColumns = new int[count + 1];
                    localPermutation = new int[count];
                    scratchRows = new int[count];
                    scratchColumns = new int[count];
                }

                // Cut the block out of the matrix, in the coordinates the block itself uses.
                int written = 0;
                for (int j = 0; j < count; j++)
                {
                    blockColumns[j] = written;
                    int column = columnPermutation[start + j];
                    for (int p = columns[column]; p < columns[column + 1]; p++)
                    {
                        int position = rowPosition[rows[p]] - start;
                        if (position >= 0 && position < count)
                            written++;
                    }
                }
                blockColumns[count] = written;
                if (blockRows == null || blockRows.Length < written)
                    blockRows = new int[written];
                written = 0;
                for (int j = 0; j < count; j++)
                {
                    int column = columnPermutation[start + j];
                    for (int p = columns[column]; p < columns[column + 1]; p++)
                    {
                        int position = rowPosition[rows[p]] - start;
                        if (position >= 0 && position < count)
                            blockRows[written++] = position;
                    }
                }

                if (parameters.Ordering == KluOrdering.ColumnApproximateMinimumDegree)
                    ColumnApproximateMinimumDegree.Order(count, blockColumns, blockRows, localPermutation);
                else
                    ApproximateMinimumDegree.Order(count, blockColumns, blockRows, localPermutation);

                for (int j = 0; j < count; j++)
                {
                    scratchRows[j] = rowPermutation[start + localPermutation[j]];
                    scratchColumns[j] = columnPermutation[start + localPermutation[j]];
                }
                for (int j = 0; j < count; j++)
                {
                    rowPermutation[start + j] = scratchRows[j];
                    columnPermutation[start + j] = scratchColumns[j];
                    rowPosition[scratchRows[j]] = start + j;
                }
            }
        }
        else
        {
            for (int block = 0; block < blocks; block++)
            {
                int count = blockStarts[block + 1] - blockStarts[block];
                if (count > largest)
                    largest = count;
            }
        }

        return new KluSymbolic(size, blocks, rowPermutation, columnPermutation, blockStarts, largest, singular);
    }
}
