using SpiceSharp.Entities.Local;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A simulation look-alike to use proxies.
    /// </summary>
    /// <seealso cref="Simulation" />
    public class SubcircuitSimulation : LocalSimulation
    {
        /// <summary>
        /// Gets the task index that the simulation will run in.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public int Task { get; }

        /// <summary>
        /// Gets the total number of tasks that will be running.
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public int Tasks { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the subcircuit simulation.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="nodemap">The structure that maps the internal to external nodes.</param>
        /// <param name="task">The task index that the simulation will be used in.</param>
        /// <param name="tasks">The total number of tasks that will be run.</param>
        public SubcircuitSimulation(string name, ISimulation parent, Dictionary<string, string> nodemap, int task, int tasks)
            : base(parent)
        {
            Name = name.ThrowIfNull(nameof(name));
            Task = task;
            Tasks = tasks;
            Variables = new SubcircuitVariableSet(name, parent.Variables);
            EntityBehaviors = new LocalBehaviorContainerCollection(parent.EntityBehaviors);
            States = new TypeDictionary<ISimulationState>();

            // Alias all pins to local nodes to allow retrieving them
            foreach (var pair in nodemap)
            {
                // Map the external node (may not have been used yet by other entities)
                parent.Variables.MapNode(pair.Value, VariableType.Voltage);
                if (!Variables.ContainsNode(pair.Key))
                    Variables.AliasNode(pair.Value, pair.Key);
            }
        }
    }
}
