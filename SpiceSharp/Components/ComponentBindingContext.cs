using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A binding context for a <see cref="Component"/>.
    /// </summary>
    public class ComponentBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the pins the component is connected to.
        /// </summary>
        public int[] Pins { get; private set; } = new int[0];

        /// <summary>
        /// Apply the indices that the pins of the component are connected to.
        /// </summary>
        /// <param name="pins">The pins.</param>
        public void Connect(params int[] pins)
        {
            if (pins == null || pins.Length == 0)
            {
                Pins = new int[0];
                return;
            }
            Pins = new int[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                Pins[i] = pins[i];
        }
    }
}
