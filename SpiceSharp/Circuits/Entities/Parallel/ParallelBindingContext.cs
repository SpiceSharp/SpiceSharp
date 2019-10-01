using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Circuits.ParallelBehaviors
{
    /// <summary>
    /// Binding context for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class ParallelBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the pool of local behaviors.
        /// </summary>
        /// <value>
        /// The pool.
        /// </value>
        public BehaviorContainerCollection Concurrent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelBindingContext"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <param name="collection">The behaviors inside the parallel loader.</param>
        public ParallelBindingContext(ISimulation simulation, BehaviorContainer behaviors, BehaviorContainerCollection collection) 
            : base(simulation, behaviors)
        {
            Concurrent = collection.ThrowIfNull(nameof(collection));
        }
    }
}
