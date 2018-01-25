using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.MUT;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.MUT
{
    /// <summary>
    /// AC behavior for <see cref="Components.MutualInductance"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        IND.LoadBehavior load1, load2;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MUTbr1br2 { get; private set; }
        protected MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double MUTfactor { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            var bp1 = provider.GetParameterSet<Components.IND.BaseParameters>(1);
            var bp2 = provider.GetParameterSet<Components.IND.BaseParameters>(2);

            // Get behaviors
            load1 = provider.GetBehavior<IND.LoadBehavior>(1);
            load2 = provider.GetBehavior<IND.LoadBehavior>(2);

            // Calculate coupling factor
            MUTfactor = bp.MUTcoupling * Math.Sqrt(bp1.INDinduct * bp2.INDinduct);
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            int INDbrEq1 = load1.INDbrEq;
            int INDbrEq2 = load2.INDbrEq;

            // Get matrix equations
            MUTbr1br2 = matrix.GetElement(INDbrEq1, INDbrEq2);
            MUTbr2br1 = matrix.GetElement(INDbrEq2, INDbrEq1);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            MUTbr1br2 = null;
            MUTbr2br1 = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            var state = sim.State;
            Complex value = state.Laplace * MUTfactor;
            MUTbr1br2.Sub(value);
            MUTbr2br1.Sub(value);
        }
    }
}
