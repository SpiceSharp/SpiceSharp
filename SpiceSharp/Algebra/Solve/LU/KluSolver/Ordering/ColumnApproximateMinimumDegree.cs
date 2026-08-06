using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// Computes a fill-reducing column ordering using the column approximate minimum degree
/// algorithm of Davis, Gilbert, Larimore and Ng. Unlike
/// <see cref="ApproximateMinimumDegree"/> it makes no assumption that the pattern is close
/// to symmetric, which makes it the better choice when rows and columns are structured
/// quite differently.
/// </summary>
/// <remarks>
/// <para>
/// The ordering that matters for an LU factorization with arbitrary row pivoting is the one
/// that reduces fill in the columns, and that depends on which columns share a row rather
/// than on the entries themselves. Two columns interact exactly when some row contains
/// both, so each row acts as a clique over the columns in it. Working with those rows
/// directly avoids ever forming the much denser product that spells out the interactions.
/// </para>
/// <para>
/// The bookkeeping mirrors <see cref="ApproximateMinimumDegree"/>: an upper bound on each
/// column's degree is maintained instead of the exact value, eliminating a column merges
/// all the rows it touches into one, rows whose columns all end up inside that merged row
/// are dropped, and columns whose row sets have become identical are folded together.
/// </para>
/// </remarks>
public static class ColumnApproximateMinimumDegree
{
    /// <summary>
    /// Computes a fill-reducing column ordering for the given sparsity pattern.
    /// </summary>
    /// <param name="size">The number of rows and columns.</param>
    /// <param name="columns">The column pointers, with <paramref name="size"/> + 1 elements.</param>
    /// <param name="rows">The row indices.</param>
    /// <param name="permutation">
    /// Receives the column that ends up at each position. Needs at least
    /// <paramref name="size"/> elements.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    public static void Order(int size, int[] columns, int[] rows, int[] permutation)
    {
        columns.ThrowIfNull(nameof(columns));
        rows.ThrowIfNull(nameof(rows));
        permutation.ThrowIfNull(nameof(permutation));
        if (size <= 0)
            return;
        new Worker(size, columns, rows).Order(permutation);
    }

    /// <summary>
    /// Holds the mutable state of one ordering run.
    /// </summary>
    private sealed class Worker
    {
        private readonly int _size;

        // Per column: the rows it appears in. These lists only ever get shorter, so they are
        // compacted where they are and never need extra room.
        private readonly int[] _columnList;
        private readonly int[] _columnStart;
        private readonly int[] _columnLength;

        /// <summary>How many original columns a live column stands for; 0 once it is gone.</summary>
        private readonly int[] _thickness;

        /// <summary>Upper bound on a column's degree in the interaction graph.</summary>
        private readonly int[] _score;

        /// <summary>The step at which a column was ordered, or -1 if it was not.</summary>
        private readonly int[] _step;
        private int _steps;

        /// <summary>Which column absorbed this one, or -1.</summary>
        private readonly int[] _parent;

        /// <summary>Marks the columns adjacent to the current pivot.</summary>
        private readonly int[] _mark;
        private int _markFlag;

        private readonly int[] _hash;
        private readonly int[] _hashHead;
        private readonly int[] _hashNext;

        private readonly int[] _compare;
        private int _compareFlag;

        // Per row: the columns in it. Merging rows together needs new room, so this one has
        // slack and a compacting pass.
        private int[] _rowList;
        private readonly int[] _rowStart;
        private readonly int[] _rowLength;
        private readonly int[] _rowDegree;
        private readonly bool[] _rowDead;
        private int _rowFree;

        /// <summary>Running count of how much of a row lies outside the pivot row.</summary>
        private readonly int[] _outside;
        private int _outsideFlag = 1;

        private readonly int[] _scoreHead;
        private readonly int[] _scoreNext;
        private readonly int[] _scorePrevious;

        private readonly int[] _saved;

