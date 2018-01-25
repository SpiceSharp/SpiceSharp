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
        IND.TransientBehavior tran1, tran2;

        /// <summary>
        /// The factor
        /// </summary>
        public double MUTfactor { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int INDbrEq1, INDbrEq2;
        protected MatrixElement MUTbr1br2 { get; private set; }
        protected MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Y-matrix contribution
        /// </summary>
        protected double geq;

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
            bp = provider.GetParameterSet<BaseParameters>(0);
            var bp1 = provider.GetParameterSet<Components.IND.BaseParameters>(1);
            var bp2 = provider.GetParameterSet<Components.IND.BaseParameters>(2);

            // Get behaviors
            load1 = provider.GetBehavior<IND.LoadBehavior>(1);
            load2 = provider.GetBehavior<IND.LoadBehavior>(2);
            tran1 = provider.GetBehavior<IND.TransientBehavior>(1);
            tran2 = provider.GetBehavior<IND.TransientBehavior>(2);

            // Calculate coupling factor
            MUTfactor = bp.MUTcoupling * Math.Sqrt(bp1.INDinduct * bp2.INDinduct);

            // Register events for modifying the flux through the inductors
            tran1.UpdateFlux += UpdateFlux1;
            tran2.UpdateFlux += UpdateFlux2;
        }

        /// <summary>
        /// Update the flux through inductor 2
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        void UpdateFlux2(object sender, IND.UpdateFluxEventArgs args)
        {
            var state = args.State;
            args.Flux.Value += MUTfactor * state.Solution[load1.INDbrEq];
        }

        /// <summary>
        /// Update the flux through inductor 1
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        void UpdateFlux1(object sender, IND.UpdateFluxEventArgs args)
        {
            var state = args.State;
            geq = args.Flux.Jacobian(MUTfactor);
            args.Flux.Value += MUTfactor * state.Solution[load2.INDbrEq];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            INDbrEq1 = load1.INDbrEq;
            INDbrEq2 = load2.INDbrEq;

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

            // Remove events
            tran1.UpdateFlux -= UpdateFlux1;
            tran2.UpdateFlux -= UpdateFlux2;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            // Load Y-matrix
            MUTbr1br2.Sub(geq);
            MUTbr2br1.Sub(geq);
        }
    }
}
