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
        protected override void Initialize(object sender, InitializeSimulationEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var eb = e.Behaviors.GetEntityBehaviors(EntityName);
            Func<RealState, double> stateExtractor = null;

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            Behavior behavior = eb.Get<TransientBehavior>();
            if (behavior != null)
                stateExtractor = behavior.CreateExport(PropertyName);

            // 2) Second, try the load behavior
            if (stateExtractor == null)
            {
                behavior = eb.Get<LoadBehavior>();
                if (behavior != null)
                    stateExtractor = behavior.CreateExport(PropertyName);
            }

            // 3) Thirdly, check temperature behavior
            if (stateExtractor == null)
            {
                behavior = eb.Get<TemperatureBehavior>();
                if (behavior != null)
                    stateExtractor = behavior.CreateExport(PropertyName);
            }

            // Create the extractor
            if (stateExtractor != null)
            {
                var state = Simulation.States.Get<RealState>();
                Extractor = () => stateExtractor(state);
            }
        }
    }
}
