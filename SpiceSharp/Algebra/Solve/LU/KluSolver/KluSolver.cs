using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// A base class for sparse linear systems solved the way KLU solves them: the sparsity
/// pattern is analysed once, up front, and every factorization after that reuses the
/// resulting ordering. Circuit simulation factors the same pattern over and over with only
/// the values changing, so paying for the analysis once and then only recomputing numbers is
/// what makes this fast.
/// </summary>
/// <typeparam name="T">The base value type.</typeparam>
/// <remarks>
/// <para>
/// The analysis first permutes the matrix to block triangular form, then reduces fill inside
/// each diagonal block. Only the diagonal blocks are ever factored; everything above them is
/// handled by a substitution during the solve. Circuit matrices tend to break up into many
/// small blocks, which is where most of the advantage comes from.
/// </para>
/// <para>
/// Equations are still assembled into the same sparse matrix and right hand side vector the
/// other solvers use, so element references, preconditioning and index translation all behave
/// identically. The analysis works from a snapshot of that matrix, and the snapshot is checked
/// on every factorization, so adding, removing or moving an entry simply causes the pattern to
/// be analysed again.
/// </para>
/// </remarks>
/// <seealso cref="SparsePivotingSolver{T}"/>
/// <seealso cref="KluParameters"/>
public abstract partial class KluSolver<T> : SparsePivotingSolver<T>,
    IParameterized<KluParameters>
{
    /// <summary>
    /// Gets the parameters controlling how the matrix is ordered and factored.
    /// </summary>
    /// <value>
    /// The parameters.
    /// </value>
    public KluParameters Parameters { get; } = new KluParameters();

    /// <summary>
    /// Gets the result of the last pattern analysis, or <c>null</c> if the pattern has not
    /// been analysed yet.
    /// </summary>
    /// <value>
    /// The analysis result.
    /// </value>
    public KluSymbolic Symbolic { get; private set; }

    /// <summary>
    /// Gets the number of nonzero entries in the factors that the last factorization
    /// produced, the diagonal included. Comparing this against the number of entries in the
    /// matrix says how much fill the ordering ended up causing.
    /// </summary>
    /// <value>
    /// The number of nonzero entries in the factors.
    /// </value>
    public int FactorNonZeroCount { get; protected set; }

    /// <summary>Column pointers of the snapshot, in the reordered system, zero based.</summary>
    private int[] _patternColumns;

    /// <summary>Row index of every snapshot entry, in the reordered system, zero based.</summary>
    private int[] _patternRows;

    /// <summary>Column index of every snapshot entry, in the reordered system, zero based.</summary>
    private int[] _patternEntryColumns;

    /// <summary>The matrix element every snapshot entry refers to.</summary>
    private ISparseMatrixElement<T>[] _patternElements;

    private int _patternSize = -1;
    private int _patternElementCount;
    private bool _patternValid;

    /// <summary>
    /// Gets the number of nonzero entries in the matrix as the last analysis saw it.
    /// Subtracting this from <see cref="FactorNonZeroCount"/> gives the amount of fill the
    /// ordering caused.
    /// </summary>
    /// <value>
    /// The number of entries.
    /// </value>
    public int MatrixNonZeroCount => _patternElements == null ? 0 : _patternElements.Length;

    /// <summary>
    /// Gets the number of entries in the snapshot of the matrix.
    /// </summary>
    /// <value>
    /// The number of entries.
    /// </value>
    protected int NonZeroCount => MatrixNonZeroCount;

    /// <summary>
    /// Gets the column pointers of the snapshot. Zero based, with <see cref="ISolver{T}.Size"/> + 1 entries.
    /// </summary>
    /// <value>
    /// The column pointers.
    /// </value>
    protected int[] PatternColumns => _patternColumns;

    /// <summary>
    /// Gets the row index of every entry of the snapshot, zero based and ascending within
    /// each column.
    /// </summary>
    /// <value>
    /// The row indices.
    /// </value>
    protected int[] PatternRows => _patternRows;

    /// <inheritdoc/>
    protected override Element<T> GetInternalElement(MatrixLocation location)
    {
        int before = Matrix.ElementCount;
        var element = base.GetInternalElement(location);
        if (Matrix.ElementCount != before)
            _patternValid = false;
        return element;
    }

    /// <inheritdoc/>
    protected override bool RemoveInternalElement(MatrixLocation location)
    {
        bool removed = base.RemoveInternalElement(location);
        if (removed)
            _patternValid = false;
        return removed;
    }

    /// <summary>
    /// Takes a fresh snapshot of the sparsity pattern and analyses it. Any factors from
    /// before are thrown away, because they describe a pattern that no longer applies.
    /// </summary>
    protected void Analyze()
    {
        CapturePattern();
        Symbolic = KluSymbolic.Analyze(_patternSize, _patternColumns, _patternRows, Parameters);
    }

    /// <summary>
    /// Copies the current matrix values into the given array, in the order of the snapshot,
    /// while checking that the snapshot still describes the matrix.
    /// </summary>
    /// <param name="values">
    /// The array to fill. It has to have at least <see cref="NonZeroCount"/> elements.
    /// </param>
    /// <returns>
    /// <c>true</c> if the snapshot is still valid and the values were copied; <c>false</c> if
    /// the pattern has changed and the matrix needs to be analysed again.
    /// </returns>
    protected bool TryGather(T[] values)
    {
        if (!_patternValid || Symbolic == null || _patternSize != Size)
            return false;
        if (Matrix.ElementCount != _patternElementCount)
            return false;

        // Reading the value and checking the position in one pass keeps this to a single
        // walk over the elements, which is the expensive part.
        var elements = _patternElements;
        int[] rows = _patternRows;
        int[] columns = _patternEntryColumns;
        for (int p = 0; p < elements.Length; p++)
        {
            var element = elements[p];
            if (element.Row - 1 != rows[p] || element.Column - 1 != columns[p])
            {
                _patternValid = false;
                return false;
            }
            values[p] = element.Value;
        }
        return true;
    }

    /// <summary>
    /// Reads the sparsity pattern out of the matrix into compressed column form, keeping a
    /// reference to every element so that later factorizations can pick up the values
    /// without walking the matrix again.
    /// </summary>
    private void CapturePattern()
    {
        int n = Size;
        int count = 0;
        for (int column = 1; column <= n; column++)
        {
            for (var element = Matrix.GetFirstInColumn(column); element != null; element = element.Below)
                count++;
        }

        if (_patternColumns == null || _patternColumns.Length != n + 1)
            _patternColumns = new int[n + 1];
        if (_patternElements == null || _patternElements.Length != count)
        {
            _patternElements = new ISparseMatrixElement<T>[count];
            _patternRows = new int[count];
            _patternEntryColumns = new int[count];
        }

        int p = 0;
        for (int column = 1; column <= n; column++)
        {
            _patternColumns[column - 1] = p;

            // The matrix keeps every column sorted by row, which is exactly the order
            // compressed column form needs.
            for (var element = Matrix.GetFirstInColumn(column); element != null; element = element.Below)
            {
                _patternElements[p] = element;
                _patternRows[p] = element.Row - 1;
                _patternEntryColumns[p] = column - 1;
                p++;
            }
        }
        _patternColumns[n] = p;

        _patternSize = n;
        _patternElementCount = Matrix.ElementCount;
        _patternValid = true;
    }

    /// <summary>
    /// Throws if the solver has been asked to leave part of the system out of the
    /// factorization, which this solver does not support.
    /// </summary>
    /// <exception cref="AlgebraException">
    /// Thrown if <see cref="ISolver{T}.Degeneracy"/> or
    /// <see cref="IPivotingSolver{M, V, T}.PivotSearchReduction"/> is not zero.
    /// </exception>
    protected void ThrowIfPartial()
    {
        if (Degeneracy != 0 || PivotSearchReduction != 0)
            throw new AlgebraException(Properties.Resources.Algebra_KluPartialNotSupported);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This only clears the values, not the pattern, so the analysis and the route the last
    /// factorization laid out both stay valid. That matters: a simulation resets the system
    /// on every iteration, and throwing the route away here would mean never refactoring at
    /// all.
    /// </remarks>
    public override void Reset()
    {
        base.Reset();
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        Symbolic = null;
        _patternColumns = null;
        _patternRows = null;
        _patternEntryColumns = null;
        _patternElements = null;
        _patternSize = -1;
        _patternElementCount = 0;
        _patternValid = false;
        FactorNonZeroCount = 0;
        ResetFactors();
    }

    /// <summary>
    /// Discards the numeric factors, without touching the analysis.
    /// </summary>
    protected abstract void ResetFactors();

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => "KLU solver ({0}x{1})".FormatString(Size, Size + 1);
}
