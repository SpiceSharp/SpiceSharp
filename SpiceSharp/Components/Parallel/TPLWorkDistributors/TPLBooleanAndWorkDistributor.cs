using System;
using System.Threading.Tasks;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IWorkDistributor{R}"/> that combines the results using the boolean And operator.
    /// </summary>
    /// <seealso cref="IWorkDistributor{R}" />
    public class TPLBooleanAndWorkDistributor : IWorkDistributor<bool>
    {
        /// <summary>
        /// Executes the specified methods and accumulates the result.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public bool Execute(Func<bool>[] methods)
        {
            methods.ThrowIfNull(nameof(methods));
            var tasks = new Task<bool>[methods.Length];
            for (int i = 0; i < methods.Length; i++)
                tasks[i] = Task.Run(methods[i]);
            Task.WaitAll(tasks);

            // Combine the results
            var result = true;
            for (int i = 0; i < tasks.Length; i++)
                result &= tasks[i].Result;
            return result;
        }
    }
}
