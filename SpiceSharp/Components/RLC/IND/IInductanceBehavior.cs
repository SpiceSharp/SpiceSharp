using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An interface that describes a behavior that defines an inductance.
    /// </summary>
    /// <seealso cref="IBehavior" />
    public interface IInductanceBehavior : IBehavior
    {
        /// <summary>
        /// Gets the inductance of the inductor.
        /// </summary>
        /// <value>
        /// The inductance.
        /// </value>
        double Inductance { get; }
    }
}
