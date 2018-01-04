using SpiceSharp.Circuits;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// Noise behavior for a <see cref="Components.DiodeModel"/>
    /// </summary>
    public class ModelNoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            DataOnly = true;
        }

        /// <summary>
        /// Noise behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            // Do nothing
        }
    }
}
