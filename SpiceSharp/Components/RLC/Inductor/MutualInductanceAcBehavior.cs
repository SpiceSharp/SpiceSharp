using SpiceSharp.Behaviors;
using System.Numerics;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var mut = ComponentTyped<MutualInductance>();
            var cstate = ckt.State;
            Complex value = cstate.Laplace * mut.MUTfactor;
            mut.MUTbr1br2.Sub(value);
            mut.MUTbr2br1.Sub(value);
        }
    }
}
