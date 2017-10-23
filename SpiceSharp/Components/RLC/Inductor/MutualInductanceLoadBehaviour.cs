using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Setup the mutual inductor
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mut = ComponentTyped<MutualInductance>();

            // Find the inductors
            mut.Inductor1 = ckt.Objects[mut.MUTind1] as Inductor ?? throw new CircuitException($"{mut.Name}: Could not find inductor '{mut.MUTind1}'");
            mut.Inductor2 = ckt.Objects[mut.MUTind2] as Inductor ?? throw new CircuitException($"{mut.Name}: Could not find inductor '{mut.MUTind2}'");
            mut.Inductor1.UpdateMutualInductance += UpdateMutualInductance;
            mut.Inductor2.UpdateMutualInductance += UpdateMutualInductance;
            mut.MUTfactor = mut.MUTcoupling * Math.Sqrt(mut.Inductor1.INDinduct * mut.Inductor2.INDinduct);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Update inductor 2
        /// </summary>
        /// <param name="sender">Inductor 2</param>
        /// <param name="ckt">The circuit</param>
        private void UpdateMutualInductance(Inductor sender, Circuit ckt)
        {
            var mut = ComponentTyped<MutualInductance>();
            var state = ckt.State;
            var rstate = ckt.State.Real;

            if (sender == mut.Inductor1)
            {
                state.States[0][mut.Inductor1.INDstate + Inductor.INDflux] += mut.MUTfactor * rstate.OldSolution[mut.Inductor2.INDbrEq];
                rstate.Matrix[mut.Inductor1.INDbrEq, mut.Inductor2.INDbrEq] -= mut.MUTfactor * ckt.Method.Slope;
            }
            else
            {
                state.States[0][mut.Inductor2.INDstate + Inductor.INDflux] += mut.MUTfactor * rstate.OldSolution[mut.Inductor1.INDbrEq];
                rstate.Matrix[mut.Inductor2.INDbrEq, mut.Inductor2.INDbrEq] -= mut.MUTfactor * ckt.Method.Slope;
            }
        }
    }
}
