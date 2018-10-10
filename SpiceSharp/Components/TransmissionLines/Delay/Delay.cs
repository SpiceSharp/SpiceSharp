namespace SpiceSharp.Components
{
    /// <summary>
    /// This component can delay a voltage signal for a specified time. The delay
    /// only works unidirectional.
    /// </summary>
    /// <seealso cref="Component" />
    public class Delay : Component
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Delay"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public Delay(string name)
            : base(name, 2)
        {
        }
    }
}
