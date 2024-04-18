using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IWorkDistributor{R}"/> that combines the results using the boolean And operator.
    /// </summary>
    /// <seealso cref="IWorkDistributor{R}" />
    public class TPLBooleanAndWorkDistributor : TPLWorkDistributor,
        IWorkDistributor<bool>
    {
        /// <inheritdoc/>
        public bool Execute(IReadOnlyList<Func<bool>> methods)
        {
            methods.ThrowIfNull(nameof(methods));
            var tasks = new Task<bool>[methods.Count];
            for (int i = 0; i < methods.Count; i++)
                tasks[i] = Task.Run(methods[i]);
            Task.WaitAll(tasks);

            // Combine the results synchronously
            bool result = true;
            for (int i = 0; i < tasks.Length; i++)
                result &= tasks[i].Result;
            return result;
        }
    }
}
