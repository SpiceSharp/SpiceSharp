using System;
using System.Threading.Tasks;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An <see cref="IWorkDistributor"/> that simply gives each method its own task using the Task Parallel Library.
    /// </summary>
    /// <seealso cref="IWorkDistributor" />
    public class TPLWorkDistributor : IWorkDistributor
    {
        /// <summary>
        /// Executes the specified methods.
        /// </summary>
        /// <param name="methods">The methods to be executed.</param>
        public void Execute(Action[] methods)
        {
            methods.ThrowIfNull(nameof(methods));
            var tasks = new Task[methods.Length];
            for (int i = 0; i < methods.Length; i++)
                tasks[i] = Task.Run(methods[i]);
            Task.WaitAll(tasks);
        }
    }
}
