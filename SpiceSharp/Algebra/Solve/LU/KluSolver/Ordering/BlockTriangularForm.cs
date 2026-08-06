namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// Permutes a sparse matrix to block upper triangular form. Circuit matrices very often
/// fall apart into many small blocks this way, which is where most of the advantage of
/// KLU over a general sparse solver comes from: only the diagonal blocks ever have to be
/// factored, and everything above them is handled by a substitution.
/// </summary>
/// <remarks>
/// The permutation is found in two steps. A maximum transversal first puts a nonzero on
/// every diagonal position, which turns the matrix into the adjacency structure of a
/// directed graph. The strongly connected components of that graph are then the diagonal
/// blocks, and ordering them the way the component search emits them leaves all remaining
/// entries above the diagonal blocks.
/// </remarks>
public static class BlockTriangularForm
{
    /// <summary>
    /// Permutes the given sparsity pattern to block upper triangular form.
    /// </summary>
    /// <param name="size">The number of rows and columns of the matrix.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices.</param>
    /// <param name="rowPermutation">
    /// Receives the row that ends up at each diagonal position. Needs at least
    /// <paramref name="size"/> elements.
    /// </param>
    /// <param name="columnPermutation">
    /// Receives the column that ends up at each diagonal position. Needs at least
    /// <paramref name="size"/> elements.
    /// </param>
    /// <param name="blockStarts">
    /// Receives the diagonal position at which each block starts, terminated by
    /// <paramref name="size"/>. Needs at least <paramref name="size"/> + 1 elements.
    /// </param>
    /// <param name="structurallySingular">
    /// Set to <c>true</c> if no nonzero diagonal could be achieved, which means the
    /// pattern is structurally singular and factoring is going to fail. The permutation
    /// is still completed to a valid one so that the caller can report which equation
    /// caused the problem.
    /// </param>
    /// <returns>The number of diagonal blocks.</returns>
    public static int Order(int size, int[] columns, int[] rows,
        int[] rowPermutation, int[] columnPermutation, int[] blockStarts, out bool structurallySingular)
    {
        structurallySingular = false;
        if (size == 0)
        {
            blockStarts[0] = 0;
            return 0;
        }

        // Put a nonzero on every diagonal position, if at all possible.
        int[] rowMatch = new int[size];
        int matched = MaximumTransversal.Find(size, columns, rows, rowMatch);
        if (matched < size)
        {
            structurallySingular = true;
            CompleteMatching(size, rowMatch);
        }

        // The matched pattern is now the adjacency structure of a directed graph whose
        // strongly connected components are the diagonal blocks.
        int[] order = new int[size];
        int blocks = StronglyConnectedComponents.Find(size, columns, rows, rowMatch, order, blockStarts);

        // Invert the matching so that we can read off the row belonging to each column.
        int[] columnMatch = new int[size];
        for (int i = 0; i < size; i++)
            columnMatch[rowMatch[i]] = i;

        for (int k = 0; k < size; k++)
        {
            int column = order[k];
            columnPermutation[k] = column;
            rowPermutation[k] = columnMatch[column];
        }
        return blocks;
    }

    /// <summary>
    /// Extends a partial matching to a full permutation by pairing up the leftover rows
    /// and columns in an arbitrary but consistent way.
    /// </summary>
    private static void CompleteMatching(int size, int[] rowMatch)
    {
        bool[] columnUsed = new bool[size];
        for (int i = 0; i < size; i++)
        {
            if (rowMatch[i] >= 0)
                columnUsed[rowMatch[i]] = true;
        }

        int column = 0;
        for (int i = 0; i < size; i++)
        {
            if (rowMatch[i] >= 0)
                continue;
            while (columnUsed[column])
                column++;
            rowMatch[i] = column;
            columnUsed[column] = true;
        }
    }
}
