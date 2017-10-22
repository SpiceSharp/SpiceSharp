using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM2 : CircuitComponent<BSIM2>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM2Model model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Sizes
        /// </summary>
        private static Dictionary<Tuple<double, double>, BSIM2SizeDependParam> sizes = new Dictionary<Tuple<double, double>, BSIM2SizeDependParam>();

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter B2w { get; } = new Parameter(5e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter B2l { get; } = new Parameter(5e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter B2sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter B2drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter B2sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter B2drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter B2sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter B2drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool B2off { get; set; }
        [SpiceName("vbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter B2icVBS { get; } = new Parameter();
        [SpiceName("vds"), SpiceInfo("Initial D-S voltage")]
        public Parameter B2icVDS { get; } = new Parameter();
        [SpiceName("vgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter B2icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: B2icVBS.Set(value[2]); goto case 2;
                case 2: B2icVGS.Set(value[1]); goto case 1;
                case 1: B2icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double B2vdsat { get; private set; }
        public double B2von { get; private set; }
        public double B2drainConductance { get; private set; }
        public double B2sourceConductance { get; private set; }
        public double B2mode { get; private set; }
        public int B2dNode { get; private set; }
        public int B2gNode { get; private set; }
        public int B2sNode { get; private set; }
        public int B2bNode { get; private set; }
        public int B2dNodePrime { get; private set; }
        public int B2sNodePrime { get; private set; }
        public int B2states { get; private set; }

        private BSIM2SizeDependParam pParam = null;

        /// <summary>
        /// Constants
        /// </summary>
        private const int B2vbd = 0;
        private const int B2vbs = 1;
        private const int B2vgs = 2;
        private const int B2vds = 3;
        private const int B2cd = 4;
        private const int B2id = 4;
        private const int B2cbs = 5;
        private const int B2ibs = 5;
        private const int B2cbd = 6;
        private const int B2ibd = 6;
        private const int B2gm = 7;
        private const int B2gds = 8;
        private const int B2gmbs = 9;
        private const int B2gbd = 10;
        private const int B2gbs = 11;
        private const int B2qb = 12;
        private const int B2cqb = 13;
        private const int B2iqb = 13;
        private const int B2qg = 14;
        private const int B2cqg = 15;
        private const int B2iqg = 15;
        private const int B2qd = 16;
        private const int B2cqd = 17;
        private const int B2iqd = 17;
        private const int B2cggb = 18;
        private const int B2cgdb = 19;
        private const int B2cgsb = 20;
        private const int B2cbgb = 21;
        private const int B2cbdb = 22;
        private const int B2cbsb = 23;
        private const int B2capbd = 24;
        private const int B2iqbd = 25;
        private const int B2cqbd = 25;
        private const int B2capbs = 26;
        private const int B2iqbs = 27;
        private const int B2cqbs = 27;
        private const int B2cdgb = 28;
        private const int B2cddb = 29;
        private const int B2cdsb = 30;
        private const int B2vono = 31;
        private const int B2vdsato = 32;
        private const int B2qbs = 33;
        private const int B2qbd = 34;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM2(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM2Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            B2dNode = nodes[0].Index;
            B2gNode = nodes[1].Index;
            B2sNode = nodes[2].Index;
            B2bNode = nodes[3].Index;

            // Allocate states
            B2states = ckt.State.GetState(35);

            /* allocate a chunk of the state vector */

            /* perform the parameter defaulting */
            B2vdsat = 0;
            B2von = 0;

            /* process drain series resistance */
            if (model.B2sheetResistance != 0 && B2drainSquares != 0.0)
                B2dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                B2dNodePrime = B2dNode;

            /* process source series resistance */
            if (model.B2sheetResistance != 0 && B2sourceSquares != 0.0)
                B2sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                B2sNodePrime = B2sNode;

            if (sizes.Count > 0)
                sizes.Clear();
            pParam = null;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as BSIM2Model;
            double EffectiveLength;
            double EffectiveWidth;
            double Inv_L;
            double Inv_W;
            double tmp;
            double CoxWoverL;

            // Get the size dependent parameters
            Tuple<double, double> mysize = new Tuple<double, double>(B2w, B2l);
            if (sizes.ContainsKey(mysize))
                pParam = sizes[mysize];

            if (pParam == null)
            {
                pParam = new BSIM2SizeDependParam();
                sizes.Add(mysize, pParam);

                EffectiveLength = B2l - model.B2deltaL * 1.0e-6;
                EffectiveWidth = B2w - model.B2deltaW * 1.0e-6;

                if (EffectiveLength <= 0)
                    throw new CircuitException($"B2: mosfet {Name}, model {model.Name}: Effective channel length <= 0");
                if (EffectiveWidth <= 0)
                    throw new CircuitException($"B2: mosfet {Name}, model {model.Name}: Effective channel width <= 0");

                Inv_L = 1.0e-6 / EffectiveLength;
                Inv_W = 1.0e-6 / EffectiveWidth;
                pParam.B2vfb = model.B2vfb0 + model.B2vfbW * Inv_W + model.B2vfbL * Inv_L;
                pParam.B2phi = model.B2phi0 + model.B2phiW * Inv_W + model.B2phiL * Inv_L;
                pParam.B2k1 = model.B2k10 + model.B2k1W * Inv_W + model.B2k1L * Inv_L;
                pParam.B2k2 = model.B2k20 + model.B2k2W * Inv_W + model.B2k2L * Inv_L;
                pParam.B2eta0 = model.B2eta00 + model.B2eta0W * Inv_W + model.B2eta0L * Inv_L;
                pParam.B2etaB = model.B2etaB0 + model.B2etaBW * Inv_W + model.B2etaBL * Inv_L;
                pParam.B2beta0 = model.B2mob00;
                pParam.B2beta0B = model.B2mob0B0 + model.B2mob0BW * Inv_W + model.B2mob0BL * Inv_L;
                pParam.B2betas0 = model.B2mobs00 + model.B2mobs0W * Inv_W + model.B2mobs0L * Inv_L;
                if (pParam.B2betas0 < 1.01 * pParam.B2beta0)

                    pParam.B2betas0 = 1.01 * pParam.B2beta0;
                pParam.B2betasB = model.B2mobsB0 + model.B2mobsBW * Inv_W + model.B2mobsBL * Inv_L;
                tmp = (pParam.B2betas0 - pParam.B2beta0 - pParam.B2beta0B * model.B2vbb);
                if ((-pParam.B2betasB * model.B2vbb) > tmp)
                    pParam.B2betasB = -tmp / model.B2vbb;
                pParam.B2beta20 = model.B2mob200 + model.B2mob20W * Inv_W + model.B2mob20L * Inv_L;
                pParam.B2beta2B = model.B2mob2B0 + model.B2mob2BW * Inv_W + model.B2mob2BL * Inv_L;
                pParam.B2beta2G = model.B2mob2G0 + model.B2mob2GW * Inv_W + model.B2mob2GL * Inv_L;
                pParam.B2beta30 = model.B2mob300 + model.B2mob30W * Inv_W + model.B2mob30L * Inv_L;
                pParam.B2beta3B = model.B2mob3B0 + model.B2mob3BW * Inv_W + model.B2mob3BL * Inv_L;
                pParam.B2beta3G = model.B2mob3G0 + model.B2mob3GW * Inv_W + model.B2mob3GL * Inv_L;
                pParam.B2beta40 = model.B2mob400 + model.B2mob40W * Inv_W + model.B2mob40L * Inv_L;
                pParam.B2beta4B = model.B2mob4B0 + model.B2mob4BW * Inv_W + model.B2mob4BL * Inv_L;
                pParam.B2beta4G = model.B2mob4G0 + model.B2mob4GW * Inv_W + model.B2mob4GL * Inv_L;

                CoxWoverL = model.B2Cox * EffectiveWidth / EffectiveLength;

                pParam.B2beta0 *= CoxWoverL;
                pParam.B2beta0B *= CoxWoverL;
                pParam.B2betas0 *= CoxWoverL;
                pParam.B2betasB *= CoxWoverL;
                pParam.B2beta30 *= CoxWoverL;
                pParam.B2beta3B *= CoxWoverL;
                pParam.B2beta3G *= CoxWoverL;
                pParam.B2beta40 *= CoxWoverL;
                pParam.B2beta4B *= CoxWoverL;
                pParam.B2beta4G *= CoxWoverL;

                pParam.B2ua0 = model.B2ua00 + model.B2ua0W * Inv_W + model.B2ua0L * Inv_L;
                pParam.B2uaB = model.B2uaB0 + model.B2uaBW * Inv_W + model.B2uaBL * Inv_L;
                pParam.B2ub0 = model.B2ub00 + model.B2ub0W * Inv_W + model.B2ub0L * Inv_L;
                pParam.B2ubB = model.B2ubB0 + model.B2ubBW * Inv_W + model.B2ubBL * Inv_L;
                pParam.B2u10 = model.B2u100 + model.B2u10W * Inv_W + model.B2u10L * Inv_L;
                pParam.B2u1B = model.B2u1B0 + model.B2u1BW * Inv_W + model.B2u1BL * Inv_L;
                pParam.B2u1D = model.B2u1D0 + model.B2u1DW * Inv_W + model.B2u1DL * Inv_L;
                pParam.B2n0 = model.B2n00 + model.B2n0W * Inv_W + model.B2n0L * Inv_L;
                pParam.B2nB = model.B2nB0 + model.B2nBW * Inv_W + model.B2nBL * Inv_L;
                pParam.B2nD = model.B2nD0 + model.B2nDW * Inv_W + model.B2nDL * Inv_L;
                if (pParam.B2n0 < 0.0)

                    pParam.B2n0 = 0.0;

                pParam.B2vof0 = model.B2vof00 + model.B2vof0W * Inv_W + model.B2vof0L * Inv_L;
                pParam.B2vofB = model.B2vofB0 + model.B2vofBW * Inv_W + model.B2vofBL * Inv_L;
                pParam.B2vofD = model.B2vofD0 + model.B2vofDW * Inv_W + model.B2vofDL * Inv_L;
                pParam.B2ai0 = model.B2ai00 + model.B2ai0W * Inv_W + model.B2ai0L * Inv_L;
                pParam.B2aiB = model.B2aiB0 + model.B2aiBW * Inv_W + model.B2aiBL * Inv_L;
                pParam.B2bi0 = model.B2bi00 + model.B2bi0W * Inv_W + model.B2bi0L * Inv_L;
                pParam.B2biB = model.B2biB0 + model.B2biBW * Inv_W + model.B2biBL * Inv_L;
                pParam.B2vghigh = model.B2vghigh0 + model.B2vghighW * Inv_W + model.B2vghighL * Inv_L;
                pParam.B2vglow = model.B2vglow0 + model.B2vglowW * Inv_W + model.B2vglowL * Inv_L;

                pParam.CoxWL = model.B2Cox * EffectiveLength * EffectiveWidth * 1.0e4;
                pParam.One_Third_CoxWL = pParam.CoxWL / 3.0;
                pParam.Two_Third_CoxWL = 2.0 * pParam.One_Third_CoxWL;
                pParam.B2GSoverlapCap = model.B2gateSourceOverlapCap * EffectiveWidth;
                pParam.B2GDoverlapCap = model.B2gateDrainOverlapCap * EffectiveWidth;
                pParam.B2GBoverlapCap = model.B2gateBulkOverlapCap * EffectiveLength;
                pParam.SqrtPhi = Math.Sqrt(pParam.B2phi);
                pParam.Phis3 = pParam.SqrtPhi * pParam.B2phi;
                pParam.Arg = pParam.B2betasB - pParam.B2beta0B - model.B2vdd * (pParam.B2beta3B - model.B2vdd * pParam.B2beta4B);

            }

            /* process drain series resistance */
            if ((B2drainConductance = model.B2sheetResistance * B2drainSquares) != 0.0)
            {
                B2drainConductance = 1.0 / B2drainConductance;
            }

            /* process source series resistance */
            if ((B2sourceConductance = model.B2sheetResistance * B2sourceSquares) != 0.0)
            {
                B2sourceConductance = 1.0 / B2sourceConductance;
            }

            pParam.B2vt0 = pParam.B2vfb + pParam.B2phi + pParam.B2k1 * pParam.SqrtPhi - pParam.B2k2 * pParam.B2phi;
            B2von = pParam.B2vt0; /* added for initialization */
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public void Load(Circuit ckt)
        {
            var model = Model as BSIM2Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double EffectiveLength, DrainArea, SourceArea, DrainPerimeter, SourcePerimeter, DrainSatCurrent, SourceSatCurrent, GateSourceOverlapCap, GateDrainOverlapCap,
                        GateBulkOverlapCap, von, vdsat, vt0;
            int Check;
            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, vcrit, vgb, gbs, cbs, evbs, gbd, cbd,
                        evbd, cd, czbd, czbs, czbdsw, czbssw, PhiB, PhiBSW, MJ, MJSW, arg, argsw, sarg, sargsw, capbs = 0.0, capbd = 0.0;
            double ceqqg, gcdgb, gcsgb, gcggb, gcbgb, cqgate, cqbulk, cqdrn, ceqqb, ceqqd, ceqbs, ceqbd, xnrm, xrev, cdreq;
            double gm, gds, gmbs, qgate, qbulk, qdrn = 0.0, cggb, cgdb, cgsb, cbgb, cbdb, cbsb, cdgb = 0.0,
                cddb = 0.0, cdsb = 0.0, cdrain, qsrc = 0.0, csgb = 0.0, cssb = 0.0, csdb = 0.0;
            double gcgdb, gcgsb, gcbdb, gcbsb, gcddb, gcdsb, gcsdb, gcssb;

            EffectiveLength = B2l - model.B2deltaL * 1.0e-6; /* m */
            DrainArea = B2drainArea;
            SourceArea = B2sourceArea;
            DrainPerimeter = B2drainPerimeter;
            SourcePerimeter = B2sourcePerimeter;
            if ((DrainSatCurrent = DrainArea * model.B2jctSatCurDensity) < 1e-15)
            {
                DrainSatCurrent = 1.0e-15;
            }
            if ((SourceSatCurrent = SourceArea * model.B2jctSatCurDensity) < 1.0e-15)
            {
                SourceSatCurrent = 1.0e-15;
            }
            GateSourceOverlapCap = model.B2gateSourceOverlapCap * B2w;
            GateDrainOverlapCap = model.B2gateDrainOverlapCap * B2w;
            GateBulkOverlapCap = model.B2gateBulkOverlapCap * EffectiveLength;
            von = model.B2type * B2von;
            vdsat = model.B2type * B2vdsat;
            vt0 = model.B2type * pParam.B2vt0;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][B2states + B2vbs];
                vgs = state.States[0][B2states + B2vgs];
                vds = state.States[0][B2states + B2vds];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vbs = state.States[1][B2states + B2vbs];
                vgs = state.States[1][B2states + B2vgs];
                vds = state.States[1][B2states + B2vds];
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !B2off)
            {
                vds = model.B2type * B2icVDS;
                vgs = model.B2type * B2icVGS;
                vbs = model.B2type * B2icVBS;
                if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                            (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                {
                    vbs = -1;
                    vgs = vt0;
                    vds = 0;
                }
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitFix) && B2off)
            {
                vbs = vgs = vds = 0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.B2type * (rstate.OldSolution[B2bNode] - rstate.OldSolution[B2sNodePrime]);
                vgs = model.B2type * (rstate.OldSolution[B2gNode] - rstate.OldSolution[B2sNodePrime]);
                vds = model.B2type * (rstate.OldSolution[B2dNodePrime] - rstate.OldSolution[B2sNodePrime]);
                /* PREDICTOR */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][B2states + B2vgs] - state.States[0][B2states + B2vds];
                delvbs = vbs - state.States[0][B2states + B2vbs];
                delvbd = vbd - state.States[0][B2states + B2vbd];
                delvgs = vgs - state.States[0][B2states + B2vgs];
                delvds = vds - state.States[0][B2states + B2vds];
                delvgd = vgd - vgdo;

                if (B2mode >= 0)
                {
                    cdhat = state.States[0][B2states + B2cd] - state.States[0][B2states + B2gbd] * delvbd + state.States[0][B2states + B2gmbs] *
                         delvbs + state.States[0][B2states + B2gm] * delvgs + state.States[0][B2states + B2gds] * delvds;
                }
                else
                {
                    cdhat = state.States[0][B2states + B2cd] - (state.States[0][B2states + B2gbd] - state.States[0][B2states + B2gmbs]) * delvbd -
                         state.States[0][B2states + B2gm] * delvgd + state.States[0][B2states + B2gds] * delvds;
                }
                cbhat = state.States[0][B2states + B2cbs] + state.States[0][B2states + B2cbd] + state.States[0][B2states + B2gbd] * delvbd +
                     state.States[0][B2states + B2gbs] * delvbs;

                /* NOBYPASS */

                von = model.B2type * B2von;
                if (state.States[0][B2states + B2vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][B2states + B2vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][B2states + B2vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][B2states + B2vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * SourceSatCurrent));
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][B2states + B2vbs], Circuit.CONSTvt0, vcrit, ref Check); /* B2 test */
                    vbd = vbs - vds;
                }
                else
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * DrainSatCurrent));
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][B2states + B2vbd], Circuit.CONSTvt0, vcrit, ref Check); /* B2 test */
                    vbs = vbd + vds;
                }
            }

            /* determine DC current and derivatives */
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            if (vbs <= 0.0)
            {
                gbs = SourceSatCurrent / Circuit.CONSTvt0 + state.Gmin;
                cbs = gbs * vbs;
            }
            else
            {
                evbs = Math.Exp(vbs / Circuit.CONSTvt0);
                gbs = SourceSatCurrent * evbs / Circuit.CONSTvt0 + state.Gmin;
                cbs = SourceSatCurrent * (evbs - 1) + state.Gmin * vbs;
            }
            if (vbd <= 0.0)
            {
                gbd = DrainSatCurrent / Circuit.CONSTvt0 + state.Gmin;
                cbd = gbd * vbd;
            }
            else
            {
                evbd = Math.Exp(vbd / Circuit.CONSTvt0);
                gbd = DrainSatCurrent * evbd / Circuit.CONSTvt0 + state.Gmin;
                cbd = DrainSatCurrent * (evbd - 1) + state.Gmin * vbd;
            }
            /* line 400 */
            if (vds >= 0)
            {
                /* normal mode */
                B2mode = 1;
            }
            else
            {
                /* inverse mode */
                B2mode = -1;
            }
            /* call B2evaluate to calculate drain current and its 
            * derivatives and charge and capacitances related to gate
            * drain, and bulk
            */
            if (vds >= 0)
            {
                B2evaluate(vds, vbs, vgs, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qdrn, out cggb, out cgdb, out cgsb, out cbgb, out cbdb, out cbsb, out cdgb,
                out cddb, out cdsb, out cdrain, out von, out vdsat, ckt);
            }
            else
            {
                B2evaluate(-vds, vbd, vgd, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qsrc, out cggb, out cgsb, out cgdb, out cbgb, out cbsb, out cbdb, out csgb,
                out cssb, out csdb, out cdrain, out von, out vdsat, ckt);
            }

            B2von = model.B2type * von;
            B2vdsat = model.B2type * vdsat;

            /* 
            * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
            */
            cd = B2mode * cdrain - cbd;
            if (method != null || state.UseSmallSignal || (state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC))
            {
                /* 
                * charge storage elements
                * 
                * bulk - drain and bulk - source depletion capacitances
                * czbd : zero bias drain junction capacitance
                * czbs : zero bias source junction capacitance
                * czbdsw:zero bias drain junction sidewall capacitance
                * czbssw:zero bias source junction sidewall capacitance
                */

                czbd = model.B2unitAreaJctCap * DrainArea;
                czbs = model.B2unitAreaJctCap * SourceArea;
                czbdsw = model.B2unitLengthSidewallJctCap * DrainPerimeter;
                czbssw = model.B2unitLengthSidewallJctCap * SourcePerimeter;
                PhiB = model.B2bulkJctPotential;
                PhiBSW = model.B2sidewallJctPotential;
                MJ = model.B2bulkJctBotGradingCoeff;
                MJSW = model.B2bulkJctSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs < 0)
                {
                    arg = 1 - vbs / PhiB;
                    argsw = 1 - vbs / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][B2states + B2qbs] = PhiB * czbs * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbssw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbs = czbs * sarg + czbssw * sargsw;
                }
                else
                {
                    state.States[0][B2states + B2qbs] = vbs * (czbs + czbssw) + vbs * vbs * (czbs * MJ * 0.5 / PhiB + czbssw * MJSW * 0.5 / PhiBSW);
                    capbs = czbs + czbssw + vbs * (czbs * MJ / PhiB + czbssw * MJSW / PhiBSW);
                }

                /* Drain Bulk Junction */
                if (vbd < 0)
                {
                    arg = 1 - vbd / PhiB;
                    argsw = 1 - vbd / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][B2states + B2qbd] = PhiB * czbd * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbdsw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbd = czbd * sarg + czbdsw * sargsw;
                }
                else
                {
                    state.States[0][B2states + B2qbd] = vbd * (czbd + czbdsw) + vbd * vbd * (czbd * MJ * 0.5 / PhiB + czbdsw * MJSW * 0.5 / PhiBSW);
                    capbd = czbd + czbdsw + vbd * (czbd * MJ / PhiB + czbdsw * MJSW / PhiBSW);
                }

            }

            /* 
            * check convergence
            */
            if (!B2off || state.Init != CircuitState.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][B2states + B2vbs] = vbs;
            state.States[0][B2states + B2vbd] = vbd;
            state.States[0][B2states + B2vgs] = vgs;
            state.States[0][B2states + B2vds] = vds;
            state.States[0][B2states + B2cd] = cd;
            state.States[0][B2states + B2cbs] = cbs;
            state.States[0][B2states + B2cbd] = cbd;
            state.States[0][B2states + B2gm] = gm;
            state.States[0][B2states + B2gds] = gds;
            state.States[0][B2states + B2gmbs] = gmbs;
            state.States[0][B2states + B2gbd] = gbd;
            state.States[0][B2states + B2gbs] = gbs;

            state.States[0][B2states + B2cggb] = cggb;
            state.States[0][B2states + B2cgdb] = cgdb;
            state.States[0][B2states + B2cgsb] = cgsb;

            state.States[0][B2states + B2cbgb] = cbgb;
            state.States[0][B2states + B2cbdb] = cbdb;
            state.States[0][B2states + B2cbsb] = cbsb;

            state.States[0][B2states + B2cdgb] = cdgb;
            state.States[0][B2states + B2cddb] = cddb;
            state.States[0][B2states + B2cdsb] = cdsb;

            state.States[0][B2states + B2capbs] = capbs;
            state.States[0][B2states + B2capbd] = capbd;

            /* bulk and channel charge plus overlaps */

            if (method == null && ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || !state.UseIC) && !state.UseSmallSignal)
                goto line850;

            if (B2mode > 0)
            {
                double[] args = new double[8];
                args[0] = GateDrainOverlapCap;
                args[1] = GateSourceOverlapCap;
                args[2] = GateBulkOverlapCap;
                args[3] = capbd;
                args[4] = capbs;
                args[5] = cggb;
                args[6] = cgdb;
                args[7] = cgsb;

                B2mosCap(ckt, vgd, vgs, vgb, args, cbgb, cbdb, cbsb, cdgb, cddb, cdsb, 
                    out gcggb, out gcgdb, out gcgsb, out gcbgb, out gcbdb, out gcbsb, out gcdgb,
                    out gcddb, out gcdsb, out gcsgb, out gcsdb, out gcssb, ref qgate, ref qbulk,
                    ref qdrn, out qsrc);
            }
            else
            {
                double[] args = new double[8];
                args[0] = GateSourceOverlapCap;
                args[1] = GateDrainOverlapCap;
                args[2] = GateBulkOverlapCap;
                args[3] = capbs;
                args[4] = capbd;
                args[5] = cggb;
                args[6] = cgsb;
                args[7] = cgdb;

                B2mosCap(ckt, vgs, vgd, vgb, args, cbgb, cbsb, cbdb, csgb, cssb, csdb,
                    out gcggb, out gcgsb, out gcgdb, out gcbgb, out gcbsb, out gcbdb, out gcsgb,
                    out gcssb, out gcsdb, out gcdgb, out gcdsb, out gcddb, ref qgate, ref qbulk,
                    ref qsrc, out qdrn);
            }

            state.States[0][B2states + B2qg] = qgate;
            state.States[0][B2states + B2qd] = qdrn - state.States[0][B2states + B2qbd];
            state.States[0][B2states + B2qb] = qbulk + state.States[0][B2states + B2qbd] + state.States[0][B2states + B2qbs];

            /* store small signal parameters */
            if (method == null && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC)
                goto line850;

            if (state.UseSmallSignal)
            {
                state.States[0][B2states + B2cggb] = cggb;
                state.States[0][B2states + B2cgdb] = cgdb;
                state.States[0][B2states + B2cgsb] = cgsb;
                state.States[0][B2states + B2cbgb] = cbgb;
                state.States[0][B2states + B2cbdb] = cbdb;
                state.States[0][B2states + B2cbsb] = cbsb;
                state.States[0][B2states + B2cdgb] = cdgb;
                state.States[0][B2states + B2cddb] = cddb;
                state.States[0][B2states + B2cdsb] = cdsb;
                state.States[0][B2states + B2capbd] = capbd;
                state.States[0][B2states + B2capbs] = capbs;

                goto line1000;
            }

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][B2states + B2qb] = state.States[0][B2states + B2qb];
                state.States[1][B2states + B2qg] = state.States[0][B2states + B2qg];
                state.States[1][B2states + B2qd] = state.States[0][B2states + B2qd];
            }

            if (method != null)
            {
                method.Integrate(state, B2states + B2qb, 0.0);
                method.Integrate(state, B2states + B2qg, 0.0);
                method.Integrate(state, B2states + B2qd, 0.0);
            }

            goto line860;

            line850:
            /* initialize to zero charge conductance and current */
            ceqqg = ceqqb = ceqqd = 0.0;
            gcdgb = gcddb = gcdsb = 0.0;
            gcsgb = gcsdb = gcssb = 0.0;
            gcggb = gcgdb = gcgsb = 0.0;
            gcbgb = gcbdb = gcbsb = 0.0;
            goto line900;

            line860:
            /* evaluate equivalent charge current */
            cqgate = state.States[0][B2states + B2iqg];
            cqbulk = state.States[0][B2states + B2iqb];
            cqdrn = state.States[0][B2states + B2iqd];
            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][B2states + B2iqb] = state.States[0][B2states + B2iqb];
                state.States[1][B2states + B2iqg] = state.States[0][B2states + B2iqg];
                state.States[1][B2states + B2iqd] = state.States[0][B2states + B2iqd];
            }

            /* 
            * load current vector
            */
            line900:

            ceqbs = model.B2type * (cbs - (gbs - state.Gmin) * vbs);
            ceqbd = model.B2type * (cbd - (gbd - state.Gmin) * vbd);

            ceqqg = model.B2type * ceqqg;
            ceqqb = model.B2type * ceqqb;
            ceqqd = model.B2type * ceqqd;
            if (B2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.B2type * (cdrain - gds * vds - gm * vgs - gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.B2type) * (cdrain + gds * vds - gm * vgd - gmbs * vbd);
            }

            rstate.Rhs[B2gNode] -= ceqqg;
            rstate.Rhs[B2bNode] -= (ceqbs + ceqbd + ceqqb);
            rstate.Rhs[B2dNodePrime] += (ceqbd - cdreq - ceqqd);
            rstate.Rhs[B2sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);

            /* 
            * load y matrix
            */

            rstate.Matrix[B2dNode, B2dNode] += (B2drainConductance);
            rstate.Matrix[B2gNode, B2gNode] += (gcggb);
            rstate.Matrix[B2sNode, B2sNode] += (B2sourceConductance);
            rstate.Matrix[B2bNode, B2bNode] += (gbd + gbs - gcbgb - gcbdb - gcbsb);
            rstate.Matrix[B2dNodePrime, B2dNodePrime] += (B2drainConductance + gds + gbd + xrev * (gm + gmbs) + gcddb);
            rstate.Matrix[B2sNodePrime, B2sNodePrime] += (B2sourceConductance + gds + gbs + xnrm * (gm + gmbs) + gcssb);
            rstate.Matrix[B2dNode, B2dNodePrime] += (-B2drainConductance);
            rstate.Matrix[B2gNode, B2bNode] += (-gcggb - gcgdb - gcgsb);
            rstate.Matrix[B2gNode, B2dNodePrime] += (gcgdb);
            rstate.Matrix[B2gNode, B2sNodePrime] += (gcgsb);
            rstate.Matrix[B2sNode, B2sNodePrime] += (-B2sourceConductance);
            rstate.Matrix[B2bNode, B2gNode] += (gcbgb);
            rstate.Matrix[B2bNode, B2dNodePrime] += (-gbd + gcbdb);
            rstate.Matrix[B2bNode, B2sNodePrime] += (-gbs + gcbsb);
            rstate.Matrix[B2dNodePrime, B2dNode] += (-B2drainConductance);
            rstate.Matrix[B2dNodePrime, B2gNode] += ((xnrm - xrev) * gm + gcdgb);
            rstate.Matrix[B2dNodePrime, B2bNode] += (-gbd + (xnrm - xrev) * gmbs - gcdgb - gcddb - gcdsb);
            rstate.Matrix[B2dNodePrime, B2sNodePrime] += (-gds - xnrm * (gm + gmbs) + gcdsb);
            rstate.Matrix[B2sNodePrime, B2gNode] += (-(xnrm - xrev) * gm + gcsgb);
            rstate.Matrix[B2sNodePrime, B2sNode] += (-B2sourceConductance);
            rstate.Matrix[B2sNodePrime, B2bNode] += (-gbs - (xnrm - xrev) * gmbs - gcsgb - gcsdb - gcssb);
            rstate.Matrix[B2sNodePrime, B2dNodePrime] += (-gds - xrev * (gm + gmbs) + gcsdb);

            line1000:;
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public void AcLoad(Circuit ckt)
        {
            var model = Model as BSIM2Model;
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm;
            int xrev;
            double gdpr, gspr, gm, gds, gmbs, gbd, gbs, capbd, capbs, cggb, cgsb, cgdb, cbgb, cbsb, cbdb, cdgb, cdsb, cddb;
            Complex xcdgb, xcddb, xcdsb, xcsgb, xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;

            if (B2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
            }
            else
            {
                xnrm = 0;
                xrev = 1;
            }
            gdpr = B2drainConductance;
            gspr = B2sourceConductance;
            gm = state.States[0][B2states + B2gm];
            gds = state.States[0][B2states + B2gds];
            gmbs = state.States[0][B2states + B2gmbs];
            gbd = state.States[0][B2states + B2gbd];
            gbs = state.States[0][B2states + B2gbs];
            capbd = state.States[0][B2states + B2capbd];
            capbs = state.States[0][B2states + B2capbs];
            /* 
            * charge oriented model parameters
            */

            cggb = state.States[0][B2states + B2cggb];
            cgsb = state.States[0][B2states + B2cgsb];
            cgdb = state.States[0][B2states + B2cgdb];

            cbgb = state.States[0][B2states + B2cbgb];
            cbsb = state.States[0][B2states + B2cbsb];
            cbdb = state.States[0][B2states + B2cbdb];

            cdgb = state.States[0][B2states + B2cdgb];
            cdsb = state.States[0][B2states + B2cdsb];
            cddb = state.States[0][B2states + B2cddb];

            xcdgb = (cdgb - pParam.B2GDoverlapCap) * cstate.Laplace;
            xcddb = (cddb + capbd + pParam.B2GDoverlapCap) * cstate.Laplace;
            xcdsb = cdsb * cstate.Laplace;
            xcsgb = -(cggb + cbgb + cdgb + pParam.B2GSoverlapCap) * cstate.Laplace;
            xcsdb = -(cgdb + cbdb + cddb) * cstate.Laplace;
            xcssb = (capbs + pParam.B2GSoverlapCap - (cgsb + cbsb + cdsb)) * cstate.Laplace;
            xcggb = (cggb + pParam.B2GDoverlapCap + pParam.B2GSoverlapCap + pParam.B2GBoverlapCap) * cstate.Laplace;
            xcgdb = (cgdb - pParam.B2GDoverlapCap) * cstate.Laplace;
            xcgsb = (cgsb - pParam.B2GSoverlapCap) * cstate.Laplace;
            xcbgb = (cbgb - pParam.B2GBoverlapCap) * cstate.Laplace;
            xcbdb = (cbdb - capbd) * cstate.Laplace;
            xcbsb = (cbsb - capbs) * cstate.Laplace;

            cstate.Matrix[B2gNode, B2gNode] += xcggb;
            cstate.Matrix[B2bNode, B2bNode] += -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[B2dNodePrime, B2dNodePrime] += xcddb;
            cstate.Matrix[B2sNodePrime, B2sNodePrime] += xcssb;
            cstate.Matrix[B2gNode, B2bNode] += -xcggb - xcgdb - xcgsb;
            cstate.Matrix[B2gNode, B2dNodePrime] += xcgdb;
            cstate.Matrix[B2gNode, B2sNodePrime] += xcgsb;
            cstate.Matrix[B2bNode, B2gNode] += xcbgb;
            cstate.Matrix[B2bNode, B2dNodePrime] += xcbdb;
            cstate.Matrix[B2bNode, B2sNodePrime] += xcbsb;
            cstate.Matrix[B2dNodePrime, B2gNode] += xcdgb;
            cstate.Matrix[B2dNodePrime, B2bNode] += -xcdgb - xcddb - xcdsb;
            cstate.Matrix[B2dNodePrime, B2sNodePrime] += xcdsb;
            cstate.Matrix[B2sNodePrime, B2gNode] += xcsgb;
            cstate.Matrix[B2sNodePrime, B2bNode] += -xcsgb - xcsdb - xcssb;
            cstate.Matrix[B2sNodePrime, B2dNodePrime] += xcsdb;
            cstate.Matrix[B2dNode, B2dNode] += gdpr;
            cstate.Matrix[B2sNode, B2sNode] += gspr;
            cstate.Matrix[B2bNode, B2bNode] += gbd + gbs + -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[B2dNodePrime, B2dNodePrime] += gdpr + gds + gbd + xrev * (gm + gmbs) + xcddb;
            cstate.Matrix[B2sNodePrime, B2sNodePrime] += gspr + gds + gbs + xnrm * (gm + gmbs) + xcssb;
            cstate.Matrix[B2dNode, B2dNodePrime] -= gdpr;
            cstate.Matrix[B2sNode, B2sNodePrime] -= gspr;
            cstate.Matrix[B2bNode, B2dNodePrime] -= gbd - xcbdb;
            cstate.Matrix[B2bNode, B2sNodePrime] -= gbs - xcbsb;
            cstate.Matrix[B2dNodePrime, B2dNode] -= gdpr;
            cstate.Matrix[B2dNodePrime, B2gNode] += (xnrm - xrev) * gm + xcdgb;
            cstate.Matrix[B2dNodePrime, B2bNode] += -gbd + (xnrm - xrev) * gmbs + -xcdgb - xcddb - xcdsb;
            cstate.Matrix[B2dNodePrime, B2sNodePrime] += -gds - xnrm * (gm + gmbs) + xcdsb;
            cstate.Matrix[B2sNodePrime, B2gNode] += -(xnrm - xrev) * gm + xcsgb;
            cstate.Matrix[B2sNodePrime, B2sNode] -= gspr;
            cstate.Matrix[B2sNodePrime, B2bNode] += -gbs - (xnrm - xrev) * gmbs + -xcsgb - xcsdb - xcssb;
            cstate.Matrix[B2sNodePrime, B2dNodePrime] += -gds - xrev * (gm + gmbs) + xcsdb;
        }

        /* This routine evaluates the drain current, its derivatives and the
         * charges associated with the gate,bulk and drain terminal
         * using the B2 (Berkeley Short-Channel IGFET Model) Equations.
         */
        private void B2evaluate(double Vds, double Vbs, double Vgs, out double gm, out double gds, out double gmb, out double qg, 
            out double qb, out double qd, out double cgg, out double cgd, out double cgs,
            out double cbg, out double cbd, out double cbs, out double cdg, out double cdd, 
            out double cds, out double Ids, out double von, out double vdsat, Circuit ckt)
        {
            var model = Model as BSIM2Model;
            double Vth, Vdsat = 0.0;
            double Phisb, T1s, Eta, Gg, Aa, Inv_Aa, U1, U1s, Vc, Kk, SqrtKk;
            double dPhisb_dVb, dT1s_dVb, dVth_dVb, dVth_dVd, dAa_dVb, dVc_dVd;
            double dVc_dVg, dVc_dVb, dKk_dVc, dVdsat_dVd = 0.0, dVdsat_dVg = 0.0, dVdsat_dVb = 0.0;
            double dUvert_dVg, dUvert_dVd, dUvert_dVb, Inv_Kk;
            double dUtot_dVd, dUtot_dVb, dUtot_dVg, Ai, Bi, Vghigh, Vglow, Vgeff, Vof;
            double Vbseff, Vgst, Vgdt, Qbulk, Utot;
            double T0, T1, T2, T3, T4, T5, Arg1, Arg2, Exp0 = 0.0;
            double tmp, tmp1, tmp2, tmp3, Uvert, Beta1, Beta2, Beta0, dGg_dVb, Exp1 = 0.0;
            double T6, T7, T8, T9, n = 0.0, ExpArg, ExpArg1;
            double Beta, dQbulk_dVb, dVgdt_dVg, dVgdt_dVd;
            double dVbseff_dVb, Ua, Ub, dVgdt_dVb, dQbulk_dVd;
            double Con1, Con3, Con4, SqrVghigh, SqrVglow, CubVghigh, CubVglow;
            double delta, Coeffa, Coeffb, Coeffc, Coeffd, Inv_Uvert, Inv_Utot;
            double Inv_Vdsat, tanh, Sqrsech, dBeta1_dVb, dU1_dVd, dU1_dVg, dU1_dVb;
            double Betaeff, FR, dFR_dVd, dFR_dVg, dFR_dVb, Betas, Beta3, Beta4;
            double dBeta_dVd, dBeta_dVg, dBeta_dVb, dVgeff_dVg, dVgeff_dVd, dVgeff_dVb;
            double dCon3_dVd, dCon3_dVb, dCon4_dVd, dCon4_dVb, dCoeffa_dVd, dCoeffa_dVb;
            double dCoeffb_dVd, dCoeffb_dVb, dCoeffc_dVd, dCoeffc_dVb;
            double dCoeffd_dVd, dCoeffd_dVb;
            bool ChargeComputationNeeded;
            int valuetypeflag;			/* added  3/19/90 JSD   */

            if (ckt.Method != null || (ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC && ckt.State.UseIC) || ckt.State.UseSmallSignal)
                ChargeComputationNeeded = true;
            else
                ChargeComputationNeeded = false;

            if (Vbs < model.B2vbb2) Vbs = model.B2vbb2;
            if (Vgs > model.B2vgg2) Vgs = model.B2vgg2;
            if (Vds > model.B2vdd2) Vds = model.B2vdd2;

            /* Threshold Voltage. */
            if (Vbs <= 0.0)
            {
                Phisb = pParam.B2phi - Vbs;
                dPhisb_dVb = -1.0;
                T1s = Math.Sqrt(Phisb);
                dT1s_dVb = -0.5 / T1s;
            }
            else
            {
                tmp = pParam.B2phi / (pParam.B2phi + Vbs);
                Phisb = pParam.B2phi * tmp;
                dPhisb_dVb = -tmp * tmp;
                T1s = pParam.Phis3 / (pParam.B2phi + 0.5 * Vbs);
                dT1s_dVb = -0.5 * T1s * T1s / pParam.Phis3;
            }

            Eta = pParam.B2eta0 + pParam.B2etaB * Vbs;
            Ua = pParam.B2ua0 + pParam.B2uaB * Vbs;
            Ub = pParam.B2ub0 + pParam.B2ubB * Vbs;
            U1s = pParam.B2u10 + pParam.B2u1B * Vbs;

            Vth = pParam.B2vfb + pParam.B2phi + pParam.B2k1
                * T1s - pParam.B2k2 * Phisb - Eta * Vds;
            dVth_dVd = -Eta;
            dVth_dVb = pParam.B2k1 * dT1s_dVb + pParam.B2k2
                 - pParam.B2etaB * Vds;

            Vgst = Vgs - Vth;

            tmp = 1.0 / (1.744 + 0.8364 * Phisb);
            Gg = 1.0 - tmp;
            dGg_dVb = 0.8364 * tmp * tmp * dPhisb_dVb;
            T0 = Gg / T1s;
            tmp1 = 0.5 * T0 * pParam.B2k1;
            Aa = 1.0 + tmp1;
            dAa_dVb = (Aa - 1.0) * (dGg_dVb / Gg - dT1s_dVb / T1s);
            Inv_Aa = 1.0 / Aa;

            Vghigh = pParam.B2vghigh;
            Vglow = pParam.B2vglow;

            if ((Vgst >= Vghigh) || (pParam.B2n0 == 0.0))
            {
                Vgeff = Vgst;
                dVgeff_dVg = 1.0;
                dVgeff_dVd = -dVth_dVd;
                dVgeff_dVb = -dVth_dVb;
            }
            else
            {
                Vof = pParam.B2vof0 + pParam.B2vofB * Vbs
                + pParam.B2vofD * Vds;
                n = pParam.B2n0 + pParam.B2nB / T1s
                      + pParam.B2nD * Vds;
                tmp = 0.5 / (n * model.B2Vtm);

                ExpArg1 = -Vds / model.B2Vtm;
                ExpArg1 = Math.Max(ExpArg1, -30.0);
                Exp1 = Math.Exp(ExpArg1);
                tmp1 = 1.0 - Exp1;
                tmp1 = Math.Max(tmp1, 1.0e-18);
                tmp2 = 2.0 * Aa * tmp1;

                if (Vgst <= Vglow)
                {
                    ExpArg = Vgst * tmp;
                    ExpArg = Math.Max(ExpArg, -30.0);
                    Exp0 = Math.Exp(0.5 * Vof + ExpArg);
                    Vgeff = Math.Sqrt(tmp2) * model.B2Vtm * Exp0;
                    T0 = n * model.B2Vtm;
                    dVgeff_dVg = Vgeff * tmp;
                    dVgeff_dVd = dVgeff_dVg * (n / tmp1 * Exp1 - dVth_dVd - Vgst
                         * pParam.B2nD / n + T0 * pParam.B2vofD);
                    dVgeff_dVb = dVgeff_dVg * (pParam.B2vofB * T0
                           - dVth_dVb + pParam.B2nB * Vgst
                           / (n * T1s * T1s) * dT1s_dVb + T0 * Inv_Aa * dAa_dVb);
                }
                else
                {
                    ExpArg = Vglow * tmp;
                    ExpArg = Math.Max(ExpArg, -30.0);
                    Exp0 = Math.Exp(0.5 * Vof + ExpArg);
                    Vgeff = Math.Sqrt(2.0 * Aa * (1.0 - Exp1)) * model.B2Vtm * Exp0;
                    Con1 = Vghigh;
                    Con3 = Vgeff;
                    Con4 = Con3 * tmp;
                    SqrVghigh = Vghigh * Vghigh;
                    SqrVglow = Vglow * Vglow;
                    CubVghigh = Vghigh * SqrVghigh;
                    CubVglow = Vglow * SqrVglow;
                    T0 = 2.0 * Vghigh;
                    T1 = 2.0 * Vglow;
                    T2 = 3.0 * SqrVghigh;
                    T3 = 3.0 * SqrVglow;
                    T4 = Vghigh - Vglow;
                    T5 = SqrVghigh - SqrVglow;
                    T6 = CubVghigh - CubVglow;
                    T7 = Con1 - Con3;
                    delta = (T1 - T0) * T6 + (T2 - T3) * T5 + (T0 * T3 - T1 * T2) * T4;
                    delta = 1.0 / delta;
                    Coeffb = (T1 - Con4 * T0) * T6 + (Con4 * T2 - T3) * T5
                   + (T0 * T3 - T1 * T2) * T7;
                    Coeffc = (Con4 - 1.0) * T6 + (T2 - T3) * T7 + (T3 - Con4 * T2) * T4;
                    Coeffd = (T1 - T0) * T7 + (1.0 - Con4) * T5 + (Con4 * T0 - T1) * T4;
                    Coeffa = SqrVghigh * (Coeffc + Coeffd * T0);
                    Vgeff = (Coeffa + Vgst * (Coeffb + Vgst * (Coeffc + Vgst * Coeffd)))

                  * delta;
                    dVgeff_dVg = (Coeffb + Vgst * (2.0 * Coeffc + 3.0 * Vgst * Coeffd))

                                   * delta;
                    T7 = Con3 * tmp;
                    T8 = dT1s_dVb * pParam.B2nB / (T1s * T1s * n);
                    T9 = n * model.B2Vtm;
                    dCon3_dVd = T7 * (n * Exp1 / tmp1 - Vglow * pParam.B2nD
                          / n + T9 * pParam.B2vofD);
                    dCon3_dVb = T7 * (T9 * Inv_Aa * dAa_dVb + Vglow * T8
                          + T9 * pParam.B2vofB);
                    dCon4_dVd = tmp * dCon3_dVd - T7 * pParam.B2nD / n;
                    dCon4_dVb = tmp * dCon3_dVb + T7 * T8;

                    dCoeffb_dVd = dCon4_dVd * (T2 * T5 - T0 * T6) + dCon3_dVd
                    * (T1 * T2 - T0 * T3);
                    dCoeffc_dVd = dCon4_dVd * (T6 - T2 * T4) + dCon3_dVd * (T3 - T2);
                    dCoeffd_dVd = dCon4_dVd * (T0 * T4 - T5) + dCon3_dVd * (T0 - T1);
                    dCoeffa_dVd = SqrVghigh * (dCoeffc_dVd + dCoeffd_dVd * T0);

                    dVgeff_dVd = -dVgeff_dVg * dVth_dVd + (dCoeffa_dVd + Vgst
                          * (dCoeffb_dVd + Vgst * (dCoeffc_dVd + Vgst
                         * dCoeffd_dVd))) * delta;

                    dCoeffb_dVb = dCon4_dVb * (T2 * T5 - T0 * T6) + dCon3_dVb
                    * (T1 * T2 - T0 * T3);
                    dCoeffc_dVb = dCon4_dVb * (T6 - T2 * T4) + dCon3_dVb * (T3 - T2);
                    dCoeffd_dVb = dCon4_dVb * (T0 * T4 - T5) + dCon3_dVb * (T0 - T1);
                    dCoeffa_dVb = SqrVghigh * (dCoeffc_dVb + dCoeffd_dVb * T0);

                    dVgeff_dVb = -dVgeff_dVg * dVth_dVb + (dCoeffa_dVb + Vgst
                          * (dCoeffb_dVb + Vgst * (dCoeffc_dVb + Vgst
                         * dCoeffd_dVb))) * delta;
                }
            }

            if (Vgeff > 0.0)
            {
                Uvert = 1.0 + Vgeff * (Ua + Vgeff * Ub);
                Uvert = Math.Max(Uvert, 0.2);
                Inv_Uvert = 1.0 / Uvert;
                T8 = Ua + 2.0 * Ub * Vgeff;
                dUvert_dVg = T8 * dVgeff_dVg;
                dUvert_dVd = T8 * dVgeff_dVd;
                dUvert_dVb = T8 * dVgeff_dVb + Vgeff * (pParam.B2uaB
                               + Vgeff * pParam.B2ubB);

                T8 = U1s * Inv_Aa * Inv_Uvert;
                Vc = T8 * Vgeff;
                T9 = Vc * Inv_Uvert;
                dVc_dVg = T8 * dVgeff_dVg - T9 * dUvert_dVg;
                dVc_dVd = T8 * dVgeff_dVd - T9 * dUvert_dVd;
                dVc_dVb = T8 * dVgeff_dVb + pParam.B2u1B * Vgeff * Inv_Aa

                            * Inv_Uvert - Vc * Inv_Aa * dAa_dVb - T9 * dUvert_dVb;


                tmp2 = Math.Sqrt(1.0 + 2.0 * Vc);
                Kk = 0.5 * (1.0 + Vc + tmp2);
                Inv_Kk = 1.0 / Kk;
                dKk_dVc = 0.5 + 0.5 / tmp2;
                SqrtKk = Math.Sqrt(Kk);

                T8 = Inv_Aa / SqrtKk;
                Vdsat = Vgeff * T8;
                Vdsat = Math.Max(Vdsat, 1.0e-18);
                Inv_Vdsat = 1.0 / Vdsat;
                T9 = 0.5 * Vdsat * Inv_Kk * dKk_dVc;
                dVdsat_dVd = T8 * dVgeff_dVd - T9 * dVc_dVd;
                dVdsat_dVg = T8 * dVgeff_dVg - T9 * dVc_dVg;
                dVdsat_dVb = T8 * dVgeff_dVb - T9 * dVc_dVb - Vdsat * Inv_Aa * dAa_dVb;

                Beta0 = pParam.B2beta0 + pParam.B2beta0B * Vbs;
                Betas = pParam.B2betas0 + pParam.B2betasB * Vbs;
                Beta2 = pParam.B2beta20 + pParam.B2beta2B * Vbs
                          + pParam.B2beta2G * Vgs;
                Beta3 = pParam.B2beta30 + pParam.B2beta3B * Vbs
                          + pParam.B2beta3G * Vgs;
                Beta4 = pParam.B2beta40 + pParam.B2beta4B * Vbs
                          + pParam.B2beta4G * Vgs;
                Beta1 = Betas - (Beta0 + model.B2vdd * (Beta3 - model.B2vdd
                 * Beta4));

                T0 = Vds * Beta2 * Inv_Vdsat;
                T0 = Math.Min(T0, 30.0);
                T1 = Math.Exp(T0);
                T2 = T1 * T1;
                T3 = T2 + 1.0;
                tanh = (T2 - 1.0) / T3;
                Sqrsech = 4.0 * T2 / (T3 * T3);

                Beta = Beta0 + Beta1 * tanh + Vds * (Beta3 - Beta4 * Vds);
                T4 = Beta1 * Sqrsech * Inv_Vdsat;
                T5 = model.B2vdd * tanh;
                dBeta_dVd = Beta3 - 2.0 * Beta4 * Vds + T4 * (Beta2 - T0 * dVdsat_dVd);
                dBeta_dVg = T4 * (pParam.B2beta2G * Vds - T0 * dVdsat_dVg)
                      + pParam.B2beta3G * (Vds - T5)
                  - pParam.B2beta4G * (Vds * Vds - model.B2vdd * T5);
                dBeta1_dVb = pParam.Arg;
                dBeta_dVb = pParam.B2beta0B + dBeta1_dVb * tanh + Vds
                     * (pParam.B2beta3B - Vds * pParam.B2beta4B)
                      + T4 * (pParam.B2beta2B * Vds - T0 * dVdsat_dVb);


                if (Vgst > Vglow)
                {
                    if (Vds <= Vdsat) /* triode region */
                    {
                        T3 = Vds * Inv_Vdsat;
                        T4 = T3 - 1.0;
                        T2 = 1.0 - pParam.B2u1D * T4 * T4;
                        U1 = U1s * T2;
                        Utot = Uvert + U1 * Vds;
                        Utot = Math.Max(Utot, 0.5);
                        Inv_Utot = 1.0 / Utot;
                        T5 = 2.0 * U1s * pParam.B2u1D * Inv_Vdsat * T4;
                        dU1_dVd = T5 * (T3 * dVdsat_dVd - 1.0);
                        dU1_dVg = T5 * T3 * dVdsat_dVg;
                        dU1_dVb = T5 * T3 * dVdsat_dVb + pParam.B2u1B * T2;
                        dUtot_dVd = dUvert_dVd + U1 + Vds * dU1_dVd;
                        dUtot_dVg = dUvert_dVg + Vds * dU1_dVg;
                        dUtot_dVb = dUvert_dVb + Vds * dU1_dVb;

                        tmp1 = (Vgeff - 0.5 * Aa * Vds);
                        tmp3 = tmp1 * Vds;
                        Betaeff = Beta * Inv_Utot;
                        Ids = Betaeff * tmp3;
                        T6 = Ids / Betaeff * Inv_Utot;

                        gds = T6 * (dBeta_dVd - Betaeff * dUtot_dVd) + Betaeff * (tmp1
                            + (dVgeff_dVd - 0.5 * Aa) * Vds);
                        gm = T6 * (dBeta_dVg - Betaeff * dUtot_dVg) + Betaeff * Vds * dVgeff_dVg;

                        gmb = T6 * (dBeta_dVb - Betaeff * dUtot_dVb) + Betaeff * Vds
                            * (dVgeff_dVb - 0.5 * Vds * dAa_dVb);
                    }
                    else  /* Saturation */
                    {
                        tmp1 = Vgeff * Inv_Aa * Inv_Kk;
                        tmp3 = 0.5 * Vgeff * tmp1;
                        Betaeff = Beta * Inv_Uvert;
                        Ids = Betaeff * tmp3;
                        T0 = Ids / Betaeff * Inv_Uvert;
                        T1 = Betaeff * Vgeff * Inv_Aa * Inv_Kk;
                        T2 = Ids * Inv_Kk * dKk_dVc;

                        if (pParam.B2ai0 != 0.0)
                        {
                            Ai = pParam.B2ai0 + pParam.B2aiB * Vbs;
                            Bi = pParam.B2bi0 + pParam.B2biB * Vbs;
                            T5 = Bi / (Vds - Vdsat);
                            T5 = Math.Min(T5, 30.0);
                            T6 = Math.Exp(-T5);
                            FR = 1.0 + Ai * T6;
                            T7 = T5 / (Vds - Vdsat);
                            T8 = (1.0 - FR) * T7;
                            dFR_dVd = T8 * (dVdsat_dVd - 1.0);
                            dFR_dVg = T8 * dVdsat_dVg;
                            dFR_dVb = T8 * dVdsat_dVb + T6 * (pParam.B2aiB - Ai
                              * pParam.B2biB / (Vds - Vdsat));


                            gds = (T0 * (dBeta_dVd - Betaeff * dUvert_dVd) + T1
                             * dVgeff_dVd - T2 * dVc_dVd) * FR + Ids * dFR_dVd;

                            gm = (T0 * (dBeta_dVg - Betaeff * dUvert_dVg)
                            + T1 * dVgeff_dVg - T2 * dVc_dVg) * FR + Ids * dFR_dVg;

                            gmb = (T0 * (dBeta_dVb - Betaeff * dUvert_dVb) + T1
                             * dVgeff_dVb - T2 * dVc_dVb - Ids * Inv_Aa * dAa_dVb)
                             * FR + Ids * dFR_dVb;

                            Ids *= FR;
                        }
                        else
                        {
                            gds = T0 * (dBeta_dVd - Betaeff * dUvert_dVd) + T1
                             * dVgeff_dVd - T2 * dVc_dVd;

                            gm = T0 * (dBeta_dVg - Betaeff * dUvert_dVg) + T1 * dVgeff_dVg
                            - T2 * dVc_dVg;

                            gmb = T0 * (dBeta_dVb - Betaeff * dUvert_dVb) + T1
                             * dVgeff_dVb - T2 * dVc_dVb - Ids * Inv_Aa * dAa_dVb;
                        }
                    } /* end of Saturation */
                }
                else
                {
                    T0 = Exp0 * Exp0;
                    T1 = Exp1;
                    Ids = Beta * model.B2Vtm * model.B2Vtm * T0 * (1.0 - T1);
                    T2 = Ids / Beta;
                    T4 = n * model.B2Vtm;
                    T3 = Ids / T4;
                    if ((Vds > Vdsat) && pParam.B2ai0 != 0.0)
                    {
                        Ai = pParam.B2ai0 + pParam.B2aiB * Vbs;
                        Bi = pParam.B2bi0 + pParam.B2biB * Vbs;
                        T5 = Bi / (Vds - Vdsat);
                        T5 = Math.Min(T5, 30.0);
                        T6 = Math.Exp(-T5);
                        FR = 1.0 + Ai * T6;
                        T7 = T5 / (Vds - Vdsat);
                        T8 = (1.0 - FR) * T7;
                        dFR_dVd = T8 * (dVdsat_dVd - 1.0);
                        dFR_dVg = T8 * dVdsat_dVg;
                        dFR_dVb = T8 * dVdsat_dVb + T6 * (pParam.B2aiB - Ai
                          * pParam.B2biB / (Vds - Vdsat));
                    }
                    else
                    {
                        FR = 1.0;
                        dFR_dVd = 0.0;
                        dFR_dVg = 0.0;
                        dFR_dVb = 0.0;
                    }

                    gds = (T2 * dBeta_dVd + T3 * (pParam.B2vofD * T4 - dVth_dVd
                     - pParam.B2nD * Vgst / n) + Beta * model.B2Vtm
                 * T0 * T1) * FR + Ids * dFR_dVd;
                    gm = (T2 * dBeta_dVg + T3) * FR + Ids * dFR_dVg;
                    gmb = (T2 * dBeta_dVb + T3 * (pParam.B2vofB * T4 - dVth_dVb
                     + pParam.B2nB * Vgst / (n * T1s * T1s) * dT1s_dVb)) * FR
                     + Ids * dFR_dVb;
                    Ids *= FR;
                }
            }
            else
            {
                Ids = 0.0;
                gm = 0.0;
                gds = 0.0;
                gmb = 0.0;
            }

            /* Some Limiting of DC Parameters */
            gds = Math.Max(gds, 1.0e-20);


            if ((model.B2channelChargePartitionFlag > 1)
             || ((!ChargeComputationNeeded) &&
             (model.B2channelChargePartitionFlag > -5)))
            {
                qg = 0.0;
                qd = 0.0;
                qb = 0.0;
                cgg = 0.0;
                cgs = 0.0;
                cgd = 0.0;
                cdg = 0.0;
                cds = 0.0;
                cdd = 0.0;
                cbg = 0.0;
                cbs = 0.0;
                cbd = 0.0;
                goto finished;
            }
            else
            {
                if (Vbs < 0.0)
                {
                    Vbseff = Vbs;
                    dVbseff_dVb = 1.0;
                }
                else
                {
                    Vbseff = pParam.B2phi - Phisb;
                    dVbseff_dVb = -dPhisb_dVb;
                }
                Arg1 = Vgs - Vbseff - pParam.B2vfb;
                Arg2 = Arg1 - Vgst;
                Qbulk = pParam.One_Third_CoxWL * Arg2;
                dQbulk_dVb = pParam.One_Third_CoxWL * (dVth_dVb - dVbseff_dVb);
                dQbulk_dVd = pParam.One_Third_CoxWL * dVth_dVd;
                if (Arg1 <= 0.0)
                {
                    qg = pParam.CoxWL * Arg1;
                    qb = -(qg);
                    qd = 0.0;


                    cgg = pParam.CoxWL;

                    cgd = 0.0;

                    cgs = -cgg * (1.0 - dVbseff_dVb);


                    cdg = 0.0;

                    cdd = 0.0;

                    cds = 0.0;


                    cbg = -pParam.CoxWL;

                    cbd = 0.0;

                    cbs = -cgs;
                }
                else if (Vgst <= 0.0)
                {
                    T2 = Arg1 / Arg2;
                    T3 = T2 * T2 * (pParam.CoxWL - pParam.Two_Third_CoxWL
                  * T2);


                    qg = pParam.CoxWL * Arg1 * (1.0 - T2 * (1.0 - T2 / 3.0));
                    qb = -(qg);
                    qd = 0.0;


                    cgg = pParam.CoxWL * (1.0 - T2 * (2.0 - T2));
                    tmp = T3 * dVth_dVb - (cgg + T3) * dVbseff_dVb;

                    cgd = T3 * dVth_dVd;

                    cgs = -(cgg + cgd + tmp);


                    cdg = 0.0;

                    cdd = 0.0;

                    cds = 0.0;


                    cbg = -cgg;

                    cbd = -cgd;

                    cbs = -cgs;
                }
                else
                {
                    if (Vgst < pParam.B2vghigh)
                    {
                        Uvert = 1.0 + Vgst * (Ua + Vgst * Ub);
                        Uvert = Math.Max(Uvert, 0.2);
                        Inv_Uvert = 1.0 / Uvert;
                        dUvert_dVg = Ua + 2.0 * Ub * Vgst;
                        dUvert_dVd = -dUvert_dVg * dVth_dVd;
                        dUvert_dVb = -dUvert_dVg * dVth_dVb + Vgst
                        * (pParam.B2uaB + Vgst * pParam.B2ubB);

                        T8 = U1s * Inv_Aa * Inv_Uvert;
                        Vc = T8 * Vgst;
                        T9 = Vc * Inv_Uvert;
                        dVc_dVg = T8 - T9 * dUvert_dVg;
                        dVc_dVd = -T8 * dVth_dVd - T9 * dUvert_dVd;
                        dVc_dVb = -T8 * dVth_dVb + pParam.B2u1B * Vgst * Inv_Aa

                                           * Inv_Uvert - Vc * Inv_Aa * dAa_dVb - T9 * dUvert_dVb;

                        tmp2 = Math.Sqrt(1.0 + 2.0 * Vc);
                        Kk = 0.5 * (1.0 + Vc + tmp2);
                        Inv_Kk = 1.0 / Kk;
                        dKk_dVc = 0.5 + 0.5 / tmp2;
                        SqrtKk = Math.Sqrt(Kk);

                        T8 = Inv_Aa / SqrtKk;
                        Vdsat = Vgst * T8;
                        T9 = 0.5 * Vdsat * Inv_Kk * dKk_dVc;
                        dVdsat_dVd = -T8 * dVth_dVd - T9 * dVc_dVd;
                        dVdsat_dVg = T8 - T9 * dVc_dVg;
                        dVdsat_dVb = -T8 * dVth_dVb - T9 * dVc_dVb
                                      - Vdsat * Inv_Aa * dAa_dVb;
                    }
                    if (Vds >= Vdsat)
                    {       /* saturation region */

                        cgg = pParam.Two_Third_CoxWL;

                        cgd = -cgg * dVth_dVd + dQbulk_dVd;
                        tmp = -cgg * dVth_dVb + dQbulk_dVb;

                        cgs = -(cgg + cgd + tmp);


                        cbg = 0.0;

                        cbd = -dQbulk_dVd;

                        cbs = dQbulk_dVd + dQbulk_dVb;


                        cdg = -0.4 * cgg;
                        tmp = -cdg * dVth_dVb;

                        cdd = -cdg * dVth_dVd;

                        cds = -(cdg + cdd + tmp);


                        qb = -Qbulk;

                        qg = pParam.Two_Third_CoxWL * Vgst + Qbulk;

                        qd = cdg * Vgst;
                    }
                    else
                    {       /* linear region  */
                        T7 = Vds / Vdsat;
                        T8 = Vgst / Vdsat;
                        T6 = T7 * T8;
                        T9 = 1.0 - T7;
                        Vgdt = Vgst * T9;
                        T0 = Vgst / (Vgst + Vgdt);
                        T1 = Vgdt / (Vgst + Vgdt);
                        T5 = T0 * T1;
                        T2 = 1.0 - T1 + T5;
                        T3 = 1.0 - T0 + T5;

                        dVgdt_dVg = T9 + T6 * dVdsat_dVg;
                        dVgdt_dVd = T6 * dVdsat_dVd - T8 - T9 * dVth_dVd;
                        dVgdt_dVb = T6 * dVdsat_dVb - T9 * dVth_dVb;


                        qg = pParam.Two_Third_CoxWL * (Vgst + Vgdt
                          - Vgdt * T0) + Qbulk;

                        qb = -Qbulk;

                        qd = -pParam.One_Third_CoxWL * (0.2 * Vgdt
                      + 0.8 * Vgst + Vgdt * T1
                      + 0.2 * T5 * (Vgdt - Vgst));


                        cgg = pParam.Two_Third_CoxWL * (T2 + T3 * dVgdt_dVg);
                        tmp = dQbulk_dVb + pParam.Two_Third_CoxWL * (T3 * dVgdt_dVb
                                      - T2 * dVth_dVb);

                        cgd = pParam.Two_Third_CoxWL * (T3 * dVgdt_dVd
                         - T2 * dVth_dVd) + dQbulk_dVd;

                        cgs = -(cgg + cgd + tmp);

                        T2 = 0.8 - 0.4 * T1 * (2.0 * T1 + T0 + T0 * (T1 - T0));
                        T3 = 0.2 + T1 + T0 * (1.0 - 0.4 * T0 * (T1 + 3.0 * T0));

                        cdg = -pParam.One_Third_CoxWL * (T2 + T3 * dVgdt_dVg);
                        tmp = pParam.One_Third_CoxWL * (T2 * dVth_dVb
                                  - T3 * dVgdt_dVb);

                        cdd = pParam.One_Third_CoxWL * (T2 * dVth_dVd
                         - T3 * dVgdt_dVd);

                        cds = -(cdg + tmp + cdd);


                        cbg = 0.0;

                        cbd = -dQbulk_dVd;

                        cbs = dQbulk_dVd + dQbulk_dVb;
                    }
                }
            }

            finished:       /* returning Values to Calling Routine */
            valuetypeflag = (int)model.B2channelChargePartitionFlag;
            switch (valuetypeflag)
            {
                case 0:
                    Ids = Math.Max(Ids, 1e-50);
                    break;
                case -1:
                    Ids = Math.Max(Ids, 1e-50);
                    break;
                case -2:
                    Ids = gm;
                    break;
                case -3:
                    Ids = gds;
                    break;
                case -4:
                    Ids = 1.0 / gds;
                    break;
                case -5:
                    Ids = gmb;
                    break;
                case -6:
                    Ids = qg / 1.0e-12;
                    break;
                case -7:
                    Ids = qb / 1.0e-12;
                    break;
                case -8:
                    Ids = qd / 1.0e-12;
                    break;
                case -9:
                    Ids = -(qb + qg + qd) / 1.0e-12;
                    break;
                case -10:
                    Ids = cgg / 1.0e-12;
                    break;
                case -11:
                    Ids = cgd / 1.0e-12;
                    break;
                case -12:
                    Ids = cgs / 1.0e-12;
                    break;
                case -13:
                    Ids = -(cgg + cgd + cgs) / 1.0e-12;
                    break;
                case -14:
                    Ids = cbg / 1.0e-12;
                    break;
                case -15:
                    Ids = cbd / 1.0e-12;
                    break;
                case -16:
                    Ids = cbs / 1.0e-12;
                    break;
                case -17:
                    Ids = -(cbg + cbd + cbs) / 1.0e-12;
                    break;
                case -18:
                    Ids = cdg / 1.0e-12;
                    break;
                case -19:
                    Ids = cdd / 1.0e-12;
                    break;
                case -20:
                    Ids = cds / 1.0e-12;
                    break;
                case -21:
                    Ids = -(cdg + cdd + cds) / 1.0e-12;
                    break;
                case -22:
                    Ids = -(cgg + cdg + cbg) / 1.0e-12;
                    break;
                case -23:
                    Ids = -(cgd + cdd + cbd) / 1.0e-12;
                    break;
                case -24:
                    Ids = -(cgs + cds + cbs) / 1.0e-12;
                    break;
                default:
                    Ids = Math.Max(Ids, 1.0e-50);
                    break;
            }
            von = Vth;
            vdsat = Vdsat;
        }

        /* routine to calculate equivalent conductance and total terminal 
         * charges
         */
        private void B2mosCap(Circuit ckt, double vgd, double vgs, double vgb, double[] args, double cbgb, double cbdb, double cbsb, double cdgb, double cddb, double cdsb,
            out double gcggbPointer, out double gcgdbPointer, out double gcgsbPointer, out double gcbgbPointer, out double gcbdbPointer,
            out double gcbsbPointer, out double gcdgbPointer, out double gcddbPointer, out double gcdsbPointer,
            out double gcsgbPointer, out double gcsdbPointer, out double gcssbPointer, ref double qGatePointer, ref double qBulkPointer,
            ref double qDrainPointer, out double qSourcePointer)
        {
            double qgd;
            double qgs;
            double qgb;
            double ag0;

            ag0 = ckt.Method.Slope;
            /* compute equivalent conductance */
            gcdgbPointer = (cdgb - args[0]) * ag0;
            gcddbPointer = (cddb + args[3] + args[0]) * ag0;
            gcdsbPointer = cdsb * ag0;
            gcsgbPointer = -(args[5] + cbgb + cdgb + args[1]) * ag0;
            gcsdbPointer = -(args[6] + cbdb + cddb) * ag0;
            gcssbPointer = (args[4] + args[1] -
                (args[7] + cbsb + cdsb)) * ag0;
            gcggbPointer = (args[5] + args[0] +
                args[1] + args[2]) * ag0;
            gcgdbPointer = (args[6] - args[0]) * ag0;
            gcgsbPointer = (args[7] - args[1]) * ag0;
            gcbgbPointer = (cbgb - args[2]) * ag0;
            gcbdbPointer = (cbdb - args[3]) * ag0;
            gcbsbPointer = (cbsb - args[4]) * ag0;

            /* compute total terminal charge */
            qgd = args[0] * vgd;
            qgs = args[1] * vgs;
            qgb = args[2] * vgb;
            qGatePointer = qGatePointer + qgd + qgs + qgb;
            qBulkPointer = qBulkPointer - qgb;
            qDrainPointer = qDrainPointer - qgd;
            qSourcePointer = -(qGatePointer + qBulkPointer + qDrainPointer);

        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(B2states + B2qb, ckt, ref timeStep);
            method.Terr(B2states + B2qg, ckt, ref timeStep);
            method.Terr(B2states + B2qd, ckt, ref timeStep);
        }
    }
}
