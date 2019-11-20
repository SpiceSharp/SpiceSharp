using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.NonlinearResistorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor
    /// </summary>
    /// <seealso cref="Component" />
    public class NonlinearResistor : Component
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearResistor"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeA">Node A</param>
        /// <param name="nodeB">Node B</param>
        public NonlinearResistor(string name, string nodeA, string nodeB) : base(name, 2)
        {
            // Add a NonlinearResistorBehaviors.BaseParameters
            Parameters.Add(new BaseParameters());

            // Connect the entity
            Connect(nodeA, nodeB);
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, IBehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model);
            if (simulation.EntityBehaviors.Tracks<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
