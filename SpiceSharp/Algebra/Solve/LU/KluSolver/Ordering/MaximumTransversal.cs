namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// Finds a maximum transversal (a maximum matching between rows and columns) of a
/// sparse matrix pattern. The result is the largest possible set of nonzero entries
/// that share no row and no column, which is the first half of permuting a matrix to
/// block triangular form.
/// </summary>
/// <remarks>
/// The matching is grown one column at a time by searching for an augmenting path,
/// which is the classic approach for this problem. A per-column "cheap" cursor makes the
/// common case - a column that has an as-yet unmatched row - cost only the entries that
/// have not already been ruled out by an earlier search. Because a row that is matched
/// never becomes unmatched again, entries before the cursor can safely be skipped forever.
/// The depth-first search keeps its own stack so that a deep search cannot overflow the
/// call stack on a large matrix.
/// </remarks>
public static class MaximumTransversal
{
    /// <summary>
    /// Finds a maximum matching for the given sparsity pattern.
    /// </summary>
    /// <param name="size">The number of rows and columns of the matrix.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices, sorted per column is not required.</param>
    /// <param name="rowMatch">
    /// Receives for every row the column it has been matched to, or -1 if the row could
    /// not be matched. The array needs at least <paramref name="size"/> elements.
    /// </param>
    /// <returns>
    /// The number of matched rows. If this is less than <paramref name="size"/>, the
    /// pattern is structurally singular.
    /// </returns>
    public static int Find(int size, int[] columns, int[] rows, int[] rowMatch)
    {
        for (int i = 0; i < size; i++)
            rowMatch[i] = -1;
        if (size == 0)
            return 0;

        int[] cheap = new int[size];
        int[] mark = new int[size];
        int[] stackColumn = new int[size];
        int[] stackRow = new int[size];
        int[] stackPointer = new int[size];
        for (int j = 0; j < size; j++)
        {
            cheap[j] = columns[j];
            mark[j] = -1;
        }

        int matched = 0;
        for (int j = 0; j < size; j++)
        {
            if (Augment(j, columns, rows, rowMatch, cheap, mark, stackColumn, stackRow, stackPointer))
                matched++;
        }
        return matched;
    }

    /// <summary>
    /// Searches for an augmenting path that starts at the given column and, if one is
    /// found, flips the matching along it so that one more row becomes matched.
    /// </summary>
    /// <returns><c>true</c> if the matching grew; otherwise, <c>false</c>.</returns>
    private static bool Augment(int start, int[] columns, int[] rows, int[] rowMatch,
        int[] cheap, int[] mark, int[] stackColumn, int[] stackRow, int[] stackPointer)
    {
        int head = 0;
        stackColumn[0] = start;
        bool found = false;

        while (head >= 0)
        {
            int column = stackColumn[head];
            if (mark[column] != start)
            {
                // First time this column is reached during the current search. Try the cheap
                // option: any row of this column that is still unmatched ends the path here.
                mark[column] = start;
                int end = columns[column + 1];
                int p = cheap[column];
                while (p < end && rowMatch[rows[p]] >= 0)
                    p++;
                cheap[column] = p;
                if (p < end)
                {
                    stackRow[head] = rows[p];
                    found = true;
                    break;
                }
                stackPointer[head] = columns[column];
            }

            // Every row of this column is already matched, so the path has to continue
            // through the column that currently owns one of those rows.
            int q = stackPointer[head];
            int last = columns[column + 1];
            while (q < last)
            {
                int row = rows[q];
                int owner = rowMatch[row];
                if (owner >= 0 && mark[owner] == start)
                {
                    // Already visited during this search, so it leads nowhere new.
                    q++;
                    continue;
                }
                stackPointer[head] = q + 1;
                stackRow[head] = row;
                if (owner < 0)
                {
                    // The cheap scan above normally catches this, but claiming the row here
                    // keeps the search correct regardless of how the cursor advanced.
                    found = true;
                    break;
                }
                head++;
                stackColumn[head] = owner;
                break;
            }

            if (found)
                break;
            if (q >= last)
                head--;
        }

        if (!found)
            return false;

        // Walk back down the path, handing every row on it to the column below it.
        for (int h = head; h >= 0; h--)
            rowMatch[stackRow[h]] = stackColumn[h];
        return true;
    }
}
