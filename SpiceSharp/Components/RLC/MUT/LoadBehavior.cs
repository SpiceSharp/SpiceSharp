using System;
using SpiceSharp.Components.MUT;
using SpiceSharp.Circuits;
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
        BaseParameters bp;
        Components.IND.BaseParameters bp1, bp2;
        IND.LoadBehavior load1, load2;

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
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            bp1 = provider.GetParameters<Components.IND.BaseParameters>(1);
            bp2 = provider.GetParameters<Components.IND.BaseParameters>(2);

            // Get behaviors
            load1 = provider.GetBehavior<IND.LoadBehavior>(1);
            load2 = provider.GetBehavior<IND.LoadBehavior>(2);

            // Register events for loading the mutual inductance
            // load1.UpdateMutualInductance += UpdateMutualInductance;
            // load2.UpdateMutualInductance += UpdateMutualInductance;
            MUTfactor = bp.MUTcoupling * Math.Sqrt(bp1.INDinduct * bp2.INDinduct);
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Get matrix pointers
            MUTbr1br2 = matrix.GetElement(load1.INDbrEq, load2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(load2.INDbrEq, load1.INDbrEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            // load1.UpdateMutualInductance -= UpdateMutualInductance;
            // load2.UpdateMutualInductance -= UpdateMutualInductance;
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
        /*
        /// <summary>
        /// Update inductor 2
        /// </summary>
        /// <param name="sender">Inductor 2</param>
        /// <param name="ckt">The circuit</param>
        void UpdateMutualInductance(IND.TransientBehavior sender, Circuit ckt)
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
        */
    }
}
