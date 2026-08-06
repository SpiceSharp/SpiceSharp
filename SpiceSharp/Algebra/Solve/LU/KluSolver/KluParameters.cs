using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Algebra.Solve;

/// <summary>
/// The fill-reducing ordering applied to each diagonal block.
/// </summary>
public enum KluOrdering
{
    /// <summary>
    /// Approximate minimum degree on the pattern of the block plus its transpose. This is
    /// the default and usually the best choice for circuit matrices, which are close to
    /// symmetric in structure.
    /// </summary>
    ApproximateMinimumDegree = 0,

    /// <summary>
    /// Column approximate minimum degree. Worth trying when the pattern is far from
    /// symmetric, because it orders the columns without assuming anything about the rows.
    /// </summary>
    ColumnApproximateMinimumDegree = 1,

    /// <summary>
    /// No fill-reducing ordering at all, leaving the blocks in the order the block
    /// triangular form produced them. Mostly useful for diagnosing ordering problems.
    /// </summary>
    Natural = 2
}

/// <summary>
/// How rows are scaled before the matrix is factored.
/// </summary>
public enum KluScaling
{
    /// <summary>
    /// No scaling.
    /// </summary>
    None = 0,

    /// <summary>
    /// Divide every row by the largest magnitude in it.
    /// </summary>
    Maximum = 1,

    /// <summary>
    /// Divide every row by the sum of the magnitudes in it. This is the default: it costs
    /// the same as <see cref="Maximum"/> and tends to make the pivot tolerance behave more
    /// predictably across rows of wildly different scale, which is common in circuit
    /// matrices that mix conductances with unit-valued incidence entries.
    /// </summary>
    Sum = 2
}

/// <summary>
/// Parameters that control how a <see cref="KluSolver{T}"/> orders and factors its matrix.
/// </summary>
/// <seealso cref="ParameterSet"/>
public partial class KluParameters : ParameterSet, ICloneable<KluParameters>
{
    /// <summary>
    /// Gets or sets how much smaller than the largest candidate in a column the diagonal
    /// entry is still allowed to be while remaining the chosen pivot.
    /// </summary>
    /// <value>
    /// The relative pivot threshold.
    /// </value>
    /// <remarks>
    /// Staying on the diagonal keeps the ordering that was computed up front intact, which
    /// is what makes refactoring cheap, so it is worth tolerating a somewhat weaker pivot to
    /// get it. Raising this towards 1 asks for stricter pivots and more numerical safety at
    /// the cost of more fill; lowering it towards 0 accepts almost any nonzero diagonal.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if the value is negative or larger than 1.
    /// </exception>
    [ParameterName("pivrel"), ParameterInfo("The relative threshold for keeping a pivot on the diagonal")]
    [GreaterThanOrEquals(0), LessThanOrEquals(1), Finite]
    private double _relativePivotThreshold = 1e-3;

    /// <summary>
    /// Gets or sets the magnitude below which a pivot counts as zero and the matrix is
    /// reported as singular.
    /// </summary>
    /// <value>
    /// The absolute pivot threshold.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if the value is negative.
    /// </exception>
    [ParameterName("pivtol"), ParameterInfo("The magnitude below which a pivot counts as zero")]
    [GreaterThanOrEquals(0), Finite]
    private double _absolutePivotThreshold = 1e-13;

    /// <summary>
    /// Gets or sets the fill-reducing ordering used for the diagonal blocks.
    /// </summary>
    /// <value>
    /// The ordering.
    /// </value>
    [ParameterName("ordering"), ParameterInfo("The fill-reducing ordering for the diagonal blocks")]
    public KluOrdering Ordering { get; set; } = KluOrdering.ApproximateMinimumDegree;

    /// <summary>
    /// Gets or sets the row scaling applied before factoring.
    /// </summary>
    /// <value>
    /// The scaling.
    /// </value>
    [ParameterName("scale"), ParameterInfo("The row scaling applied before factoring")]
    public KluScaling Scaling { get; set; } = KluScaling.Sum;

    /// <summary>
    /// Gets or sets a value indicating whether the matrix is permuted to block triangular
    /// form before it is ordered and factored.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the block triangular form is used; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Circuit matrices tend to break up into many small blocks, and only the blocks
    /// themselves ever need to be factored, so leaving this on is almost always the right
    /// choice. Turning it off is mainly a way to check whether it is responsible for a
    /// problem.
    /// </remarks>
    [ParameterName("btf"), ParameterInfo("Permute to block triangular form before factoring")]
    public bool UseBlockTriangularForm { get; set; } = true;

    /// <inheritdoc/>
    public KluParameters Clone()
    {
        return (KluParameters)MemberwiseClone();
    }
}
