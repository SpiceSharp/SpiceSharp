using SpiceSharp.Behaviors;
using SpiceSharp.Components.NonlinearResistorBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor.
    /// </summary>
    /// <seealso cref="Component" />
    public class NonlinearResistor : Component,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearResistor"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <param name="nodeA">Node A</param>
        /// <param name="nodeB">Node B</param>
        public NonlinearResistor(string name, string nodeA, string nodeB) : base(name, 2)
        {
            Connect(nodeA, nodeB);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors);
            if (simulation.UsesBehaviors<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
