using SpiceSharp.Algebra.Solve;
using System;
using System.Numerics;

namespace SpiceSharp.Algebra;

/// <summary>
/// Solves sparse sets of equations with complex numbers the way KLU does: the sparsity pattern
/// is analysed once and every factorization afterwards reuses that ordering.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OrderAndFactor"/> analyses the pattern if it has changed and then factors from
/// scratch, choosing pivots as it goes. <see cref="Factor"/> reuses both the pattern and the
/// pivots that were chosen, which reduces the work to recomputing numbers along a route that
/// is already laid out. It reports failure if a pivot has collapsed, at which point the
/// caller is expected to fall back to <see cref="OrderAndFactor"/> - which is exactly what
/// the simulations in this library already do.
/// </para>
/// <para>
/// Note that <see cref="ForwardSubstitute"/> performs the whole solve and
/// <see cref="BackwardSubstitute"/> only writes the answer out. The two halves cannot be
/// separated here, because block triangular form interleaves them: each diagonal block has
/// to be solved completely before the block above it can be started.
/// </para>
/// </remarks>
/// <seealso cref="KluSolver{T}"/>
public partial class KluComplexSolver : KluSolver<Complex>
{
    private Complex[] _values;
    private double[] _rowScaleReciprocal;

    // The factors, stored per column of the reordered matrix. Row indices are positions in
    // the reordered matrix, so the diagonal of column k sits at position k.
    private int[] _lowerStart;
    private int[] _lowerRows;
    private Complex[] _lowerValues;
    private int _lowerCount;
    private int[] _upperStart;
    private int[] _upperRows;
    private Complex[] _upperValues;
    private int _upperCount;
    private Complex[] _diagonal;

    // The entries above the diagonal blocks. These never need factoring, only substituting.
    private int[] _offStart;
    private int[] _offRows;
    private Complex[] _offValues;
    private int _offCount;

    private int[] _rowPermutation;
    private int[] _rowPosition;
    private int[] _columnPosition;
    private int[] _blockRows;

    // Where each matrix entry ends up once the pivots are known, so that refactoring can
    // scatter the values without working any of it out again.
    private int[] _entrySource;
    private int[] _entryTarget;
    private int[] _entryColumnStart;
    private bool _planValid;

    // Kernel workspace.
    private Complex[] _work;
    private int[] _stack;
    private int[] _progress;
    private int[] _pattern;
    private int[] _candidates;
    private int[] _visited;
    private int _visitFlag;
    private int[] _pivotRow;
    private int[] _pivotColumn;

    private Complex[] _intermediate;

    /// <inheritdoc/>
    public override int OrderAndFactor()
    {
        ThrowIfPartial();
        IsFactored = false;

        int size = Size;
        EnsureValueCapacity();
        if (NeedsReordering || Symbolic == null || !TryGather(_values))
        {
            Analyze();
            EnsureValueCapacity();
            if (!TryGather(_values))
                throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored);
        }

        Allocate(size);
        int eliminated = FactorFromScratch();
        if (eliminated < size)
            return eliminated;

