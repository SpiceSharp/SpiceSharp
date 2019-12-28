using SpiceSharp.Components;

namespace SpiceSharp.Validation.Components
{
    /// <summary>
    /// Describes a rule for an <see cref="IComponent"/>.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IComponentRule : IRule
    {
        /// <summary>
        /// Applies the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        void Apply(IComponent component);
    }
}
