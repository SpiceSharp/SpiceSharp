using System;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.MUT
{
    /// <summary>
    /// General behavior for <see cref="MutualInductance"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private IND.TransientBehavior load1, load2;

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
        /// Constructor
        /// </summary>
        public LoadBehavior()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="coupling">Mutual inductance</param>
        public LoadBehavior(double coupling)
        {
            MUTcoupling.Set(coupling);
        }

        /// <summary>
        /// Setup the mutual inductor
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var mut = component as MutualInductance;

            // Get behaviors
            load1 = GetBehavior<IND.TransientBehavior>(mut.Inductor1);
            load2 = GetBehavior<IND.TransientBehavior>(mut.Inductor2);

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MUTbr1br2 = matrix.GetElement(load1.INDbrEq, load2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(load2.INDbrEq, load1.INDbrEq);

            // Register events for loading the mutual inductance
            load1.UpdateMutualInductance += UpdateMutualInductance;
            load2.UpdateMutualInductance += UpdateMutualInductance;
            MUTfactor = MUTcoupling * Math.Sqrt(load1.INDinduct * load2.INDinduct);
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
        /// Execute behavior
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
        private void UpdateMutualInductance(IND.TransientBehavior sender, Circuit ckt)
        {
            var state = ckt.State;
            var rstate = ckt.State;

            if (ckt.Method != null)
            {
                if (sender == load1)
                {
                    state.States[0][load1.INDstate + IND.TransientBehavior.INDflux] += MUTfactor * rstate.Solution[load2.INDbrEq];
                    MUTbr1br2.Sub(MUTfactor * ckt.Method.Slope);
                }
                else
                {
                    state.States[0][load2.INDstate + IND.TransientBehavior.INDflux] += MUTfactor * rstate.Solution[load1.INDbrEq];
                    MUTbr2br1.Sub(MUTfactor * ckt.Method.Slope);
                }
            }
        }
    }
}