        BuildRefactorPlan();
        FactorNonZeroCount = _lowerCount + _upperCount + size;
        IsFactored = true;
        NeedsReordering = false;
        return size;
    }

    /// <inheritdoc/>
    public override bool Factor()
    {
        ThrowIfPartial();
        IsFactored = false;
        if (Symbolic == null || !_planValid || Symbolic.Size != Size)
            return false;
        EnsureValueCapacity();
        if (!TryGather(_values))
            return false;

        if (!Refactor())
            return false;
        IsFactored = true;
        return true;
    }

    private void EnsureValueCapacity()
    {
        int count = NonZeroCount;
        if (_values == null || _values.Length < count)
            _values = new Complex[Math.Max(count, 4)];
    }

    /// <summary>
    /// Makes sure every array the factorization needs is present and large enough.
    /// </summary>
    private void Allocate(int size)
    {
        if (_rowPermutation != null && _rowPermutation.Length >= size && _work != null)
            return;

        int capacity = Math.Max(size, 4);
        _rowScaleReciprocal = new double[capacity];
        _rowPermutation = new int[capacity];
        _rowPosition = new int[capacity];
        _columnPosition = new int[capacity];
        _blockRows = new int[capacity];
        _diagonal = new Complex[capacity];
        _lowerStart = new int[capacity + 1];
        _upperStart = new int[capacity + 1];
        _offStart = new int[capacity + 1];
        _work = new Complex[capacity];
        _stack = new int[capacity];
        _progress = new int[capacity];
        _pattern = new int[capacity];
        _candidates = new int[capacity];
        _visited = new int[capacity];
        _pivotRow = new int[capacity];
        _pivotColumn = new int[capacity];
        _intermediate = new Complex[capacity];
        _visitFlag = 0;

        int guess = Math.Max(4 * NonZeroCount, 16);
        _lowerRows = new int[guess];
        _lowerValues = new Complex[guess];
        _upperRows = new int[guess];
        _upperValues = new Complex[guess];
        _offRows = new int[Math.Max(NonZeroCount, 4)];
        _offValues = new Complex[Math.Max(NonZeroCount, 4)];
    }

    /// <inheritdoc/>
    protected override void ResetFactors()
    {
        _planValid = false;
        _lowerCount = 0;
        _upperCount = 0;
        _offCount = 0;
    }

    /// <summary>
    /// Divides every row by a measure of its size, so that the pivot tolerance means the same
    /// thing in a row of conductances as in a row of unit-valued incidence entries. The
    /// reciprocals are kept, because the right hand side has to be scaled the same way and
    /// multiplying is cheaper than dividing.
    /// </summary>
    private void ApplyScaling(int size)
    {
        int[] rows = PatternRows;
        int count = NonZeroCount;
        var scaling = Parameters.Scaling;

        if (scaling == KluScaling.None)
        {
            for (int i = 0; i < size; i++)
                _rowScaleReciprocal[i] = 1.0;
            return;
        }

        for (int i = 0; i < size; i++)
            _rowScaleReciprocal[i] = 0.0;
        if (scaling == KluScaling.Sum)
        {
            for (int p = 0; p < count; p++)
                _rowScaleReciprocal[rows[p]] += Magnitude(_values[p]);
        }
        else
        {
            for (int p = 0; p < count; p++)
            {
                double magnitude = Magnitude(_values[p]);
                if (magnitude > _rowScaleReciprocal[rows[p]])
                    _rowScaleReciprocal[rows[p]] = magnitude;
            }
        }

        for (int i = 0; i < size; i++)
        {
            // An empty row, or one that has gone non-finite, is left alone. Also catches NaN.
            double scale = _rowScaleReciprocal[i];
            _rowScaleReciprocal[i] = scale > 0.0 && !double.IsPositiveInfinity(scale) ? 1.0 / scale : 1.0;
        }
        for (int p = 0; p < count; p++)
            _values[p] = Scale(_values[p], _rowScaleReciprocal[rows[p]]);
    }

    /// <summary>
    /// Factors the matrix from scratch, choosing pivots as it goes.
    /// </summary>
    /// <returns>
    /// The number of positions that were successfully eliminated, which is the size of the
    /// matrix if the factorization succeeded.
    /// </returns>
    private int FactorFromScratch()
    {
        var symbolic = Symbolic;
        int size = symbolic.Size;
        int[] columnPermutation = symbolic.ColumnPermutation;
        int[] blockStarts = symbolic.BlockStarts;

        Array.Copy(symbolic.RowPermutation, _rowPermutation, size);
        for (int k = 0; k < size; k++)
        {
            _rowPosition[_rowPermutation[k]] = k;
            _columnPosition[columnPermutation[k]] = k;
        }

        ApplyScaling(size);
        _lowerCount = 0;
        _upperCount = 0;
        _offCount = 0;
        for (int i = 0; i < size; i++)
            _work[i] = 0.0;
        if (_visitFlag > int.MaxValue - size - 2)
        {
            Array.Clear(_visited, 0, _visited.Length);
            _visitFlag = 0;
        }

        double relative = Parameters.RelativePivotThreshold;
        double absolute = Parameters.AbsolutePivotThreshold;

        for (int block = 0; block < symbolic.Blocks; block++)
        {
            int from = blockStarts[block];
            int to = blockStarts[block + 1];
            int failed = to - from == 1
                ? FactorSingleton(from, absolute)
                : FactorBlock(from, to, relative, absolute);
            if (failed >= 0)
                return failed;
        }

        _lowerStart[size] = _lowerCount;
        _upperStart[size] = _upperCount;
        _offStart[size] = _offCount;
        return size;
    }

    /// <summary>
    /// Handles a block of one, which needs no factoring at all: the single entry on the
    /// diagonal is the whole factorization.
    /// </summary>
    /// <returns>The position that failed, or -1 on success.</returns>
    private int FactorSingleton(int position, double absolute)
    {
        int[] patternColumns = PatternColumns;
        int[] patternRows = PatternRows;

        _lowerStart[position] = _lowerCount;
        _upperStart[position] = _upperCount;
        _offStart[position] = _offCount;

        int column = Symbolic.ColumnPermutation[position];
        int end = patternColumns[column + 1];
        Complex pivot = 0.0;
        bool found = false;
        for (int p = patternColumns[column]; p < end; p++)
        {
            int row = _rowPosition[patternRows[p]];
            if (row < position)
                AddOff(row, _values[p]);
            else if (row == position)
            {
                pivot = _values[p];
                found = true;
            }
        }

        if (!found || Magnitude(pivot) <= absolute)
            return position;
        _diagonal[position] = pivot;
        return -1;
    }

    /// <summary>
    /// Factors one diagonal block, one column at a time. Each column is obtained by solving
    /// against the columns of the factor that are already known, and only then is a pivot
    /// picked from what is left. Because the columns already known are exactly the ones the
    /// new column reaches through the factor, the work stays proportional to the number of
    /// nonzero entries rather than to the size of the block.
    /// </summary>
    /// <returns>The position that failed, or -1 on success.</returns>
    private int FactorBlock(int from, int to, double relative, double absolute)
    {
        int[] patternColumns = PatternColumns;
        int[] patternRows = PatternRows;
        int[] columnPermutation = Symbolic.ColumnPermutation;
        int size = to - from;

        // Inside the block, rows are numbered by where the analysis put them; pivoting then
        // decides which of them ends up on the diagonal of which column.
        for (int r = 0; r < size; r++)
            _pivotColumn[r] = -1;

        for (int j = 0; j < size; j++)
        {
            int position = from + j;
            _lowerStart[position] = _lowerCount;
            _upperStart[position] = _upperCount;
            _offStart[position] = _offCount;

            // ----- spread the column out, setting aside what belongs above this block -----
            _visitFlag++;
            int top = size;
            int candidateCount = 0;
            int column = columnPermutation[position];
            int end = patternColumns[column + 1];
            for (int p = patternColumns[column]; p < end; p++)
            {
                int row = _rowPosition[patternRows[p]];
                if (row < from)
                {
                    AddOff(row, _values[p]);
                    continue;
                }
                int local = row - from;
                _work[local] = _values[p];
                if (_visited[local] != _visitFlag)
                    top = Reach(local, top, from, ref candidateCount);
            }

            // ----- solve against the columns already computed -----
            for (int s = top; s < size; s++)
            {
                int earlier = _pattern[s];
                int pivotRow = _pivotRow[earlier];
                Complex value = _work[pivotRow];
                _work[pivotRow] = 0.0;
                AddUpper(from + earlier, value);

                int lowerEnd = _lowerStart[from + earlier + 1];
                for (int p = _lowerStart[from + earlier]; p < lowerEnd; p++)
                    _work[_lowerRows[p]] -= _lowerValues[p] * value;
            }

            // ----- pick the pivot -----
            double largest = 0.0;
            int chosen = -1;
            double diagonal = 0.0;
            bool diagonalAvailable = false;
            for (int c = 0; c < candidateCount; c++)
            {
                int row = _candidates[c];
                double magnitude = Magnitude(_work[row]);
                if (magnitude > largest)
                {
                    largest = magnitude;
                    chosen = row;
                }
                if (row == j)
                {
                    diagonalAvailable = true;
                    diagonal = magnitude;
                }
            }
            if (chosen < 0 || largest <= absolute)
            {
                for (int c = 0; c < candidateCount; c++)
                    _work[_candidates[c]] = 0.0;
                return position;
            }

            // Staying on the diagonal is worth a weaker pivot, because it is what keeps the
            // ordering the analysis computed intact.
            if (diagonalAvailable && diagonal >= relative * largest && diagonal > absolute)
                chosen = j;

            Complex pivot = _work[chosen];
            _diagonal[position] = pivot;
            _pivotRow[j] = chosen;
            _pivotColumn[chosen] = j;
            _work[chosen] = 0.0;

            // ----- whatever is left below the pivot becomes the column of L -----
            Complex reciprocal = Inverse(pivot);
            for (int c = 0; c < candidateCount; c++)
            {
                int row = _candidates[c];
                if (row == chosen)
                    continue;
                AddLower(row, _work[row] * reciprocal);
                _work[row] = 0.0;
            }
        }

        _lowerStart[to] = _lowerCount;
        _upperStart[to] = _upperCount;
        _offStart[to] = _offCount;

        // The columns of L were built while the rows were still numbered the way the analysis
        // left them. Now that every row has a pivot, they can be renumbered by position.
        int lowerEndOfBlock = _lowerStart[to];
        for (int p = _lowerStart[from]; p < lowerEndOfBlock; p++)
            _lowerRows[p] = from + _pivotColumn[_lowerRows[p]];

        // And the block's share of the row permutation follows the pivots.
        for (int r = 0; r < size; r++)
            _blockRows[r] = _rowPermutation[from + r];
        for (int j = 0; j < size; j++)
        {
            int row = _blockRows[_pivotRow[j]];
            _rowPermutation[from + j] = row;
            _rowPosition[row] = from + j;
        }
        return -1;
    }

    /// <summary>
    /// Walks the factor backwards from one entry of the column being computed, to find out
    /// which of the columns already computed feed into it. They come out in an order that
    /// lets the substitution run straight through, and any row that has no pivot yet is
    /// recorded as a candidate to become one.
    /// </summary>
    private int Reach(int start, int top, int from, ref int candidateCount)
    {
        int head = 0;
        _stack[0] = start;
        while (head >= 0)
        {
            int row = _stack[head];
            int column = _pivotColumn[row];
            if (_visited[row] != _visitFlag)
            {
                _visited[row] = _visitFlag;
                if (column < 0)
                {
                    // No pivot yet, so this row is a candidate and there is nothing under it.
                    _candidates[candidateCount++] = row;
                    head--;
                    continue;
                }
                _progress[head] = _lowerStart[from + column];
            }

            int end = _lowerStart[from + column + 1];
            bool descended = false;
            for (int p = _progress[head]; p < end; p++)
            {
                int next = _lowerRows[p];
                if (_visited[next] == _visitFlag)
                    continue;
                _progress[head] = p + 1;
                head++;
                _stack[head] = next;
                descended = true;
                break;
            }
            if (descended)
                continue;

            _pattern[--top] = column;
            head--;
        }
        return top;
    }

    /// <summary>
    /// Records where every matrix entry has to go once the pivots are settled, so that a
    /// refactorization is a straight pass over the values.
    /// </summary>
    private void BuildRefactorPlan()
    {
        var symbolic = Symbolic;
        int size = symbolic.Size;
        int[] patternColumns = PatternColumns;
        int[] patternRows = PatternRows;
        int[] columnPermutation = symbolic.ColumnPermutation;
        int[] blockStarts = symbolic.BlockStarts;
        int count = NonZeroCount;

        if (_entrySource == null || _entrySource.Length < count)
        {
            _entrySource = new int[Math.Max(count, 4)];
            _entryTarget = new int[Math.Max(count, 4)];
        }
        if (_entryColumnStart == null || _entryColumnStart.Length < size + 1)
            _entryColumnStart = new int[size + 1];

        int entry = 0;
        int off = 0;
        for (int block = 0; block < symbolic.Blocks; block++)
        {
            int from = blockStarts[block];
            int to = blockStarts[block + 1];
            for (int position = from; position < to; position++)
            {
                _entryColumnStart[position] = entry;
                int column = columnPermutation[position];
                int end = patternColumns[column + 1];
                for (int p = patternColumns[column]; p < end; p++)
                {
                    int row = _rowPosition[patternRows[p]];
                    _entrySource[entry] = p;
                    _entryTarget[entry] = row < from ? -off++ - 1 : row;
                    entry++;
                }
            }
        }
        _entryColumnStart[size] = entry;
        _planValid = true;
    }

    /// <summary>
    /// Recomputes the factors along the route the previous factorization laid out.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the route still works; <c>false</c> if a pivot has collapsed, in which
    /// case the caller has to go back to <see cref="OrderAndFactor"/>.
    /// </returns>
    private bool Refactor()
    {
        var symbolic = Symbolic;
        int size = symbolic.Size;
        int[] blockStarts = symbolic.BlockStarts;
        double absolute = Parameters.AbsolutePivotThreshold;
        double relative = Parameters.RelativePivotThreshold;

        ApplyScaling(size);

        for (int block = 0; block < symbolic.Blocks; block++)
        {
            int from = blockStarts[block];
            int to = blockStarts[block + 1];
            for (int position = from; position < to; position++)
            {
                int entryEnd = _entryColumnStart[position + 1];
                for (int entry = _entryColumnStart[position]; entry < entryEnd; entry++)
                {
                    int target = _entryTarget[entry];
                    Complex value = _values[_entrySource[entry]];
                    if (target >= 0)
                        _work[target] = value;
                    else
                        _offValues[-target - 1] = value;
                }

                int upperEnd = _upperStart[position + 1];
                for (int p = _upperStart[position]; p < upperEnd; p++)
                {
                    int earlier = _upperRows[p];
                    Complex value = _work[earlier];
                    _work[earlier] = 0.0;
                    _upperValues[p] = value;

                    int lowerEnd = _lowerStart[earlier + 1];
                    for (int q = _lowerStart[earlier]; q < lowerEnd; q++)
                        _work[_lowerRows[q]] -= _lowerValues[q] * value;
                }

                Complex pivot = _work[position];
                _work[position] = 0.0;
                double magnitude = Magnitude(pivot);
                double largest = magnitude;

                // Empty the rest of the column into L while noting how big it got. Emptying it
                // as we go means the workspace is already clean if we have to give up below.
                int start = _lowerStart[position];
                int end = _lowerStart[position + 1];
                for (int p = start; p < end; p++)
                {
                    int row = _lowerRows[p];
                    Complex value = _work[row];
                    _work[row] = 0.0;
                    _lowerValues[p] = value;
                    double candidate = Magnitude(value);
                    if (candidate > largest)
                        largest = candidate;
                }

                if (!(magnitude > absolute) || magnitude < relative * largest)
                    return false;
                _diagonal[position] = pivot;

                Complex reciprocal = Inverse(pivot);
                for (int p = start; p < end; p++)
                    _lowerValues[p] *= reciprocal;
            }
        }
        return true;
    }

    /// <summary>
    /// Measures how big a complex value is. The sum of the two magnitudes is used rather than
    /// the true modulus, which is the same choice <see cref="SparseComplexSolver"/> makes: it
    /// costs no square root and never differs from the modulus by more than a factor of two,
    /// which is far finer than any pivot decision needs.
    /// </summary>
    private static double Magnitude(Complex value)
        => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

    /// <summary>
    /// Multiplies a complex value by a real one without going through a full complex multiply.
    /// </summary>
    private static Complex Scale(Complex value, double factor)
        => new Complex(value.Real * factor, value.Imaginary * factor);

    /// <summary>
    /// Calculates the reciprocal of a complex number, dividing by whichever part is larger so
    /// that a value with one very large part cannot overflow on the way.
    /// </summary>
    private static Complex Inverse(Complex value)
    {
        double real, imaginary, ratio;
        if ((value.Real >= value.Imaginary && value.Real > -value.Imaginary) ||
            (value.Real < value.Imaginary && value.Real <= -value.Imaginary))
        {
            ratio = value.Imaginary / value.Real;
            real = 1.0 / (value.Real + (ratio * value.Imaginary));
            imaginary = -ratio * real;
        }
        else
        {
            ratio = value.Real / value.Imaginary;
            imaginary = -1.0 / (value.Imaginary + (ratio * value.Real));
            real = -ratio * imaginary;
        }
        return new Complex(real, imaginary);
    }

    private void AddLower(int row, Complex value)
    {
        if (_lowerCount == _lowerRows.Length)
        {
            Array.Resize(ref _lowerRows, 2 * _lowerCount);
            Array.Resize(ref _lowerValues, 2 * _lowerCount);
        }
        _lowerRows[_lowerCount] = row;
        _lowerValues[_lowerCount++] = value;
    }

    private void AddUpper(int row, Complex value)
    {
        if (_upperCount == _upperRows.Length)
        {
            Array.Resize(ref _upperRows, 2 * _upperCount);
            Array.Resize(ref _upperValues, 2 * _upperCount);
        }
        _upperRows[_upperCount] = row;
        _upperValues[_upperCount++] = value;
    }

    private void AddOff(int row, Complex value)
    {
        if (_offCount == _offRows.Length)
        {
            Array.Resize(ref _offRows, 2 * _offCount);
            Array.Resize(ref _offValues, 2 * _offCount);
        }
        _offRows[_offCount] = row;
        _offValues[_offCount++] = value;
    }

    /// <inheritdoc/>
    public override void ForwardSubstitute(IVector<Complex> solution)
    {
        int size = PrepareSolve(solution);

        // Gather the right hand side into the order the blocks are in, scaled the way the
        // matrix was.
        for (int k = 0; k < size; k++)
            _intermediate[k] = 0.0;
        for (var element = Vector.GetFirstInVector(); element != null; element = element.Below)
        {
            int row = element.Index - 1;
            if (row >= 0 && row < size)
                _intermediate[_rowPosition[row]] = Scale(element.Value, _rowScaleReciprocal[row]);
        }

        int[] blockStarts = Symbolic.BlockStarts;
        for (int block = Symbolic.Blocks - 1; block >= 0; block--)
        {
            int from = blockStarts[block];
            int to = blockStarts[block + 1];

            for (int k = from; k < to; k++)
            {
                Complex value = _intermediate[k];
                if (value == Complex.Zero)
                    continue;
                int end = _lowerStart[k + 1];
                for (int p = _lowerStart[k]; p < end; p++)
                    _intermediate[_lowerRows[p]] -= _lowerValues[p] * value;
            }

            for (int k = to - 1; k >= from; k--)
            {
                Complex value = _intermediate[k] / _diagonal[k];
                _intermediate[k] = value;
                if (value == Complex.Zero)
                    continue;
                int end = _upperStart[k + 1];
                for (int p = _upperStart[k]; p < end; p++)
                    _intermediate[_upperRows[p]] -= _upperValues[p] * value;
            }

            // Now that this block is known, take its contribution out of the blocks above it.
            for (int k = from; k < to; k++)
            {
                Complex value = _intermediate[k];
                if (value == Complex.Zero)
                    continue;
                int end = _offStart[k + 1];
                for (int p = _offStart[k]; p < end; p++)
                    _intermediate[_offRows[p]] -= _offValues[p] * value;
            }
        }
    }

    /// <inheritdoc/>
    public override void BackwardSubstitute(IVector<Complex> solution)
    {
        int size = Symbolic.Size;
        int[] columnPermutation = Symbolic.ColumnPermutation;
        for (int k = 0; k < size; k++)
            solution[Column.Reverse(columnPermutation[k] + 1)] = _intermediate[k];
    }

    /// <inheritdoc/>
    public override void ForwardSubstituteTransposed(IVector<Complex> solution)
    {
        int size = PrepareSolve(solution);

        // The right hand side of the transposed system is indexed by column, so every entry
        // has to travel through the outside index to get there.
        for (int k = 0; k < size; k++)
            _intermediate[k] = 0.0;
        for (var element = Vector.GetFirstInVector(); element != null; element = element.Below)
        {
            int column = Column[Row.Reverse(element.Index)] - 1;
            if (column >= 0 && column < size)
                _intermediate[_columnPosition[column]] = element.Value;
        }

        int[] blockStarts = Symbolic.BlockStarts;
        for (int block = 0; block < Symbolic.Blocks; block++)
        {
            int from = blockStarts[block];
            int to = blockStarts[block + 1];

            // Transposing turns the entries above the diagonal blocks into entries below
            // them, so they are consumed before the block instead of after it.
            for (int k = from; k < to; k++)
            {
                Complex sum = _intermediate[k];
                int end = _offStart[k + 1];
                for (int p = _offStart[k]; p < end; p++)
                    sum -= _offValues[p] * _intermediate[_offRows[p]];
                _intermediate[k] = sum;
            }

            for (int k = from; k < to; k++)
            {
                Complex sum = _intermediate[k];
                int end = _upperStart[k + 1];
                for (int p = _upperStart[k]; p < end; p++)
                    sum -= _upperValues[p] * _intermediate[_upperRows[p]];
                _intermediate[k] = sum / _diagonal[k];
            }

            for (int k = to - 1; k >= from; k--)
            {
                Complex sum = _intermediate[k];
                int end = _lowerStart[k + 1];
                for (int p = _lowerStart[k]; p < end; p++)
                    sum -= _lowerValues[p] * _intermediate[_lowerRows[p]];
                _intermediate[k] = sum;
            }
        }
    }

    /// <inheritdoc/>
    public override void BackwardSubstituteTransposed(IVector<Complex> solution)
    {
        int size = Symbolic.Size;
        for (int k = 0; k < size; k++)
        {
            int row = _rowPermutation[k];
            solution[Row.Reverse(row + 1)] = Scale(_intermediate[k], _rowScaleReciprocal[row]);
        }
    }

    /// <inheritdoc/>
    public override Complex ComputeDegenerateContribution(int index)
    {
        ThrowIfPartial();
        return 0.0;
    }

    /// <inheritdoc/>
    public override Complex ComputeDegenerateContributionTransposed(int index)
    {
        ThrowIfPartial();
        return 0.0;
    }

    /// <summary>
    /// Checks that the solver is in a state where it can produce a solution.
    /// </summary>
    /// <returns>The size of the system.</returns>
    private int PrepareSolve(IVector<Complex> solution)
    {
        solution.ThrowIfNull(nameof(solution));
        ThrowIfPartial();
        if (!IsFactored || Symbolic == null || Symbolic.Size != Size)
            throw new AlgebraException(Properties.Resources.Algebra_SolverNotFactored);
        if (solution.Length != Size)
            throw new ArgumentException(Properties.Resources.Algebra_VectorLengthMismatch.FormatString(solution.Length, Size), nameof(solution));
        return Symbolic.Size;
    }
}
