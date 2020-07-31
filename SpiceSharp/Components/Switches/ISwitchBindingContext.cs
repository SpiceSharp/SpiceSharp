using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Describes a binding context for switches.
    /// </summary>
    /// <seealso cref="IComponentBindingContext" />
    public interface ISwitchBindingContext : IComponentBindingContext
    {
        /// <summary>
        /// Gets the function that can return the controlling value.
        /// </summary>
        /// <value>
        /// The controlling value.
        /// </value>
        Func<double> ControlValue { get; }
    }
}
