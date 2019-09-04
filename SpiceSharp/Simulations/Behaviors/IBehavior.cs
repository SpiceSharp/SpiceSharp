using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Contract for a behavior.
    /// </summary>
    public interface IBehavior : IParameterSet
    {
        /// <summary>
        /// Gets the parameters of the behavior.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        ParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Bind the behavior to the specified simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        void Bind(BindingContext context);

        /// <summary>
        /// Unbind the behavior from any allocated resources.
        /// </summary>
        void Unbind();
    }
}
