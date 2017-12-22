using SpiceSharp.Circuits;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors.MOS3
{
    /// <summary>
    /// Noise behavior for a <see cref="Components.MOS3Model"/>
    /// </summary>
    public class ModelNoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS3fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS3fNexp { get; } = new Parameter(1);

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
