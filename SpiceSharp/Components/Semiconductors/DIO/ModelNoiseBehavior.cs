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
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("flicker noise coefficient")]
        public Parameter DIOfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("flicker noise exponent")]
        public Parameter DIOfNexp { get; } = new Parameter(1.0);

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
