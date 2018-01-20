using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level3;

namespace SpiceSharp.Behaviors.Mosfet.Level3
{
    /// <summary>
    /// AC behavior for <see cref="Components.MOS3"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode, MOS3dNodePrime, MOS3sNodePrime;
        protected MatrixElement MOS3DdPtr { get; private set; }
        protected MatrixElement MOS3GgPtr { get; private set; }
        protected MatrixElement MOS3SsPtr { get; private set; }
        protected MatrixElement MOS3BbPtr { get; private set; }
        protected MatrixElement MOS3DPdpPtr { get; private set; }
        protected MatrixElement MOS3SPspPtr { get; private set; }
        protected MatrixElement MOS3DdpPtr { get; private set; }
        protected MatrixElement MOS3GbPtr { get; private set; }
        protected MatrixElement MOS3GdpPtr { get; private set; }
        protected MatrixElement MOS3GspPtr { get; private set; }
        protected MatrixElement MOS3SspPtr { get; private set; }
        protected MatrixElement MOS3BdpPtr { get; private set; }
        protected MatrixElement MOS3BspPtr { get; private set; }
        protected MatrixElement MOS3DPspPtr { get; private set; }
        protected MatrixElement MOS3DPdPtr { get; private set; }
        protected MatrixElement MOS3BgPtr { get; private set; }
        protected MatrixElement MOS3DPgPtr { get; private set; }
        protected MatrixElement MOS3SPgPtr { get; private set; }
        protected MatrixElement MOS3SPsPtr { get; private set; }
        protected MatrixElement MOS3DPbPtr { get; private set; }
        protected MatrixElement MOS3SPbPtr { get; private set; }
        protected MatrixElement MOS3SPdpPtr { get; private set; }

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
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            load = provider.GetBehavior<LoadBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS3dNode = pins[0];
            MOS3gNode = pins[1];
            MOS3sNode = pins[2];
            MOS3bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pionters
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            MOS3dNodePrime = load.MOS3dNodePrime;
            MOS3sNodePrime = load.MOS3sNodePrime;

