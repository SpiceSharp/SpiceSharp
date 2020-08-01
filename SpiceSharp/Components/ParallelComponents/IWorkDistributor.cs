using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A distributor of work that can be done in parallel.
    /// </summary>
    public interface IWorkDistributor
    {
        /// <summary>
        /// Executes the specified methods.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methods"/> is <c>null</c>.</exception>
        void Execute(IReadOnlyList<Action> methods);
    }

    /// <summary>
    /// A distributor of work that can be done in parallel while needing to return a value.
    /// </summary>
    /// <typeparam name="R">The return type.</typeparam>
    public interface IWorkDistributor<R> : IWorkDistributor
    {
        /// <summary>
        /// Executes the specified methods and accumulates the result.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        /// <returns>The combined result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methods"/> is <c>null</c>.</exception>
        R Execute(IReadOnlyList<Func<R>> methods);
    }
}
