using SpiceSharp.Validation;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An interface that describes a subcircuit definition that can be validated.
    /// </summary>
    public interface ISubcircuitValidator : ISubcircuitDefinition
    {
        /// <summary>
        /// Validates the subcircuit definition.
        /// </summary>
        /// <param name="subckt">The subcircuit entity that needs to validated.</param>
        /// <param name="nodes">The nodes that the subcircuit definition is connected to.</param>
        /// <param name="container">The rule container.</param>
        void Validate(Subcircuit subckt, string[] nodes, IRuleContainer container);
    }
}
