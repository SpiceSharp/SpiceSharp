using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private InductorLoadBehavior load1, load2;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("k"), SpiceName("coefficient"), SpiceInfo("Mutual inductance", IsPrincipal = true)]
        public Parameter MUTcoupling { get; } = new Parameter();
        
        /// <summary>
        /// The factor
        /// </summary>
        public double MUTfactor { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MUTbr1br2 { get; private set; }
        protected MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Setup the mutual inductor
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var mut = component as MutualInductance;

            // Get behaviors
            load1 = GetBehavior<InductorLoadBehavior>(mut.Inductor1);
            load2 = GetBehavior<InductorLoadBehavior>(mut.Inductor2);

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MUTbr1br2 = matrix.GetElement(load1.INDbrEq, load2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(load2.INDbrEq, load1.INDbrEq);

            // Register events for loading the mutual inductance
            load1.UpdateMutualInductance += UpdateMutualInductance;
            load2.UpdateMutualInductance += UpdateMutualInductance;
            MUTfactor = MUTcoupling * Math.Sqrt(load1.INDinduct * load2.INDinduct);
            return true;
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            load1.UpdateMutualInductance -= UpdateMutualInductance;
            load2.UpdateMutualInductance -= UpdateMutualInductance;
            MUTbr1br2 = null;
            MUTbr2br1 = null;
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
        /// Update inductor 2
        /// </summary>
        /// <param name="sender">Inductor 2</param>
        /// <param name="ckt">The circuit</param>
        private void UpdateMutualInductance(InductorLoadBehavior sender, Circuit ckt)
        {
            var mut = ComponentTyped<MutualInductance>();
            var state = ckt.State;
            var rstate = ckt.State;

            if (ckt.Method != null)
            {
                if (sender == load1)
                {
                    state.States[0][load1.INDstate + InductorLoadBehavior.INDflux] += MUTfactor * rstate.Solution[load2.INDbrEq];
                    MUTbr1br2.Sub(MUTfactor * ckt.Method.Slope);
                }
                else
                {
                    state.States[0][load2.INDstate + InductorLoadBehavior.INDflux] += MUTfactor * rstate.Solution[load1.INDbrEq];
                    MUTbr2br1.Sub(MUTfactor * ckt.Method.Slope);
                }
            }
        }
    }
}
