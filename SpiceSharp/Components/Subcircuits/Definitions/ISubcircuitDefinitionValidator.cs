using SpiceSharp.Validation;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Describes a subcircuit that is also a validator.
    /// </summary>
    /// <seealso cref="IValidator" />
    public interface ISubcircuitDefinitionValidator : ISubcircuitDefinition
    {
        /// <summary>
        /// Gets a value indicating whether this instance has ground node.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has ground node; otherwise, <c>false</c>.
        /// </value>
        bool HasGroundNode { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has independent source.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has independent source; otherwise, <c>false</c>.
        /// </value>
        bool HasIndependentSource { get; }

        /// <summary>
        /// Determines whether the specified pins are connected by an equivalent conductor.
        /// </summary>
        /// <param name="pin1">The first pin.</param>
        /// <param name="pin2">The second pin.</param>
        /// <returns>
        ///   <c>true</c> if the specified pin1 is conductive; otherwise, <c>false</c>.
        /// </returns>
        bool IsConductive(int pin1, int pin2);

        /// <summary>
        /// Validates the subcircuit definition.
        /// </summary>
        void Validate();
    }
}
