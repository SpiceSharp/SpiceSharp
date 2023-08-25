using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real properties.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealPropertyExport : Export<IEventfulSimulation, double>
    {
        /// <summary>
        /// Gets the path to the name of the entity.
        /// </summary>
        public IReadOnlyList<string> EntityPath { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityPath">The path to the name of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        public RealPropertyExport(IEventfulSimulation simulation, string entityPath, string propertyName)
            : base(simulation)
        {
            entityPath.ThrowIfNull(nameof(entityPath));
            EntityPath = new List<string> { entityPath };
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityPath">The subcircuit list of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        public RealPropertyExport(IEventfulSimulation simulation, string[] entityPath, string propertyName)
            : base(simulation)
        {
            if (entityPath == null || entityPath.Length == 0)
                throw new ArgumentNullException("entityPath cannot be null or empty.", nameof(entityPath));
            EntityPath = new List<string>(entityPath);
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            // Collect all behaviors in the most top-level entity
            var behaviors = new HashSet<IBehavior>();
            foreach (var behavior in Simulation.EntityBehaviors[EntityPath[0]])
                behaviors.Add(behavior);

            // For every subsequent name in the path, we need to collect the next set of behaviors
            for (int i = 1; i < EntityPath.Count; i++)
            {
                string nextComponentName = EntityPath[i];
                // Keep track of the behaviors one level deeper
                var subBehaviors = new HashSet<IBehavior>();

                // Go through all the behaviors of the previously found level, and collect the new set of behaviors
                foreach (var behavior in behaviors)
                {
                    if (behavior is SpiceSharp.Components.Subcircuits.EntitiesBehavior subcktBehavior)
                    {
                        // Add all the behaviors in this one to the new level of found behaviors
                        if (subcktBehavior.LocalBehaviors.TryGetBehaviors(nextComponentName, out IBehaviorContainer behaviorContainer))
                        {
                            foreach (var subBehavior in behaviorContainer)
                            {
                                subBehaviors.Add(subBehavior);
                            }
                        }
                    }
                }

                // If we didn't find any new behaviors, then that means that the last level was not a subcircuit
                if (subBehaviors.Count == 0)
                    throw new SpiceSharpException($"Entity {EntityPath[i - 1]} is not a subcircuit.");
                behaviors = subBehaviors; // Completed this level
            }

            // We have found all the behaviors for the relevant entity path, let's now find the property
            // This code is basically identical to how it is implemented for BehaviorContainer
            foreach (var behavior in behaviors)
            {
                Extractor = behavior.CreatePropertyGetter<double>(PropertyName);
                if (Extractor != null)
                    return; // Success
            }

            // If we reached this part, then none of the behaviors have defined the property...
            throw new ParameterNotFoundException(this, PropertyName, typeof(double));
        }
    }
}
