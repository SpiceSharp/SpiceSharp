using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Inductor"/>
    /// </summary>
    public class InductorAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var ind = ComponentTyped<Inductor>();
            var cstate = ckt.State;
            Complex val = cstate.Laplace * ind.INDinduct.Value;

            ind.INDposIbrptr.Add(1.0);
            ind.INDnegIbrptr.Sub(1.0);
            ind.INDibrNegptr.Sub(1.0);
            ind.INDibrPosptr.Add(1.0);
            ind.INDibrIbrptr.Sub(val);
        }
    }
}
