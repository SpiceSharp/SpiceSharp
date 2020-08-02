using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// Base parameters for a <see cref="ParallelComponents"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the entities that should be run in parallel.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        [ParameterName("entities"), ParameterInfo("The entities that can be run in parallel.")]
        public IEntityCollection Entities { get; set; }

        /// <summary>
        /// Gets the work distributors.
        /// </summary>
        /// <value>
        /// The work distributors.
        /// </value>
        [ParameterName("workdistributors"), ParameterInfo("Workload distributors by the behavior type.")]
        public Dictionary<Type, IWorkDistributor> WorkDistributors { get; } = new Dictionary<Type, IWorkDistributor>();

        /// <summary>
        /// Sets the work distributor for a specified type.
        /// </summary>
        /// <param name="pair">The key-value pair.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pair"/> or the key is <c>null</c>.</exception>
        [ParameterName("workdistributor"), ParameterInfo("Sets a workload distributor.")]
        public void SetWorkDistributor(KeyValuePair<Type, IWorkDistributor> pair)
        {
            pair.ThrowIfNull(nameof(pair));
            pair.Key.ThrowIfNull(nameof(pair.Key));
            WorkDistributors.Add(pair.Key, pair.Value);
        }
    }
}
