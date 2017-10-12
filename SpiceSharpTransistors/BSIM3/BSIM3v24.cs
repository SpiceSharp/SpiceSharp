using System;
using System.IO;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM3v24 : CircuitComponent<BSIM3v24>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM3v24Model model) => Model = model;
		
        /// <summary>
        /// mysize dependent parameters
        /// </summary>
        private BSIM3SizeDependParam pParam = null;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter BSIM3w { get; } = new Parameter(5.0e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter BSIM3l { get; } = new Parameter(5.0e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter BSIM3sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter BSIM3drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter BSIM3sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter BSIM3drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter BSIM3sourceSquares { get; } = new Parameter(1.0);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter BSIM3drainSquares { get; } = new Parameter(1.0);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool BSIM3off { get; set; }
        [SpiceName("nqsmod"), SpiceInfo("Non-quasi-static model selector")]
        public Parameter BSIM3nqsMod { get; } = new Parameter();
        [SpiceName("id"), SpiceInfo("Ids")]
        public double BSIM3cd { get; private set; }
        [SpiceName("gm"), SpiceInfo("Gm")]
        public double BSIM3gm { get; private set; }
        [SpiceName("gds"), SpiceInfo("Gds")]
        public double BSIM3gds { get; private set; }
        [SpiceName("gmbs"), SpiceInfo("Gmb")]
        public double BSIM3gmbs { get; private set; }
        [SpiceName("vth"), SpiceInfo("Vth")]
        public double BSIM3von { get; private set; }
        [SpiceName("vdsat"), SpiceInfo("Vdsat")]
        public double BSIM3vdsat { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: BSIM3icVBS.Set(value[2]); goto case 2;
                case 2: BSIM3icVGS.Set(value[1]); goto case 1;
                case 1: BSIM3icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("vbs"), SpiceInfo("Vbs")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vbs];
        [SpiceName("vgs"), SpiceInfo("Vgs")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vgs];
        [SpiceName("vds"), SpiceInfo("Vds")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vds];

        /// <summary>
        /// Extra variables
        /// </summary>
        public Parameter BSIM3icVBS { get; } = new Parameter();
        public Parameter BSIM3icVDS { get; } = new Parameter();
        public Parameter BSIM3icVGS { get; } = new Parameter();
        public double BSIM3drainConductance { get; private set; }
        public double BSIM3sourceConductance { get; private set; }
        public double BSIM3cgso { get; private set; }
        public double BSIM3cgdo { get; private set; }
        public double BSIM3vjsm { get; private set; }
        public double BSIM3IsEvjsm { get; private set; }
        public double BSIM3vjdm { get; private set; }
        public double BSIM3IsEvjdm { get; private set; }
        public double BSIM3mode { get; private set; }
        public double BSIM3csub { get; private set; }
        public double BSIM3cbd { get; private set; }
        public double BSIM3gbd { get; private set; }
        public double BSIM3gbbs { get; private set; }
        public double BSIM3gbgs { get; private set; }
        public double BSIM3gbds { get; private set; }
        public double BSIM3cbs { get; private set; }
        public double BSIM3gbs { get; private set; }
        public double BSIM3thetavth { get; private set; }
        public double BSIM3Vgsteff { get; private set; }
        public double BSIM3rds { get; private set; }
        public double BSIM3Abulk { get; private set; }
        public double BSIM3ueff { get; private set; }
        public double BSIM3AbovVgst2Vtm { get; private set; }
        public double BSIM3Vdseff { get; private set; }
        public double BSIM3qinv { get; private set; }
        public double BSIM3cggb { get; private set; }
        public double BSIM3cgsb { get; private set; }
        public double BSIM3cgdb { get; private set; }
        public double BSIM3cdgb { get; private set; }
        public double BSIM3cdsb { get; private set; }
        public double BSIM3cddb { get; private set; }
        public double BSIM3cbgb { get; private set; }
        public double BSIM3cbsb { get; private set; }
        public double BSIM3cbdb { get; private set; }
        public double BSIM3cqdb { get; private set; }
        public double BSIM3cqsb { get; private set; }
        public double BSIM3cqgb { get; private set; }
        public double BSIM3cqbb { get; private set; }
        public double BSIM3gtau { get; private set; }
        public double BSIM3qgate { get; private set; }
        public double BSIM3qbulk { get; private set; }
        public double BSIM3qdrn { get; private set; }
        public double BSIM3capbs { get; private set; }
        public double BSIM3capbd { get; private set; }
        public double BSIM3gtg { get; private set; }
        public double BSIM3gtd { get; private set; }
        public double BSIM3gts { get; private set; }
        public double BSIM3gtb { get; private set; }
        public int BSIM3dNode { get; private set; }
        public int BSIM3gNode { get; private set; }
        public int BSIM3sNode { get; private set; }
        public int BSIM3bNode { get; private set; }
        public int BSIM3dNodePrime { get; private set; }
        public int BSIM3sNodePrime { get; private set; }
        public int BSIM3qNode { get; private set; }
        public int BSIM3states { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int BSIM3vbd = 0;
        private const int BSIM3vbs = 1;
        private const int BSIM3vgs = 2;
        private const int BSIM3vds = 3;
        private const int BSIM3qb = 4;
        private const int BSIM3cqb = 5;
        private const int BSIM3qg = 6;
        private const int BSIM3cqg = 7;
        private const int BSIM3qd = 8;
        private const int BSIM3cqd = 9;
        private const int BSIM3qbs = 10;
        private const int BSIM3qbd = 11;
        private const int BSIM3qcheq = 12;
        private const int BSIM3cqcheq = 13;
        private const int BSIM3qcdump = 14;
        private const int BSIM3cqcdump = 15;
        private const int BSIM3qdef = 16;

        private const double ScalingFactor = 1e-9;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM3v24(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM3v24Model;
            pParam = null;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            BSIM3dNode = nodes[0].Index;
            BSIM3gNode = nodes[1].Index;
            BSIM3sNode = nodes[2].Index;
            BSIM3bNode = nodes[3].Index;

            // Allocate states
            BSIM3states = ckt.State.GetState(17);

            /* process drain series resistance */
            if ((model.BSIM3sheetResistance > 0.0) && (BSIM3drainSquares > 0.0))
                BSIM3dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                BSIM3dNodePrime = BSIM3dNode;

            /* process source series resistance */
            if ((model.BSIM3sheetResistance > 0.0) && (BSIM3sourceSquares > 0.0))
                BSIM3sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                BSIM3sNodePrime = BSIM3sNode;

            /* internal charge node */

            if (BSIM3nqsMod != 0)
                BSIM3qNode = CreateNode(ckt, Name.Grow("#nqs")).Index;
            else
                BSIM3qNode = 0;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as BSIM3v24Model;

            double Ldrn, Wdrn, T0, T1, tmp1, tmp2, T2, T3, Inv_L, Inv_W, Inv_LW, tmp, T4, T5, tmp3,
                Nvtm, SourceSatCurrent, DrainSatCurrent;

            Tuple<double, double> mysize = new Tuple<double, double>(BSIM3w, BSIM3l);
            if (model.Sizes.ContainsKey(mysize))
                pParam = model.Sizes[mysize];
            else
            {
                pParam = new BSIM3SizeDependParam();
                model.Sizes.Add(mysize, pParam);

                Ldrn = BSIM3l;
                Wdrn = BSIM3w;

                T0 = Math.Pow(Ldrn, model.BSIM3Lln);
                T1 = Math.Pow(Wdrn, model.BSIM3Lwn);
                tmp1 = model.BSIM3Ll / T0 + model.BSIM3Lw / T1 + model.BSIM3Lwl / (T0 * T1);
                pParam.BSIM3dl = model.BSIM3Lint + tmp1;
                tmp2 = model.BSIM3Llc / T0 + model.BSIM3Lwc / T1 + model.BSIM3Lwlc / (T0 * T1);
                pParam.BSIM3dlc = model.BSIM3dlc + tmp2;

                T2 = Math.Pow(Ldrn, model.BSIM3Wln);
                T3 = Math.Pow(Wdrn, model.BSIM3Wwn);
                tmp1 = model.BSIM3Wl / T2 + model.BSIM3Ww / T3 + model.BSIM3Wwl / (T2 * T3);
                pParam.BSIM3dw = model.BSIM3Wint + tmp1;
                tmp2 = model.BSIM3Wlc / T2 + model.BSIM3Wwc / T3 + model.BSIM3Wwlc / (T2 * T3);
                pParam.BSIM3dwc = model.BSIM3dwc + tmp2;

                pParam.BSIM3leff = BSIM3l - 2.0 * pParam.BSIM3dl;
                if (pParam.BSIM3leff <= 0.0)
                    throw new CircuitException($"BSIM3v24: mosfet {Name}, model {model.Name}: Effective channel length <= 0");

                pParam.BSIM3weff = BSIM3w - 2.0 * pParam.BSIM3dw;
                if (pParam.BSIM3weff <= 0.0)
                    throw new CircuitException($"BSIM3v24: mosfet {Name}, model {model.Name}: Effective channel width <= 0");

                pParam.BSIM3leffCV = BSIM3l - 2.0 * pParam.BSIM3dlc;
                if (pParam.BSIM3leffCV <= 0.0)
                    throw new CircuitException($"BSIM3v24: mosfet {Name}, model {model.Name}: Effective channel length for C-V <= 0");

                pParam.BSIM3weffCV = BSIM3w - 2.0 * pParam.BSIM3dwc;
                if (pParam.BSIM3weffCV <= 0.0)
                    throw new CircuitException($"BSIM3v24: mosfet {Name}, model {model.Name}: Effective channel width for C-V <= 0");

                if (model.BSIM3binUnit.Value == 1)
                {
                    Inv_L = 1.0e-6 / pParam.BSIM3leff;
                    Inv_W = 1.0e-6 / pParam.BSIM3weff;
                    Inv_LW = 1.0e-12 / (pParam.BSIM3leff * pParam.BSIM3weff);
                }
                else
                {
                    Inv_L = 1.0 / pParam.BSIM3leff;
                    Inv_W = 1.0 / pParam.BSIM3weff;
                    Inv_LW = 1.0 / (pParam.BSIM3leff * pParam.BSIM3weff);
                }
                pParam.BSIM3cdsc = model.BSIM3cdsc + model.BSIM3lcdsc * Inv_L + model.BSIM3wcdsc * Inv_W + model.BSIM3pcdsc * Inv_LW;
                pParam.BSIM3cdscb = model.BSIM3cdscb + model.BSIM3lcdscb * Inv_L + model.BSIM3wcdscb * Inv_W + model.BSIM3pcdscb * Inv_LW;

                pParam.BSIM3cdscd = model.BSIM3cdscd + model.BSIM3lcdscd * Inv_L + model.BSIM3wcdscd * Inv_W + model.BSIM3pcdscd * Inv_LW;

                pParam.BSIM3cit = model.BSIM3cit + model.BSIM3lcit * Inv_L + model.BSIM3wcit * Inv_W + model.BSIM3pcit * Inv_LW;
                pParam.BSIM3nfactor = model.BSIM3nfactor + model.BSIM3lnfactor * Inv_L + model.BSIM3wnfactor * Inv_W + model.BSIM3pnfactor *
                  Inv_LW;
                pParam.BSIM3xj = model.BSIM3xj + model.BSIM3lxj * Inv_L + model.BSIM3wxj * Inv_W + model.BSIM3pxj * Inv_LW;
                pParam.BSIM3vsat = model.BSIM3vsat + model.BSIM3lvsat * Inv_L + model.BSIM3wvsat * Inv_W + model.BSIM3pvsat * Inv_LW;
                pParam.BSIM3at = model.BSIM3at + model.BSIM3lat * Inv_L + model.BSIM3wat * Inv_W + model.BSIM3pat * Inv_LW;
                pParam.BSIM3a0 = model.BSIM3a0 + model.BSIM3la0 * Inv_L + model.BSIM3wa0 * Inv_W + model.BSIM3pa0 * Inv_LW;

                pParam.BSIM3ags = model.BSIM3ags + model.BSIM3lags * Inv_L + model.BSIM3wags * Inv_W + model.BSIM3pags * Inv_LW;

                pParam.BSIM3a1 = model.BSIM3a1 + model.BSIM3la1 * Inv_L + model.BSIM3wa1 * Inv_W + model.BSIM3pa1 * Inv_LW;
                pParam.BSIM3a2 = model.BSIM3a2 + model.BSIM3la2 * Inv_L + model.BSIM3wa2 * Inv_W + model.BSIM3pa2 * Inv_LW;
                pParam.BSIM3keta = model.BSIM3keta + model.BSIM3lketa * Inv_L + model.BSIM3wketa * Inv_W + model.BSIM3pketa * Inv_LW;
                pParam.BSIM3nsub = model.BSIM3nsub + model.BSIM3lnsub * Inv_L + model.BSIM3wnsub * Inv_W + model.BSIM3pnsub * Inv_LW;
                pParam.BSIM3npeak = model.BSIM3npeak + model.BSIM3lnpeak * Inv_L + model.BSIM3wnpeak * Inv_W + model.BSIM3pnpeak * Inv_LW;
                pParam.BSIM3ngate = model.BSIM3ngate + model.BSIM3lngate * Inv_L + model.BSIM3wngate * Inv_W + model.BSIM3pngate * Inv_LW;
                pParam.BSIM3gamma1 = model.BSIM3gamma1 + model.BSIM3lgamma1 * Inv_L + model.BSIM3wgamma1 * Inv_W + model.BSIM3pgamma1 * Inv_LW;
                pParam.BSIM3gamma2 = model.BSIM3gamma2 + model.BSIM3lgamma2 * Inv_L + model.BSIM3wgamma2 * Inv_W + model.BSIM3pgamma2 * Inv_LW;
                pParam.BSIM3vbx = model.BSIM3vbx + model.BSIM3lvbx * Inv_L + model.BSIM3wvbx * Inv_W + model.BSIM3pvbx * Inv_LW;
                pParam.BSIM3vbm = model.BSIM3vbm + model.BSIM3lvbm * Inv_L + model.BSIM3wvbm * Inv_W + model.BSIM3pvbm * Inv_LW;
                pParam.BSIM3xt = model.BSIM3xt + model.BSIM3lxt * Inv_L + model.BSIM3wxt * Inv_W + model.BSIM3pxt * Inv_LW;
                pParam.BSIM3vfb = model.BSIM3vfb + model.BSIM3lvfb * Inv_L + model.BSIM3wvfb * Inv_W + model.BSIM3pvfb * Inv_LW;
                pParam.BSIM3k1 = model.BSIM3k1 + model.BSIM3lk1 * Inv_L + model.BSIM3wk1 * Inv_W + model.BSIM3pk1 * Inv_LW;
                pParam.BSIM3kt1 = model.BSIM3kt1 + model.BSIM3lkt1 * Inv_L + model.BSIM3wkt1 * Inv_W + model.BSIM3pkt1 * Inv_LW;
                pParam.BSIM3kt1l = model.BSIM3kt1l + model.BSIM3lkt1l * Inv_L + model.BSIM3wkt1l * Inv_W + model.BSIM3pkt1l * Inv_LW;
                pParam.BSIM3k2 = model.BSIM3k2 + model.BSIM3lk2 * Inv_L + model.BSIM3wk2 * Inv_W + model.BSIM3pk2 * Inv_LW;
                pParam.BSIM3kt2 = model.BSIM3kt2 + model.BSIM3lkt2 * Inv_L + model.BSIM3wkt2 * Inv_W + model.BSIM3pkt2 * Inv_LW;
                pParam.BSIM3k3 = model.BSIM3k3 + model.BSIM3lk3 * Inv_L + model.BSIM3wk3 * Inv_W + model.BSIM3pk3 * Inv_LW;
                pParam.BSIM3k3b = model.BSIM3k3b + model.BSIM3lk3b * Inv_L + model.BSIM3wk3b * Inv_W + model.BSIM3pk3b * Inv_LW;
                pParam.BSIM3w0 = model.BSIM3w0 + model.BSIM3lw0 * Inv_L + model.BSIM3ww0 * Inv_W + model.BSIM3pw0 * Inv_LW;
                pParam.BSIM3nlx = model.BSIM3nlx + model.BSIM3lnlx * Inv_L + model.BSIM3wnlx * Inv_W + model.BSIM3pnlx * Inv_LW;
                pParam.BSIM3dvt0 = model.BSIM3dvt0 + model.BSIM3ldvt0 * Inv_L + model.BSIM3wdvt0 * Inv_W + model.BSIM3pdvt0 * Inv_LW;
                pParam.BSIM3dvt1 = model.BSIM3dvt1 + model.BSIM3ldvt1 * Inv_L + model.BSIM3wdvt1 * Inv_W + model.BSIM3pdvt1 * Inv_LW;
                pParam.BSIM3dvt2 = model.BSIM3dvt2 + model.BSIM3ldvt2 * Inv_L + model.BSIM3wdvt2 * Inv_W + model.BSIM3pdvt2 * Inv_LW;
                pParam.BSIM3dvt0w = model.BSIM3dvt0w + model.BSIM3ldvt0w * Inv_L + model.BSIM3wdvt0w * Inv_W + model.BSIM3pdvt0w * Inv_LW;
                pParam.BSIM3dvt1w = model.BSIM3dvt1w + model.BSIM3ldvt1w * Inv_L + model.BSIM3wdvt1w * Inv_W + model.BSIM3pdvt1w * Inv_LW;
                pParam.BSIM3dvt2w = model.BSIM3dvt2w + model.BSIM3ldvt2w * Inv_L + model.BSIM3wdvt2w * Inv_W + model.BSIM3pdvt2w * Inv_LW;
                pParam.BSIM3drout = model.BSIM3drout + model.BSIM3ldrout * Inv_L + model.BSIM3wdrout * Inv_W + model.BSIM3pdrout * Inv_LW;
                pParam.BSIM3dsub = model.BSIM3dsub + model.BSIM3ldsub * Inv_L + model.BSIM3wdsub * Inv_W + model.BSIM3pdsub * Inv_LW;
                pParam.BSIM3vth0 = model.BSIM3vth0 + model.BSIM3lvth0 * Inv_L + model.BSIM3wvth0 * Inv_W + model.BSIM3pvth0 * Inv_LW;
                pParam.BSIM3ua = model.BSIM3ua + model.BSIM3lua * Inv_L + model.BSIM3wua * Inv_W + model.BSIM3pua * Inv_LW;
                pParam.BSIM3ua1 = model.BSIM3ua1 + model.BSIM3lua1 * Inv_L + model.BSIM3wua1 * Inv_W + model.BSIM3pua1 * Inv_LW;
                pParam.BSIM3ub = model.BSIM3ub + model.BSIM3lub * Inv_L + model.BSIM3wub * Inv_W + model.BSIM3pub * Inv_LW;
                pParam.BSIM3ub1 = model.BSIM3ub1 + model.BSIM3lub1 * Inv_L + model.BSIM3wub1 * Inv_W + model.BSIM3pub1 * Inv_LW;
                pParam.BSIM3uc = model.BSIM3uc + model.BSIM3luc * Inv_L + model.BSIM3wuc * Inv_W + model.BSIM3puc * Inv_LW;
                pParam.BSIM3uc1 = model.BSIM3uc1 + model.BSIM3luc1 * Inv_L + model.BSIM3wuc1 * Inv_W + model.BSIM3puc1 * Inv_LW;
                pParam.BSIM3u0 = model.BSIM3u0 + model.BSIM3lu0 * Inv_L + model.BSIM3wu0 * Inv_W + model.BSIM3pu0 * Inv_LW;
                pParam.BSIM3ute = model.BSIM3ute + model.BSIM3lute * Inv_L + model.BSIM3wute * Inv_W + model.BSIM3pute * Inv_LW;
                pParam.BSIM3voff = model.BSIM3voff + model.BSIM3lvoff * Inv_L + model.BSIM3wvoff * Inv_W + model.BSIM3pvoff * Inv_LW;
                pParam.BSIM3delta = model.BSIM3delta + model.BSIM3ldelta * Inv_L + model.BSIM3wdelta * Inv_W + model.BSIM3pdelta * Inv_LW;
                pParam.BSIM3rdsw = model.BSIM3rdsw + model.BSIM3lrdsw * Inv_L + model.BSIM3wrdsw * Inv_W + model.BSIM3prdsw * Inv_LW;
                pParam.BSIM3prwg = model.BSIM3prwg + model.BSIM3lprwg * Inv_L + model.BSIM3wprwg * Inv_W + model.BSIM3pprwg * Inv_LW;
                pParam.BSIM3prwb = model.BSIM3prwb + model.BSIM3lprwb * Inv_L + model.BSIM3wprwb * Inv_W + model.BSIM3pprwb * Inv_LW;
                pParam.BSIM3prt = model.BSIM3prt + model.BSIM3lprt * Inv_L + model.BSIM3wprt * Inv_W + model.BSIM3pprt * Inv_LW;
                pParam.BSIM3eta0 = model.BSIM3eta0 + model.BSIM3leta0 * Inv_L + model.BSIM3weta0 * Inv_W + model.BSIM3peta0 * Inv_LW;
                pParam.BSIM3etab = model.BSIM3etab + model.BSIM3letab * Inv_L + model.BSIM3wetab * Inv_W + model.BSIM3petab * Inv_LW;
                pParam.BSIM3pclm = model.BSIM3pclm + model.BSIM3lpclm * Inv_L + model.BSIM3wpclm * Inv_W + model.BSIM3ppclm * Inv_LW;
                pParam.BSIM3pdibl1 = model.BSIM3pdibl1 + model.BSIM3lpdibl1 * Inv_L + model.BSIM3wpdibl1 * Inv_W + model.BSIM3ppdibl1 * Inv_LW;
                pParam.BSIM3pdibl2 = model.BSIM3pdibl2 + model.BSIM3lpdibl2 * Inv_L + model.BSIM3wpdibl2 * Inv_W + model.BSIM3ppdibl2 * Inv_LW;
                pParam.BSIM3pdiblb = model.BSIM3pdiblb + model.BSIM3lpdiblb * Inv_L + model.BSIM3wpdiblb * Inv_W + model.BSIM3ppdiblb * Inv_LW;
                pParam.BSIM3pscbe1 = model.BSIM3pscbe1 + model.BSIM3lpscbe1 * Inv_L + model.BSIM3wpscbe1 * Inv_W + model.BSIM3ppscbe1 * Inv_LW;
                pParam.BSIM3pscbe2 = model.BSIM3pscbe2 + model.BSIM3lpscbe2 * Inv_L + model.BSIM3wpscbe2 * Inv_W + model.BSIM3ppscbe2 * Inv_LW;
                pParam.BSIM3pvag = model.BSIM3pvag + model.BSIM3lpvag * Inv_L + model.BSIM3wpvag * Inv_W + model.BSIM3ppvag * Inv_LW;
                pParam.BSIM3wr = model.BSIM3wr + model.BSIM3lwr * Inv_L + model.BSIM3wwr * Inv_W + model.BSIM3pwr * Inv_LW;
                pParam.BSIM3dwg = model.BSIM3dwg + model.BSIM3ldwg * Inv_L + model.BSIM3wdwg * Inv_W + model.BSIM3pdwg * Inv_LW;
                pParam.BSIM3dwb = model.BSIM3dwb + model.BSIM3ldwb * Inv_L + model.BSIM3wdwb * Inv_W + model.BSIM3pdwb * Inv_LW;
                pParam.BSIM3b0 = model.BSIM3b0 + model.BSIM3lb0 * Inv_L + model.BSIM3wb0 * Inv_W + model.BSIM3pb0 * Inv_LW;
                pParam.BSIM3b1 = model.BSIM3b1 + model.BSIM3lb1 * Inv_L + model.BSIM3wb1 * Inv_W + model.BSIM3pb1 * Inv_LW;
                pParam.BSIM3alpha0 = model.BSIM3alpha0 + model.BSIM3lalpha0 * Inv_L + model.BSIM3walpha0 * Inv_W + model.BSIM3palpha0 * Inv_LW;
                pParam.BSIM3alpha1 = model.BSIM3alpha1 + model.BSIM3lalpha1 * Inv_L + model.BSIM3walpha1 * Inv_W + model.BSIM3palpha1 * Inv_LW;
                pParam.BSIM3beta0 = model.BSIM3beta0 + model.BSIM3lbeta0 * Inv_L + model.BSIM3wbeta0 * Inv_W + model.BSIM3pbeta0 * Inv_LW;
                /* CV model */
                pParam.BSIM3elm = model.BSIM3elm + model.BSIM3lelm * Inv_L + model.BSIM3welm * Inv_W + model.BSIM3pelm * Inv_LW;
                pParam.BSIM3cgsl = model.BSIM3cgsl + model.BSIM3lcgsl * Inv_L + model.BSIM3wcgsl * Inv_W + model.BSIM3pcgsl * Inv_LW;
                pParam.BSIM3cgdl = model.BSIM3cgdl + model.BSIM3lcgdl * Inv_L + model.BSIM3wcgdl * Inv_W + model.BSIM3pcgdl * Inv_LW;
                pParam.BSIM3ckappa = model.BSIM3ckappa + model.BSIM3lckappa * Inv_L + model.BSIM3wckappa * Inv_W + model.BSIM3pckappa * Inv_LW;
                pParam.BSIM3cf = model.BSIM3cf + model.BSIM3lcf * Inv_L + model.BSIM3wcf * Inv_W + model.BSIM3pcf * Inv_LW;
                pParam.BSIM3clc = model.BSIM3clc + model.BSIM3lclc * Inv_L + model.BSIM3wclc * Inv_W + model.BSIM3pclc * Inv_LW;
                pParam.BSIM3cle = model.BSIM3cle + model.BSIM3lcle * Inv_L + model.BSIM3wcle * Inv_W + model.BSIM3pcle * Inv_LW;
                pParam.BSIM3vfbcv = model.BSIM3vfbcv + model.BSIM3lvfbcv * Inv_L + model.BSIM3wvfbcv * Inv_W + model.BSIM3pvfbcv * Inv_LW;
                pParam.BSIM3acde = model.BSIM3acde + model.BSIM3lacde * Inv_L + model.BSIM3wacde * Inv_W + model.BSIM3pacde * Inv_LW;
                pParam.BSIM3moin = model.BSIM3moin + model.BSIM3lmoin * Inv_L + model.BSIM3wmoin * Inv_W + model.BSIM3pmoin * Inv_LW;
                pParam.BSIM3noff = model.BSIM3noff + model.BSIM3lnoff * Inv_L + model.BSIM3wnoff * Inv_W + model.BSIM3pnoff * Inv_LW;
                pParam.BSIM3voffcv = model.BSIM3voffcv + model.BSIM3lvoffcv * Inv_L + model.BSIM3wvoffcv * Inv_W + model.BSIM3pvoffcv * Inv_LW;

                pParam.BSIM3abulkCVfactor = 1.0 + Math.Pow((pParam.BSIM3clc / pParam.BSIM3leffCV), pParam.BSIM3cle);

                T0 = (model.TRatio - 1.0);
                pParam.BSIM3ua = pParam.BSIM3ua + pParam.BSIM3ua1 * T0;
                pParam.BSIM3ub = pParam.BSIM3ub + pParam.BSIM3ub1 * T0;
                pParam.BSIM3uc = pParam.BSIM3uc + pParam.BSIM3uc1 * T0;
                if (pParam.BSIM3u0 > 1.0)
                    pParam.BSIM3u0 = pParam.BSIM3u0 / 1.0e4;

                pParam.BSIM3u0temp = pParam.BSIM3u0 * Math.Pow(model.TRatio, pParam.BSIM3ute);
                pParam.BSIM3vsattemp = pParam.BSIM3vsat - pParam.BSIM3at * T0;
                pParam.BSIM3rds0 = (pParam.BSIM3rdsw + pParam.BSIM3prt * T0) / Math.Pow(pParam.BSIM3weff * 1E6, pParam.BSIM3wr);

                if (BSIM3checkModel())
                    throw new CircuitException("Fatal error(s) detected during BSIM3v24 parameter checking");

                pParam.BSIM3cgdo = (model.BSIM3cgdo + pParam.BSIM3cf) * pParam.BSIM3weffCV;
                pParam.BSIM3cgso = (model.BSIM3cgso + pParam.BSIM3cf) * pParam.BSIM3weffCV;
                pParam.BSIM3cgbo = model.BSIM3cgbo * pParam.BSIM3leffCV;

                T0 = pParam.BSIM3leffCV * pParam.BSIM3leffCV;
                pParam.BSIM3tconst = pParam.BSIM3u0temp * pParam.BSIM3elm / (model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV *
                    T0);

                if (!model.BSIM3npeak.Given && model.BSIM3gamma1.Given)
                {
                    T0 = pParam.BSIM3gamma1 * model.BSIM3cox;
                    pParam.BSIM3npeak = 3.021E22 * T0 * T0;
                }

                pParam.BSIM3phi = 2.0 * model.Vtm0 * Math.Log(pParam.BSIM3npeak / model.ni);

                pParam.BSIM3sqrtPhi = Math.Sqrt(pParam.BSIM3phi);
                pParam.BSIM3phis3 = pParam.BSIM3sqrtPhi * pParam.BSIM3phi;

                pParam.BSIM3Xdep0 = Math.Sqrt(2.0 * Transistor.EPSSI / (Transistor.Charge_q * pParam.BSIM3npeak * 1.0e6)) * pParam.BSIM3sqrtPhi;
                pParam.BSIM3sqrtXdep0 = Math.Sqrt(pParam.BSIM3Xdep0);
                pParam.BSIM3litl = Math.Sqrt(3.0 * pParam.BSIM3xj * model.BSIM3tox);
                pParam.BSIM3vbi = model.Vtm0 * Math.Log(1.0e20 * pParam.BSIM3npeak / (model.ni * model.ni));
                pParam.BSIM3cdep0 = Math.Sqrt(Transistor.Charge_q * Transistor.EPSSI * pParam.BSIM3npeak * 1.0e6 / 2.0 / pParam.BSIM3phi);

                pParam.BSIM3ldeb = Math.Sqrt(Transistor.EPSSI * model.Vtm0 / (Transistor.Charge_q * pParam.BSIM3npeak * 1.0e6)) / 3.0;
                pParam.BSIM3acde *= Math.Pow((pParam.BSIM3npeak / 2.0e16), -0.25);

                if (model.BSIM3k1.Given || model.BSIM3k2.Given)
                {
                    if (!model.BSIM3k1.Given)
                    {
                        CircuitWarning.Warning(this, "Warning: k1 should be specified with k2.");
                        pParam.BSIM3k1 = 0.53;
                    }
                    if (!model.BSIM3k2.Given)
                    {
                        CircuitWarning.Warning(this, "Warning: k2 should be specified with k1.");
                        pParam.BSIM3k2 = -0.0186;
                    }
                    if (model.BSIM3nsub.Given)
                        CircuitWarning.Warning(this, "Warning: nsub is ignored because k1 or k2 is given.");
                    if (model.BSIM3xt.Given)
                        CircuitWarning.Warning(this, "Warning: xt is ignored because k1 or k2 is given.");
                    if (model.BSIM3vbx.Given)
                        CircuitWarning.Warning(this, "Warning: vbx is ignored because k1 or k2 is given.");
                    if (model.BSIM3gamma1.Given)
                        CircuitWarning.Warning(this, "Warning: gamma1 is ignored because k1 or k2 is given.");
                    if (model.BSIM3gamma2.Given)
                        CircuitWarning.Warning(this, "Warning: gamma2 is ignored because k1 or k2 is given.");
                }
                else
                {
                    if (!model.BSIM3vbx.Given)
                        pParam.BSIM3vbx = pParam.BSIM3phi - 7.7348e-4 * pParam.BSIM3npeak * pParam.BSIM3xt * pParam.BSIM3xt;
                    if (pParam.BSIM3vbx > 0.0)
                        pParam.BSIM3vbx = -pParam.BSIM3vbx;
                    if (pParam.BSIM3vbm > 0.0)
                        pParam.BSIM3vbm = -pParam.BSIM3vbm;

                    if (!model.BSIM3gamma1.Given)
                        pParam.BSIM3gamma1 = 5.753e-12 * Math.Sqrt(pParam.BSIM3npeak) / model.BSIM3cox;
                    if (!model.BSIM3gamma2.Given)
                        pParam.BSIM3gamma2 = 5.753e-12 * Math.Sqrt(pParam.BSIM3nsub) / model.BSIM3cox;

                    T0 = pParam.BSIM3gamma1 - pParam.BSIM3gamma2;
                    T1 = Math.Sqrt(pParam.BSIM3phi - pParam.BSIM3vbx) - pParam.BSIM3sqrtPhi;
                    T2 = Math.Sqrt(pParam.BSIM3phi * (pParam.BSIM3phi - pParam.BSIM3vbm)) - pParam.BSIM3phi;
                    pParam.BSIM3k2 = T0 * T1 / (2.0 * T2 + pParam.BSIM3vbm);
                    pParam.BSIM3k1 = pParam.BSIM3gamma2 - 2.0 * pParam.BSIM3k2 * Math.Sqrt(pParam.BSIM3phi - pParam.BSIM3vbm);
                }

                if (pParam.BSIM3k2 < 0.0)
                {
                    T0 = 0.5 * pParam.BSIM3k1 / pParam.BSIM3k2;
                    pParam.BSIM3vbsc = 0.9 * (pParam.BSIM3phi - T0 * T0);
                    if (pParam.BSIM3vbsc > -3.0)
                        pParam.BSIM3vbsc = -3.0;
                    else if (pParam.BSIM3vbsc < -30.0)

                        pParam.BSIM3vbsc = -30.0;
                }
                else
                {
                    pParam.BSIM3vbsc = -30.0;
                }
                if (pParam.BSIM3vbsc > pParam.BSIM3vbm)
                    pParam.BSIM3vbsc = pParam.BSIM3vbm;

                if (!model.BSIM3vfb.Given)
                {
                    if (model.BSIM3vth0.Given)
                    {
                        pParam.BSIM3vfb = model.BSIM3type * pParam.BSIM3vth0 - pParam.BSIM3phi - pParam.BSIM3k1 * pParam.BSIM3sqrtPhi;
                    }
                    else
                    {
                        pParam.BSIM3vfb = -1.0;
                    }
                }
                if (!model.BSIM3vth0.Given)
                {
                    pParam.BSIM3vth0 = model.BSIM3type * (pParam.BSIM3vfb + pParam.BSIM3phi + pParam.BSIM3k1 * pParam.BSIM3sqrtPhi);
                }

                pParam.BSIM3k1ox = pParam.BSIM3k1 * model.BSIM3tox / model.BSIM3toxm;
                pParam.BSIM3k2ox = pParam.BSIM3k2 * model.BSIM3tox / model.BSIM3toxm;

                T1 = Math.Sqrt(Transistor.EPSSI / Transistor.EPSOX * model.BSIM3tox * pParam.BSIM3Xdep0);
                T0 = Math.Exp(-0.5 * pParam.BSIM3dsub * pParam.BSIM3leff / T1);
                pParam.BSIM3theta0vb0 = (T0 + 2.0 * T0 * T0);

                T0 = Math.Exp(-0.5 * pParam.BSIM3drout * pParam.BSIM3leff / T1);
                T2 = (T0 + 2.0 * T0 * T0);
                pParam.BSIM3thetaRout = pParam.BSIM3pdibl1 * T2 + pParam.BSIM3pdibl2;

                tmp = Math.Sqrt(pParam.BSIM3Xdep0);
                tmp1 = pParam.BSIM3vbi - pParam.BSIM3phi;
                tmp2 = model.BSIM3factor1 * tmp;

                T0 = -0.5 * pParam.BSIM3dvt1w * pParam.BSIM3weff * pParam.BSIM3leff / tmp2;
                if (T0 > -Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 * (1.0 + 2.0 * T1);
                }
                else
                {
                    T1 = Transistor.MIN_EXP;
                    T2 = T1 * (1.0 + 2.0 * T1);
                }
                T0 = pParam.BSIM3dvt0w * T2;
                T2 = T0 * tmp1;

                T0 = -0.5 * pParam.BSIM3dvt1 * pParam.BSIM3leff / tmp2;
                if (T0 > -Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T3 = T1 * (1.0 + 2.0 * T1);
                }
                else
                {
                    T1 = Transistor.MIN_EXP;
                    T3 = T1 * (1.0 + 2.0 * T1);
                }
                T3 = pParam.BSIM3dvt0 * T3 * tmp1;

                T4 = model.BSIM3tox * pParam.BSIM3phi / (pParam.BSIM3weff + pParam.BSIM3w0);

                T0 = Math.Sqrt(1.0 + pParam.BSIM3nlx / pParam.BSIM3leff);
                T5 = pParam.BSIM3k1ox * (T0 - 1.0) * pParam.BSIM3sqrtPhi + (pParam.BSIM3kt1 + pParam.BSIM3kt1l / pParam.BSIM3leff) *
					(model.TRatio - 1.0);

                tmp3 = model.BSIM3type * pParam.BSIM3vth0 - T2 - T3 + pParam.BSIM3k3 * T4 + T5;
                pParam.BSIM3vfbzb = tmp3 - pParam.BSIM3phi - pParam.BSIM3k1 * pParam.BSIM3sqrtPhi;
                /* End of vfbzb */
            }

            /* process source / drain series resistance */
            BSIM3drainConductance = model.BSIM3sheetResistance * BSIM3drainSquares;
            if (BSIM3drainConductance > 0.0)
                BSIM3drainConductance = 1.0 / BSIM3drainConductance;
            else
                BSIM3drainConductance = 0.0;

            BSIM3sourceConductance = model.BSIM3sheetResistance * BSIM3sourceSquares;
            if (BSIM3sourceConductance > 0.0)
                BSIM3sourceConductance = 1.0 / BSIM3sourceConductance;
            else
                BSIM3sourceConductance = 0.0;
            BSIM3cgso = pParam.BSIM3cgso;
            BSIM3cgdo = pParam.BSIM3cgdo;

            Nvtm = model.BSIM3vtm * model.BSIM3jctEmissionCoeff;
            if ((BSIM3sourceArea <= 0.0) && (BSIM3sourcePerimeter <= 0.0))
            {
                SourceSatCurrent = 1.0e-14;
            }
            else
            {
                SourceSatCurrent = BSIM3sourceArea * model.BSIM3jctTempSatCurDensity + BSIM3sourcePerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if ((SourceSatCurrent > 0.0) && (model.BSIM3ijth > 0.0))
            {
                BSIM3vjsm = Nvtm * Math.Log(model.BSIM3ijth / SourceSatCurrent + 1.0);
                BSIM3IsEvjsm = SourceSatCurrent * Math.Exp(BSIM3vjsm / Nvtm);
            }

            if ((BSIM3drainArea <= 0.0) && (BSIM3drainPerimeter <= 0.0))
            {
                DrainSatCurrent = 1.0e-14;
            }
            else
            {
                DrainSatCurrent = BSIM3drainArea * model.BSIM3jctTempSatCurDensity + BSIM3drainPerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if ((DrainSatCurrent > 0.0) && (model.BSIM3ijth > 0.0))
            {
                BSIM3vjdm = Nvtm * Math.Log(model.BSIM3ijth / DrainSatCurrent + 1.0);
                BSIM3IsEvjdm = DrainSatCurrent * Math.Exp(BSIM3vjdm / Nvtm);
            }
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as BSIM3v24Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;

            int Check;
            double vbs, vgs, vds, qdef, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, Idtot, cdhat, Ibtot, cbhat, von,
                vgb, Nvtm, SourceSatCurrent, evbs, T0, DrainSatCurrent, evbd, Vds, Vgs, Vbs, T1, Vbseff, dVbseff_dVb, Phis, dPhis_dVb,
                sqrtPhis, dsqrtPhis_dVb, Xdep, dXdep_dVb, Leff, Vtm, T3, V0, T2, T4, lt1, dlt1_dVb, ltw, dltw_dVb, Theta0, dT1_dVb,
                dTheta0_dVb, Delt_vth, dDelt_vth_dVb, dT2_dVb, TempRatio, tmp2, T9, dDIBL_Sft_dVd, DIBL_Sft, Vth, dVth_dVb, dVth_dVd, tmp3,
                tmp4, n, dn_dVb, dn_dVd, T6, T5, Vgs_eff, dVgs_eff_dVg, Vgst, T10, VgstNVt, ExpArg, Vgsteff, dVgsteff_dVg, dVgsteff_dVd,
                dVgsteff_dVb, ExpVgst, dT1_dVg, dT1_dVd, dT2_dVg, dT2_dVd, Weff, dWeff_dVg, dWeff_dVb, Rds, dRds_dVg, dRds_dVb, tmp1, T7,
                Abulk0, dAbulk0_dVb, T8, dAbulk_dVg, Abulk, dAbulk_dVb, dT0_dVb, dDenomi_dVg, dDenomi_dVd, dDenomi_dVb, Denomi, ueff,
                dueff_dVg, dueff_dVd, dueff_dVb, WVCox, WVCoxRds, Esat, EsatL, dEsatL_dVg, dEsatL_dVd, dEsatL_dVb, a1, dLambda_dVg, Lambda,
                Vgst2Vtm, Vdsat, dT0_dVg, dT0_dVd, dVdsat_dVg, dVdsat_dVd, dVdsat_dVb, dT3_dVg, dT3_dVd, dT3_dVb, Vdseff, dVdseff_dVg,
                dVdseff_dVd, dVdseff_dVb, Vasat, dVasat_dVg, dVasat_dVb, dVasat_dVd, diffVds, VACLM, dVACLM_dVg, dVACLM_dVb, dVACLM_dVd,
                VADIBL, dVADIBL_dVg, dVADIBL_dVb, dVADIBL_dVd, Va, dVa_dVg, dVa_dVd, dVa_dVb, VASCBE, dVASCBE_dVg, dVASCBE_dVd, dVASCBE_dVb,
                CoxWovL, beta, dbeta_dVg, dbeta_dVd, dbeta_dVb, fgche1, dfgche1_dVg, dfgche1_dVd, dfgche1_dVb, fgche2, dfgche2_dVg,
                dfgche2_dVd, dfgche2_dVb, gche, dgche_dVg, dgche_dVd, dgche_dVb, Idl, dIdl_dVg, dIdl_dVd, dIdl_dVb, Idsa, dIdsa_dVg, dIdsa_dVd,
                dIdsa_dVb, Ids, Gm, Gds, Gmb, tmp, Isub, Gbg, Gbd, Gbb, cdrain, qgate = 0.0, Vfb, dVgst_dVb, dVgst_dVg, CoxWL, Arg1, qbulk = 0.0, qdrn = 0.0,
                One_Third_CoxWL, Two_Third_CoxWL, AbulkCV, dAbulkCV_dVb, Alphaz, T11, dAlphaz_dVg, dAlphaz_dVb, T12, VbseffCV, dVbseffCV_dVb,
                noff, dnoff_dVd, dnoff_dVb, voffcv, Cgg, Cgd, Cgb, Cbg, Cbd, Cbb, VdsatCV, dVdsatCV_dVg, dVdsatCV_dVb, Cgg1, Cgb1, Cgd1, Cbg1,
                Cbb1, Cbd1, qsrc, Csg, Csb, Csd, V3, Vfbeff, dVfbeff_dVg, dVfbeff_dVb, Qac0, dQac0_dVg, dQac0_dVb, Qsub0, dQsub0_dVg,
                dQsub0_dVd, dQsub0_dVb, V4, VdseffCV, dVdseffCV_dVg, dVdseffCV_dVd, dVdseffCV_dVb, qinoi, Cox, Tox, Tcen, dTcen_dVg, dTcen_dVb,
                LINK, Ccen, Coxeff, dCoxeff_dVg, dCoxeff_dVb, CoxWLcen, QovCox, DeltaPhi, dDeltaPhi_dVg, dDeltaPhi_dVd, dDeltaPhi_dVb,
                dTcen_dVd, dCoxeff_dVd, czbd, czbs, czbdswg, czbdsw, czbssw, czbsswg, MJ, MJSW, MJSWG, arg, sarg, qcheq = 0.0, gtau_drift, gtau_diff,
                cgdo, qgdo, cgso, qgso, ag0, gcggb, gcgdb, gcgsb, gcdgb, gcddb, gcdsb, gcsgb, gcsdb, gcssb, gcbgb, gcbdb, gcbsb, qgd, qgs, qgb,
                ggtg, sxpart, dxpart, ddxpart_dVd, dsxpart_dVd, ggtd, ggts, ggtb, gqdef = 0.0, gcqgb = 0.0, gcqdb = 0.0, gcqsb = 0.0, gcqbb = 0.0, Cdd, Cdg, ddxpart_dVg,
                Cds, Css, ddxpart_dVs, ddxpart_dVb, dsxpart_dVg, dsxpart_dVs, dsxpart_dVb, cqdef, ceqqg, cqcheq, cqgate, cqbulk, cqdrn, ceqqb,
                ceqqd, Gmbs, FwdSum, RevSum, cdreq, ceqbd, ceqbs, gbbdp, gbbsp, gbdpg, gbdpdp, gbdpb, gbdpsp, gbspg, gbspdp, gbspb, gbspsp;
            bool ChargeComputationNeeded = ((method != null || state.UseSmallSignal) || ((state.Domain == CircuitState.DomainTypes.Time &&
                state.UseDC) && state.UseIC)) ? true : false;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][BSIM3states + BSIM3vbs];
                vgs = state.States[0][BSIM3states + BSIM3vgs];
                vds = state.States[0][BSIM3states + BSIM3vds];
                qdef = state.States[0][BSIM3states + BSIM3qdef];
            }
            else if ((method != null && method.SavedTime == 0.0))
            {
                vbs = state.States[1][BSIM3states + BSIM3vbs];
                vgs = state.States[1][BSIM3states + BSIM3vgs];
                vds = state.States[1][BSIM3states + BSIM3vds];
                qdef = state.States[1][BSIM3states + BSIM3qdef];
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && !BSIM3off)
            {
                vds = model.BSIM3type * BSIM3icVDS;
                vgs = model.BSIM3type * BSIM3icVGS;
                vbs = model.BSIM3type * BSIM3icVBS;
                qdef = 0.0;

                if ((vds == 0.0) && (vgs == 0.0) && (vbs == 0.0) && ((method != null || state.UseDC ||
                    state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                {
                    vbs = 0.0;
                    vgs = model.BSIM3type * pParam.BSIM3vth0 + 0.1;
                    vds = 0.1;
                }
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitFix) && (BSIM3off))
            {
                qdef = vbs = vgs = vds = 0.0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.BSIM3type * (rstate.OldSolution[BSIM3bNode] - rstate.OldSolution[BSIM3sNodePrime]);
                vgs = model.BSIM3type * (rstate.OldSolution[BSIM3gNode] - rstate.OldSolution[BSIM3sNodePrime]);
                vds = model.BSIM3type * (rstate.OldSolution[BSIM3dNodePrime] - rstate.OldSolution[BSIM3sNodePrime]);
                qdef = model.BSIM3type * (rstate.OldSolution[BSIM3qNode]);
                /* PREDICTOR */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][BSIM3states + BSIM3vgs] - state.States[0][BSIM3states + BSIM3vds];
                delvbs = vbs - state.States[0][BSIM3states + BSIM3vbs];
                delvbd = vbd - state.States[0][BSIM3states + BSIM3vbd];
                delvgs = vgs - state.States[0][BSIM3states + BSIM3vgs];
                delvds = vds - state.States[0][BSIM3states + BSIM3vds];
                delvgd = vgd - vgdo;

                if (BSIM3mode >= 0)
                {
                    Idtot = BSIM3cd + BSIM3csub - BSIM3cbd;
                    cdhat = Idtot - BSIM3gbd * delvbd + (BSIM3gmbs + BSIM3gbbs) * delvbs + (BSIM3gm + BSIM3gbgs) * delvgs + (BSIM3gds + BSIM3gbds) *
                        delvds;
                    Ibtot = BSIM3cbs + BSIM3cbd - BSIM3csub;
                    cbhat = Ibtot + BSIM3gbd * delvbd + (BSIM3gbs - BSIM3gbbs) * delvbs - BSIM3gbgs * delvgs - BSIM3gbds * delvds;
                }
                else
                {
                    Idtot = BSIM3cd - BSIM3cbd;
                    cdhat = Idtot - (BSIM3gbd - BSIM3gmbs) * delvbd + BSIM3gm * delvgd - BSIM3gds * delvds;
                    Ibtot = BSIM3cbs + BSIM3cbd - BSIM3csub;
                    cbhat = Ibtot + BSIM3gbs * delvbs + (BSIM3gbd - BSIM3gbbs) * delvbd - BSIM3gbgs * delvgd + BSIM3gbds * delvds;
                }

                /* NOBYPASS */
                von = BSIM3von;
                if (state.States[0][BSIM3states + BSIM3vds] >= 0.0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][BSIM3states + BSIM3vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][BSIM3states + BSIM3vds]);
                    vgd = vgs - vds;

                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][BSIM3states + BSIM3vds]));
                    vgs = vgd + vds;
                }

                if (vds >= 0.0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][BSIM3states + BSIM3vbs], Circuit.CONSTvt0, model.BSIM3vcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][BSIM3states + BSIM3vbd], Circuit.CONSTvt0, model.BSIM3vcrit, ref Check);
                    vbs = vbd + vds;
                }
            }

            /* determine DC current and derivatives */
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            /* Source / drain junction diode DC model begins */
            Nvtm = model.BSIM3vtm * model.BSIM3jctEmissionCoeff;
            if ((BSIM3sourceArea <= 0.0) && (BSIM3sourcePerimeter <= 0.0))
            {
                SourceSatCurrent = 1.0e-14;
            }
            else
            {
                SourceSatCurrent = BSIM3sourceArea * model.BSIM3jctTempSatCurDensity + BSIM3sourcePerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if (SourceSatCurrent <= 0.0)
            {
                BSIM3gbs = state.Gmin;
                BSIM3cbs = BSIM3gbs * vbs;
            }
            else
            {
                if (model.BSIM3ijth.Value == 0.0)
                {
                    evbs = Math.Exp(vbs / Nvtm);
                    BSIM3gbs = SourceSatCurrent * evbs / Nvtm + state.Gmin;
                    BSIM3cbs = SourceSatCurrent * (evbs - 1.0) + state.Gmin * vbs;
                }
                else
                {
                    if (vbs < BSIM3vjsm)
                    {
                        evbs = Math.Exp(vbs / Nvtm);
                        BSIM3gbs = SourceSatCurrent * evbs / Nvtm + state.Gmin;
                        BSIM3cbs = SourceSatCurrent * (evbs - 1.0) + state.Gmin * vbs;
                    }
                    else
                    {
                        T0 = BSIM3IsEvjsm / Nvtm;
                        BSIM3gbs = T0 + state.Gmin;
                        BSIM3cbs = BSIM3IsEvjsm - SourceSatCurrent + T0 * (vbs - BSIM3vjsm) + state.Gmin * vbs;
                    }
                }
            }

            if ((BSIM3drainArea <= 0.0) && (BSIM3drainPerimeter <= 0.0))
            {
                DrainSatCurrent = 1.0e-14;
            }
            else
            {
                DrainSatCurrent = BSIM3drainArea * model.BSIM3jctTempSatCurDensity + BSIM3drainPerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if (DrainSatCurrent <= 0.0)
            {
                BSIM3gbd = state.Gmin;
                BSIM3cbd = BSIM3gbd * vbd;
            }
            else
            {
                if (model.BSIM3ijth.Value == 0.0)
                {
                    evbd = Math.Exp(vbd / Nvtm);
                    BSIM3gbd = DrainSatCurrent * evbd / Nvtm + state.Gmin;
                    BSIM3cbd = DrainSatCurrent * (evbd - 1.0) + state.Gmin * vbd;
                }
                else
                {
                    if (vbd < BSIM3vjdm)
                    {
                        evbd = Math.Exp(vbd / Nvtm);
                        BSIM3gbd = DrainSatCurrent * evbd / Nvtm + state.Gmin;
                        BSIM3cbd = DrainSatCurrent * (evbd - 1.0) + state.Gmin * vbd;
                    }
                    else
                    {
                        T0 = BSIM3IsEvjdm / Nvtm;
                        BSIM3gbd = T0 + state.Gmin;
                        BSIM3cbd = BSIM3IsEvjdm - DrainSatCurrent + T0 * (vbd - BSIM3vjdm) + state.Gmin * vbd;
                    }
                }
            }
            /* End of diode DC model */

            if (vds >= 0.0)
            {
                /* normal mode */
                BSIM3mode = 1;
                Vds = vds;
                Vgs = vgs;
                Vbs = vbs;
            }
            else
            {
                /* inverse mode */
                BSIM3mode = -1;
                Vds = -vds;
                Vgs = vgd;
                Vbs = vbd;
            }

            T0 = Vbs - pParam.BSIM3vbsc - 0.001;
            T1 = Math.Sqrt(T0 * T0 - 0.004 * pParam.BSIM3vbsc);
            Vbseff = pParam.BSIM3vbsc + 0.5 * (T0 + T1);
            dVbseff_dVb = 0.5 * (1.0 + T0 / T1);
            if (Vbseff < Vbs)
            {
                Vbseff = Vbs;
            }

            if (Vbseff > 0.0)
            {
                T0 = pParam.BSIM3phi / (pParam.BSIM3phi + Vbseff);
                Phis = pParam.BSIM3phi * T0;
                dPhis_dVb = -T0 * T0;
                sqrtPhis = pParam.BSIM3phis3 / (pParam.BSIM3phi + 0.5 * Vbseff);
                dsqrtPhis_dVb = -0.5 * sqrtPhis * sqrtPhis / pParam.BSIM3phis3;
            }
            else
            {
                Phis = pParam.BSIM3phi - Vbseff;
                dPhis_dVb = -1.0;
                sqrtPhis = Math.Sqrt(Phis);
                dsqrtPhis_dVb = -0.5 / sqrtPhis;
            }
            Xdep = pParam.BSIM3Xdep0 * sqrtPhis / pParam.BSIM3sqrtPhi;
            dXdep_dVb = (pParam.BSIM3Xdep0 / pParam.BSIM3sqrtPhi) * dsqrtPhis_dVb;

            Leff = pParam.BSIM3leff;
            Vtm = model.BSIM3vtm;
            /* Vth Calculation */
            T3 = Math.Sqrt(Xdep);
            V0 = pParam.BSIM3vbi - pParam.BSIM3phi;

            T0 = pParam.BSIM3dvt2 * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = pParam.BSIM3dvt2;
            }
            else /* Added to avoid any discontinuity problems caused by dvt2 */
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = pParam.BSIM3dvt2 * T4 * T4;
            }
            lt1 = model.BSIM3factor1 * T3 * T1;
            dlt1_dVb = model.BSIM3factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = pParam.BSIM3dvt2w * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = pParam.BSIM3dvt2w;
            }
            else /* Added to avoid any discontinuity problems caused by dvt2w */
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = pParam.BSIM3dvt2w * T4 * T4;
            }
            ltw = model.BSIM3factor1 * T3 * T1;
            dltw_dVb = model.BSIM3factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = -0.5 * pParam.BSIM3dvt1 * Leff / lt1;
            if (T0 > -Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                Theta0 = T1 * (1.0 + 2.0 * T1);
                dT1_dVb = -T0 / lt1 * T1 * dlt1_dVb;
                dTheta0_dVb = (1.0 + 4.0 * T1) * dT1_dVb;
            }
            else
            {
                T1 = Transistor.MIN_EXP;
                Theta0 = T1 * (1.0 + 2.0 * T1);
                dTheta0_dVb = 0.0;
            }

            BSIM3thetavth = pParam.BSIM3dvt0 * Theta0;
            Delt_vth = BSIM3thetavth * V0;
            dDelt_vth_dVb = pParam.BSIM3dvt0 * dTheta0_dVb * V0;

            T0 = -0.5 * pParam.BSIM3dvt1w * pParam.BSIM3weff * Leff / ltw;
            if (T0 > -Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                T2 = T1 * (1.0 + 2.0 * T1);
                dT1_dVb = -T0 / ltw * T1 * dltw_dVb;
                dT2_dVb = (1.0 + 4.0 * T1) * dT1_dVb;
            }
            else
            {
                T1 = Transistor.MIN_EXP;
                T2 = T1 * (1.0 + 2.0 * T1);
                dT2_dVb = 0.0;
            }

            T0 = pParam.BSIM3dvt0w * T2;
            T2 = T0 * V0;
            dT2_dVb = pParam.BSIM3dvt0w * dT2_dVb * V0;

            TempRatio = state.Temperature / model.BSIM3tnom - 1.0;
            T0 = Math.Sqrt(1.0 + pParam.BSIM3nlx / Leff);
            T1 = pParam.BSIM3k1ox * (T0 - 1.0) * pParam.BSIM3sqrtPhi + (pParam.BSIM3kt1 + pParam.BSIM3kt1l / Leff + pParam.BSIM3kt2 *
                Vbseff) * TempRatio;
            tmp2 = model.BSIM3tox * pParam.BSIM3phi / (pParam.BSIM3weff + pParam.BSIM3w0);

            T3 = pParam.BSIM3eta0 + pParam.BSIM3etab * Vbseff;
            if (T3 < 1.0e-4)
            /* avoid  discontinuity problems caused by etab */
            {
                T9 = 1.0 / (3.0 - 2.0e4 * T3);
                T3 = (2.0e-4 - T3) * T9;
                T4 = T9 * T9;
            }
            else
            {
                T4 = 1.0;
            }
            dDIBL_Sft_dVd = T3 * pParam.BSIM3theta0vb0;
            DIBL_Sft = dDIBL_Sft_dVd * Vds;

            Vth = model.BSIM3type * pParam.BSIM3vth0 - pParam.BSIM3k1 * pParam.BSIM3sqrtPhi + pParam.BSIM3k1ox * sqrtPhis -
                pParam.BSIM3k2ox * Vbseff - Delt_vth - T2 + (pParam.BSIM3k3 + pParam.BSIM3k3b * Vbseff) * tmp2 + T1 - DIBL_Sft;

            BSIM3von = Vth;

            dVth_dVb = pParam.BSIM3k1ox * dsqrtPhis_dVb - pParam.BSIM3k2ox - dDelt_vth_dVb - dT2_dVb + pParam.BSIM3k3b * tmp2 -
                pParam.BSIM3etab * Vds * pParam.BSIM3theta0vb0 * T4 + pParam.BSIM3kt2 * TempRatio;
            dVth_dVd = -dDIBL_Sft_dVd;

            /* Calculate n */
            tmp2 = pParam.BSIM3nfactor * Transistor.EPSSI / Xdep;
            tmp3 = pParam.BSIM3cdsc + pParam.BSIM3cdscb * Vbseff + pParam.BSIM3cdscd * Vds;
            tmp4 = (tmp2 + tmp3 * Theta0 + pParam.BSIM3cit) / model.BSIM3cox;
            if (tmp4 >= -0.5)
            {
                n = 1.0 + tmp4;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + pParam.BSIM3cdscb * Theta0) / model.BSIM3cox;
                dn_dVd = pParam.BSIM3cdscd * Theta0 / model.BSIM3cox;
            }
            else
            /* avoid  discontinuity problems caused by tmp4 */
            {
                T0 = 1.0 / (3.0 + 8.0 * tmp4);
                n = (1.0 + 3.0 * tmp4) * T0;
                T0 *= T0;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + pParam.BSIM3cdscb * Theta0) / model.BSIM3cox * T0;
                dn_dVd = pParam.BSIM3cdscd * Theta0 / model.BSIM3cox * T0;
            }

            /* Poly Gate Si Depletion Effect */
            T0 = pParam.BSIM3vfb + pParam.BSIM3phi;
            if ((pParam.BSIM3ngate > 1.0e18) && (pParam.BSIM3ngate < 1.0e25) && (Vgs > T0))
            /* added to avoid the problem caused by ngate */
            {
                T1 = 1.0e6 * Transistor.Charge_q * Transistor.EPSSI * pParam.BSIM3ngate / (model.BSIM3cox * model.BSIM3cox);
                T4 = Math.Sqrt(1.0 + 2.0 * (Vgs - T0) / T1);
                T2 = T1 * (T4 - 1.0);
                T3 = 0.5 * T2 * T2 / T1; /* T3 = Vpoly */
                T7 = 1.12 - T3 - 0.05;
                T6 = Math.Sqrt(T7 * T7 + 0.224);
                T5 = 1.12 - 0.5 * (T7 + T6);
                Vgs_eff = Vgs - T5;
                dVgs_eff_dVg = 1.0 - (0.5 - 0.5 / T4) * (1.0 + T7 / T6);
            }
            else
            {
                Vgs_eff = Vgs;
                dVgs_eff_dVg = 1.0;
            }
            Vgst = Vgs_eff - Vth;

            /* Effective Vgst (Vgsteff) Calculation */

            T10 = 2.0 * n * Vtm;
            VgstNVt = Vgst / T10;
            ExpArg = (2.0 * pParam.BSIM3voff - Vgst) / T10;

            /* MCJ: Very small Vgst */
            if (VgstNVt > Transistor.EXP_THRESHOLD)
            {
                Vgsteff = Vgst;
                dVgsteff_dVg = dVgs_eff_dVg;
                dVgsteff_dVd = -dVth_dVd;
                dVgsteff_dVb = -dVth_dVb;
            }
            else if (ExpArg > Transistor.EXP_THRESHOLD)
            {
                T0 = (Vgst - pParam.BSIM3voff) / (n * Vtm);
                ExpVgst = Math.Exp(T0);
                Vgsteff = Vtm * pParam.BSIM3cdep0 / model.BSIM3cox * ExpVgst;
                dVgsteff_dVg = Vgsteff / (n * Vtm);
                dVgsteff_dVd = -dVgsteff_dVg * (dVth_dVd + T0 * Vtm * dn_dVd);
                dVgsteff_dVb = -dVgsteff_dVg * (dVth_dVb + T0 * Vtm * dn_dVb);
                dVgsteff_dVg *= dVgs_eff_dVg;
            }
            else
            {
                ExpVgst = Math.Exp(VgstNVt);
                T1 = T10 * Math.Log(1.0 + ExpVgst);
                dT1_dVg = ExpVgst / (1.0 + ExpVgst);
                dT1_dVb = -dT1_dVg * (dVth_dVb + Vgst / n * dn_dVb) + T1 / n * dn_dVb;
                dT1_dVd = -dT1_dVg * (dVth_dVd + Vgst / n * dn_dVd) + T1 / n * dn_dVd;

                dT2_dVg = -model.BSIM3cox / (Vtm * pParam.BSIM3cdep0) * Math.Exp(ExpArg);
                T2 = 1.0 - T10 * dT2_dVg;
                dT2_dVd = -dT2_dVg * (dVth_dVd - 2.0 * Vtm * ExpArg * dn_dVd) + (T2 - 1.0) / n * dn_dVd;
                dT2_dVb = -dT2_dVg * (dVth_dVb - 2.0 * Vtm * ExpArg * dn_dVb) + (T2 - 1.0) / n * dn_dVb;

                Vgsteff = T1 / T2;
                T3 = T2 * T2;
                dVgsteff_dVg = (T2 * dT1_dVg - T1 * dT2_dVg) / T3 * dVgs_eff_dVg;
                dVgsteff_dVd = (T2 * dT1_dVd - T1 * dT2_dVd) / T3;
                dVgsteff_dVb = (T2 * dT1_dVb - T1 * dT2_dVb) / T3;
            }
            BSIM3Vgsteff = Vgsteff;

            /* Calculate Effective Channel Geometry */
            T9 = sqrtPhis - pParam.BSIM3sqrtPhi;
            Weff = pParam.BSIM3weff - 2.0 * (pParam.BSIM3dwg * Vgsteff + pParam.BSIM3dwb * T9);
            dWeff_dVg = -2.0 * pParam.BSIM3dwg;
            dWeff_dVb = -2.0 * pParam.BSIM3dwb * dsqrtPhis_dVb;

            if (Weff < 2.0e-8)
            /* to avoid the discontinuity problem due to Weff */
            {
                T0 = 1.0 / (6.0e-8 - 2.0 * Weff);
                Weff = 2.0e-8 * (4.0e-8 - Weff) * T0;
                T0 *= T0 * 4.0e-16;
                dWeff_dVg *= T0;
                dWeff_dVb *= T0;
            }

            T0 = pParam.BSIM3prwg * Vgsteff + pParam.BSIM3prwb * T9;
            if (T0 >= -0.9)
            {
                Rds = pParam.BSIM3rds0 * (1.0 + T0);
                dRds_dVg = pParam.BSIM3rds0 * pParam.BSIM3prwg;
                dRds_dVb = pParam.BSIM3rds0 * pParam.BSIM3prwb * dsqrtPhis_dVb;
            }
            else
            /* to avoid the discontinuity problem due to prwg and prwb */
            {
                T1 = 1.0 / (17.0 + 20.0 * T0);
                Rds = pParam.BSIM3rds0 * (0.8 + T0) * T1;
                T1 *= T1;
                dRds_dVg = pParam.BSIM3rds0 * pParam.BSIM3prwg * T1;
                dRds_dVb = pParam.BSIM3rds0 * pParam.BSIM3prwb * dsqrtPhis_dVb * T1;
            }
            BSIM3rds = Rds; /* Noise Bugfix */

            /* Calculate Abulk */
            T1 = 0.5 * pParam.BSIM3k1ox / sqrtPhis;
            dT1_dVb = -T1 / sqrtPhis * dsqrtPhis_dVb;

            T9 = Math.Sqrt(pParam.BSIM3xj * Xdep);
            tmp1 = Leff + 2.0 * T9;
            T5 = Leff / tmp1;
            tmp2 = pParam.BSIM3a0 * T5;
            tmp3 = pParam.BSIM3weff + pParam.BSIM3b1;
            tmp4 = pParam.BSIM3b0 / tmp3;
            T2 = tmp2 + tmp4;
            dT2_dVb = -T9 / tmp1 / Xdep * dXdep_dVb;
            T6 = T5 * T5;
            T7 = T5 * T6;

            Abulk0 = 1.0 + T1 * T2;
            dAbulk0_dVb = T1 * tmp2 * dT2_dVb + T2 * dT1_dVb;

            T8 = pParam.BSIM3ags * pParam.BSIM3a0 * T7;
            dAbulk_dVg = -T1 * T8;
            Abulk = Abulk0 + dAbulk_dVg * Vgsteff;
            dAbulk_dVb = dAbulk0_dVb - T8 * Vgsteff * (dT1_dVb + 3.0 * T1 * dT2_dVb);

            if (Abulk0 < 0.1)
            /* added to avoid the problems caused by Abulk0 */
            {
                T9 = 1.0 / (3.0 - 20.0 * Abulk0);
                Abulk0 = (0.2 - Abulk0) * T9;
                dAbulk0_dVb *= T9 * T9;
            }

            if (Abulk < 0.1)
            /* added to avoid the problems caused by Abulk */
            {
                T9 = 1.0 / (3.0 - 20.0 * Abulk);
                Abulk = (0.2 - Abulk) * T9;
                T10 = T9 * T9;
                dAbulk_dVb *= T10;
                dAbulk_dVg *= T10;
            }
            BSIM3Abulk = Abulk;

            T2 = pParam.BSIM3keta * Vbseff;
            if (T2 >= -0.9)
            {
                T0 = 1.0 / (1.0 + T2);
                dT0_dVb = -pParam.BSIM3keta * T0 * T0;
            }
            else
            /* added to avoid the problems caused by Keta */
            {
                T1 = 1.0 / (0.8 + T2);
                T0 = (17.0 + 20.0 * T2) * T1;
                dT0_dVb = -pParam.BSIM3keta * T1 * T1;
            }
            dAbulk_dVg *= T0;
            dAbulk_dVb = dAbulk_dVb * T0 + Abulk * dT0_dVb;
            dAbulk0_dVb = dAbulk0_dVb * T0 + Abulk0 * dT0_dVb;
            Abulk *= T0;
            Abulk0 *= T0;

            /* Mobility calculation */
            if (model.BSIM3mobMod.Value == 1)
            {
                T0 = Vgsteff + Vth + Vth;
                T2 = pParam.BSIM3ua + pParam.BSIM3uc * Vbseff;
                T3 = T0 / model.BSIM3tox;
                T5 = T3 * (T2 + pParam.BSIM3ub * T3);
                dDenomi_dVg = (T2 + 2.0 * pParam.BSIM3ub * T3) / model.BSIM3tox;
                dDenomi_dVd = dDenomi_dVg * 2.0 * dVth_dVd;
                dDenomi_dVb = dDenomi_dVg * 2.0 * dVth_dVb + pParam.BSIM3uc * T3;
            }
            else if (model.BSIM3mobMod.Value == 2)
            {
                T5 = Vgsteff / model.BSIM3tox * (pParam.BSIM3ua + pParam.BSIM3uc * Vbseff + pParam.BSIM3ub * Vgsteff / model.BSIM3tox);
                dDenomi_dVg = (pParam.BSIM3ua + pParam.BSIM3uc * Vbseff + 2.0 * pParam.BSIM3ub * Vgsteff / model.BSIM3tox) / model.BSIM3tox;
                dDenomi_dVd = 0.0;
                dDenomi_dVb = Vgsteff * pParam.BSIM3uc / model.BSIM3tox;
            }
            else
            {
                T0 = Vgsteff + Vth + Vth;
                T2 = 1.0 + pParam.BSIM3uc * Vbseff;
                T3 = T0 / model.BSIM3tox;
                T4 = T3 * (pParam.BSIM3ua + pParam.BSIM3ub * T3);
                T5 = T4 * T2;
                dDenomi_dVg = (pParam.BSIM3ua + 2.0 * pParam.BSIM3ub * T3) * T2 / model.BSIM3tox;
                dDenomi_dVd = dDenomi_dVg * 2.0 * dVth_dVd;
                dDenomi_dVb = dDenomi_dVg * 2.0 * dVth_dVb + pParam.BSIM3uc * T4;
            }

            if (T5 >= -0.8)
            {
                Denomi = 1.0 + T5;
            }
            else /* Added to avoid the discontinuity problem caused by ua and ub */
            {
                T9 = 1.0 / (7.0 + 10.0 * T5);
                Denomi = (0.6 + T5) * T9;
                T9 *= T9;
                dDenomi_dVg *= T9;
                dDenomi_dVd *= T9;
                dDenomi_dVb *= T9;
            }

            BSIM3ueff = ueff = pParam.BSIM3u0temp / Denomi;
            T9 = -ueff / Denomi;
            dueff_dVg = T9 * dDenomi_dVg;
            dueff_dVd = T9 * dDenomi_dVd;
            dueff_dVb = T9 * dDenomi_dVb;

            /* Saturation Drain Voltage  Vdsat */
            WVCox = Weff * pParam.BSIM3vsattemp * model.BSIM3cox;
            WVCoxRds = WVCox * Rds;

            Esat = 2.0 * pParam.BSIM3vsattemp / ueff;
            EsatL = Esat * Leff;
            T0 = -EsatL / ueff;
            dEsatL_dVg = T0 * dueff_dVg;
            dEsatL_dVd = T0 * dueff_dVd;
            dEsatL_dVb = T0 * dueff_dVb;

            /* Sqrt() */
            a1 = pParam.BSIM3a1;
            if (a1 == 0.0)
            {
                Lambda = pParam.BSIM3a2;
                dLambda_dVg = 0.0;
            }
            else if (a1 > 0.0)
            /* Added to avoid the discontinuity problem
            caused by a1 and a2 (Lambda) */
            {
                T0 = 1.0 - pParam.BSIM3a2;
                T1 = T0 - pParam.BSIM3a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * T0);
                Lambda = pParam.BSIM3a2 + T0 - 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * pParam.BSIM3a1 * (1.0 + T1 / T2);
            }
            else
            {
                T1 = pParam.BSIM3a2 + pParam.BSIM3a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * pParam.BSIM3a2);
                Lambda = 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * pParam.BSIM3a1 * (1.0 + T1 / T2);
            }

            Vgst2Vtm = Vgsteff + 2.0 * Vtm;
            BSIM3AbovVgst2Vtm = Abulk / Vgst2Vtm;

            if (Rds > 0)
            {
                tmp2 = dRds_dVg / Rds + dWeff_dVg / Weff;
                tmp3 = dRds_dVb / Rds + dWeff_dVb / Weff;
            }
            else
            {
                tmp2 = dWeff_dVg / Weff;
                tmp3 = dWeff_dVb / Weff;
            }
            if ((Rds == 0.0) && (Lambda == 1.0))
            {
                T0 = 1.0 / (Abulk * EsatL + Vgst2Vtm);
                tmp1 = 0.0;
                T1 = T0 * T0;
                T2 = Vgst2Vtm * T0;
                T3 = EsatL * Vgst2Vtm;
                Vdsat = T3 * T0;

                dT0_dVg = -(Abulk * dEsatL_dVg + EsatL * dAbulk_dVg + 1.0) * T1;
                dT0_dVd = -(Abulk * dEsatL_dVd) * T1;
                dT0_dVb = -(Abulk * dEsatL_dVb + dAbulk_dVb * EsatL) * T1;

                dVdsat_dVg = T3 * dT0_dVg + T2 * dEsatL_dVg + EsatL * T0;
                dVdsat_dVd = T3 * dT0_dVd + T2 * dEsatL_dVd;
                dVdsat_dVb = T3 * dT0_dVb + T2 * dEsatL_dVb;
            }
            else
            {
                tmp1 = dLambda_dVg / (Lambda * Lambda);
                T9 = Abulk * WVCoxRds;
                T8 = Abulk * T9;
                T7 = Vgst2Vtm * T9;
                T6 = Vgst2Vtm * WVCoxRds;
                T0 = 2.0 * Abulk * (T9 - 1.0 + 1.0 / Lambda);
                dT0_dVg = 2.0 * (T8 * tmp2 - Abulk * tmp1 + (2.0 * T9 + 1.0 / Lambda - 1.0) * dAbulk_dVg);

                dT0_dVb = 2.0 * (T8 * (2.0 / Abulk * dAbulk_dVb + tmp3) + (1.0 / Lambda - 1.0) * dAbulk_dVb);
                dT0_dVd = 0.0;
                T1 = Vgst2Vtm * (2.0 / Lambda - 1.0) + Abulk * EsatL + 3.0 * T7;

                dT1_dVg = (2.0 / Lambda - 1.0) - 2.0 * Vgst2Vtm * tmp1 + Abulk * dEsatL_dVg + EsatL * dAbulk_dVg + 3.0 * (T9 + T7 * tmp2 + T6 *
                    dAbulk_dVg);
                dT1_dVb = Abulk * dEsatL_dVb + EsatL * dAbulk_dVb + 3.0 * (T6 * dAbulk_dVb + T7 * tmp3);
                dT1_dVd = Abulk * dEsatL_dVd;

                T2 = Vgst2Vtm * (EsatL + 2.0 * T6);
                dT2_dVg = EsatL + Vgst2Vtm * dEsatL_dVg + T6 * (4.0 + 2.0 * Vgst2Vtm * tmp2);
                dT2_dVb = Vgst2Vtm * (dEsatL_dVb + 2.0 * T6 * tmp3);
                dT2_dVd = Vgst2Vtm * dEsatL_dVd;

                T3 = Math.Sqrt(T1 * T1 - 2.0 * T0 * T2);
                Vdsat = (T1 - T3) / T0;

                dT3_dVg = (T1 * dT1_dVg - 2.0 * (T0 * dT2_dVg + T2 * dT0_dVg)) / T3;
                dT3_dVd = (T1 * dT1_dVd - 2.0 * (T0 * dT2_dVd + T2 * dT0_dVd)) / T3;
                dT3_dVb = (T1 * dT1_dVb - 2.0 * (T0 * dT2_dVb + T2 * dT0_dVb)) / T3;

                dVdsat_dVg = (dT1_dVg - (T1 * dT1_dVg - dT0_dVg * T2 - T0 * dT2_dVg) / T3 - Vdsat * dT0_dVg) / T0;
                dVdsat_dVb = (dT1_dVb - (T1 * dT1_dVb - dT0_dVb * T2 - T0 * dT2_dVb) / T3 - Vdsat * dT0_dVb) / T0;
                dVdsat_dVd = (dT1_dVd - (T1 * dT1_dVd - T0 * dT2_dVd) / T3) / T0;
            }
            BSIM3vdsat = Vdsat;

            /* Effective Vds (Vdseff) Calculation */
            T1 = Vdsat - Vds - pParam.BSIM3delta;
            dT1_dVg = dVdsat_dVg;
            dT1_dVd = dVdsat_dVd - 1.0;
            dT1_dVb = dVdsat_dVb;

            T2 = Math.Sqrt(T1 * T1 + 4.0 * pParam.BSIM3delta * Vdsat);
            T0 = T1 / T2;
            T3 = 2.0 * pParam.BSIM3delta / T2;
            dT2_dVg = T0 * dT1_dVg + T3 * dVdsat_dVg;
            dT2_dVd = T0 * dT1_dVd + T3 * dVdsat_dVd;
            dT2_dVb = T0 * dT1_dVb + T3 * dVdsat_dVb;

            Vdseff = Vdsat - 0.5 * (T1 + T2);
            dVdseff_dVg = dVdsat_dVg - 0.5 * (dT1_dVg + dT2_dVg);
            dVdseff_dVd = dVdsat_dVd - 0.5 * (dT1_dVd + dT2_dVd);
            dVdseff_dVb = dVdsat_dVb - 0.5 * (dT1_dVb + dT2_dVb);
            /* Added to eliminate non - zero Vdseff at Vds = 0.0 */
            if (Vds == 0.0)
            {
                Vdseff = 0.0;
                dVdseff_dVg = 0.0;
                dVdseff_dVb = 0.0;
            }

            /* Calculate VAsat */
            tmp4 = 1.0 - 0.5 * Abulk * Vdsat / Vgst2Vtm;
            T9 = WVCoxRds * Vgsteff;
            T8 = T9 / Vgst2Vtm;
            T0 = EsatL + Vdsat + 2.0 * T9 * tmp4;

            T7 = 2.0 * WVCoxRds * tmp4;
            dT0_dVg = dEsatL_dVg + dVdsat_dVg + T7 * (1.0 + tmp2 * Vgsteff) - T8 * (Abulk * dVdsat_dVg - Abulk * Vdsat / Vgst2Vtm + Vdsat *
                dAbulk_dVg);

            dT0_dVb = dEsatL_dVb + dVdsat_dVb + T7 * tmp3 * Vgsteff - T8 * (dAbulk_dVb * Vdsat + Abulk * dVdsat_dVb);
            dT0_dVd = dEsatL_dVd + dVdsat_dVd - T8 * Abulk * dVdsat_dVd;

            T9 = WVCoxRds * Abulk;
            T1 = 2.0 / Lambda - 1.0 + T9;
            dT1_dVg = -2.0 * tmp1 + WVCoxRds * (Abulk * tmp2 + dAbulk_dVg);
            dT1_dVb = dAbulk_dVb * WVCoxRds + T9 * tmp3;

            Vasat = T0 / T1;
            dVasat_dVg = (dT0_dVg - Vasat * dT1_dVg) / T1;
            dVasat_dVb = (dT0_dVb - Vasat * dT1_dVb) / T1;
            dVasat_dVd = dT0_dVd / T1;

            if (Vdseff > Vds)
                Vdseff = Vds;
            diffVds = Vds - Vdseff;
            BSIM3Vdseff = Vdseff;

            /* Calculate VACLM */
            if ((pParam.BSIM3pclm > 0.0) && (diffVds > 1.0e-10))
            {
                T0 = 1.0 / (pParam.BSIM3pclm * Abulk * pParam.BSIM3litl);
                dT0_dVb = -T0 / Abulk * dAbulk_dVb;
                dT0_dVg = -T0 / Abulk * dAbulk_dVg;

                T2 = Vgsteff / EsatL;
                T1 = Leff * (Abulk + T2);
                dT1_dVg = Leff * ((1.0 - T2 * dEsatL_dVg) / EsatL + dAbulk_dVg);
                dT1_dVb = Leff * (dAbulk_dVb - T2 * dEsatL_dVb / EsatL);
                dT1_dVd = -T2 * dEsatL_dVd / Esat;

                T9 = T0 * T1;
                VACLM = T9 * diffVds;
                dVACLM_dVg = T0 * dT1_dVg * diffVds - T9 * dVdseff_dVg + T1 * diffVds * dT0_dVg;
                dVACLM_dVb = (dT0_dVb * T1 + T0 * dT1_dVb) * diffVds - T9 * dVdseff_dVb;
                dVACLM_dVd = T0 * dT1_dVd * diffVds + T9 * (1.0 - dVdseff_dVd);
            }
            else
            {
                VACLM = Transistor.MAX_EXP;
                dVACLM_dVd = dVACLM_dVg = dVACLM_dVb = 0.0;
            }

            /* Calculate VADIBL */
            if (pParam.BSIM3thetaRout > 0.0)
            {
                T8 = Abulk * Vdsat;
                T0 = Vgst2Vtm * T8;
                dT0_dVg = Vgst2Vtm * Abulk * dVdsat_dVg + T8 + Vgst2Vtm * Vdsat * dAbulk_dVg;
                dT0_dVb = Vgst2Vtm * (dAbulk_dVb * Vdsat + Abulk * dVdsat_dVb);
                dT0_dVd = Vgst2Vtm * Abulk * dVdsat_dVd;

                T1 = Vgst2Vtm + T8;
                dT1_dVg = 1.0 + Abulk * dVdsat_dVg + Vdsat * dAbulk_dVg;
                dT1_dVb = Abulk * dVdsat_dVb + dAbulk_dVb * Vdsat;
                dT1_dVd = Abulk * dVdsat_dVd;

                T9 = T1 * T1;
                T2 = pParam.BSIM3thetaRout;
                VADIBL = (Vgst2Vtm - T0 / T1) / T2;
                dVADIBL_dVg = (1.0 - dT0_dVg / T1 + T0 * dT1_dVg / T9) / T2;
                dVADIBL_dVb = (-dT0_dVb / T1 + T0 * dT1_dVb / T9) / T2;
                dVADIBL_dVd = (-dT0_dVd / T1 + T0 * dT1_dVd / T9) / T2;

                T7 = pParam.BSIM3pdiblb * Vbseff;
                if (T7 >= -0.9)
                {
                    T3 = 1.0 / (1.0 + T7);
                    VADIBL *= T3;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = (dVADIBL_dVb - VADIBL * pParam.BSIM3pdiblb) * T3;
                    dVADIBL_dVd *= T3;
                }
                else
                /* Added to avoid the discontinuity problem caused by pdiblcb */
                {
                    T4 = 1.0 / (0.8 + T7);
                    T3 = (17.0 + 20.0 * T7) * T4;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = dVADIBL_dVb * T3 - VADIBL * pParam.BSIM3pdiblb * T4 * T4;
                    dVADIBL_dVd *= T3;
                    VADIBL *= T3;
                }
            }
            else
            {
                VADIBL = Transistor.MAX_EXP;
                dVADIBL_dVd = dVADIBL_dVg = dVADIBL_dVb = 0.0;
            }

            /* Calculate VA */

            T8 = pParam.BSIM3pvag / EsatL;
            T9 = T8 * Vgsteff;
            if (T9 > -0.9)
            {
                T0 = 1.0 + T9;
                dT0_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL);
                dT0_dVb = -T9 * dEsatL_dVb / EsatL;
                dT0_dVd = -T9 * dEsatL_dVd / EsatL;
            }
            else /* Added to avoid the discontinuity problems caused by pvag */
            {
                T1 = 1.0 / (17.0 + 20.0 * T9);
                T0 = (0.8 + T9) * T1;
                T1 *= T1;
                dT0_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL) * T1;

                T9 *= T1 / EsatL;
                dT0_dVb = -T9 * dEsatL_dVb;
                dT0_dVd = -T9 * dEsatL_dVd;
            }

            tmp1 = VACLM * VACLM;
            tmp2 = VADIBL * VADIBL;
            tmp3 = VACLM + VADIBL;

            T1 = VACLM * VADIBL / tmp3;
            tmp3 *= tmp3;
            dT1_dVg = (tmp1 * dVADIBL_dVg + tmp2 * dVACLM_dVg) / tmp3;
            dT1_dVd = (tmp1 * dVADIBL_dVd + tmp2 * dVACLM_dVd) / tmp3;
            dT1_dVb = (tmp1 * dVADIBL_dVb + tmp2 * dVACLM_dVb) / tmp3;

            Va = Vasat + T0 * T1;
            dVa_dVg = dVasat_dVg + T1 * dT0_dVg + T0 * dT1_dVg;
            dVa_dVd = dVasat_dVd + T1 * dT0_dVd + T0 * dT1_dVd;
            dVa_dVb = dVasat_dVb + T1 * dT0_dVb + T0 * dT1_dVb;

            /* Calculate VASCBE */
            if (pParam.BSIM3pscbe2 > 0.0)
            {
                if (diffVds > pParam.BSIM3pscbe1 * pParam.BSIM3litl / Transistor.EXP_THRESHOLD)
                {
                    T0 = pParam.BSIM3pscbe1 * pParam.BSIM3litl / diffVds;
                    VASCBE = Leff * Math.Exp(T0) / pParam.BSIM3pscbe2;
                    T1 = T0 * VASCBE / diffVds;
                    dVASCBE_dVg = T1 * dVdseff_dVg;
                    dVASCBE_dVd = -T1 * (1.0 - dVdseff_dVd);
                    dVASCBE_dVb = T1 * dVdseff_dVb;
                }
                else
                {
                    VASCBE = Transistor.MAX_EXP * Leff / pParam.BSIM3pscbe2;
                    dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
                }
            }
            else
            {
                VASCBE = Transistor.MAX_EXP;
                dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
            }

            /* Calculate Ids */
            CoxWovL = model.BSIM3cox * Weff / Leff;
            beta = ueff * CoxWovL;
            dbeta_dVg = CoxWovL * dueff_dVg + beta * dWeff_dVg / Weff;
            dbeta_dVd = CoxWovL * dueff_dVd;
            dbeta_dVb = CoxWovL * dueff_dVb + beta * dWeff_dVb / Weff;

            T0 = 1.0 - 0.5 * Abulk * Vdseff / Vgst2Vtm;
            dT0_dVg = -0.5 * (Abulk * dVdseff_dVg - Abulk * Vdseff / Vgst2Vtm + Vdseff * dAbulk_dVg) / Vgst2Vtm;
            dT0_dVd = -0.5 * Abulk * dVdseff_dVd / Vgst2Vtm;
            dT0_dVb = -0.5 * (Abulk * dVdseff_dVb + dAbulk_dVb * Vdseff) / Vgst2Vtm;

            fgche1 = Vgsteff * T0;
            dfgche1_dVg = Vgsteff * dT0_dVg + T0;
            dfgche1_dVd = Vgsteff * dT0_dVd;
            dfgche1_dVb = Vgsteff * dT0_dVb;

            T9 = Vdseff / EsatL;
            fgche2 = 1.0 + T9;
            dfgche2_dVg = (dVdseff_dVg - T9 * dEsatL_dVg) / EsatL;
            dfgche2_dVd = (dVdseff_dVd - T9 * dEsatL_dVd) / EsatL;
            dfgche2_dVb = (dVdseff_dVb - T9 * dEsatL_dVb) / EsatL;

            gche = beta * fgche1 / fgche2;
            dgche_dVg = (beta * dfgche1_dVg + fgche1 * dbeta_dVg - gche * dfgche2_dVg) / fgche2;
            dgche_dVd = (beta * dfgche1_dVd + fgche1 * dbeta_dVd - gche * dfgche2_dVd) / fgche2;
            dgche_dVb = (beta * dfgche1_dVb + fgche1 * dbeta_dVb - gche * dfgche2_dVb) / fgche2;

            T0 = 1.0 + gche * Rds;
            T9 = Vdseff / T0;
            Idl = gche * T9;

            dIdl_dVg = (gche * dVdseff_dVg + T9 * dgche_dVg) / T0 - Idl * gche / T0 * dRds_dVg;

            dIdl_dVd = (gche * dVdseff_dVd + T9 * dgche_dVd) / T0;
            dIdl_dVb = (gche * dVdseff_dVb + T9 * dgche_dVb - Idl * dRds_dVb * gche) / T0;

            T9 = diffVds / Va;
            T0 = 1.0 + T9;
            Idsa = Idl * T0;
            dIdsa_dVg = T0 * dIdl_dVg - Idl * (dVdseff_dVg + T9 * dVa_dVg) / Va;
            dIdsa_dVd = T0 * dIdl_dVd + Idl * (1.0 - dVdseff_dVd - T9 * dVa_dVd) / Va;
            dIdsa_dVb = T0 * dIdl_dVb - Idl * (dVdseff_dVb + T9 * dVa_dVb) / Va;

            T9 = diffVds / VASCBE;
            T0 = 1.0 + T9;
            Ids = Idsa * T0;

            Gm = T0 * dIdsa_dVg - Idsa * (dVdseff_dVg + T9 * dVASCBE_dVg) / VASCBE;
            Gds = T0 * dIdsa_dVd + Idsa * (1.0 - dVdseff_dVd - T9 * dVASCBE_dVd) / VASCBE;
            Gmb = T0 * dIdsa_dVb - Idsa * (dVdseff_dVb + T9 * dVASCBE_dVb) / VASCBE;

            Gds += Gm * dVgsteff_dVd;
            Gmb += Gm * dVgsteff_dVb;
            Gm *= dVgsteff_dVg;
            Gmb *= dVbseff_dVb;

            /* Substrate current begins */
            tmp = pParam.BSIM3alpha0 + pParam.BSIM3alpha1 * Leff;
            if ((tmp <= 0.0) || (pParam.BSIM3beta0 <= 0.0))
            {
                Isub = Gbd = Gbb = Gbg = 0.0;
            }
            else
            {
                T2 = tmp / Leff;
                if (diffVds > pParam.BSIM3beta0 / Transistor.EXP_THRESHOLD)
                {
                    T0 = -pParam.BSIM3beta0 / diffVds;
                    T1 = T2 * diffVds * Math.Exp(T0);
                    T3 = T1 / diffVds * (T0 - 1.0);
                    dT1_dVg = T3 * dVdseff_dVg;
                    dT1_dVd = T3 * (dVdseff_dVd - 1.0);
                    dT1_dVb = T3 * dVdseff_dVb;
                }
                else
                {
                    T3 = T2 * Transistor.MIN_EXP;
                    T1 = T3 * diffVds;
                    dT1_dVg = -T3 * dVdseff_dVg;
                    dT1_dVd = T3 * (1.0 - dVdseff_dVd);
                    dT1_dVb = -T3 * dVdseff_dVb;
                }
                Isub = T1 * Idsa;
                Gbg = T1 * dIdsa_dVg + Idsa * dT1_dVg;
                Gbd = T1 * dIdsa_dVd + Idsa * dT1_dVd;
                Gbb = T1 * dIdsa_dVb + Idsa * dT1_dVb;

                Gbd += Gbg * dVgsteff_dVd;
                Gbb += Gbg * dVgsteff_dVb;
                Gbg *= dVgsteff_dVg;
                Gbb *= dVbseff_dVb; /* bug fixing */
            }

            cdrain = Ids;
            BSIM3gds = Gds;
            BSIM3gm = Gm;
            BSIM3gmbs = Gmb;

            BSIM3gbbs = Gbb;
            BSIM3gbgs = Gbg;
            BSIM3gbds = Gbd;

            BSIM3csub = Isub;

            /* BSIM3v24 thermal noise Qinv calculated from all capMod 
            * 0, 1, 2 & 3 stored in BSIM3qinv1 / 1998 */

            if ((model.BSIM3xpart < 0) || (!ChargeComputationNeeded))
            {
                qgate = qdrn = qsrc = qbulk = 0.0;
                BSIM3cggb = BSIM3cgsb = BSIM3cgdb = 0.0;
                BSIM3cdgb = BSIM3cdsb = BSIM3cddb = 0.0;
                BSIM3cbgb = BSIM3cbsb = BSIM3cbdb = 0.0;
                BSIM3cqdb = BSIM3cqsb = BSIM3cqgb = BSIM3cqbb = 0.0;
                BSIM3gtau = 0.0;
                goto finished;
            }
            else if (model.BSIM3capMod.Value == 0)
            {
                if (Vbseff < 0.0)
                {
                    Vbseff = Vbs;
                    dVbseff_dVb = 1.0;
                }
                else
                {
                    Vbseff = pParam.BSIM3phi - Phis;
                    dVbseff_dVb = -dPhis_dVb;
                }

                Vfb = pParam.BSIM3vfbcv;
                Vth = Vfb + pParam.BSIM3phi + pParam.BSIM3k1ox * sqrtPhis;
                Vgst = Vgs_eff - Vth;
                dVth_dVb = pParam.BSIM3k1ox * dsqrtPhis_dVb;
                dVgst_dVb = -dVth_dVb;
                dVgst_dVg = dVgs_eff_dVg;

                CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;
                Arg1 = Vgs_eff - Vbseff - Vfb;

                if (Arg1 <= 0.0)
                {
                    qgate = CoxWL * Arg1;
                    qbulk = -qgate;
                    qdrn = 0.0;

                    BSIM3cggb = CoxWL * dVgs_eff_dVg;
                    BSIM3cgdb = 0.0;
                    BSIM3cgsb = CoxWL * (dVbseff_dVb - dVgs_eff_dVg);

                    BSIM3cdgb = 0.0;
                    BSIM3cddb = 0.0;
                    BSIM3cdsb = 0.0;

                    BSIM3cbgb = -CoxWL * dVgs_eff_dVg;
                    BSIM3cbdb = 0.0;
                    BSIM3cbsb = -BSIM3cgsb;
                    BSIM3qinv = 0.0;
                }
                else if (Vgst <= 0.0)
                {
                    T1 = 0.5 * pParam.BSIM3k1ox;
                    T2 = Math.Sqrt(T1 * T1 + Arg1);
                    qgate = CoxWL * pParam.BSIM3k1ox * (T2 - T1);
                    qbulk = -qgate;
                    qdrn = 0.0;

                    T0 = CoxWL * T1 / T2;
                    BSIM3cggb = T0 * dVgs_eff_dVg;
                    BSIM3cgdb = 0.0;
                    BSIM3cgsb = T0 * (dVbseff_dVb - dVgs_eff_dVg);

                    BSIM3cdgb = 0.0;
                    BSIM3cddb = 0.0;
                    BSIM3cdsb = 0.0;

                    BSIM3cbgb = -BSIM3cggb;
                    BSIM3cbdb = 0.0;
                    BSIM3cbsb = -BSIM3cgsb;
                    BSIM3qinv = 0.0;
                }
                else
                {
                    One_Third_CoxWL = CoxWL / 3.0;
                    Two_Third_CoxWL = 2.0 * One_Third_CoxWL;

                    AbulkCV = Abulk0 * pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    Vdsat = Vgst / AbulkCV;
                    dVdsat_dVg = dVgs_eff_dVg / AbulkCV;
                    dVdsat_dVb = -(Vdsat * dAbulkCV_dVb + dVth_dVb) / AbulkCV;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 Charge partition model */
                        if (Vdsat <= Vds)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.0;

                            BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            BSIM3cgsb = -(BSIM3cggb + T2);
                            BSIM3cgdb = 0.0;

                            BSIM3cdgb = 0.0;
                            BSIM3cddb = 0.0;
                            BSIM3cdsb = 0.0;

                            BSIM3cbgb = -(BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            BSIM3cbsb = -(BSIM3cbgb + T3);
                            BSIM3cbdb = 0.0;
                            BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            T7 = 2.0 * Vds - T1 - 3.0 * T3;
                            T8 = T3 - T1 - 2.0 * Vds;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - 0.5 * (Vds - T3));
                            T10 = T4 * T8;
                            qdrn = T4 * T7;
                            qbulk = -(qgate + qdrn + T10);

                            T5 = T3 / T1;
                            BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = -CoxWL * T5 * dVdsat_dVb;
                            BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            BSIM3cgsb = -(BSIM3cggb + T11 + BSIM3cgdb);
                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);
                            T7 = T9 * T7;
                            T8 = T9 * T8;
                            T9 = 2.0 * T4 * (1.0 - 3.0 * T5);
                            BSIM3cdgb = (T7 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T12 = T7 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            BSIM3cddb = T4 * (3.0 - 6.0 * T2 - 3.0 * T5);
                            BSIM3cdsb = -(BSIM3cdgb + T12 + BSIM3cddb);

                            T9 = 2.0 * T4 * (1.0 + T5);
                            T10 = (T8 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = T8 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            T12 = T4 * (2.0 * T2 + T5 - 1.0);
                            T0 = -(T10 + T11 + T12);

                            BSIM3cbgb = -(BSIM3cggb + BSIM3cdgb + T10);
                            BSIM3cbdb = -(BSIM3cgdb + BSIM3cddb + T12);
                            BSIM3cbsb = -(BSIM3cgsb + BSIM3cdsb + T0);
                            BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 Charge partition model */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.4 * T2;

                            BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            BSIM3cgsb = -(BSIM3cggb + T2);
                            BSIM3cgdb = 0.0;

                            T3 = 0.4 * Two_Third_CoxWL;
                            BSIM3cdgb = -T3 * dVgs_eff_dVg;
                            BSIM3cddb = 0.0;
                            T4 = T3 * dVth_dVb;
                            BSIM3cdsb = -(T4 + BSIM3cdgb);

                            BSIM3cbgb = -(BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            BSIM3cbsb = -(BSIM3cbgb + T3);
                            BSIM3cbdb = 0.0;
                            BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            BSIM3cgsb = -(BSIM3cggb + BSIM3cgdb + tmp);

                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);

                            T6 = 8.0 * Vdsat * Vdsat - 6.0 * Vdsat * Vds + 1.2 * Vds * Vds;
                            T8 = T2 / T1;
                            T7 = Vds - T1 - T8 * T6;
                            qdrn = T4 * T7;
                            T7 *= T9;
                            tmp = T8 / T1;
                            tmp1 = T4 * (2.0 - 4.0 * tmp * T6 + T8 * (16.0 * Vdsat - 6.0 * Vds));

                            BSIM3cdgb = (T7 * dAlphaz_dVg - tmp1 * dVdsat_dVg) * dVgs_eff_dVg;
                            T10 = T7 * dAlphaz_dVb - tmp1 * dVdsat_dVb;
                            BSIM3cddb = T4 * (2.0 - (1.0 / (3.0 * T1 * T1) + 2.0 * tmp) * T6 + T8 * (6.0 * Vdsat - 2.4 * Vds));
                            BSIM3cdsb = -(BSIM3cdgb + T10 + BSIM3cddb);

                            T7 = 2.0 * (T1 + T3);
                            qbulk = -(qgate - T4 * T7);
                            T7 *= T9;
                            T0 = 4.0 * T4 * (1.0 - T5);
                            T12 = (-T7 * dAlphaz_dVg - BSIM3cdgb - T0 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = -T7 * dAlphaz_dVb - T10 - T0 * dVdsat_dVb;
                            T10 = -4.0 * T4 * (T2 - 0.5 + 0.5 * T5) - BSIM3cddb;
                            tmp = -(T10 + T11 + T12);

                            BSIM3cbgb = -(BSIM3cggb + BSIM3cdgb + T12);
                            BSIM3cbdb = -(BSIM3cgdb + BSIM3cddb + T10); /* bug fix */
                            BSIM3cbsb = -(BSIM3cgsb + BSIM3cdsb + tmp);
                            BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                    else
                    {
                        /* 50 / 50 partitioning */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.5 * T2;

                            BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            BSIM3cgsb = -(BSIM3cggb + T2);
                            BSIM3cgdb = 0.0;

                            BSIM3cdgb = -One_Third_CoxWL * dVgs_eff_dVg;
                            BSIM3cddb = 0.0;
                            T4 = One_Third_CoxWL * dVth_dVb;
                            BSIM3cdsb = -(T4 + BSIM3cdgb);

                            BSIM3cbgb = -(BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            BSIM3cbsb = -(BSIM3cbgb + T3);
                            BSIM3cbdb = 0.0;
                            BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            qgate = CoxWL * (Vgs_eff - Vfb - pParam.BSIM3phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            BSIM3cgsb = -(BSIM3cggb + BSIM3cgdb + tmp);

                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);

                            T7 = T1 + T3;
                            qdrn = -T4 * T7;
                            qbulk = -(qgate + qdrn + qdrn);
                            T7 *= T9;
                            T0 = T4 * (2.0 * T5 - 2.0);

                            BSIM3cdgb = (T0 * dVdsat_dVg - T7 * dAlphaz_dVg) * dVgs_eff_dVg;
                            T12 = T0 * dVdsat_dVb - T7 * dAlphaz_dVb;
                            BSIM3cddb = T4 * (1.0 - 2.0 * T2 - T5);
                            BSIM3cdsb = -(BSIM3cdgb + T12 + BSIM3cddb);

                            BSIM3cbgb = -(BSIM3cggb + 2.0 * BSIM3cdgb);
                            BSIM3cbdb = -(BSIM3cgdb + 2.0 * BSIM3cddb);
                            BSIM3cbsb = -(BSIM3cgsb + 2.0 * BSIM3cdsb);
                            BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                }
            }
            else
            {
                if (Vbseff < 0.0)
                {
                    VbseffCV = Vbseff;
                    dVbseffCV_dVb = 1.0;
                }
                else
                {
                    VbseffCV = pParam.BSIM3phi - Phis;
                    dVbseffCV_dVb = -dPhis_dVb;
                }

                CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;

                /* Seperate VgsteffCV with noff and voffcv */
                noff = n * pParam.BSIM3noff;
                dnoff_dVd = pParam.BSIM3noff * dn_dVd;
                dnoff_dVb = pParam.BSIM3noff * dn_dVb;
                T0 = Vtm * noff;
                voffcv = pParam.BSIM3voffcv;
                VgstNVt = (Vgst - voffcv) / T0;

                if (VgstNVt > Transistor.EXP_THRESHOLD)
                {
                    Vgsteff = Vgst - voffcv;
                    dVgsteff_dVg = dVgs_eff_dVg;
                    dVgsteff_dVd = -dVth_dVd;
                    dVgsteff_dVb = -dVth_dVb;
                }
                else if (VgstNVt < -Transistor.EXP_THRESHOLD)
                {
                    Vgsteff = T0 * Math.Log(1.0 + Transistor.MIN_EXP);
                    dVgsteff_dVg = 0.0;
                    dVgsteff_dVd = Vgsteff / noff;
                    dVgsteff_dVb = dVgsteff_dVd * dnoff_dVb;
                    dVgsteff_dVd *= dnoff_dVd;
                }
                else
                {
                    ExpVgst = Math.Exp(VgstNVt);
                    Vgsteff = T0 * Math.Log(1.0 + ExpVgst);
                    dVgsteff_dVg = ExpVgst / (1.0 + ExpVgst);
                    dVgsteff_dVd = -dVgsteff_dVg * (dVth_dVd + (Vgst - voffcv) / noff * dnoff_dVd) + Vgsteff / noff * dnoff_dVd;
                    dVgsteff_dVb = -dVgsteff_dVg * (dVth_dVb + (Vgst - voffcv) / noff * dnoff_dVb) + Vgsteff / noff * dnoff_dVb;
                    dVgsteff_dVg *= dVgs_eff_dVg;
                } /* End of VgsteffCV */

                if (model.BSIM3capMod.Value == 1)
                {
                    Vfb = pParam.BSIM3vfbzb;
                    Arg1 = Vgs_eff - VbseffCV - Vfb - Vgsteff;

                    if (Arg1 <= 0.0)
                    {
                        qgate = CoxWL * Arg1;
                        Cgg = CoxWL * (dVgs_eff_dVg - dVgsteff_dVg);
                        Cgd = -CoxWL * dVgsteff_dVd;
                        Cgb = -CoxWL * (dVbseffCV_dVb + dVgsteff_dVb);
                    }
                    else
                    {
                        T0 = 0.5 * pParam.BSIM3k1ox;
                        T1 = Math.Sqrt(T0 * T0 + Arg1);
                        T2 = CoxWL * T0 / T1;

                        qgate = CoxWL * pParam.BSIM3k1ox * (T1 - T0);

                        Cgg = T2 * (dVgs_eff_dVg - dVgsteff_dVg);
                        Cgd = -T2 * dVgsteff_dVd;
                        Cgb = -T2 * (dVbseffCV_dVb + dVgsteff_dVb);
                    }
                    qbulk = -qgate;
                    Cbg = -Cgg;
                    Cbd = -Cgd;
                    Cbb = -Cgb;

                    One_Third_CoxWL = CoxWL / 3.0;
                    Two_Third_CoxWL = 2.0 * One_Third_CoxWL;
                    AbulkCV = Abulk0 * pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = Vgsteff / AbulkCV;
                    if (VdsatCV < Vds)
                    {
                        dVdsatCV_dVg = 1.0 / AbulkCV;
                        dVdsatCV_dVb = -VdsatCV * dAbulkCV_dVb / AbulkCV;
                        T0 = Vgsteff - VdsatCV / 3.0;
                        dT0_dVg = 1.0 - dVdsatCV_dVg / 3.0;
                        dT0_dVb = -dVdsatCV_dVb / 3.0;
                        qgate += CoxWL * T0;
                        Cgg1 = CoxWL * dT0_dVg;
                        Cgb1 = CoxWL * dT0_dVb + Cgg1 * dVgsteff_dVb;
                        Cgd1 = Cgg1 * dVgsteff_dVd;
                        Cgg1 *= dVgsteff_dVg;
                        Cgg += Cgg1;
                        Cgb += Cgb1;
                        Cgd += Cgd1;

                        T0 = VdsatCV - Vgsteff;
                        dT0_dVg = dVdsatCV_dVg - 1.0;
                        dT0_dVb = dVdsatCV_dVb;
                        qbulk += One_Third_CoxWL * T0;
                        Cbg1 = One_Third_CoxWL * dT0_dVg;
                        Cbb1 = One_Third_CoxWL * dT0_dVb + Cbg1 * dVgsteff_dVb;
                        Cbd1 = Cbg1 * dVgsteff_dVd;
                        Cbg1 *= dVgsteff_dVg;
                        Cbg += Cbg1;
                        Cbb += Cbb1;
                        Cbd += Cbd1;

                        if (model.BSIM3xpart > 0.5)
                            T0 = -Two_Third_CoxWL;
                        else if (model.BSIM3xpart < 0.5)
                            T0 = -0.4 * CoxWL;
                        else
                            T0 = -One_Third_CoxWL;

                        qsrc = T0 * Vgsteff;
                        Csg = T0 * dVgsteff_dVg;
                        Csb = T0 * dVgsteff_dVb;
                        Csd = T0 * dVgsteff_dVd;
                        Cgb *= dVbseff_dVb;
                        Cbb *= dVbseff_dVb;
                        Csb *= dVbseff_dVb;
                    }
                    else
                    {
                        T0 = AbulkCV * Vds;
                        T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1.0e-20);
                        T2 = Vds / T1;
                        T3 = T0 * T2;
                        dT3_dVg = -12.0 * T2 * T2 * AbulkCV;
                        dT3_dVd = 6.0 * T0 * (4.0 * Vgsteff - T0) / T1 / T1 - 0.5;
                        dT3_dVb = 12.0 * T2 * T2 * dAbulkCV_dVb * Vgsteff;

                        qgate += CoxWL * (Vgsteff - 0.5 * Vds + T3);
                        Cgg1 = CoxWL * (1.0 + dT3_dVg);
                        Cgb1 = CoxWL * dT3_dVb + Cgg1 * dVgsteff_dVb;
                        Cgd1 = CoxWL * dT3_dVd + Cgg1 * dVgsteff_dVd;
                        Cgg1 *= dVgsteff_dVg;
                        Cgg += Cgg1;
                        Cgb += Cgb1;
                        Cgd += Cgd1;

                        qbulk += CoxWL * (1.0 - AbulkCV) * (0.5 * Vds - T3);
                        Cbg1 = -CoxWL * ((1.0 - AbulkCV) * dT3_dVg);
                        Cbb1 = -CoxWL * ((1.0 - AbulkCV) * dT3_dVb + (0.5 * Vds - T3) * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb;
                        Cbd1 = -CoxWL * (1.0 - AbulkCV) * dT3_dVd + Cbg1 * dVgsteff_dVd;
                        Cbg1 *= dVgsteff_dVg;
                        Cbg += Cbg1;
                        Cbb += Cbb1;
                        Cbd += Cbd1;

                        if (model.BSIM3xpart > 0.5)
                        {
                            /* 0 / 100 Charge petition model */
                            T1 = T1 + T1;
                            qsrc = -CoxWL * (0.5 * Vgsteff + 0.25 * T0 - T0 * T0 / T1);
                            Csg = -CoxWL * (0.5 + 24.0 * T0 * Vds / T1 / T1 * AbulkCV);
                            Csb = -CoxWL * (0.25 * Vds * dAbulkCV_dVb - 12.0 * T0 * Vds / T1 / T1 * (4.0 * Vgsteff - T0) * dAbulkCV_dVb) + Csg *
                                dVgsteff_dVb;
                            Csd = -CoxWL * (0.25 * AbulkCV - 12.0 * AbulkCV * T0 / T1 / T1 * (4.0 * Vgsteff - T0)) + Csg * dVgsteff_dVd;
                            Csg *= dVgsteff_dVg;
                        }
                        else if (model.BSIM3xpart < 0.5)
                        {
                            /* 40 / 60 Charge petition model */
                            T1 = T1 / 12.0;
                            T2 = 0.5 * CoxWL / (T1 * T1);
                            T3 = Vgsteff * (2.0 * T0 * T0 / 3.0 + Vgsteff * (Vgsteff - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                            qsrc = -T2 * T3;
                            T4 = 4.0 / 3.0 * Vgsteff * (Vgsteff - T0) + 0.4 * T0 * T0;
                            Csg = -2.0 * qsrc / T1 - T2 * (Vgsteff * (3.0 * Vgsteff - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                            Csb = (qsrc / T1 * Vds + T2 * T4 * Vds) * dAbulkCV_dVb + Csg * dVgsteff_dVb;
                            Csd = (qsrc / T1 + T2 * T4) * AbulkCV + Csg * dVgsteff_dVd;
                            Csg *= dVgsteff_dVg;
                        }
                        else
                        {
                            /* 50 / 50 Charge petition model */
                            qsrc = -0.5 * (qgate + qbulk);
                            Csg = -0.5 * (Cgg1 + Cbg1);
                            Csb = -0.5 * (Cgb1 + Cbb1);
                            Csd = -0.5 * (Cgd1 + Cbd1);
                        }
                        Cgb *= dVbseff_dVb;
                        Cbb *= dVbseff_dVb;
                        Csb *= dVbseff_dVb;
                    }
                    qdrn = -(qgate + qbulk + qsrc);
                    BSIM3cggb = Cgg;
                    BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    BSIM3cgdb = Cgd;
                    BSIM3cdgb = -(Cgg + Cbg + Csg);
                    BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    BSIM3cddb = -(Cgd + Cbd + Csd);
                    BSIM3cbgb = Cbg;
                    BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    BSIM3cbdb = Cbd;
                    BSIM3qinv = -(qgate + qbulk);
                }

                else if (model.BSIM3capMod.Value == 2)
                {
                    Vfb = pParam.BSIM3vfbzb;
                    V3 = Vfb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (Vfb <= 0.0)
                    {
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * Vfb);
                        T2 = -Transistor.DELTA_3 / T0;
                    }
                    else
                    {
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * Vfb);
                        T2 = Transistor.DELTA_3 / T0;
                    }

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = Vfb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;
                    Qac0 = CoxWL * (Vfbeff - Vfb);
                    dQac0_dVg = CoxWL * dVfbeff_dVg;
                    dQac0_dVb = CoxWL * dVfbeff_dVb;

                    T0 = 0.5 * pParam.BSIM3k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (pParam.BSIM3k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / pParam.BSIM3k1ox;
                        T2 = CoxWL;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWL * T0 / T1;
                    }

                    Qsub0 = CoxWL * pParam.BSIM3k1ox * (T1 - T0);

                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg);
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb);

                    AbulkCV = Abulk0 * pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = Vgsteff / AbulkCV;

                    V4 = VdsatCV - Vds - Transistor.DELTA_4;
                    T0 = Math.Sqrt(V4 * V4 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    VdseffCV = VdsatCV - 0.5 * (V4 + T0);
                    T1 = 0.5 * (1.0 + V4 / T0);
                    T2 = Transistor.DELTA_4 / T0;
                    T3 = (1.0 - T1 - T2) / AbulkCV;
                    dVdseffCV_dVg = T3;
                    dVdseffCV_dVd = T1;
                    dVdseffCV_dVb = -T3 * VdsatCV * dAbulkCV_dVb;
                    /* Added to eliminate non - zero VdseffCV at Vds = 0.0 */
                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1e-20);
                    T2 = VdseffCV / T1;
                    T3 = T0 * T2;

                    T4 = (1.0 - 12.0 * T2 * T2 * AbulkCV);
                    T5 = (6.0 * T0 * (4.0 * Vgsteff - T0) / (T1 * T1) - 0.5);
                    T6 = 12.0 * T2 * T2 * Vgsteff;

                    qinoi = -CoxWL * (Vgsteff - 0.5 * T0 + AbulkCV * T3);
                    qgate = CoxWL * (Vgsteff - 0.5 * VdseffCV + T3);
                    Cgg1 = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                    Cgd1 = CoxWL * T5 * dVdseffCV_dVd + Cgg1 * dVgsteff_dVd;
                    Cgb1 = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cgg1 * dVgsteff_dVb;
                    Cgg1 *= dVgsteff_dVg;

                    T7 = 1.0 - AbulkCV;
                    qbulk = CoxWL * T7 * (0.5 * VdseffCV - T3);
                    T4 = -T7 * (T4 - 1.0);
                    T5 = -T7 * T5;
                    T6 = -(T7 * T6 + (0.5 * VdseffCV - T3));
                    Cbg1 = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                    Cbd1 = CoxWL * T5 * dVdseffCV_dVd + Cbg1 * dVgsteff_dVd;
                    Cbb1 = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb;
                    Cbg1 *= dVgsteff_dVg;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 Charge petition model */
                        T1 = T1 + T1;
                        qsrc = -CoxWL * (0.5 * Vgsteff + 0.25 * T0 - T0 * T0 / T1);
                        T7 = (4.0 * Vgsteff - T0) / (T1 * T1);
                        T4 = -(0.5 + 24.0 * T0 * T0 / (T1 * T1));
                        T5 = -(0.25 * AbulkCV - 12.0 * AbulkCV * T0 * T7);
                        T6 = -(0.25 * VdseffCV - 12.0 * T0 * VdseffCV * T7);
                        Csg = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                        Csd = CoxWL * T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd;
                        Csb = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb;
                        Csg *= dVgsteff_dVg;
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 Charge petition model */
                        T1 = T1 / 12.0;
                        T2 = 0.5 * CoxWL / (T1 * T1);
                        T3 = Vgsteff * (2.0 * T0 * T0 / 3.0 + Vgsteff * (Vgsteff - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                        qsrc = -T2 * T3;
                        T7 = 4.0 / 3.0 * Vgsteff * (Vgsteff - T0) + 0.4 * T0 * T0;
                        T4 = -2.0 * qsrc / T1 - T2 * (Vgsteff * (3.0 * Vgsteff - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                        T5 = (qsrc / T1 + T2 * T7) * AbulkCV;
                        T6 = (qsrc / T1 * VdseffCV + T2 * T7 * VdseffCV);
                        Csg = (T4 + T5 * dVdseffCV_dVg);
                        Csd = T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd;
                        Csb = (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb;
                        Csg *= dVgsteff_dVg;
                    }
                    else
                    {
                        /* 50 / 50 Charge petition model */
                        qsrc = -0.5 * (qgate + qbulk);
                        Csg = -0.5 * (Cgg1 + Cbg1);
                        Csb = -0.5 * (Cgb1 + Cbb1);
                        Csd = -0.5 * (Cgd1 + Cbd1);
                    }

                    qgate += Qac0 + Qsub0;
                    qbulk -= (Qac0 + Qsub0);
                    qdrn = -(qgate + qbulk + qsrc);

                    Cgg = dQac0_dVg + dQsub0_dVg + Cgg1;
                    Cgd = dQsub0_dVd + Cgd1;
                    Cgb = dQac0_dVb + dQsub0_dVb + Cgb1;

                    Cbg = Cbg1 - dQac0_dVg - dQsub0_dVg;
                    Cbd = Cbd1 - dQsub0_dVd;
                    Cbb = Cbb1 - dQac0_dVb - dQsub0_dVb;

                    Cgb *= dVbseff_dVb;
                    Cbb *= dVbseff_dVb;
                    Csb *= dVbseff_dVb;

                    BSIM3cggb = Cgg;
                    BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    BSIM3cgdb = Cgd;
                    BSIM3cdgb = -(Cgg + Cbg + Csg);
                    BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    BSIM3cddb = -(Cgd + Cbd + Csd);
                    BSIM3cbgb = Cbg;
                    BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    BSIM3cbdb = Cbd;
                    BSIM3qinv = qinoi;
                }

                /* New Charge - Thickness capMod (CTM) begins */
                else if (model.BSIM3capMod.Value == 3)
                {
                    V3 = pParam.BSIM3vfbzb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (pParam.BSIM3vfbzb <= 0.0)
                    {
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * pParam.BSIM3vfbzb);
                        T2 = -Transistor.DELTA_3 / T0;
                    }
                    else
                    {
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * pParam.BSIM3vfbzb);
                        T2 = Transistor.DELTA_3 / T0;
                    }

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = pParam.BSIM3vfbzb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;

                    Cox = model.BSIM3cox;
                    Tox = 1.0e8 * model.BSIM3tox;
                    T0 = (Vgs_eff - VbseffCV - pParam.BSIM3vfbzb) / Tox;
                    dT0_dVg = dVgs_eff_dVg / Tox;
                    dT0_dVb = -dVbseffCV_dVb / Tox;

                    tmp = T0 * pParam.BSIM3acde;
                    if ((-Transistor.EXP_THRESHOLD < tmp) && (tmp < Transistor.EXP_THRESHOLD))
                    {
                        Tcen = pParam.BSIM3ldeb * Math.Exp(tmp);
                        dTcen_dVg = pParam.BSIM3acde * Tcen;
                        dTcen_dVb = dTcen_dVg * dT0_dVb;
                        dTcen_dVg *= dT0_dVg;
                    }
                    else if (tmp <= -Transistor.EXP_THRESHOLD)
                    {
                        Tcen = pParam.BSIM3ldeb * Transistor.MIN_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }
                    else
                    {
                        Tcen = pParam.BSIM3ldeb * Transistor.MAX_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }

                    LINK = 1.0e-3 * model.BSIM3tox;
                    V3 = pParam.BSIM3ldeb - Tcen - LINK;
                    V4 = Math.Sqrt(V3 * V3 + 4.0 * LINK * pParam.BSIM3ldeb);
                    Tcen = pParam.BSIM3ldeb - 0.5 * (V3 + V4);
                    T1 = 0.5 * (1.0 + V3 / V4);
                    dTcen_dVg *= T1;
                    dTcen_dVb *= T1;

                    Ccen = Transistor.EPSSI / Tcen;
                    T2 = Cox / (Cox + Ccen);
                    Coxeff = T2 * Ccen;
                    T3 = -Ccen / Tcen;
                    dCoxeff_dVg = T2 * T2 * T3;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / Cox;

                    Qac0 = CoxWLcen * (Vfbeff - pParam.BSIM3vfbzb);
                    QovCox = Qac0 / Coxeff;
                    dQac0_dVg = CoxWLcen * dVfbeff_dVg + QovCox * dCoxeff_dVg;
                    dQac0_dVb = CoxWLcen * dVfbeff_dVb + QovCox * dCoxeff_dVb;

                    T0 = 0.5 * pParam.BSIM3k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (pParam.BSIM3k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / pParam.BSIM3k1ox;
                        T2 = CoxWLcen;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWLcen * T0 / T1;
                    }

                    Qsub0 = CoxWLcen * pParam.BSIM3k1ox * (T1 - T0);
                    QovCox = Qsub0 / Coxeff;
                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg) + QovCox * dCoxeff_dVg;
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb) + QovCox * dCoxeff_dVb;

                    /* Gate - bias dependent delta Phis begins */
                    if (pParam.BSIM3k1ox <= 0.0)
                    {
                        Denomi = 0.25 * pParam.BSIM3moin * Vtm;
                        T0 = 0.5 * pParam.BSIM3sqrtPhi;
                    }
                    else
                    {
                        Denomi = pParam.BSIM3moin * Vtm * pParam.BSIM3k1ox * pParam.BSIM3k1ox;
                        T0 = pParam.BSIM3k1ox * pParam.BSIM3sqrtPhi;
                    }
                    T1 = 2.0 * T0 + Vgsteff;

                    DeltaPhi = Vtm * Math.Log(1.0 + T1 * Vgsteff / Denomi);
                    dDeltaPhi_dVg = 2.0 * Vtm * (T1 - T0) / (Denomi + T1 * Vgsteff);
                    dDeltaPhi_dVd = dDeltaPhi_dVg * dVgsteff_dVd;
                    dDeltaPhi_dVb = dDeltaPhi_dVg * dVgsteff_dVb;
                    /* End of delta Phis */

                    T3 = 4.0 * (Vth - pParam.BSIM3vfbzb - pParam.BSIM3phi);
                    Tox += Tox;
                    if (T3 >= 0.0)
                    {
                        T0 = (Vgsteff + T3) / Tox;
                        dT0_dVd = (dVgsteff_dVd + 4.0 * dVth_dVd) / Tox;
                        dT0_dVb = (dVgsteff_dVb + 4.0 * dVth_dVb) / Tox;
                    }
                    else
                    {
                        T0 = (Vgsteff + 1.0e-20) / Tox;
                        dT0_dVd = dVgsteff_dVd / Tox;
                        dT0_dVb = dVgsteff_dVb / Tox;
                    }
                    tmp = Math.Exp(0.7 * Math.Log(T0));
                    T1 = 1.0 + tmp;
                    T2 = 0.7 * tmp / (T0 * Tox);
                    Tcen = 1.9e-9 / T1;
                    dTcen_dVg = -1.9e-9 * T2 / T1 / T1;
                    dTcen_dVd = Tox * dTcen_dVg;
                    dTcen_dVb = dTcen_dVd * dT0_dVb;
                    dTcen_dVd *= dT0_dVd;
                    dTcen_dVg *= dVgsteff_dVg;

                    Ccen = Transistor.EPSSI / Tcen;
                    T0 = Cox / (Cox + Ccen);
                    Coxeff = T0 * Ccen;
                    T1 = -Ccen / Tcen;
                    dCoxeff_dVg = T0 * T0 * T1;
                    dCoxeff_dVd = dCoxeff_dVg * dTcen_dVd;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / Cox;

                    AbulkCV = Abulk0 * pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = (Vgsteff - DeltaPhi) / AbulkCV;
                    V4 = VdsatCV - Vds - Transistor.DELTA_4;
                    T0 = Math.Sqrt(V4 * V4 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    VdseffCV = VdsatCV - 0.5 * (V4 + T0);
                    T1 = 0.5 * (1.0 + V4 / T0);
                    T2 = Transistor.DELTA_4 / T0;
                    T3 = (1.0 - T1 - T2) / AbulkCV;
                    T4 = T3 * (1.0 - dDeltaPhi_dVg);
                    dVdseffCV_dVg = T4;
                    dVdseffCV_dVd = T1;
                    dVdseffCV_dVb = -T3 * VdsatCV * dAbulkCV_dVb;
                    /* Added to eliminate non - zero VdseffCV at Vds = 0.0 */
                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = Vgsteff - DeltaPhi;
                    T2 = 12.0 * (T1 - 0.5 * T0 + 1.0e-20);
                    T3 = T0 / T2;
                    T4 = 1.0 - 12.0 * T3 * T3;
                    T5 = AbulkCV * (6.0 * T0 * (4.0 * T1 - T0) / (T2 * T2) - 0.5);
                    T6 = T5 * VdseffCV / AbulkCV;

                    qgate = qinoi = CoxWLcen * (T1 - T0 * (0.5 - T3));
                    QovCox = qgate / Coxeff;
                    Cgg1 = CoxWLcen * (T4 * (1.0 - dDeltaPhi_dVg) + T5 * dVdseffCV_dVg);
                    Cgd1 = CoxWLcen * T5 * dVdseffCV_dVd + Cgg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cgb1 = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cgg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cgg1 = Cgg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    T7 = 1.0 - AbulkCV;
                    T8 = T2 * T2;
                    T9 = 12.0 * T7 * T0 * T0 / (T8 * AbulkCV);
                    T10 = T9 * (1.0 - dDeltaPhi_dVg);
                    T11 = -T7 * T5 / AbulkCV;
                    T12 = -(T9 * T1 / AbulkCV + VdseffCV * (0.5 - T0 / T2));

                    qbulk = CoxWLcen * T7 * (0.5 * VdseffCV - T0 * VdseffCV / T2);
                    QovCox = qbulk / Coxeff;
                    Cbg1 = CoxWLcen * (T10 + T11 * dVdseffCV_dVg);
                    Cbd1 = CoxWLcen * T11 * dVdseffCV_dVd + Cbg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cbb1 = CoxWLcen * (T11 * dVdseffCV_dVb + T12 * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cbg1 = Cbg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 partition */
                        qsrc = -CoxWLcen * (T1 / 2.0 + T0 / 4.0 - 0.5 * T0 * T0 / T2);
                        QovCox = qsrc / Coxeff;
                        T2 += T2;
                        T3 = T2 * T2;
                        T7 = -(0.25 - 12.0 * T0 * (4.0 * T1 - T0) / T3);
                        T4 = -(0.5 + 24.0 * T0 * T0 / T3) * (1.0 - dDeltaPhi_dVg);
                        T5 = T7 * AbulkCV;
                        T6 = T7 * VdseffCV;

                        Csg = CoxWLcen * (T4 + T5 * dVdseffCV_dVg);
                        Csd = CoxWLcen * T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                        Csb = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                        Csg = Csg * dVgsteff_dVg + QovCox * dCoxeff_dVg;
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 partition */
                        T2 = T2 / 12.0;
                        T3 = 0.5 * CoxWLcen / (T2 * T2);
                        T4 = T1 * (2.0 * T0 * T0 / 3.0 + T1 * (T1 - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                        qsrc = -T3 * T4;
                        QovCox = qsrc / Coxeff;
                        T8 = 4.0 / 3.0 * T1 * (T1 - T0) + 0.4 * T0 * T0;
                        T5 = -2.0 * qsrc / T2 - T3 * (T1 * (3.0 * T1 - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                        T6 = AbulkCV * (qsrc / T2 + T3 * T8);
                        T7 = T6 * VdseffCV / AbulkCV;

                        Csg = T5 * (1.0 - dDeltaPhi_dVg) + T6 * dVdseffCV_dVg;
                        Csd = Csg * dVgsteff_dVd + T6 * dVdseffCV_dVd + QovCox * dCoxeff_dVd;
                        Csb = Csg * dVgsteff_dVb + T6 * dVdseffCV_dVb + T7 * dAbulkCV_dVb + QovCox * dCoxeff_dVb;
                        Csg = Csg * dVgsteff_dVg + QovCox * dCoxeff_dVg;
                    }
                    else
                    {
                        /* 50 / 50 partition */
                        qsrc = -0.5 * qgate;
                        Csg = -0.5 * Cgg1;
                        Csd = -0.5 * Cgd1;
                        Csb = -0.5 * Cgb1;
                    }

                    qgate += Qac0 + Qsub0 - qbulk;
                    qbulk -= (Qac0 + Qsub0);
                    qdrn = -(qgate + qbulk + qsrc);

                    Cbg = Cbg1 - dQac0_dVg - dQsub0_dVg;
                    Cbd = Cbd1 - dQsub0_dVd;
                    Cbb = Cbb1 - dQac0_dVb - dQsub0_dVb;

                    Cgg = Cgg1 - Cbg;
                    Cgd = Cgd1 - Cbd;
                    Cgb = Cgb1 - Cbb;

                    Cgb *= dVbseff_dVb;
                    Cbb *= dVbseff_dVb;
                    Csb *= dVbseff_dVb;

                    BSIM3cggb = Cgg;
                    BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    BSIM3cgdb = Cgd;
                    BSIM3cdgb = -(Cgg + Cbg + Csg);
                    BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    BSIM3cddb = -(Cgd + Cbd + Csd);
                    BSIM3cbgb = Cbg;
                    BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    BSIM3cbdb = Cbd;
                    BSIM3qinv = -qinoi;
                } /* End of CTM */
            }

            finished:
            /* Returning Values to Calling Routine */
            /* 
            * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
            */

            BSIM3qgate = qgate;
            BSIM3qbulk = qbulk;
            BSIM3qdrn = qdrn;
            BSIM3cd = cdrain;

            if (ChargeComputationNeeded)
            {
                /* charge storage elements
                * bulk - drain and bulk - source depletion capacitances
                * czbd : zero bias drain junction capacitance
                * czbs : zero bias source junction capacitance
                * czbdsw: zero bias drain junction sidewall capacitance
                along field oxide
                * czbssw: zero bias source junction sidewall capacitance
                along field oxide
                * czbdswg: zero bias drain junction sidewall capacitance
                along gate side
                * czbsswg: zero bias source junction sidewall capacitance
                along gate side
                */

                czbd = model.BSIM3unitAreaTempJctCap * BSIM3drainArea; /* bug fix */
                czbs = model.BSIM3unitAreaTempJctCap * BSIM3sourceArea;
                if (BSIM3drainPerimeter < pParam.BSIM3weff)
                {
                    czbdswg = model.BSIM3unitLengthGateSidewallTempJctCap * BSIM3drainPerimeter;
                    czbdsw = 0.0;
                }
                else
                {
                    czbdsw = model.BSIM3unitLengthSidewallTempJctCap * (BSIM3drainPerimeter - pParam.BSIM3weff);
                    czbdswg = model.BSIM3unitLengthGateSidewallTempJctCap * pParam.BSIM3weff;
                }
                if (BSIM3sourcePerimeter < pParam.BSIM3weff)
                {
                    czbssw = 0.0;
                    czbsswg = model.BSIM3unitLengthGateSidewallTempJctCap * BSIM3sourcePerimeter;
                }
                else
                {
                    czbssw = model.BSIM3unitLengthSidewallTempJctCap * (BSIM3sourcePerimeter - pParam.BSIM3weff);
                    czbsswg = model.BSIM3unitLengthGateSidewallTempJctCap * pParam.BSIM3weff;
                }

                MJ = model.BSIM3bulkJctBotGradingCoeff;
                MJSW = model.BSIM3bulkJctSideGradingCoeff;
                MJSWG = model.BSIM3bulkJctGateSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs == 0.0)
                {
                    state.States[0][BSIM3states + BSIM3qbs] = 0.0;
                    BSIM3capbs = czbs + czbssw + czbsswg;
                }
                else if (vbs < 0.0)
                {
                    if (czbs > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiB;
                        if (MJ == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJ * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbs] = model.BSIM3PhiB * czbs * (1.0 - arg * sarg) / (1.0 - MJ);
                        BSIM3capbs = czbs * sarg;
                    }
                    else
                    {
                        state.States[0][BSIM3states + BSIM3qbs] = 0.0;
                        BSIM3capbs = 0.0;
                    }
                    if (czbssw > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiBSW;
                        if (MJSW == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSW * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbs] += model.BSIM3PhiBSW * czbssw * (1.0 - arg * sarg) / (1.0 - MJSW);
                        BSIM3capbs += czbssw * sarg;
                    }
                    if (czbsswg > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiBSWG;
                        if (MJSWG == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWG * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbs] += model.BSIM3PhiBSWG * czbsswg * (1.0 - arg * sarg) / (1.0 - MJSWG);
                        BSIM3capbs += czbsswg * sarg;
                    }

                }
                else
                {
                    T0 = czbs + czbssw + czbsswg;
                    T1 = vbs * (czbs * MJ / model.BSIM3PhiB + czbssw * MJSW / model.BSIM3PhiBSW + czbsswg * MJSWG / model.BSIM3PhiBSWG);
                    state.States[0][BSIM3states + BSIM3qbs] = vbs * (T0 + 0.5 * T1);
                    BSIM3capbs = T0 + T1;
                }

                /* Drain Bulk Junction */
                if (vbd == 0.0)
                {
                    state.States[0][BSIM3states + BSIM3qbd] = 0.0;
                    BSIM3capbd = czbd + czbdsw + czbdswg;
                }
                else if (vbd < 0.0)
                {
                    if (czbd > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiB;
                        if (MJ == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJ * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbd] = model.BSIM3PhiB * czbd * (1.0 - arg * sarg) / (1.0 - MJ);
                        BSIM3capbd = czbd * sarg;
                    }
                    else
                    {
                        state.States[0][BSIM3states + BSIM3qbd] = 0.0;
                        BSIM3capbd = 0.0;
                    }
                    if (czbdsw > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiBSW;
                        if (MJSW == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSW * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbd] += model.BSIM3PhiBSW * czbdsw * (1.0 - arg * sarg) / (1.0 - MJSW);
                        BSIM3capbd += czbdsw * sarg;
                    }
                    if (czbdswg > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiBSWG;
                        if (MJSWG == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWG * Math.Log(arg));
                        state.States[0][BSIM3states + BSIM3qbd] += model.BSIM3PhiBSWG * czbdswg * (1.0 - arg * sarg) / (1.0 - MJSWG);
                        BSIM3capbd += czbdswg * sarg;
                    }
                }
                else
                {
                    T0 = czbd + czbdsw + czbdswg;
                    T1 = vbd * (czbd * MJ / model.BSIM3PhiB + czbdsw * MJSW / model.BSIM3PhiBSW + czbdswg * MJSWG / model.BSIM3PhiBSWG);
                    state.States[0][BSIM3states + BSIM3qbd] = vbd * (T0 + 0.5 * T1);
                    BSIM3capbd = T0 + T1;
                }
            }

            /* 
            * check convergence
            */
            if (!BSIM3off || state.Init != CircuitState.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][BSIM3states + BSIM3vbs] = vbs;
            state.States[0][BSIM3states + BSIM3vbd] = vbd;
            state.States[0][BSIM3states + BSIM3vgs] = vgs;
            state.States[0][BSIM3states + BSIM3vds] = vds;
            state.States[0][BSIM3states + BSIM3qdef] = qdef;

            /* bulk and channel charge plus overlaps */

            if (!ChargeComputationNeeded)
                goto line850;

            /* NQS begins */
            if (BSIM3nqsMod != 0)
            {
                qcheq = -(qbulk + qgate);

                BSIM3cqgb = -(BSIM3cggb + BSIM3cbgb);
                BSIM3cqdb = -(BSIM3cgdb + BSIM3cbdb);
                BSIM3cqsb = -(BSIM3cgsb + BSIM3cbsb);
                BSIM3cqbb = -(BSIM3cqgb + BSIM3cqdb + BSIM3cqsb);

                gtau_drift = Math.Abs(pParam.BSIM3tconst * qcheq) * ScalingFactor;
                T0 = pParam.BSIM3leffCV * pParam.BSIM3leffCV;
                gtau_diff = 16.0 * pParam.BSIM3u0temp * model.BSIM3vtm / T0 * ScalingFactor;
                BSIM3gtau = gtau_drift + gtau_diff;
            }

            if (model.BSIM3capMod.Value == 0)
            /* code merge - JX */
            {
                cgdo = pParam.BSIM3cgdo;
                qgdo = pParam.BSIM3cgdo * vgd;
                cgso = pParam.BSIM3cgso;
                qgso = pParam.BSIM3cgso * vgs;
            }
            else if (model.BSIM3capMod.Value == 1)
            {
                if (vgd < 0.0)
                {
                    T1 = Math.Sqrt(1.0 - 4.0 * vgd / pParam.BSIM3ckappa);
                    cgdo = pParam.BSIM3cgdo + pParam.BSIM3weffCV * pParam.BSIM3cgdl / T1;
                    qgdo = pParam.BSIM3cgdo * vgd - pParam.BSIM3weffCV * 0.5 * pParam.BSIM3cgdl * pParam.BSIM3ckappa * (T1 - 1.0);
                }
                else
                {
                    cgdo = pParam.BSIM3cgdo + pParam.BSIM3weffCV * pParam.BSIM3cgdl;
                    qgdo = (pParam.BSIM3weffCV * pParam.BSIM3cgdl + pParam.BSIM3cgdo) * vgd;
                }

                if (vgs < 0.0)
                {
                    T1 = Math.Sqrt(1.0 - 4.0 * vgs / pParam.BSIM3ckappa);
                    cgso = pParam.BSIM3cgso + pParam.BSIM3weffCV * pParam.BSIM3cgsl / T1;
                    qgso = pParam.BSIM3cgso * vgs - pParam.BSIM3weffCV * 0.5 * pParam.BSIM3cgsl * pParam.BSIM3ckappa * (T1 - 1.0);
                }
                else
                {
                    cgso = pParam.BSIM3cgso + pParam.BSIM3weffCV * pParam.BSIM3cgsl;
                    qgso = (pParam.BSIM3weffCV * pParam.BSIM3cgsl + pParam.BSIM3cgso) * vgs;
                }
            }
            else
            {
                T0 = vgd + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);

                T3 = pParam.BSIM3weffCV * pParam.BSIM3cgdl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / pParam.BSIM3ckappa);
                cgdo = pParam.BSIM3cgdo + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgdo = (pParam.BSIM3cgdo + T3) * vgd - T3 * (T2 + 0.5 * pParam.BSIM3ckappa * (T4 - 1.0));

                T0 = vgs + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);
                T3 = pParam.BSIM3weffCV * pParam.BSIM3cgsl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / pParam.BSIM3ckappa);
                cgso = pParam.BSIM3cgso + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgso = (pParam.BSIM3cgso + T3) * vgs - T3 * (T2 + 0.5 * pParam.BSIM3ckappa * (T4 - 1.0));
            }

            BSIM3cgdo = cgdo;
            BSIM3cgso = cgso;

            ag0 = method.Slope;
            if (BSIM3mode > 0)
            {
                if (BSIM3nqsMod.Value == 0)
                {
                    gcggb = (BSIM3cggb + cgdo + cgso + pParam.BSIM3cgbo) * ag0;
                    gcgdb = (BSIM3cgdb - cgdo) * ag0;
                    gcgsb = (BSIM3cgsb - cgso) * ag0;

                    gcdgb = (BSIM3cdgb - cgdo) * ag0;
                    gcddb = (BSIM3cddb + BSIM3capbd + cgdo) * ag0;
                    gcdsb = BSIM3cdsb * ag0;

                    gcsgb = -(BSIM3cggb + BSIM3cbgb + BSIM3cdgb + cgso) * ag0;
                    gcsdb = -(BSIM3cgdb + BSIM3cbdb + BSIM3cddb) * ag0;
                    gcssb = (BSIM3capbs + cgso - (BSIM3cgsb + BSIM3cbsb + BSIM3cdsb)) * ag0;

                    gcbgb = (BSIM3cbgb - pParam.BSIM3cgbo) * ag0;
                    gcbdb = (BSIM3cbdb - BSIM3capbd) * ag0;
                    gcbsb = (BSIM3cbsb - BSIM3capbs) * ag0;

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = pParam.BSIM3cgbo * vgb;
                    qgate += qgd + qgs + qgb;
                    qbulk -= qgb;
                    qdrn -= qgd;
                    qsrc = -(qgate + qbulk + qdrn);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.6;
                    dxpart = 0.4;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    if (qcheq > 0.0)
                        T0 = pParam.BSIM3tconst * qdef * ScalingFactor;
                    else
                        T0 = -pParam.BSIM3tconst * qdef * ScalingFactor;
                    ggtg = BSIM3gtg = T0 * BSIM3cqgb;
                    ggtd = BSIM3gtd = T0 * BSIM3cqdb;
                    ggts = BSIM3gts = T0 * BSIM3cqsb;
                    ggtb = BSIM3gtb = T0 * BSIM3cqbb;
                    gqdef = ScalingFactor * ag0;

                    gcqgb = BSIM3cqgb * ag0;
                    gcqdb = BSIM3cqdb * ag0;
                    gcqsb = BSIM3cqsb * ag0;
                    gcqbb = BSIM3cqbb * ag0;

                    gcggb = (cgdo + cgso + pParam.BSIM3cgbo) * ag0;
                    gcgdb = -cgdo * ag0;
                    gcgsb = -cgso * ag0;

                    gcdgb = -cgdo * ag0;
                    gcddb = (BSIM3capbd + cgdo) * ag0;
                    gcdsb = 0.0;

                    gcsgb = -cgso * ag0;
                    gcsdb = 0.0;
                    gcssb = (BSIM3capbs + cgso) * ag0;

                    gcbgb = -pParam.BSIM3cgbo * ag0;
                    gcbdb = -BSIM3capbd * ag0;
                    gcbsb = -BSIM3capbs * ag0;

                    CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            dxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            dxpart = 0.0;
                        }
                        else
                        {
                            dxpart = 0.5;
                        }
                        ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    }
                    else
                    {
                        dxpart = qdrn / qcheq;
                        Cdd = BSIM3cddb;
                        Csd = -(BSIM3cgdb + BSIM3cddb + BSIM3cbdb);
                        ddxpart_dVd = (Cdd - dxpart * (Cdd + Csd)) / qcheq;
                        Cdg = BSIM3cdgb;
                        Csg = -(BSIM3cggb + BSIM3cdgb + BSIM3cbgb);
                        ddxpart_dVg = (Cdg - dxpart * (Cdg + Csg)) / qcheq;

                        Cds = BSIM3cdsb;
                        Css = -(BSIM3cgsb + BSIM3cdsb + BSIM3cbsb);
                        ddxpart_dVs = (Cds - dxpart * (Cds + Css)) / qcheq;

                        ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);
                    }
                    sxpart = 1.0 - dxpart;
                    dsxpart_dVd = -ddxpart_dVd;
                    dsxpart_dVg = -ddxpart_dVg;
                    dsxpart_dVs = -ddxpart_dVs;
                    dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = pParam.BSIM3cgbo * vgb;
                    qgate = qgd + qgs + qgb;
                    qbulk = -qgb;
                    qdrn = -qgd;
                    qsrc = -(qgate + qbulk + qdrn);
                }
            }
            else
            {
                if (BSIM3nqsMod.Value == 0)
                {
                    gcggb = (BSIM3cggb + cgdo + cgso + pParam.BSIM3cgbo) * ag0;
                    gcgdb = (BSIM3cgsb - cgdo) * ag0;
                    gcgsb = (BSIM3cgdb - cgso) * ag0;

                    gcdgb = -(BSIM3cggb + BSIM3cbgb + BSIM3cdgb + cgdo) * ag0;
                    gcddb = (BSIM3capbd + cgdo - (BSIM3cgsb + BSIM3cbsb + BSIM3cdsb)) * ag0;
                    gcdsb = -(BSIM3cgdb + BSIM3cbdb + BSIM3cddb) * ag0;

                    gcsgb = (BSIM3cdgb - cgso) * ag0;
                    gcsdb = BSIM3cdsb * ag0;
                    gcssb = (BSIM3cddb + BSIM3capbs + cgso) * ag0;

                    gcbgb = (BSIM3cbgb - pParam.BSIM3cgbo) * ag0;
                    gcbdb = (BSIM3cbsb - BSIM3capbd) * ag0;
                    gcbsb = (BSIM3cbdb - BSIM3capbs) * ag0;

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = pParam.BSIM3cgbo * vgb;
                    qgate += qgd + qgs + qgb;
                    qbulk -= qgb;
                    qsrc = qdrn - qgs;
                    qdrn = -(qgate + qbulk + qsrc);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.4;
                    dxpart = 0.6;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    if (qcheq > 0.0)
                        T0 = pParam.BSIM3tconst * qdef * ScalingFactor;
                    else
                        T0 = -pParam.BSIM3tconst * qdef * ScalingFactor;
                    ggtg = BSIM3gtg = T0 * BSIM3cqgb;
                    ggts = BSIM3gtd = T0 * BSIM3cqdb;
                    ggtd = BSIM3gts = T0 * BSIM3cqsb;
                    ggtb = BSIM3gtb = T0 * BSIM3cqbb;
                    gqdef = ScalingFactor * ag0;

                    gcqgb = BSIM3cqgb * ag0;
                    gcqdb = BSIM3cqsb * ag0;
                    gcqsb = BSIM3cqdb * ag0;
                    gcqbb = BSIM3cqbb * ag0;

                    gcggb = (cgdo + cgso + pParam.BSIM3cgbo) * ag0;
                    gcgdb = -cgdo * ag0;
                    gcgsb = -cgso * ag0;

                    gcdgb = -cgdo * ag0;
                    gcddb = (BSIM3capbd + cgdo) * ag0;
                    gcdsb = 0.0;

                    gcsgb = -cgso * ag0;
                    gcsdb = 0.0;
                    gcssb = (BSIM3capbs + cgso) * ag0;

                    gcbgb = -pParam.BSIM3cgbo * ag0;
                    gcbdb = -BSIM3capbd * ag0;
                    gcbsb = -BSIM3capbs * ag0;

                    CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            sxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            sxpart = 0.0;
                        }
                        else
                        {
                            sxpart = 0.5;
                        }
                        dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                    }
                    else
                    {
                        sxpart = qdrn / qcheq;
                        Css = BSIM3cddb;
                        Cds = -(BSIM3cgdb + BSIM3cddb + BSIM3cbdb);
                        dsxpart_dVs = (Css - sxpart * (Css + Cds)) / qcheq;
                        Csg = BSIM3cdgb;
                        Cdg = -(BSIM3cggb + BSIM3cdgb + BSIM3cbgb);
                        dsxpart_dVg = (Csg - sxpart * (Csg + Cdg)) / qcheq;

                        Csd = BSIM3cdsb;
                        Cdd = -(BSIM3cgsb + BSIM3cdsb + BSIM3cbsb);
                        dsxpart_dVd = (Csd - sxpart * (Csd + Cdd)) / qcheq;

                        dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);
                    }
                    dxpart = 1.0 - sxpart;
                    ddxpart_dVd = -dsxpart_dVd;
                    ddxpart_dVg = -dsxpart_dVg;
                    ddxpart_dVs = -dsxpart_dVs;
                    ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = pParam.BSIM3cgbo * vgb;
                    qgate = qgd + qgs + qgb;
                    qbulk = -qgb;
                    qsrc = -qgs;
                    qdrn = -(qgate + qbulk + qsrc);
                }
            }

            cqdef = cqcheq = 0.0;

            state.States[0][BSIM3states + BSIM3qg] = qgate;
            state.States[0][BSIM3states + BSIM3qd] = qdrn - state.States[0][BSIM3states + BSIM3qbd];
            state.States[0][BSIM3states + BSIM3qb] = qbulk + state.States[0][BSIM3states + BSIM3qbd] + state.States[0][BSIM3states +
                BSIM3qbs];

            if (BSIM3nqsMod != 0)
            {
                state.States[0][BSIM3states + BSIM3qcdump] = qdef * ScalingFactor;
                state.States[0][BSIM3states + BSIM3qcheq] = qcheq;
            }

            /* store small signal parameters */
            if (state.UseSmallSignal)
                goto line1000;
            if (!ChargeComputationNeeded)
                goto line850;

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][BSIM3states + BSIM3qb] = state.States[0][BSIM3states + BSIM3qb];
                state.States[1][BSIM3states + BSIM3qg] = state.States[0][BSIM3states + BSIM3qg];
                state.States[1][BSIM3states + BSIM3qd] = state.States[0][BSIM3states + BSIM3qd];
                if (BSIM3nqsMod != 0)
                {
                    state.States[1][BSIM3states + BSIM3qcheq] = state.States[0][BSIM3states + BSIM3qcheq];
                    state.States[1][BSIM3states + BSIM3qcdump] = state.States[0][BSIM3states + BSIM3qcdump];
                }
            }

			if (method != null)
			{
				method.Integrate(state, BSIM3states + BSIM3qb, 0.0);
				method.Integrate(state, BSIM3states + BSIM3qg, 0.0);
				method.Integrate(state, BSIM3states + BSIM3qd, 0.0);
				if (BSIM3nqsMod != 0)
				{
					method.Integrate(state, BSIM3states + BSIM3qcdump, 0.0);
					method.Integrate(state, BSIM3states + BSIM3qcheq, 0.0);
				}
			}

            goto line860;

            line850:
            /* initialize to zero charge conductance and current */
            ceqqg = ceqqb = ceqqd = 0.0;
            cqcheq = cqdef = 0.0;

            gcdgb = gcddb = gcdsb = 0.0;
            gcsgb = gcsdb = gcssb = 0.0;
            gcggb = gcgdb = gcgsb = 0.0;
            gcbgb = gcbdb = gcbsb = 0.0;

            gqdef = gcqgb = gcqdb = gcqsb = gcqbb = 0.0;
            ggtg = ggtd = ggtb = ggts = 0.0;
            sxpart = (1.0 - (dxpart = (BSIM3mode > 0) ? 0.4 : 0.6));
            ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
            dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;

            if (BSIM3nqsMod != 0)
                BSIM3gtau = 16.0 * pParam.BSIM3u0temp * model.BSIM3vtm / pParam.BSIM3leffCV / pParam.BSIM3leffCV * ScalingFactor;
            else
                BSIM3gtau = 0.0;

            goto line900;

            line860:
            /* evaluate equivalent charge current */

            cqgate = state.States[0][BSIM3states + BSIM3cqg];
            cqbulk = state.States[0][BSIM3states + BSIM3cqb];
            cqdrn = state.States[0][BSIM3states + BSIM3cqd];

            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (BSIM3nqsMod != 0)
            {
                T0 = ggtg * vgb - ggtd * vbd - ggts * vbs;
                ceqqg += T0;
                T1 = qdef * BSIM3gtau;
                ceqqd -= dxpart * T0 + T1 * (ddxpart_dVg * vgb - ddxpart_dVd * vbd - ddxpart_dVs * vbs);
                cqdef = state.States[0][BSIM3states + BSIM3cqcdump] - gqdef * qdef;
                cqcheq = state.States[0][BSIM3states + BSIM3cqcheq] - (gcqgb * vgb - gcqdb * vbd - gcqsb * vbs) + T0;
            }

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][BSIM3states + BSIM3cqb] = state.States[0][BSIM3states + BSIM3cqb];
                state.States[1][BSIM3states + BSIM3cqg] = state.States[0][BSIM3states + BSIM3cqg];
                state.States[1][BSIM3states + BSIM3cqd] = state.States[0][BSIM3states + BSIM3cqd];

                if (BSIM3nqsMod != 0)
                {
                    state.States[1][BSIM3states + BSIM3cqcheq] = state.States[0][BSIM3states + BSIM3cqcheq];
                    state.States[1][BSIM3states + BSIM3cqcdump] = state.States[0][BSIM3states + BSIM3cqcdump];
                }
            }

            /* 
            * load current vector
            */
            line900:

            if (BSIM3mode >= 0)
            {
                Gm = BSIM3gm;
                Gmbs = BSIM3gmbs;
                FwdSum = Gm + Gmbs;
                RevSum = 0.0;
                cdreq = model.BSIM3type * (cdrain - BSIM3gds * vds - Gm * vgs - Gmbs * vbs);

                ceqbd = -model.BSIM3type * (BSIM3csub - BSIM3gbds * vds - BSIM3gbgs * vgs - BSIM3gbbs * vbs);
                ceqbs = 0.0;

                gbbdp = -BSIM3gbds;
                gbbsp = (BSIM3gbds + BSIM3gbgs + BSIM3gbbs);

                gbdpg = BSIM3gbgs;
                gbdpdp = BSIM3gbds;
                gbdpb = BSIM3gbbs;
                gbdpsp = -(gbdpg + gbdpdp + gbdpb);

                gbspg = 0.0;
                gbspdp = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;
            }
            else
            {
                Gm = -BSIM3gm;
                Gmbs = -BSIM3gmbs;
                FwdSum = 0.0;
                RevSum = -(Gm + Gmbs);
                cdreq = -model.BSIM3type * (cdrain + BSIM3gds * vds + Gm * vgd + Gmbs * vbd);

                ceqbs = -model.BSIM3type * (BSIM3csub + BSIM3gbds * vds - BSIM3gbgs * vgd - BSIM3gbbs * vbd);
                ceqbd = 0.0;

                gbbsp = -BSIM3gbds;
                gbbdp = (BSIM3gbds + BSIM3gbgs + BSIM3gbbs);

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = BSIM3gbgs;
                gbspsp = BSIM3gbds;
                gbspb = BSIM3gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);
            }

            if (model.BSIM3type > 0)
            {
                ceqbs += (BSIM3cbs - BSIM3gbs * vbs);
                ceqbd += (BSIM3cbd - BSIM3gbd * vbd);
                /* 
                ceqqg = ceqqg;
                ceqqb = ceqqb;
                ceqqd = ceqqd;
                cqdef = cqdef;
                cqcheq = cqcheq;
                */
            }
            else
            {
                ceqbs -= (BSIM3cbs - BSIM3gbs * vbs);
                ceqbd -= (BSIM3cbd - BSIM3gbd * vbd);
                ceqqg = -ceqqg;
                ceqqb = -ceqqb;
                ceqqd = -ceqqd;
                cqdef = -cqdef;
                cqcheq = -cqcheq;
            }
            rstate.Rhs[BSIM3gNode] -= ceqqg;
            rstate.Rhs[BSIM3bNode] -= (ceqbs + ceqbd + ceqqb);
            rstate.Rhs[BSIM3dNodePrime] += (ceqbd - cdreq - ceqqd);
            rstate.Rhs[BSIM3sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);
            if (BSIM3nqsMod != 0)
                rstate.Rhs[BSIM3qNode] += (cqcheq - cqdef);

            /* 
            * load y matrix
            */

            T1 = qdef * BSIM3gtau;
            rstate.Matrix[BSIM3dNode, BSIM3dNode] += BSIM3drainConductance;
            rstate.Matrix[BSIM3gNode, BSIM3gNode] += gcggb - ggtg;
            rstate.Matrix[BSIM3sNode, BSIM3sNode] += BSIM3sourceConductance;
            rstate.Matrix[BSIM3bNode, BSIM3bNode] += BSIM3gbd + BSIM3gbs - gcbgb - gcbdb - gcbsb - BSIM3gbbs;
            rstate.Matrix[BSIM3dNodePrime, BSIM3dNodePrime] += BSIM3drainConductance + BSIM3gds + BSIM3gbd + RevSum + gcddb + dxpart *
                    ggtd + T1 * ddxpart_dVd + gbdpdp;
            rstate.Matrix[BSIM3sNodePrime, BSIM3sNodePrime] += BSIM3sourceConductance + BSIM3gds + BSIM3gbs + FwdSum + gcssb + sxpart *
                    ggts + T1 * dsxpart_dVs + gbspsp;
            rstate.Matrix[BSIM3dNode, BSIM3dNodePrime] -= BSIM3drainConductance;
            rstate.Matrix[BSIM3gNode, BSIM3bNode] -= gcggb + gcgdb + gcgsb + ggtb;
            rstate.Matrix[BSIM3gNode, BSIM3dNodePrime] += gcgdb - ggtd;
            rstate.Matrix[BSIM3gNode, BSIM3sNodePrime] += gcgsb - ggts;
            rstate.Matrix[BSIM3sNode, BSIM3sNodePrime] -= BSIM3sourceConductance;
            rstate.Matrix[BSIM3bNode, BSIM3gNode] += gcbgb - BSIM3gbgs;
            rstate.Matrix[BSIM3bNode, BSIM3dNodePrime] += gcbdb - BSIM3gbd + gbbdp;
            rstate.Matrix[BSIM3bNode, BSIM3sNodePrime] += gcbsb - BSIM3gbs + gbbsp;
            rstate.Matrix[BSIM3dNodePrime, BSIM3dNode] -= BSIM3drainConductance;
            rstate.Matrix[BSIM3dNodePrime, BSIM3gNode] += Gm + gcdgb + dxpart * ggtg + T1 * ddxpart_dVg + gbdpg;
            rstate.Matrix[BSIM3dNodePrime, BSIM3bNode] -= BSIM3gbd - Gmbs + gcdgb + gcddb + gcdsb - dxpart * ggtb - T1 * ddxpart_dVb -
                    gbdpb;
            rstate.Matrix[BSIM3dNodePrime, BSIM3sNodePrime] -= BSIM3gds + FwdSum - gcdsb - dxpart * ggts - T1 * ddxpart_dVs - gbdpsp;
            rstate.Matrix[BSIM3sNodePrime, BSIM3gNode] += gcsgb - Gm + sxpart * ggtg + T1 * dsxpart_dVg + gbspg;
            rstate.Matrix[BSIM3sNodePrime, BSIM3sNode] -= BSIM3sourceConductance;
            rstate.Matrix[BSIM3sNodePrime, BSIM3bNode] -= BSIM3gbs + Gmbs + gcsgb + gcsdb + gcssb - sxpart * ggtb - T1 * dsxpart_dVb -
                    gbspb;
            rstate.Matrix[BSIM3sNodePrime, BSIM3dNodePrime] -= BSIM3gds + RevSum - gcsdb - sxpart * ggtd - T1 * dsxpart_dVd - gbspdp;

            if (BSIM3nqsMod != 0)
            {
                rstate.Matrix[BSIM3qNode, BSIM3qNode] += (gqdef + BSIM3gtau);

                rstate.Matrix[BSIM3dNodePrime, BSIM3qNode] += (dxpart * BSIM3gtau);
                rstate.Matrix[BSIM3sNodePrime, BSIM3qNode] += (sxpart * BSIM3gtau);
                rstate.Matrix[BSIM3gNode, BSIM3qNode] -= BSIM3gtau;

                rstate.Matrix[BSIM3qNode, BSIM3gNode] += (ggtg - gcqgb);
                rstate.Matrix[BSIM3qNode, BSIM3dNodePrime] += (ggtd - gcqdb);
                rstate.Matrix[BSIM3qNode, BSIM3sNodePrime] += (ggts - gcqsb);
                rstate.Matrix[BSIM3qNode, BSIM3bNode] += (ggtb - gcqbb);
            }

            line1000:;
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as BSIM3v24Model;
            var state = ckt.State;
            var cstate = state.Complex;
            double Gm, Gmbs, FwdSum, RevSum, gbbdp, gbbsp, gbdpg, gbdpb, gbdpdp, gbdpsp, gbspdp, gbspg, gbspb, gbspsp, cggb, cgsb, cgdb,
                cbgb, cbsb, cbdb, cdgb, cdsb, cddb, xgtg, sxpart, dxpart, ddxpart_dVd, dsxpart_dVd, xgtd, xgts, xgtb, xcqgb = 0.0, xcqdb = 0.0, xcqsb = 0.0,
                xcqbb = 0.0, CoxWL, qcheq, Cdd, Csd, Cdg, Csg, ddxpart_dVg, Cds, Css, ddxpart_dVs, ddxpart_dVb, dsxpart_dVg, dsxpart_dVs,
                dsxpart_dVb, T1, gdpr, gspr, gds, gbd, gbs, capbd, capbs, GSoverlapCap, GDoverlapCap, GBoverlapCap, xcdgb, xcddb, xcdsb, xcsgb,
                xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;
            double omega = cstate.Laplace.Imaginary;

            if (BSIM3mode >= 0)
            {
                Gm = BSIM3gm;
                Gmbs = BSIM3gmbs;
                FwdSum = Gm + Gmbs;
                RevSum = 0.0;

                gbbdp = -BSIM3gbds;
                gbbsp = BSIM3gbds + BSIM3gbgs + BSIM3gbbs;

                gbdpg = BSIM3gbgs;
                gbdpb = BSIM3gbbs;
                gbdpdp = BSIM3gbds;
                gbdpsp = -(gbdpg + gbdpb + gbdpdp);

                gbspdp = 0.0;
                gbspg = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;

                if (BSIM3nqsMod.Value == 0)
                {
                    cggb = BSIM3cggb;
                    cgsb = BSIM3cgsb;
                    cgdb = BSIM3cgdb;

                    cbgb = BSIM3cbgb;
                    cbsb = BSIM3cbsb;
                    cbdb = BSIM3cbdb;

                    cdgb = BSIM3cdgb;
                    cdsb = BSIM3cdsb;
                    cddb = BSIM3cddb;

                    xgtg = xgtd = xgts = xgtb = 0.0;
                    sxpart = 0.6;
                    dxpart = 0.4;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    cggb = cgdb = cgsb = 0.0;
                    cbgb = cbdb = cbsb = 0.0;
                    cdgb = cddb = cdsb = 0.0;

                    xgtg = BSIM3gtg;
                    xgtd = BSIM3gtd;
                    xgts = BSIM3gts;
                    xgtb = BSIM3gtb;

                    xcqgb = BSIM3cqgb * omega;
                    xcqdb = BSIM3cqdb * omega;
                    xcqsb = BSIM3cqsb * omega;
                    xcqbb = BSIM3cqbb * omega;

                    CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;
                    qcheq = -(BSIM3qgate + BSIM3qbulk);
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            dxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            dxpart = 0.0;
                        }
                        else
                        {
                            dxpart = 0.5;
                        }
                        ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    }
                    else
                    {
                        dxpart = BSIM3qdrn / qcheq;
                        Cdd = BSIM3cddb;
                        Csd = -(BSIM3cgdb + BSIM3cddb + BSIM3cbdb);
                        ddxpart_dVd = (Cdd - dxpart * (Cdd + Csd)) / qcheq;
                        Cdg = BSIM3cdgb;
                        Csg = -(BSIM3cggb + BSIM3cdgb + BSIM3cbgb);
                        ddxpart_dVg = (Cdg - dxpart * (Cdg + Csg)) / qcheq;

                        Cds = BSIM3cdsb;
                        Css = -(BSIM3cgsb + BSIM3cdsb + BSIM3cbsb);
                        ddxpart_dVs = (Cds - dxpart * (Cds + Css)) / qcheq;

                        ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);
                    }
                    sxpart = 1.0 - dxpart;
                    dsxpart_dVd = -ddxpart_dVd;
                    dsxpart_dVg = -ddxpart_dVg;
                    dsxpart_dVs = -ddxpart_dVs;
                    dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);
                }
            }
            else
            {
                Gm = -BSIM3gm;
                Gmbs = -BSIM3gmbs;
                FwdSum = 0.0;
                RevSum = -(Gm + Gmbs);

                gbbsp = -BSIM3gbds;
                gbbdp = BSIM3gbds + BSIM3gbgs + BSIM3gbbs;

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = BSIM3gbgs;
                gbspsp = BSIM3gbds;
                gbspb = BSIM3gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);

                if (BSIM3nqsMod.Value == 0)
                {
                    cggb = BSIM3cggb;
                    cgsb = BSIM3cgdb;
                    cgdb = BSIM3cgsb;

                    cbgb = BSIM3cbgb;
                    cbsb = BSIM3cbdb;
                    cbdb = BSIM3cbsb;

                    cdgb = -(BSIM3cdgb + cggb + cbgb);
                    cdsb = -(BSIM3cddb + cgsb + cbsb);
                    cddb = -(BSIM3cdsb + cgdb + cbdb);

                    xgtg = xgtd = xgts = xgtb = 0.0;
                    sxpart = 0.4;
                    dxpart = 0.6;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    cggb = cgdb = cgsb = 0.0;
                    cbgb = cbdb = cbsb = 0.0;
                    cdgb = cddb = cdsb = 0.0;

                    xgtg = BSIM3gtg;
                    xgtd = BSIM3gts;
                    xgts = BSIM3gtd;
                    xgtb = BSIM3gtb;

                    xcqgb = BSIM3cqgb * omega;
                    xcqdb = BSIM3cqsb * omega;
                    xcqsb = BSIM3cqdb * omega;
                    xcqbb = BSIM3cqbb * omega;

                    CoxWL = model.BSIM3cox * pParam.BSIM3weffCV * pParam.BSIM3leffCV;
                    qcheq = -(BSIM3qgate + BSIM3qbulk);
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            sxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            sxpart = 0.0;
                        }
                        else
                        {
                            sxpart = 0.5;
                        }
                        dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                    }
                    else
                    {
                        sxpart = BSIM3qdrn / qcheq;
                        Css = BSIM3cddb;
                        Cds = -(BSIM3cgdb + BSIM3cddb + BSIM3cbdb);
                        dsxpart_dVs = (Css - sxpart * (Css + Cds)) / qcheq;
                        Csg = BSIM3cdgb;
                        Cdg = -(BSIM3cggb + BSIM3cdgb + BSIM3cbgb);
                        dsxpart_dVg = (Csg - sxpart * (Csg + Cdg)) / qcheq;

                        Csd = BSIM3cdsb;
                        Cdd = -(BSIM3cgsb + BSIM3cdsb + BSIM3cbsb);
                        dsxpart_dVd = (Csd - sxpart * (Csd + Cdd)) / qcheq;

                        dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);
                    }
                    dxpart = 1.0 - sxpart;
                    ddxpart_dVd = -dsxpart_dVd;
                    ddxpart_dVg = -dsxpart_dVg;
                    ddxpart_dVs = -dsxpart_dVs;
                    ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);
                }
            }

            T1 = state.States[0][BSIM3states + BSIM3qdef] * BSIM3gtau;
            gdpr = BSIM3drainConductance;
            gspr = BSIM3sourceConductance;
            gds = BSIM3gds;
            gbd = BSIM3gbd;
            gbs = BSIM3gbs;
            capbd = BSIM3capbd;
            capbs = BSIM3capbs;

            GSoverlapCap = BSIM3cgso;
            GDoverlapCap = BSIM3cgdo;
            GBoverlapCap = pParam.BSIM3cgbo;

            xcdgb = (cdgb - GDoverlapCap) * omega;
            xcddb = (cddb + capbd + GDoverlapCap) * omega;
            xcdsb = cdsb * omega;
            xcsgb = -(cggb + cbgb + cdgb + GSoverlapCap) * omega;
            xcsdb = -(cgdb + cbdb + cddb) * omega;
            xcssb = (capbs + GSoverlapCap - (cgsb + cbsb + cdsb)) * omega;
            xcggb = (cggb + GDoverlapCap + GSoverlapCap + GBoverlapCap) * omega;
            xcgdb = (cgdb - GDoverlapCap) * omega;
            xcgsb = (cgsb - GSoverlapCap) * omega;
            xcbgb = (cbgb - GBoverlapCap) * omega;
            xcbdb = (cbdb - capbd) * omega;
            xcbsb = (cbsb - capbs) * omega;

            cstate.Matrix[BSIM3gNode, BSIM3gNode] -= new Complex(xgtg, -xcggb);
            cstate.Matrix[BSIM3bNode, BSIM3bNode] += new Complex(gbd + gbs - BSIM3gbbs, -(xcbgb + xcbdb + xcbsb));
            cstate.Matrix[BSIM3dNodePrime, BSIM3dNodePrime] += new Complex(gdpr + gds + gbd + RevSum + dxpart * xgtd + T1 * ddxpart_dVd +
                gbdpdp, xcddb);
            cstate.Matrix[BSIM3sNodePrime, BSIM3sNodePrime] += new Complex(gspr + gds + gbs + FwdSum + sxpart * xgts + T1 * dsxpart_dVs +
                gbspsp, xcssb);
            cstate.Matrix[BSIM3gNode, BSIM3bNode] -= new Complex(xgtb, xcggb + xcgdb + xcgsb);
            cstate.Matrix[BSIM3gNode, BSIM3dNodePrime] -= new Complex(xgtd, -xcgdb);
            cstate.Matrix[BSIM3gNode, BSIM3sNodePrime] -= new Complex(xgts, -xcgsb);
            cstate.Matrix[BSIM3bNode, BSIM3gNode] -= new Complex(BSIM3gbgs, -xcbgb);
            cstate.Matrix[BSIM3bNode, BSIM3dNodePrime] -= new Complex(gbd - gbbdp, -xcbdb);
            cstate.Matrix[BSIM3bNode, BSIM3sNodePrime] -= new Complex(gbs - gbbsp, -xcbsb);
            cstate.Matrix[BSIM3dNodePrime, BSIM3gNode] += new Complex(Gm + dxpart * xgtg + T1 * ddxpart_dVg + gbdpg, xcdgb);
            cstate.Matrix[BSIM3dNodePrime, BSIM3bNode] -= new Complex(gbd - Gmbs - dxpart * xgtb - T1 * ddxpart_dVb - gbdpb, xcdgb + xcddb +
                xcdsb);
            cstate.Matrix[BSIM3dNodePrime, BSIM3sNodePrime] -= new Complex(gds + FwdSum - dxpart * xgts - T1 * ddxpart_dVs - gbdpsp, -
                xcdsb);
            cstate.Matrix[BSIM3sNodePrime, BSIM3gNode] -= new Complex(Gm - sxpart * xgtg - T1 * dsxpart_dVg - gbspg, -xcsgb);
            cstate.Matrix[BSIM3sNodePrime, BSIM3bNode] -= new Complex(gbs + Gmbs - sxpart * xgtb - T1 * dsxpart_dVb - gbspb, xcsgb + xcsdb +
                xcssb);
            cstate.Matrix[BSIM3sNodePrime, BSIM3dNodePrime] -= new Complex(gds + RevSum - sxpart * xgtd - T1 * dsxpart_dVd - gbspdp, -
                xcsdb);

            cstate.Matrix[BSIM3dNode, BSIM3dNode] += gdpr;
            cstate.Matrix[BSIM3sNode, BSIM3sNode] += gspr;

            cstate.Matrix[BSIM3dNode, BSIM3dNodePrime] -= gdpr;
            cstate.Matrix[BSIM3sNode, BSIM3sNodePrime] -= gspr;

            cstate.Matrix[BSIM3dNodePrime, BSIM3dNode] -= gdpr;

            cstate.Matrix[BSIM3sNodePrime, BSIM3sNode] -= gspr;

            if (BSIM3nqsMod != 0)
            {
                cstate.Matrix[BSIM3qNode, BSIM3qNode] += new Complex(BSIM3gtau, omega * ScalingFactor);
                cstate.Matrix[BSIM3qNode, BSIM3gNode] += new Complex(xgtg, -xcqgb);
                cstate.Matrix[BSIM3qNode, BSIM3dNodePrime] += new Complex(xgtd, -xcqdb);
                cstate.Matrix[BSIM3qNode, BSIM3sNodePrime] += new Complex(xgts, -xcqsb);
                cstate.Matrix[BSIM3qNode, BSIM3bNode] += new Complex(xgtb, -xcqbb);

                cstate.Matrix[BSIM3dNodePrime, BSIM3qNode] += dxpart * BSIM3gtau;
                cstate.Matrix[BSIM3sNodePrime, BSIM3qNode] += sxpart * BSIM3gtau;
                cstate.Matrix[BSIM3gNode, BSIM3qNode] -= BSIM3gtau;
            }
        }

        /// <summary>
        /// BSIM3 check model
        /// </summary>
        /// <returns></returns>
        bool BSIM3checkModel()
        {
            var model = Model as BSIM3v24Model;
            bool Fatal_Flag = false;

            using (StreamWriter sw = new StreamWriter("b3v3check.log", true))
            {
                sw.WriteLine("BSIM3v3.2.4 Parameter Checking.");
                if (model.BSIM3version != "3.2.4")
                {
                    sw.WriteLine("Warning: This model is BSIM3v3.2.4; you specified a wrong version number.");
                    CircuitWarning.Warning(this, "Warning: This model is BSIM3v3.2.4; you specified a wrong version number.");
                }

                sw.WriteLine($"Model = {Name}");
                if (pParam.BSIM3nlx < -pParam.BSIM3leff)
                {
                    sw.WriteLine($"Fatal: Nlx = {pParam.BSIM3nlx} is less than -Leff.");
                    CircuitWarning.Warning(this, $"Fatal: Nlx = {pParam.BSIM3nlx} is less than -Leff.");
                    Fatal_Flag = true;
                }

                if (model.BSIM3tox <= 0.0)
                {
                    sw.WriteLine($"Fatal: Tox = {model.BSIM3tox} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Tox = {model.BSIM3tox} is not positive.");
                    Fatal_Flag = true;
                }

                if (model.BSIM3toxm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxm = {model.BSIM3toxm} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Toxm = {model.BSIM3toxm} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3npeak <= 0.0)
                {
                    sw.WriteLine($"Fatal: Nch = {pParam.BSIM3npeak} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Nch = {pParam.BSIM3npeak} is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3nsub <= 0.0)
                {
                    sw.WriteLine($"Fatal: Nsub = {pParam.BSIM3nsub} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Nsub = {pParam.BSIM3nsub} is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3ngate < 0.0)
                {
                    sw.WriteLine($"Fatal: Ngate = {pParam.BSIM3ngate} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Ngate = {pParam.BSIM3ngate} Ngate is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3ngate > 1.0e25)
                {
                    sw.WriteLine($"Fatal: Ngate = {pParam.BSIM3ngate} is too high.");
                    CircuitWarning.Warning(this, $"Fatal: Ngate = {pParam.BSIM3ngate} Ngate is too high");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3xj <= 0.0)
                {
                    sw.WriteLine($"Fatal: Xj = {pParam.BSIM3xj} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Xj = {pParam.BSIM3xj} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3dvt1 < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1 = {pParam.BSIM3dvt1} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Dvt1 = {pParam.BSIM3dvt1} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3dvt1w < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1w = {pParam.BSIM3dvt1w} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Dvt1w = {pParam.BSIM3dvt1w} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3w0 == -pParam.BSIM3weff)
                {
                    sw.WriteLine("Fatal: (W0 + Weff) = 0 causing divided-by-zero.");

                    CircuitWarning.Warning(this, "Fatal: (W0 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3dsub < 0.0)
                {
                    sw.WriteLine($"Fatal: Dsub = {pParam.BSIM3dsub} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Dsub = {pParam.BSIM3dsub} is negative.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3b1 == -pParam.BSIM3weff)
                {
                    sw.WriteLine("Fatal: (B1 + Weff) = 0 causing divided-by-zero.");
                    CircuitWarning.Warning(this, "Fatal: (B1 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM3u0temp <= 0.0)
                {
                    sw.WriteLine($"Fatal: u0 at current temperature = {pParam.BSIM3u0temp} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: u0 at current temperature = {pParam.BSIM3u0temp} is not positive.");
                    Fatal_Flag = true;
                }

                /* Check delta parameter */
                if (pParam.BSIM3delta < 0.0)
                {
                    sw.WriteLine($"Fatal: Delta = {pParam.BSIM3delta} is less than zero.");
                    CircuitWarning.Warning(this, $"Fatal: Delta = {pParam.BSIM3delta} is less than zero.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3vsattemp <= 0.0)
                {
                    sw.WriteLine($"Fatal: Vsat at current temperature = {pParam.BSIM3vsattemp} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Vsat at current temperature = {pParam.BSIM3vsattemp} is not positive.");
                    Fatal_Flag = true;
                }
                /* Check Rout parameters */
                if (pParam.BSIM3pclm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Pclm = {pParam.BSIM3pclm} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Pclm = {pParam.BSIM3pclm} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3drout < 0.0)
                {
                    sw.WriteLine($"Fatal: Drout = {pParam.BSIM3drout} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Drout = {pParam.BSIM3drout} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3pscbe2 <= 0.0)
                {
                    sw.WriteLine($"Warning: Pscbe2 = {pParam.BSIM3pscbe2} is not positive.");
                    CircuitWarning.Warning(this, $"Warning: Pscbe2 = {pParam.BSIM3pscbe2} is not positive.");
                }

                if (model.BSIM3unitLengthSidewallJctCap > 0.0 || model.BSIM3unitLengthGateSidewallJctCap > 0.0)
                {
                    if (BSIM3drainPerimeter < pParam.BSIM3weff)
                    {
                        sw.WriteLine($"Warning: Pd = {BSIM3drainPerimeter} is less than W.");
                        CircuitWarning.Warning(this, $"Warning: Pd = {BSIM3drainPerimeter} is less than W.");
                    }
                    if (BSIM3sourcePerimeter < pParam.BSIM3weff)
                    {
                        sw.WriteLine($"Warning: Ps = {BSIM3sourcePerimeter} is less than W.");
                        CircuitWarning.Warning(this, $"Warning: Ps = {BSIM3sourcePerimeter} is less than W.");
                    }
                }

                if (pParam.BSIM3noff < 0.1)
                {
                    sw.WriteLine($"Warning: Noff = {pParam.BSIM3noff} is too small.");
                    CircuitWarning.Warning(this, $"Warning: Noff = {pParam.BSIM3noff} is too small.");
                }
                if (pParam.BSIM3noff > 4.0)
                {
                    sw.WriteLine($"Warning: Noff = {pParam.BSIM3noff} is too large.");
                    CircuitWarning.Warning(this, $"Warning: Noff = {pParam.BSIM3noff} is too large.");
                }

                if (pParam.BSIM3voffcv < -0.5)
                {
                    sw.WriteLine($"Warning: Voffcv = {pParam.BSIM3voffcv} is too small.");
                    CircuitWarning.Warning(this, $"Warning: Voffcv = {pParam.BSIM3voffcv} is too small.");
                }
                if (pParam.BSIM3voffcv > 0.5)
                {
                    sw.WriteLine($"Warning: Voffcv = {pParam.BSIM3voffcv} is too large.");
                    CircuitWarning.Warning(this, $"Warning: Voffcv = {pParam.BSIM3voffcv} is too large.");
                }

                if (model.BSIM3ijth < 0.0)
                {
                    sw.WriteLine($"Fatal: Ijth = {model.BSIM3ijth} cannot be negative.");
                    CircuitWarning.Warning(this, $"Fatal: Ijth = {model.BSIM3ijth} cannot be negative.");
                    Fatal_Flag = true;
                }

                /* Check capacitance parameters */
                if (pParam.BSIM3clc < 0.0)
                {
                    sw.WriteLine($"Fatal: Clc = {pParam.BSIM3clc} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Clc = {pParam.BSIM3clc} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM3moin < 5.0)
                {
                    sw.WriteLine($"Warning: Moin = {pParam.BSIM3moin} is too small.");
                    CircuitWarning.Warning(this, $"Warning: Moin = {pParam.BSIM3moin} is too small.");
                }
                if (pParam.BSIM3moin > 25.0)
                {
                    sw.WriteLine($"Warning: Moin = {pParam.BSIM3moin} is too large.");
                    CircuitWarning.Warning(this, $"Warning: Moin = {pParam.BSIM3moin} is too large.");
                }

                if (model.BSIM3capMod == 3)
				{
                    if (pParam.BSIM3acde < 0.4)
                    {
                        sw.WriteLine($"Warning:  Acde = {pParam.BSIM3acde} is too small.");
                        CircuitWarning.Warning(this, $"Warning: Acde = {pParam.BSIM3acde} is too small.");
                    }
                    if (pParam.BSIM3acde > 1.6)
                    {
                        sw.WriteLine($"Warning:  Acde = {pParam.BSIM3acde} is too large.");
                        CircuitWarning.Warning(this, $"Warning: Acde = {pParam.BSIM3acde} is too large.");
                    }
                }

                if (model.BSIM3paramChk == 1)
                {
                    /* Check L and W parameters */
                    if (pParam.BSIM3leff <= 5.0e-8)
                    {
                        sw.WriteLine($"Warning: Leff = {pParam.BSIM3leff} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Leff = {pParam.BSIM3leff} may be too small.");
                    }

                    if (pParam.BSIM3leffCV <= 5.0e-8)
                    {
                        sw.WriteLine($"Warning: Leff for CV = {pParam.BSIM3leffCV} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Leff for CV = {pParam.BSIM3leffCV} may be too small.");
                    }

                    if (pParam.BSIM3weff <= 1.0e-7)
                    {
                        sw.WriteLine($"Warning: Weff = {pParam.BSIM3weff} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Weff = {pParam.BSIM3weff} may be too small.");
                    }

                    if (pParam.BSIM3weffCV <= 1.0e-7)
                    {
                        sw.WriteLine($"Warning: Weff for CV = {pParam.BSIM3weffCV} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Weff for CV = {pParam.BSIM3weffCV} may be too small.");
                    }

                    /* Check threshold voltage parameters */
                    if (pParam.BSIM3nlx < 0.0)
                    {
                        sw.WriteLine($"Warning: Nlx = {pParam.BSIM3nlx} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Nlx = {pParam.BSIM3nlx} is negative.");
                    }
                    if (model.BSIM3tox < 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Tox = {model.BSIM3tox} is less than 10A.");
                        CircuitWarning.Warning(this, $"Warning: Tox = {model.BSIM3tox} is less than 10A.");
                    }

                    if (pParam.BSIM3npeak <= 1.0e15)
                    {
                        sw.WriteLine($"Warning: Nch = {pParam.BSIM3npeak} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Nch = {pParam.BSIM3npeak} may be too small.");
                    }
                    else if (pParam.BSIM3npeak >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Nch = {pParam.BSIM3npeak} may be too large.");
                        CircuitWarning.Warning(this, $"Warning: Nch = {pParam.BSIM3npeak} may be too large.");
                    }

                    if (pParam.BSIM3nsub <= 1.0e14)
                    {
                        sw.WriteLine($"Warning: Nsub = {pParam.BSIM3nsub} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Nsub = {pParam.BSIM3nsub} may be too small.");
                    }
                    else if (pParam.BSIM3nsub >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Nsub = {pParam.BSIM3nsub} may be too large.");
                        CircuitWarning.Warning(this, $"Warning: Nsub = {pParam.BSIM3nsub} may be too large.");
                    }

                    if ((pParam.BSIM3ngate > 0.0) &&
                        (pParam.BSIM3ngate <= 1.0e18))
                    {
                        sw.WriteLine($"Warning: Ngate = {pParam.BSIM3ngate} is less than 1.E18cm^-3.");
                        CircuitWarning.Warning(this, $"Warning: Ngate = {pParam.BSIM3ngate} is less than 1.E18cm^-3.");
                    }

                    if (pParam.BSIM3dvt0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Dvt0 = {pParam.BSIM3dvt0} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Dvt0 = {pParam.BSIM3dvt0} is negative.");
                    }

                    if (Math.Abs(1.0e-6 / (pParam.BSIM3w0 + pParam.BSIM3weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (W0 + Weff) may be too small.");
                        CircuitWarning.Warning(this, "Warning: (W0 + Weff) may be too small.");
                    }

                    /* Check subthreshold parameters */
                    if (pParam.BSIM3nfactor < 0.0)
                    {
                        sw.WriteLine($"Warning: Nfactor = {pParam.BSIM3nfactor} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Nfactor = {pParam.BSIM3nfactor} is negative.");
                    }
                    if (pParam.BSIM3cdsc < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdsc = {pParam.BSIM3cdsc} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Cdsc = {pParam.BSIM3cdsc} is negative.");
                    }
                    if (pParam.BSIM3cdscd < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdscd = {pParam.BSIM3cdscd} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Cdscd = {pParam.BSIM3cdscd} is negative.");
                    }
                    /* Check DIBL parameters */
                    if (pParam.BSIM3eta0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Eta0 = {pParam.BSIM3eta0} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Eta0 = {pParam.BSIM3eta0} is negative.");
                    }

                    /* Check Abulk parameters */
                    if (Math.Abs(1.0e-6 / (pParam.BSIM3b1 + pParam.BSIM3weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (B1 + Weff) may be too small.");
                        CircuitWarning.Warning(this, "Warning: (B1 + Weff) may be too small.");
                    }

                    /* Check Saturation parameters */
                    if (pParam.BSIM3a2 < 0.01)
                    {
                        sw.WriteLine($"Warning: A2 = {pParam.BSIM3a2} is too small. Set to 0.01.");
                        CircuitWarning.Warning(this, $"Warning: A2 = {pParam.BSIM3a2} is too small. Set to 0.01.");
                        pParam.BSIM3a2 = 0.01;
                    }
                    else if (pParam.BSIM3a2 > 1.0)
                    {
                        sw.WriteLine($"Warning: A2 = {pParam.BSIM3a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");
                        CircuitWarning.Warning(this, $"Warning: A2 = {pParam.BSIM3a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");
                        pParam.BSIM3a2 = 1.0;
                        pParam.BSIM3a1 = 0.0;
                    }

                    if (pParam.BSIM3rdsw < 0.0)
                    {
                        sw.WriteLine($"Warning: Rdsw = {pParam.BSIM3rdsw} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: Rdsw = {pParam.BSIM3rdsw} is negative. Set to zero.");
                        pParam.BSIM3rdsw = 0.0;
                        pParam.BSIM3rds0 = 0.0;
                    }
                    else if ((pParam.BSIM3rds0 > 0.0) && (pParam.BSIM3rds0 < 0.001))
                    {
                        sw.WriteLine($"Warning: Rds at current temperature = {pParam.BSIM3rds0} is less than 0.001 ohm. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: Rds at current temperature = {pParam.BSIM3rds0} is less than 0.001 ohm. Set to zero.");
                        pParam.BSIM3rds0 = 0.0;
                    }
                    if (pParam.BSIM3vsattemp < 1.0e3)
                    {
                        sw.WriteLine($"Warning: Vsat at current temperature = {pParam.BSIM3vsattemp} may be too small.");
                        CircuitWarning.Warning(this, $"Warning: Vsat at current temperature = {pParam.BSIM3vsattemp} may be too small.");
                    }

                    if (pParam.BSIM3pdibl1 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl1 = {pParam.BSIM3pdibl1} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Pdibl1 = {pParam.BSIM3pdibl1} is negative.");
                    }
                    if (pParam.BSIM3pdibl2 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl2 = {pParam.BSIM3pdibl2} is negative.");
                        CircuitWarning.Warning(this, $"Warning: Pdibl2 = {pParam.BSIM3pdibl2} is negative.");
                    }
                    /* Check overlap capacitance parameters */
                    if (model.BSIM3cgdo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgdo = {model.BSIM3cgdo} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: cgdo = {model.BSIM3cgdo} is negative. Set to zero.");
                        model.BSIM3cgdo.Value = 0.0;
                    }
                    if (model.BSIM3cgso < 0.0)
                    {
                        sw.WriteLine($"Warning: cgso = {model.BSIM3cgso} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: cgso = {model.BSIM3cgso} is negative. Set to zero.");
                        model.BSIM3cgso.Value = 0.0;
                    }
                    if (model.BSIM3cgbo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgbo = {model.BSIM3cgbo} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: cgbo = {model.BSIM3cgbo} is negative. Set to zero.");
                        model.BSIM3cgbo.Value = 0.0;
                    }

                }
            }

            return Fatal_Flag;
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(BSIM3states + BSIM3qb, ckt, ref timeStep);
            method.Terr(BSIM3states + BSIM3qg, ckt, ref timeStep);
            method.Terr(BSIM3states + BSIM3qd, ckt, ref timeStep);
        }
    }
}
