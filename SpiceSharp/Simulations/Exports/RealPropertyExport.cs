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
            e.ThrowIfNull(nameof(e));
            if (EntityPath.Count == 1)//then path is entity name
            {
                var eb = Simulation.EntityBehaviors[EntityPath[0]];
                if (eb != null)
                    Extractor = eb.CreatePropertyGetter<double>(PropertyName);
            }
            else//using subcircuit array of strings
            {
                string curComponentName = EntityPath[0];
                System.Collections.Generic.IEnumerator<Behaviors.IBehavior> entityEnum = Simulation.EntityBehaviors[curComponentName].GetEnumerator();
                for (int i = 1; i < EntityPath.Count; i++)
                {
                    string nextComponentName = EntityPath[i];

                    entityEnum.MoveNext();
                    Components.Subcircuits.EntitiesBehavior behavior = (SpiceSharp.Components.Subcircuits.EntitiesBehavior)entityEnum.Current;
                    Behaviors.IBehaviorContainerCollection localBehavior = behavior.LocalBehaviors;
                    foreach (Behaviors.IBehaviorContainer bc in localBehavior)
                    {
                        if (bc.Name == nextComponentName)
                        {
                            if (i == EntityPath.Count - 1)//if last name
                            {
                                Extractor = bc.CreatePropertyGetter<double>(PropertyName);
                                return;
                            }
                            else//move to the next subcircuit
                            {
                                entityEnum = bc.GetEnumerator();
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
