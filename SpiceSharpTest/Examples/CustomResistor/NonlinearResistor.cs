using SpiceSharp.Behaviors;
using SpiceSharp.Components.NonlinearResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A nonlinear resistor
    /// </summary>
    /// <seealso cref="Component" />
    public class NonlinearResistor : Component
    {
        static NonlinearResistor()
        {
            RegisterBehaviorFactory(typeof(NonlinearResistor), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)}
            });
        }

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

            // Connect the entity
            Connect(nodeA, nodeB);
        }
    }
}
