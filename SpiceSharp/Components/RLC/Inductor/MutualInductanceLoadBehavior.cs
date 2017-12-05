using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceLoadBehavior : CircuitObjectBehaviorLoad
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

            // Register events for loading the mutual inductance
            mut.Inductor1.UpdateMutualInductance += UpdateMutualInductance;
            mut.Inductor2.UpdateMutualInductance += UpdateMutualInductance;
            mut.MUTfactor = mut.MUTcoupling * Math.Sqrt(mut.Inductor1.INDinduct * mut.Inductor2.INDinduct);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup the behaviour
        /// </summary>
        public override void Unsetup()
        {
            var mut = ComponentTyped<MutualInductance>();
            mut.Inductor1.UpdateMutualInductance -= UpdateMutualInductance;
            mut.Inductor2.UpdateMutualInductance -= UpdateMutualInductance;
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
            var rstate = ckt.State;

            if (ckt.Method != null)
            {
                if (sender == mut.Inductor1)
                {
                    state.States[0][mut.Inductor1.INDstate + Inductor.INDflux] += mut.MUTfactor * rstate.Solution[mut.Inductor2.INDbrEq];
                    mut.MUTbr1br2.Sub(mut.MUTfactor * ckt.Method.Slope);
                }
                else
                {
                    state.States[0][mut.Inductor2.INDstate + Inductor.INDflux] += mut.MUTfactor * rstate.Solution[mut.Inductor1.INDbrEq];
                    mut.MUTbr2br1.Sub(mut.MUTfactor * ckt.Method.Slope);
                }
            }
        }
    }
}
