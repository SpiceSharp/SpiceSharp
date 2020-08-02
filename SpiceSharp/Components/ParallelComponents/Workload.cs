using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// A piece of work that can be executed.
    /// </summary>
    public class Workload
    {
        private readonly IWorkDistributor _distributor;

        /// <summary>
        /// Gets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public List<Action> Actions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Workload"/> class.
        /// </summary>
        /// <param name="distributor">The distributor.</param>
        /// <param name="capacity">The initial capacity.</param>
        public Workload(IWorkDistributor distributor, int capacity)
        {
            _distributor = distributor.ThrowIfNull(nameof(distributor));
            Actions = new List<Action>(capacity);
        }

        /// <summary>
        /// Executes the work.
        /// </summary>
        public void Execute() => _distributor.Execute(Actions);
    }

    /// <summary>
    /// A piece of work that can be executed.
    /// </summary>
    /// <typeparam name="R">The return value.</typeparam>
    public class Workload<R>
    {
        private readonly IWorkDistributor<R> _distributor;

        /// <summary>
        /// Gets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public List<Func<R>> Functions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Workload{R}"/> class.
        /// </summary>
        /// <param name="distributor">The distributor.</param>
        /// <param name="capacity">The initial capacity.</param>
        public Workload(IWorkDistributor<R> distributor, int capacity)
        {
            _distributor = distributor.ThrowIfNull(nameof(distributor));
            Functions = new List<Func<R>>(capacity);
        }

        /// <summary>
        /// Executes the work.
        /// </summary>
        /// <returns>The combined result.</returns>
        public R Execute() => _distributor.Execute(Functions);
    }
}