        public Worker(int size, int[] columns, int[] rows)
        {
            int n = _size = size;
            int count = columns[n];

            _columnList = new int[Math.Max(count, 1)];
            Array.Copy(rows, _columnList, count);
            _columnStart = new int[n];
            _columnLength = new int[n];
            _thickness = new int[n];
            _score = new int[n];
            _step = new int[n];
            _parent = new int[n];
            _mark = new int[n];
            _hash = new int[n];
            _hashHead = new int[n];
            _hashNext = new int[n];
            _compare = new int[n];
            _outside = new int[n];
            _saved = new int[n];
            _scoreHead = new int[n + 1];
            _scoreNext = new int[n];
            _scorePrevious = new int[n];

            _rowStart = new int[n];
            _rowLength = new int[n];
            _rowDegree = new int[n];
            _rowDead = new bool[n];

            for (int j = 0; j < n; j++)
            {
                int length = columns[j + 1] - columns[j];
                _columnStart[j] = columns[j];
                _columnLength[j] = length;
                _thickness[j] = 1;
                _step[j] = -1;
                _parent[j] = -1;
                _hashHead[j] = -1;
            }

            // Transpose to get the columns of each row.
            for (int p = 0; p < count; p++)
                _rowLength[rows[p]]++;
            int running = 0;
            for (int i = 0; i < n; i++)
            {
                _rowStart[i] = running;
                running += _rowLength[i];
                _rowDegree[i] = _rowLength[i];
                _rowDead[i] = _rowLength[i] == 0;
            }
            _rowList = new int[count + (count / 5) + n + 1];
            int[] fill = new int[n];
            Array.Copy(_rowStart, fill, n);
            for (int j = 0; j < n; j++)
            {
                for (int p = columns[j]; p < columns[j + 1]; p++)
                    _rowList[fill[rows[p]]++] = j;
            }
            _rowFree = count;

            // A column's starting degree is the total width of every row it appears in,
            // minus itself.
            for (int d = 0; d <= n; d++)
                _scoreHead[d] = -1;
            for (int j = n - 1; j >= 0; j--)
            {
                int score = 0;
                int end = _columnStart[j] + _columnLength[j];
                for (int p = _columnStart[j]; p < end; p++)
                    score += _rowDegree[_columnList[p]] - 1;
                if (score > n)
                    score = n;
                _score[j] = score;
                AddToScoreList(j, score);
            }
        }

        public void Order(int[] permutation)
        {
            Eliminate();
            BuildPermutation(permutation);
        }

