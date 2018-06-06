using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Export for real properties
    /// </summary>
    public class RealPropertyExport : Export<double>
    {
        /// <summary>
        /// Gets the name of the entity
        /// </summary>
        public Identifier EntityName { get; }

        /// <summary>
        /// Gets the property name
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="propertyName">Property name</param>
        public RealPropertyExport(Simulation simulation, Identifier entityName, string propertyName)
            : base(simulation)
        {
            EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            var simulation = (Simulation) sender;
            var eb = simulation.EntityBehaviors.GetEntityBehaviors(EntityName);

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            if (eb.TryGetValue(typeof(BaseTransientBehavior), out var behavior))
                Extractor = behavior.CreateExport(Simulation, PropertyName);

            // 2) Second, try the load behavior
            if (Extractor == null)
            {
                if (eb.TryGetValue(typeof(BaseLoadBehavior), out behavior))
                    Extractor = behavior.CreateExport(Simulation, PropertyName);
            }

            // 3) Thirdly, check temperature behavior
            if (Extractor == null)
            {
                if (eb.TryGetValue(typeof(BaseTemperatureBehavior), out behavior))
                    Extractor = behavior.CreateExport(Simulation, PropertyName);
            }

            // 4) Check parameter sets
            if (Extractor == null)
            {
                // Get all parameter sets associated with the entity
                var ps = simulation.EntityParameters.GetEntityParameters(EntityName);
                foreach (var p in ps.Values)
                {
                    Extractor = p.GetGetter(PropertyName);
                    if (Extractor != null)
                        break;
                }
            }
        }
    }
}
