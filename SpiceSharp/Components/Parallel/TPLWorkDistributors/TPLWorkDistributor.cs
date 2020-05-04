﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IWorkDistributor"/> that simply gives each method its own task using the Task Parallel Library.
    /// </summary>
    /// <seealso cref="IWorkDistributor" />
    public class TPLWorkDistributor : IWorkDistributor
    {
        /// <inheritdoc/>
        public void Execute(IReadOnlyList<Action> methods)
        {
            methods.ThrowIfNull(nameof(methods));
            var tasks = new Task[methods.Count];
            for (int i = 0; i < methods.Count; i++)
                tasks[i] = Task.Run(methods[i]);
            Task.WaitAll(tasks);
        }
    }
}
