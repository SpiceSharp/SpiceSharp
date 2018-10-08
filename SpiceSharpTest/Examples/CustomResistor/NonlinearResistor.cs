using SpiceSharp.Components.NonlinearResistorBehaviors;

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
            ParameterSets.Add(new BaseParameters());

            // Add a NonlinearResistorBehaviors.LoadBehavior factory
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));

            // Connect the entity
            Connect(nodeA, nodeB);
        }
    }
}
