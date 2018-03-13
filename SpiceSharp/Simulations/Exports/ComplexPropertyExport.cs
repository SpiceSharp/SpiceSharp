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
        /// <param name="simulation">Simulation</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="propertyName">Property name</param>
        public ComplexPropertyExport(Simulation simulation, Identifier entityName, string propertyName)
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

            var eb = simulation.Behaviors.GetEntityBehaviors(EntityName);

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            var behavior = eb.Get<BaseFrequencyBehavior>();
            if (behavior != null)
                Extractor = behavior.CreateAcExport(Simulation, PropertyName);
        }
    }
}
