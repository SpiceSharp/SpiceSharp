using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// Noise behavior for a <see cref="Components.MOS1Model"/>
    /// </summary>
    public class ModelNoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Components</param>
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