            // Get matrix pointers
            MOS3DdPtr = matrix.GetElement(MOS3dNode, MOS3dNode);
            MOS3GgPtr = matrix.GetElement(MOS3gNode, MOS3gNode);
            MOS3SsPtr = matrix.GetElement(MOS3sNode, MOS3sNode);
            MOS3BbPtr = matrix.GetElement(MOS3bNode, MOS3bNode);
            MOS3DPdpPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNodePrime);
            MOS3SPspPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNodePrime);
            MOS3DdpPtr = matrix.GetElement(MOS3dNode, MOS3dNodePrime);
            MOS3GbPtr = matrix.GetElement(MOS3gNode, MOS3bNode);
            MOS3GdpPtr = matrix.GetElement(MOS3gNode, MOS3dNodePrime);
            MOS3GspPtr = matrix.GetElement(MOS3gNode, MOS3sNodePrime);
            MOS3SspPtr = matrix.GetElement(MOS3sNode, MOS3sNodePrime);
            MOS3BdpPtr = matrix.GetElement(MOS3bNode, MOS3dNodePrime);
            MOS3BspPtr = matrix.GetElement(MOS3bNode, MOS3sNodePrime);
            MOS3DPspPtr = matrix.GetElement(MOS3dNodePrime, MOS3sNodePrime);
            MOS3DPdPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNode);
            MOS3BgPtr = matrix.GetElement(MOS3bNode, MOS3gNode);
            MOS3DPgPtr = matrix.GetElement(MOS3dNodePrime, MOS3gNode);
            MOS3SPgPtr = matrix.GetElement(MOS3sNodePrime, MOS3gNode);
            MOS3SPsPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNode);
            MOS3DPbPtr = matrix.GetElement(MOS3dNodePrime, MOS3bNode);
            MOS3SPbPtr = matrix.GetElement(MOS3sNodePrime, MOS3bNode);
            MOS3SPdpPtr = matrix.GetElement(MOS3sNodePrime, MOS3dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS3DdPtr = null;
            MOS3GgPtr = null;
            MOS3SsPtr = null;
            MOS3BbPtr = null;
            MOS3DPdpPtr = null;
            MOS3SPspPtr = null;
            MOS3DdpPtr = null;
            MOS3GbPtr = null;
            MOS3GdpPtr = null;
            MOS3GspPtr = null;
            MOS3SspPtr = null;
            MOS3BdpPtr = null;
            MOS3BspPtr = null;
            MOS3DPspPtr = null;
            MOS3DPdPtr = null;
            MOS3BgPtr = null;
            MOS3DPgPtr = null;
            MOS3SPgPtr = null;
            MOS3SPsPtr = null;
            MOS3DPbPtr = null;
            MOS3SPbPtr = null;
            MOS3SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS3mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }

            /* 
			 * charge oriented model parameters
			 */
            EffectiveLength = bp.MOS3l - 2 * mbp.MOS3latDiff;
            GateSourceOverlapCap = mbp.MOS3gateSourceOverlapCapFactor * bp.MOS3w;
            GateDrainOverlapCap = mbp.MOS3gateDrainOverlapCapFactor * bp.MOS3w;
            GateBulkOverlapCap = mbp.MOS3gateBulkOverlapCapFactor * EffectiveLength;

            /* 
			 * meyer"s model parameters
			 */
            capgs = (state.States[0][load.MOS3states + LoadBehavior.MOS3capgs] + state.States[0][load.MOS3states + LoadBehavior.MOS3capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][load.MOS3states + LoadBehavior.MOS3capgd] + state.States[0][load.MOS3states + LoadBehavior.MOS3capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][load.MOS3states + LoadBehavior.MOS3capgb] + state.States[0][load.MOS3states + LoadBehavior.MOS3capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = load.MOS3capbd * cstate.Laplace.Imaginary;
            xbs = load.MOS3capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS3GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS3BbPtr.Add(new Complex(load.MOS3gbd + load.MOS3gbs, xgb + xbd + xbs));
            MOS3DPdpPtr.Add(new Complex(temp.MOS3drainConductance + load.MOS3gds + load.MOS3gbd + xrev * (load.MOS3gm + load.MOS3gmbs), xgd + xbd));
            MOS3SPspPtr.Add(new Complex(temp.MOS3sourceConductance + load.MOS3gds + load.MOS3gbs + xnrm * (load.MOS3gm + load.MOS3gmbs), xgs + xbs));
            MOS3GbPtr.Sub(new Complex(0.0, xgb));
            MOS3GdpPtr.Sub(new Complex(0.0, xgd));
            MOS3GspPtr.Sub(new Complex(0.0, xgs));
            MOS3BgPtr.Sub(new Complex(0.0, xgb));
            MOS3BdpPtr.Sub(new Complex(load.MOS3gbd, xbd));
            MOS3BspPtr.Sub(new Complex(load.MOS3gbs, xbs));
            MOS3DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS3gm, -xgd));
            MOS3DPbPtr.Add(new Complex(-load.MOS3gbd + (xnrm - xrev) * load.MOS3gmbs, -xbd));
            MOS3SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS3gm, xgs));
            MOS3SPbPtr.Sub(new Complex(load.MOS3gbs + (xnrm - xrev) * load.MOS3gmbs, xbs));
            MOS3DdPtr.Add(temp.MOS3drainConductance);
            MOS3SsPtr.Add(temp.MOS3sourceConductance);
            MOS3DdpPtr.Sub(temp.MOS3drainConductance);
            MOS3SspPtr.Sub(temp.MOS3sourceConductance);
            MOS3DPdPtr.Sub(temp.MOS3drainConductance);
            MOS3DPspPtr.Sub(load.MOS3gds + xnrm * (load.MOS3gm + load.MOS3gmbs));
            MOS3SPsPtr.Sub(temp.MOS3sourceConductance);
            MOS3SPdpPtr.Sub(load.MOS3gds + xrev * (load.MOS3gm + load.MOS3gmbs));
        }
    }
}
