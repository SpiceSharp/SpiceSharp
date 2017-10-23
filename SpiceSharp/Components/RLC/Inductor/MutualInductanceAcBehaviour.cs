using SpiceSharp.Behaviours;
using System.Numerics;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mut = ComponentTyped<MutualInductance>();
            var cstate = ckt.State.Complex;
            Complex value = cstate.Laplace * mut.MUTfactor;
            cstate.Matrix[mut.Inductor1.INDbrEq, mut.Inductor2.INDbrEq] -= value;
            cstate.Matrix[mut.Inductor2.INDbrEq, mut.Inductor1.INDbrEq] -= value;
        }
    }
}
