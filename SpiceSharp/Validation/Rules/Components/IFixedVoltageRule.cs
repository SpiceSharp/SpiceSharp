using SpiceSharp.Components;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that checks validity using fixed/applied voltages.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IFixedVoltageRule : IRule
    {
        /// <summary>
        /// Applies a fixed voltage between nodes.
        /// </summary>
        /// <param name="component">The component that applies a fixed voltage.</param>
        /// <param name="nodes">The nodes over which a fixed voltage is applied.</param>
        void ApplyFixedVoltage(IComponent component, params string[] nodes);
    }
}
