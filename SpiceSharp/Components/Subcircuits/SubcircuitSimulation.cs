using SpiceSharp.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A simulation look-alike to use proxies.
    /// </summary>
    /// <seealso cref="Simulation" />
    public class SubcircuitSimulation : LocalSimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the subcircuit simulation.</param>
        /// <param name="parent">The parent simulation.</param>
        public SubcircuitSimulation(string name, ISimulation parent)
            : base(parent)
        {
            Name = name.ThrowIfNull(nameof(name));
            Variables = new SubcircuitVariableSet(name, parent.Variables);
            EntityBehaviors = new LocalBehaviorContainerCollection(parent.EntityBehaviors, parent);
            States = new TypeDictionary<ISimulationState>();
        }
    }
}