        private void Eliminate()
        {
            int n = _size;
            int ordered = 0;
            int minimum = 0;
            int step = 0;

            while (ordered < n)
            {
                int pivot = -1;
                while (minimum <= n)
                {
                    pivot = _scoreHead[minimum];
                    if (pivot >= 0)
                        break;
                    minimum++;
                }
                if (pivot < 0)
                    break;
                RemoveFromScoreList(pivot, _score[pivot]);

                int pivotThickness = _thickness[pivot];
                _thickness[pivot] = 0;
                _step[pivot] = step++;
                ordered += pivotThickness;

                if (_markFlag > int.MaxValue - 2)
                {
                    Array.Clear(_mark, 0, n);
                    _markFlag = 0;
                }
                if (_outsideFlag > int.MaxValue - n - 2)
                {
                    Array.Clear(_outside, 0, n);
                    _outsideFlag = 1;
                }
                _markFlag++;

                // ----- merge every row the pivot column touches into one -----
                // Eliminating the column makes all of those rows interchangeable, so a
                // single row holding their combined column set carries the same information.
                EnsureRowSpace(n);
                _mark[pivot] = _markFlag;

                int pivotRow = -1;
                int pivotRowStart = _rowFree;
                int pivotRowDegree = 0;
                int columnEnd = _columnStart[pivot] + _columnLength[pivot];
                for (int p = _columnStart[pivot]; p < columnEnd; p++)
                {
                    int row = _columnList[p];
                    if (_rowDead[row])
                        continue;
                    if (pivotRow < 0)
                        pivotRow = row;
                    int end = _rowStart[row] + _rowLength[row];
                    for (int q = _rowStart[row]; q < end; q++)
                    {
                        int column = _rowList[q];
                        if (_thickness[column] <= 0 || _mark[column] == _markFlag)
                            continue;
                        _mark[column] = _markFlag;
                        RemoveFromScoreList(column, _score[column]);
                        _rowList[_rowFree++] = column;
                        pivotRowDegree += _thickness[column];
                    }
                }
                for (int p = _columnStart[pivot]; p < columnEnd; p++)
                {
                    int row = _columnList[p];
                    if (row != pivotRow)
                        _rowDead[row] = true;
                }
                _columnLength[pivot] = 0;

                if (pivotRow < 0 || _rowFree == pivotRowStart)
                {
                    // The pivot column shared no row with any other live column.
                    if (pivotRow >= 0)
                        _rowDead[pivotRow] = true;
                    _rowFree = pivotRowStart;
                    continue;
                }

                _rowStart[pivotRow] = pivotRowStart;
                _rowLength[pivotRow] = _rowFree - pivotRowStart;
                _rowDegree[pivotRow] = pivotRowDegree;
                int pivotRowEnd = _rowFree;

                // ----- how much of each neighbouring row sticks out of the pivot row -----
                int highest = _outsideFlag;
                for (int p = pivotRowStart; p < pivotRowEnd; p++)
                {
                    int column = _rowList[p];
                    int end = _columnStart[column] + _columnLength[column];
                    for (int q = _columnStart[column]; q < end; q++)
                    {
                        int row = _columnList[q];
                        if (_rowDead[row] || row == pivotRow)
                            continue;
                        if (_outside[row] < _outsideFlag)
                        {
                            _outside[row] = _rowDegree[row] + _outsideFlag;
                            if (_outside[row] > highest)
                                highest = _outside[row];
                        }
                        _outside[row] -= _thickness[column];
                    }
                }

                // ----- rescore every column adjacent to the pivot -----
                int remaining = n - ordered;
                for (int p = pivotRowStart; p < pivotRowEnd; p++)
                {
                    int column = _rowList[p];
                    int thickness = _thickness[column];
                    if (thickness <= 0)
                        continue;

                    int listStart = _columnStart[column];
                    int listEnd = listStart + _columnLength[column];
                    int write = listStart;
                    int score = pivotRowDegree - thickness;
                    long hash = 0;

                    for (int q = listStart; q < listEnd; q++)
                    {
                        int row = _columnList[q];
                        if (_rowDead[row] || row == pivotRow)
                            continue;
                        int outside = _outside[row] - _outsideFlag;
                        if (outside <= 0)
                        {
                            // Everything this row connects is already in the pivot row.
                            _rowDead[row] = true;
                            continue;
                        }
                        score += outside;
                        _columnList[write++] = row;
                        hash += row;
                    }

                    // The merged row replaces whatever the column lost, and there is always
                    // at least one entry to spare because the column shared a row with the
                    // pivot in the first place.
                    _columnList[write++] = pivotRow;
                    hash += pivotRow;
                    _columnLength[column] = write - listStart;

                    int cap = remaining - thickness;
                    if (score > cap)
                        score = cap;
                    if (score < 0)
                        score = 0;
                    _score[column] = score;
                    _hash[column] = (int)(hash % n);
                }

                MergeSuperColumns(pivotRowStart, pivotRowEnd);

                // ----- put the survivors back in their score lists, and drop the rest -----
                int kept = pivotRowStart;
                for (int p = pivotRowStart; p < pivotRowEnd; p++)
                {
                    int column = _rowList[p];
                    if (_thickness[column] <= 0)
                        continue;
                    int score = _score[column];
                    AddToScoreList(column, score);
                    if (score < minimum)
                        minimum = score;
                    _rowList[kept++] = column;
                }

                _rowLength[pivotRow] = kept - pivotRowStart;
                if (_rowLength[pivotRow] == 0)
                    _rowDead[pivotRow] = true;
                else
                {
                    int degree = 0;
                    for (int p = pivotRowStart; p < kept; p++)
                        degree += _thickness[_rowList[p]];
                    _rowDegree[pivotRow] = degree;
                }
                _rowFree = kept;
                _outsideFlag = highest + 1;
            }

            for (int j = 0; j < n; j++)
            {
                if (_step[j] < 0 && _parent[j] < 0)
                    _step[j] = step++;
            }
            _steps = step;
        }

