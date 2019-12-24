using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A distributor of work (potentially in parallel).
    /// </summary>
    public interface IWorkDistributor
    {
        /// <summary>
        /// Executes the specified methods.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        void Execute(Action[] methods);
    }

    /// <summary>
    /// A distributor of work (potentially in parallel).
    /// </summary>
    /// <typeparam name="R">The return type.</typeparam>
    public interface IWorkDistributor<R>
    {
        /// <summary>
        /// Executes the specified methods and accumulates the result.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        /// <returns>The result.</returns>
        R Execute(Func<R>[] methods);
    }
}
