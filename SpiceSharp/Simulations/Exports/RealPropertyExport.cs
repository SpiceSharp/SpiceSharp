using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real properties.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealPropertyExport : Export<IEventfulSimulation, double>
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the name of the entity in a subcircuit. Use an array of strings.
        /// </summary>
        public string[] SubcircuitEntityName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        public RealPropertyExport(IEventfulSimulation simulation, string entityName, string propertyName)
            : base(simulation)
        {
            EntityName = entityName.ThrowIfNull(nameof(entityName));
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
            SubcircuitEntityName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="subcircuitEntityName">The subcircuit list of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        public RealPropertyExport(IEventfulSimulation simulation, string[] subcircuitEntityName, string propertyName)
            : base(simulation)
        {
            if (subcircuitEntityName == null || subcircuitEntityName.Length == 0)
                throw new ArgumentException("subcircuitEntityName cannot be null or empty.", nameof(subcircuitEntityName));
            EntityName = subcircuitEntityName[0];
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
            SubcircuitEntityName = subcircuitEntityName.ThrowIfNull(nameof(subcircuitEntityName));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            e.ThrowIfNull(nameof(e));
            if (SubcircuitEntityName == null)//then using entity name
            {
                var eb = Simulation.EntityBehaviors[EntityName];
                if (eb != null)
                    Extractor = eb.CreatePropertyGetter<double>(PropertyName);
            }
            else//using subcircuit array of strings
            {
                string curComponentName = SubcircuitEntityName[0];
                System.Collections.Generic.IEnumerator<Behaviors.IBehavior> entityEnum = Simulation.EntityBehaviors[curComponentName].GetEnumerator();
                for (int i = 1; i < SubcircuitEntityName.Length; i++)
                {
                    string nextComponentName = SubcircuitEntityName[i];

                    entityEnum.MoveNext();
                    Components.Subcircuits.EntitiesBehavior behavior = (SpiceSharp.Components.Subcircuits.EntitiesBehavior)entityEnum.Current;
                    Behaviors.IBehaviorContainerCollection localBehavior = behavior.LocalBehaviors;
                    foreach (Behaviors.IBehaviorContainer bc in localBehavior)
                    {
                        if (bc.Name == nextComponentName)
                        {
                            if (i == SubcircuitEntityName.Length - 1)//if last name
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
