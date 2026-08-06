namespace SpiceSharp.Simulations;

/// <summary>
/// The kind of solver a simulation uses for the linear system it has to solve at every
/// iteration.
/// </summary>
public enum SolverTypes
{
    /// <summary>
    /// Sparse LU decomposition that searches for a pivot while it eliminates, balancing
    /// numerical quality against the fill it causes. Robust and the default.
    /// </summary>
    Sparse = 0,

    /// <summary>
    /// The approach KLU takes: work out an elimination order from the sparsity pattern once,
    /// then reuse it for every factorization afterwards. Circuit simulation factors the same
    /// pattern thousands of times with only the values changing, so the analysis is paid for
    /// once and the repeated work drops considerably. It falls back to a full factorization
    /// with a fresh pivot search whenever a pivot collapses or the pattern changes.
    /// </summary>
    Klu = 1
}
