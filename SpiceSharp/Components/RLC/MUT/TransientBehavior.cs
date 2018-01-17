using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.MUT;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.MUT
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.MutualInductance"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        IND.LoadBehavior load1, load2;

        /// <summary>
        /// The factor
        /// </summary>
        public double MUTfactor { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement MUTbr1br2 { get; private set; }
        protected MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            var bp1 = provider.GetParameters<Components.IND.BaseParameters>(1);
            var bp2 = provider.GetParameters<Components.IND.BaseParameters>(2);

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

            // Get matrix pointers
            MUTbr1br2 = matrix.GetElement(INDbrEq1, INDbrEq2);
            MUTbr2br1 = matrix.GetElement(INDbrEq2, INDbrEq1);
        }

        /// <summary>
        /// Unsetup behavior
        /// </summary>
        public override void Unsetup()
        {
            MUTbr1br2 = null;
            MUTbr2br1 = null;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
        }
    }
}