        /// <summary>
        /// Folds together the columns adjacent to the pivot whose row sets have become
        /// identical. Such columns can be eliminated one after the other without changing
        /// anything, so treating them as one column of larger width is free.
        /// </summary>
        private void MergeSuperColumns(int from, int to)
        {
            for (int p = from; p < to; p++)
            {
                int column = _rowList[p];
                if (_thickness[column] <= 0)
                    continue;
                int bucket = _hash[column];
                _hashNext[column] = _hashHead[bucket];
                _hashHead[bucket] = column;
            }

            for (int p = from; p < to; p++)
            {
                int column = _rowList[p];
                if (_thickness[column] <= 0)
                    continue;
                int bucket = _hash[column];
                if (_hashHead[bucket] < 0)
                    continue;

                for (int a = _hashHead[bucket]; a >= 0; a = _hashNext[a])
                {
                    if (_thickness[a] <= 0)
                        continue;

                    if (_compareFlag > int.MaxValue - 2)
                    {
                        Array.Clear(_compare, 0, _size);
                        _compareFlag = 0;
                    }
                    _compareFlag++;
                    int endA = _columnStart[a] + _columnLength[a];
                    for (int q = _columnStart[a]; q < endA; q++)
                        _compare[_columnList[q]] = _compareFlag;

                    for (int b = _hashNext[a]; b >= 0; b = _hashNext[b])
                    {
                        if (_thickness[b] <= 0 || _columnLength[b] != _columnLength[a])
                            continue;

                        bool identical = true;
                        int endB = _columnStart[b] + _columnLength[b];
                        for (int q = _columnStart[b]; q < endB; q++)
                        {
                            if (_compare[_columnList[q]] != _compareFlag)
                            {
                                identical = false;
                                break;
                            }
                        }
                        if (!identical)
                            continue;

                        _thickness[a] += _thickness[b];
                        _thickness[b] = 0;
                        _parent[b] = a;
                        _columnLength[b] = 0;
                    }
                }
                _hashHead[bucket] = -1;
            }
        }

        /// <summary>
        /// Charges every original column to the step at which it was ordered, following the
        /// merges, and groups the columns by that step.
        /// </summary>
        private void BuildPermutation(int[] permutation)
        {
            int n = _size;
            for (int i = 0; i < n; i++)
            {
                if (_step[i] >= 0)
                    continue;
                int node = i;
                while (_step[node] < 0)
                    node = _parent[node];
                int resolved = _step[node];
                node = i;
                while (_step[node] < 0)
                {
                    int next = _parent[node];
                    _step[node] = resolved;
                    node = next;
                }
            }

            int[] offsets = new int[_steps + 1];
            for (int i = 0; i < n; i++)
                offsets[_step[i] + 1]++;
            for (int s = 1; s <= _steps; s++)
                offsets[s] += offsets[s - 1];
            for (int i = 0; i < n; i++)
                permutation[offsets[_step[i]]++] = i;
        }

        private void AddToScoreList(int column, int score)
        {
            int next = _scoreHead[score];
            _scoreNext[column] = next;
            _scorePrevious[column] = -1;
            if (next >= 0)
                _scorePrevious[next] = column;
            _scoreHead[score] = column;
        }

        private void RemoveFromScoreList(int column, int score)
        {
            int previous = _scorePrevious[column];
            int next = _scoreNext[column];
            if (previous >= 0)
                _scoreNext[previous] = next;
            else
                _scoreHead[score] = next;
            if (next >= 0)
                _scorePrevious[next] = previous;
        }

        private void EnsureRowSpace(int needed)
        {
            if (_rowFree + needed <= _rowList.Length)
                return;
            CompactRows();
            if (_rowFree + needed > _rowList.Length)
                Array.Resize(ref _rowList, Math.Max(2 * _rowList.Length, _rowFree + needed));
        }

        /// <summary>
        /// Slides the live row lists to the front of the storage, closing the gaps that
        /// merged and dropped rows have left behind.
        /// </summary>
        private void CompactRows()
        {
            int n = _size;
            for (int i = 0; i < n; i++)
            {
                if (_rowDead[i] || _rowLength[i] <= 0)
                    continue;
                _saved[i] = _rowList[_rowStart[i]];
                _rowList[_rowStart[i]] = -(i + 1);
            }

            int read = 0, write = 0;
            while (read < _rowFree)
            {
                int entry = _rowList[read++];
                if (entry >= 0)
                    continue;
                int owner = -entry - 1;
                _rowList[write] = _saved[owner];
                _rowStart[owner] = write++;
                int rest = _rowLength[owner] - 1;
                for (int k = 0; k < rest; k++)
                    _rowList[write++] = _rowList[read++];
            }
            _rowFree = write;
        }
    }
}
