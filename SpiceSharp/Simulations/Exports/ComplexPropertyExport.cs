using System;
using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    public class ComplexPropertyExport : Export<Complex>
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
        /// <param name="entityName">Entity name</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="simulation">Simulation</param>
        public ComplexPropertyExport(Identifier entityName, string propertyName, Simulation simulation)
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
        protected override void Initialize(object sender, InitializationDataEventArgs e)
        {
            var eb = e.Behaviors.GetEntityBehaviors(EntityName);
            Func<RealState, double> stateExtractor = null;

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            Behavior behavior = eb.Get<FrequencyBehavior>();
            if (behavior != null)
                stateExtractor = behavior.CreateExport(PropertyName);
            
            // Create the extractor
            if (stateExtractor != null)
            {
                var state = Simulation.States.Get<RealState>();
                Extractor = () => stateExtractor(state);
            }
        }
    }
}
