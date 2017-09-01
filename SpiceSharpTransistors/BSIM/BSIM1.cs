using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    [SpiceNodes("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM1 : CircuitComponent<BSIM1>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM1Model model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter B1w { get; } = new Parameter(5e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter B1l { get; } = new Parameter(5e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter B1sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter B1drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter B1sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter B1drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter B1sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter B1drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool B1off { get; set; }
        [SpiceName("vbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter B1icVBS { get; } = new Parameter();
        [SpiceName("vds"), SpiceInfo("Initial D-S voltage")]
        public Parameter B1icVDS { get; } = new Parameter();
        [SpiceName("vgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter B1icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: B1icVBS.Set(value[2]); goto case 2;
                case 2: B1icVGS.Set(value[1]); goto case 1;
                case 1: B1icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double B1vdsat { get; private set; }
        public double B1von { get; private set; }
        public double B1GDoverlapCap { get; private set; }
        public double B1GSoverlapCap { get; private set; }
        public double B1GBoverlapCap { get; private set; }
        public double B1drainConductance { get; private set; }
        public double B1sourceConductance { get; private set; }
        public double B1vfb { get; private set; }
        public double B1phi { get; private set; }
        public double B1K1 { get; private set; }
        public double B1K2 { get; private set; }
        public double B1eta { get; private set; }
        public double B1etaB { get; private set; }
        public double B1etaD { get; private set; }
        public double B1betaZero { get; private set; }
        public double B1betaZeroB { get; private set; }
        public double B1ugs { get; private set; }
        public double B1ugsB { get; private set; }
        public double B1uds { get; private set; }
        public double B1udsB { get; private set; }
        public double B1udsD { get; private set; }
        public double B1betaVdd { get; private set; }
        public double B1betaVddB { get; private set; }
        public double B1betaVddD { get; private set; }
        public double B1subthSlope { get; private set; }
        public double B1subthSlopeB { get; private set; }
        public double B1subthSlopeD { get; private set; }
        public double B1vt0 { get; private set; }
        public double B1mode { get; private set; }
        public int B1dNode { get; private set; }
        public int B1gNode { get; private set; }
        public int B1sNode { get; private set; }
        public int B1bNode { get; private set; }
        public int B1dNodePrime { get; private set; }
        public int B1sNodePrime { get; private set; }
        public int B1states { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int B1vbd = 0;
        private const int B1vbs = 1;
        private const int B1vgs = 2;
        private const int B1vds = 3;
        private const int B1cd = 4;
        private const int B1id = 4;
        private const int B1cbs = 5;
        private const int B1ibs = 5;
        private const int B1cbd = 6;
        private const int B1ibd = 6;
        private const int B1gm = 7;
        private const int B1gds = 8;
        private const int B1gmbs = 9;
        private const int B1gbd = 10;
        private const int B1gbs = 11;
        private const int B1qb = 12;
        private const int B1cqb = 13;
        private const int B1iqb = 13;
        private const int B1qg = 14;
        private const int B1cqg = 15;
        private const int B1iqg = 15;
        private const int B1qd = 16;
        private const int B1cqd = 17;
        private const int B1iqd = 17;
        private const int B1cggb = 18;
        private const int B1cgdb = 19;
        private const int B1cgsb = 20;
        private const int B1cbgb = 21;
        private const int B1cbdb = 22;
        private const int B1cbsb = 23;
        private const int B1capbd = 24;
        private const int B1iqbd = 25;
        private const int B1cqbd = 25;
        private const int B1capbs = 26;
        private const int B1iqbs = 27;
        private const int B1cqbs = 27;
        private const int B1cdgb = 28;
        private const int B1cddb = 29;
        private const int B1cdsb = 30;
        private const int B1vono = 31;
        private const int B1vdsato = 32;
        private const int B1qbs = 33;
        private const int B1qbd = 34;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM1(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM1Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            B1dNode = nodes[0].Index;
            B1gNode = nodes[1].Index;
            B1sNode = nodes[2].Index;
            B1bNode = nodes[3].Index;

            // Allocate states
            B1states = ckt.State.GetState(35);

            /* allocate a chunk of the state vector */

            /* perform the parameter defaulting */

            B1vdsat = 0;
            B1von = 0;

            /* process drain series resistance */
            if ((model.B1sheetResistance.Value != 0) && (B1drainSquares.Value != 0.0) && (B1dNodePrime == 0))
            {
                B1dNodePrime = CreateNode(ckt).Index;
            }
            else
            {
                B1dNodePrime = B1dNode;
            }

            /* process source series resistance */
            if ((model.B1sheetResistance.Value != 0) && (B1sourceSquares.Value != 0.0) && (B1sNodePrime == 0))
            {
                if (B1sNodePrime == 0)
                {
                    B1sNodePrime = CreateNode(ckt).Index;
                }
            }
            else
            {
                B1sNodePrime = B1sNode;
            }

            /* set Sparse Matrix Pointers */

            /* macro to make elements with built in test for out of memory */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as BSIM1Model;
            double EffChanLength;
            double EffChanWidth;
            double Leff;
            double Weff;
            double CoxWoverL;

            if ((EffChanLength = B1l.Value - model.B1deltaL.Value * 1e-6) <= 0)
                throw new CircuitException($"B1: mosfet {Name}, model {model.Name}: Effective channel length <= 0");
            if ((EffChanWidth = B1w.Value - model.B1deltaW.Value * 1e-6) <= 0)
                throw new CircuitException($"B1: mosfet {Name}, model {model.Name}: Effective channel width <= 0");
            B1GDoverlapCap = EffChanWidth * model.B1gateDrainOverlapCap.Value;
            B1GSoverlapCap = EffChanWidth * model.B1gateSourceOverlapCap.Value;
            B1GBoverlapCap = B1l.Value * model.B1gateBulkOverlapCap.Value;

            /* process drain series resistance */
            if ((B1drainConductance = model.B1sheetResistance.Value * B1drainSquares.Value) != 0.0)
            {
                B1drainConductance = 1.0 / B1drainConductance;
            }

            /* process source series resistance */
            if ((B1sourceConductance = model.B1sheetResistance.Value * B1sourceSquares.Value) != 0.0)
            {
                B1sourceConductance = 1.0 / B1sourceConductance;
            }

            Leff = EffChanLength * 1.0e6; /* convert into micron */
            Weff = EffChanWidth * 1.0e6; /* convert into micron */
            CoxWoverL = model.Cox * Weff / Leff; /* F / cm *  * 2 */

            B1vfb = model.B1vfb0.Value + model.B1vfbL.Value / Leff + model.B1vfbW.Value / Weff;
            B1phi = model.B1phi0.Value + model.B1phiL.Value / Leff + model.B1phiW.Value / Weff;
            B1K1 = model.B1K10.Value + model.B1K1L.Value / Leff + model.B1K1W.Value / Weff;
            B1K2 = model.B1K20.Value + model.B1K2L.Value / Leff + model.B1K2W.Value / Weff;
            B1eta = model.B1eta0.Value + model.B1etaL.Value / Leff + model.B1etaW.Value / Weff;
            B1etaB = model.B1etaB0.Value + model.B1etaBl.Value / Leff + model.B1etaBw.Value / Weff;
            B1etaD = model.B1etaD0.Value + model.B1etaDl.Value / Leff + model.B1etaDw.Value / Weff;
            B1betaZero = model.B1mobZero.Value;
            B1betaZeroB = model.B1mobZeroB0.Value + model.B1mobZeroBl.Value / Leff + model.B1mobZeroBw.Value / Weff;
            B1ugs = model.B1ugs0.Value + model.B1ugsL.Value / Leff + model.B1ugsW.Value / Weff;
            B1ugsB = model.B1ugsB0.Value + model.B1ugsBL.Value / Leff + model.B1ugsBW.Value / Weff;
            B1uds = model.B1uds0.Value + model.B1udsL.Value / Leff + model.B1udsW.Value / Weff;
            B1udsB = model.B1udsB0.Value + model.B1udsBL.Value / Leff + model.B1udsBW.Value / Weff;
            B1udsD = model.B1udsD0.Value + model.B1udsDL.Value / Leff + model.B1udsDW.Value / Weff;
            B1betaVdd = model.B1mobVdd0.Value + model.B1mobVddl.Value / Leff + model.B1mobVddw.Value / Weff;
            B1betaVddB = model.B1mobVddB0.Value + model.B1mobVddBl.Value / Leff + model.B1mobVddBw.Value / Weff;
            B1betaVddD = model.B1mobVddD0.Value + model.B1mobVddDl.Value / Leff + model.B1mobVddDw.Value / Weff;
            B1subthSlope = model.B1subthSlope0.Value + model.B1subthSlopeL.Value / Leff + model.B1subthSlopeW.Value / Weff;
            B1subthSlopeB = model.B1subthSlopeB0.Value + model.B1subthSlopeBL.Value / Leff + model.B1subthSlopeBW.Value / Weff;
            B1subthSlopeD = model.B1subthSlopeD0.Value + model.B1subthSlopeDL.Value / Leff + model.B1subthSlopeDW.Value / Weff;

            if (B1phi < 0.1) B1phi = 0.1;
            if (B1K1 < 0.0) B1K1 = 0.0;
            if (B1K2 < 0.0) B1K2 = 0.0;

            B1vt0 = B1vfb + B1phi + B1K1 * Math.Sqrt(B1phi) - B1K2 * B1phi;

            B1von = B1vt0; /* added for initialization */

            /* process Beta Parameters (unit: A / V *  * 2) */

            B1betaZero = B1betaZero * CoxWoverL;
            B1betaZeroB = B1betaZeroB * CoxWoverL;
            B1betaVdd = B1betaVdd * CoxWoverL;
            B1betaVddB = B1betaVddB * CoxWoverL;
            B1betaVddD = Math.Max(B1betaVddD * CoxWoverL, 0.0);
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as BSIM1Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double EffectiveLength, DrainArea, SourceArea, DrainPerimeter, SourcePerimeter, DrainSatCurrent, SourceSatCurrent,
                GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, von, vdsat, vt0;
            int Check;  
            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, vcrit, vgb, gbs, cbs,
                evbs, gbd, cbd, evbd, cd, czbd, czbs, czbdsw, czbssw, PhiB, PhiBSW, MJ, MJSW, arg, argsw, sarg, sargsw, capbs = 0.0,
                capbd = 0.0, ceqqg, gcdgb, gcsgb, gcggb, gcbgb, cqgate, cqbulk, cqdrn, ceqqb, ceqqd, ceqbs, ceqbd, xnrm, xrev, cdreq;
            double gm, gds, gmbs, qgate, qbulk, qdrn = 0.0, cggb = 0.0, cgdb = 0.0, cgsb = 0.0, cbgb = 0.0, cbdb = 0.0, cbsb = 0.0, cdgb = 0.0,
                cddb = 0.0, cdsb = 0.0, cdrain, qsrc = 0.0, csgb = 0.0, cssb = 0.0, csdb = 0.0, gcgdb, gcgsb, gcbdb, gcbsb, gcddb, gcdsb, gcsdb, gcssb;

            EffectiveLength = B1l - model.B1deltaL * 1.0e-6; /* m */
            DrainArea = B1drainArea;
            SourceArea = B1sourceArea;
            DrainPerimeter = B1drainPerimeter;
            SourcePerimeter = B1sourcePerimeter;
            if ((DrainSatCurrent = DrainArea * model.B1jctSatCurDensity) < 1e-15)
            {
                DrainSatCurrent = 1.0e-15;
            }
            if ((SourceSatCurrent = SourceArea * model.B1jctSatCurDensity) < 1.0e-15)
            {
                SourceSatCurrent = 1.0e-15;
            }
            GateSourceOverlapCap = model.B1gateSourceOverlapCap * B1w;
            GateDrainOverlapCap = model.B1gateDrainOverlapCap * B1w;
            GateBulkOverlapCap = model.B1gateBulkOverlapCap * EffectiveLength;
            von = model.B1type * B1von;
            vdsat = model.B1type * B1vdsat;
            vt0 = model.B1type * B1vt0;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][B1states + B1vbs];
                vgs = state.States[0][B1states + B1vgs];
                vds = state.States[0][B1states + B1vds];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vbs = state.States[1][B1states + B1vbs];
                vgs = state.States[1][B1states + B1vgs];
                vds = state.States[1][B1states + B1vds];
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !B1off)
            {
                vds = model.B1type * B1icVDS;
                vgs = model.B1type * B1icVGS;
                vbs = model.B1type * B1icVBS;
                if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                    (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                {
                    vbs = -1;
                    vgs = vt0;
                    vds = 0;
                }
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitFix) && (B1off))
            {
                vbs = vgs = vds = 0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.B1type * (rstate.OldSolution[B1bNode] - rstate.OldSolution[B1sNodePrime]);
                vgs = model.B1type * (rstate.OldSolution[B1gNode] - rstate.OldSolution[B1sNodePrime]);
                vds = model.B1type * (rstate.OldSolution[B1dNodePrime] - rstate.OldSolution[B1sNodePrime]);
                /* PREDICTOR */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][B1states + B1vgs] - state.States[0][B1states + B1vds];
                delvbs = vbs - state.States[0][B1states + B1vbs];
                delvbd = vbd - state.States[0][B1states + B1vbd];
                delvgs = vgs - state.States[0][B1states + B1vgs];
                delvds = vds - state.States[0][B1states + B1vds];
                delvgd = vgd - vgdo;

                if (B1mode >= 0)
                {
                    cdhat = state.States[0][B1states + B1cd] - state.States[0][B1states + B1gbd] * delvbd + state.States[0][B1states + B1gmbs] *
                         delvbs + state.States[0][B1states + B1gm] * delvgs + state.States[0][B1states + B1gds] * delvds;
                }
                else
                {
                    cdhat = state.States[0][B1states + B1cd] - (state.States[0][B1states + B1gbd] - state.States[0][B1states + B1gmbs]) * delvbd -
                         state.States[0][B1states + B1gm] * delvgd + state.States[0][B1states + B1gds] * delvds;
                }
                cbhat = state.States[0][B1states + B1cbs] + state.States[0][B1states + B1cbd] + state.States[0][B1states + B1gbd] * delvbd +
                     state.States[0][B1states + B1gbs] * delvbs;

                /* NOBYPASS */

                von = model.B1type * B1von;
                if (state.States[0][B1states + B1vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][B1states + B1vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][B1states + B1vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][B1states + B1vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * SourceSatCurrent));
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][B1states + B1vbs], Circuit.CONSTvt0, vcrit, ref Check); /* B1 test */
                    vbd = vbs - vds;
                }
                else
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * DrainSatCurrent));
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][B1states + B1vbd], Circuit.CONSTvt0, vcrit, ref Check); /* B1 test */
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
                B1mode = 1;
            }
            else
            {
                /* inverse mode */
                B1mode = -1;
            }
            /* call B1evaluate to calculate drain current and its 
			* derivatives and charge and capacitances related to gate
			* drain, and bulk
			*/
            if (vds >= 0)
            {
                B1evaluate(vds, vbs, vgs, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qdrn, out cggb, out cgdb, out cgsb, out cbgb, out cbdb, out cbsb, out cdgb,
                out cddb, out cdsb, out cdrain, out von, out vdsat, ckt);
            }
            else
            {
                B1evaluate(-vds, vbd, vgd, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qsrc, out cggb, out cgsb, out cgdb, out cbgb, out cbsb, out cbdb, out csgb,
                out cssb, out csdb, out cdrain, out von, out vdsat, ckt);
            }

            B1von = model.B1type * von;
            B1vdsat = model.B1type * vdsat;

            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            cd = B1mode * cdrain - cbd;
            if ((method != null || state.UseSmallSignal) || ((state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC))
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

                czbd = model.B1unitAreaJctCap * DrainArea;
                czbs = model.B1unitAreaJctCap * SourceArea;
                czbdsw = model.B1unitLengthSidewallJctCap * DrainPerimeter;
                czbssw = model.B1unitLengthSidewallJctCap * SourcePerimeter;
                PhiB = model.B1bulkJctPotential;
                PhiBSW = model.B1sidewallJctPotential;
                MJ = model.B1bulkJctBotGradingCoeff;
                MJSW = model.B1bulkJctSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs < 0)
                {
                    arg = 1 - vbs / PhiB;
                    argsw = 1 - vbs / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][B1states + B1qbs] = PhiB * czbs * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbssw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbs = czbs * sarg + czbssw * sargsw;
                }
                else
                {
                    state.States[0][B1states + B1qbs] = vbs * (czbs + czbssw) + vbs * vbs * (czbs * MJ * 0.5 / PhiB + czbssw * MJSW * 0.5 / PhiBSW);
                    capbs = czbs + czbssw + vbs * (czbs * MJ / PhiB + czbssw * MJSW / PhiBSW);
                }

                /* Drain Bulk Junction */
                if (vbd < 0)
                {
                    arg = 1 - vbd / PhiB;
                    argsw = 1 - vbd / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][B1states + B1qbd] = PhiB * czbd * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbdsw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbd = czbd * sarg + czbdsw * sargsw;
                }
                else
                {
                    state.States[0][B1states + B1qbd] = vbd * (czbd + czbdsw) + vbd * vbd * (czbd * MJ * 0.5 / PhiB + czbdsw * MJSW * 0.5 / PhiBSW);
                    capbd = czbd + czbdsw + vbd * (czbd * MJ / PhiB + czbdsw * MJSW / PhiBSW);
                }

            }

            /* 
			* check convergence
			*/
            if (!B1off || state.Init != CircuitState.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][B1states + B1vbs] = vbs;
            state.States[0][B1states + B1vbd] = vbd;
            state.States[0][B1states + B1vgs] = vgs;
            state.States[0][B1states + B1vds] = vds;
            state.States[0][B1states + B1cd] = cd;
            state.States[0][B1states + B1cbs] = cbs;
            state.States[0][B1states + B1cbd] = cbd;
            state.States[0][B1states + B1gm] = gm;
            state.States[0][B1states + B1gds] = gds;
            state.States[0][B1states + B1gmbs] = gmbs;
            state.States[0][B1states + B1gbd] = gbd;
            state.States[0][B1states + B1gbs] = gbs;

            state.States[0][B1states + B1cggb] = cggb;
            state.States[0][B1states + B1cgdb] = cgdb;
            state.States[0][B1states + B1cgsb] = cgsb;

            state.States[0][B1states + B1cbgb] = cbgb;
            state.States[0][B1states + B1cbdb] = cbdb;
            state.States[0][B1states + B1cbsb] = cbsb;

            state.States[0][B1states + B1cdgb] = cdgb;
            state.States[0][B1states + B1cddb] = cddb;
            state.States[0][B1states + B1cdsb] = cdsb;

            state.States[0][B1states + B1capbs] = capbs;
            state.States[0][B1states + B1capbd] = capbd;

            /* bulk and channel charge plus overlaps */
            if (method == null && ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || (!state.UseIC)) && (!state.UseSmallSignal))
                goto line850;

            if (B1mode > 0)
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

                B1mosCap(ckt, vgd, vgs, vgb, args, cbgb, cbdb, cbsb, cdgb, cddb, cdsb,
                    out gcggb, out gcgdb, out gcgsb, out gcbgb, out gcbdb, out gcbsb,
                    out gcdgb, out gcddb, out gcdsb, out gcsgb, out gcsdb, out gcssb,
                    ref qgate, ref qbulk, ref qdrn, out qsrc);
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

                B1mosCap(ckt, vgs, vgd, vgb, args, cbgb, cbsb, cbdb, csgb, cssb, csdb,
                out gcggb, out gcgsb, out gcgdb,
                out gcbgb, out gcbsb, out gcbdb,
                out gcsgb, out gcssb, out gcsdb, out gcdgb, out gcdsb, out gcddb,
                ref qgate, ref qbulk, ref qsrc, out qdrn);
            }

            state.States[0][B1states + B1qg] = qgate;
            state.States[0][B1states + B1qd] = qdrn - state.States[0][B1states + B1qbd];
            state.States[0][B1states + B1qb] = qbulk + state.States[0][B1states + B1qbd] + state.States[0][B1states + B1qbs];

            /* store small signal parameters */
            if (method == null && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC)
                goto line850;
            if (state.UseSmallSignal)
            {
                state.States[0][B1states + B1cggb] = cggb;
                state.States[0][B1states + B1cgdb] = cgdb;
                state.States[0][B1states + B1cgsb] = cgsb;
                state.States[0][B1states + B1cbgb] = cbgb;
                state.States[0][B1states + B1cbdb] = cbdb;
                state.States[0][B1states + B1cbsb] = cbsb;
                state.States[0][B1states + B1cdgb] = cdgb;
                state.States[0][B1states + B1cddb] = cddb;
                state.States[0][B1states + B1cdsb] = cdsb;
                state.States[0][B1states + B1capbd] = capbd;
                state.States[0][B1states + B1capbs] = capbs;

                goto line1000;
            }

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][B1states + B1qb] = state.States[0][B1states + B1qb];
                state.States[1][B1states + B1qg] = state.States[0][B1states + B1qg];
                state.States[1][B1states + B1qd] = state.States[0][B1states + B1qd];
            }

            if (method != null)
            {
                method.Integrate(state, B1states + B1qb, 0.0);
                method.Integrate(state, B1states + B1qg, 0.0);
                method.Integrate(state, B1states + B1qd, 0.0);
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
            cqgate = state.States[0][B1states + B1iqg];
            cqbulk = state.States[0][B1states + B1iqb];
            cqdrn = state.States[0][B1states + B1iqd];
            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][B1states + B1iqb] = state.States[0][B1states + B1iqb];
                state.States[1][B1states + B1iqg] = state.States[0][B1states + B1iqg];
                state.States[1][B1states + B1iqd] = state.States[0][B1states + B1iqd];
            }

            /* 
			* load current vector
			*/
            line900:

            ceqbs = model.B1type * (cbs - (gbs - state.Gmin) * vbs);
            ceqbd = model.B1type * (cbd - (gbd - state.Gmin) * vbd);

            ceqqg = model.B1type * ceqqg;
            ceqqb = model.B1type * ceqqb;
            ceqqd = model.B1type * ceqqd;
            if (B1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.B1type * (cdrain - gds * vds - gm * vgs - gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.B1type) * (cdrain + gds * vds - gm * vgd - gmbs * vbd);
            }

            rstate.Rhs[B1gNode] -= ceqqg;
            rstate.Rhs[B1bNode] -= (ceqbs + ceqbd + ceqqb);
            rstate.Rhs[B1dNodePrime] += (ceqbd - cdreq - ceqqd);
            rstate.Rhs[B1sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);

            /* 
			* load y matrix
			*/

            rstate.Matrix[B1dNode, B1dNode] += (B1drainConductance);
            rstate.Matrix[B1gNode, B1gNode] += (gcggb);
            rstate.Matrix[B1sNode, B1sNode] += (B1sourceConductance);
            rstate.Matrix[B1bNode, B1bNode] += (gbd + gbs - gcbgb - gcbdb - gcbsb);
            rstate.Matrix[B1dNodePrime, B1dNodePrime] += (B1drainConductance + gds + gbd + xrev * (gm + gmbs) + gcddb);
            rstate.Matrix[B1sNodePrime, B1sNodePrime] += (B1sourceConductance + gds + gbs + xnrm * (gm + gmbs) + gcssb);
            rstate.Matrix[B1dNode, B1dNodePrime] += (-B1drainConductance);
            rstate.Matrix[B1gNode, B1bNode] += (-gcggb - gcgdb - gcgsb);
            rstate.Matrix[B1gNode, B1dNodePrime] += (gcgdb);
            rstate.Matrix[B1gNode, B1sNodePrime] += (gcgsb);
            rstate.Matrix[B1sNode, B1sNodePrime] += (-B1sourceConductance);
            rstate.Matrix[B1bNode, B1gNode] += (gcbgb);
            rstate.Matrix[B1bNode, B1dNodePrime] += (-gbd + gcbdb);
            rstate.Matrix[B1bNode, B1sNodePrime] += (-gbs + gcbsb);
            rstate.Matrix[B1dNodePrime, B1dNode] += (-B1drainConductance);
            rstate.Matrix[B1dNodePrime, B1gNode] += ((xnrm - xrev) * gm + gcdgb);
            rstate.Matrix[B1dNodePrime, B1bNode] += (-gbd + (xnrm - xrev) * gmbs - gcdgb - gcddb - gcdsb);
            rstate.Matrix[B1dNodePrime, B1sNodePrime] += (-gds - xnrm * (gm + gmbs) + gcdsb);
            rstate.Matrix[B1sNodePrime, B1gNode] += (-(xnrm - xrev) * gm + gcsgb);
            rstate.Matrix[B1sNodePrime, B1sNode] += (-B1sourceConductance);
            rstate.Matrix[B1sNodePrime, B1bNode] += (-gbs - (xnrm - xrev) * gmbs - gcsgb - gcsdb - gcssb);
            rstate.Matrix[B1sNodePrime, B1dNodePrime] += (-gds - xrev * (gm + gmbs) + gcsdb);

            line1000:;
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm;
            int xrev;
            double gdpr, gspr, gm, gds, gmbs, gbd, gbs, capbd, capbs, cggb, cgsb, cgdb, cbgb, cbsb, cbdb, cdgb, cdsb, cddb;
            Complex xcdgb, xcddb, xcdsb, xcsgb, xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;

            if (B1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
            }
            else
            {
                xnrm = 0;
                xrev = 1;
            }
            gdpr = B1drainConductance;
            gspr = B1sourceConductance;
            gm = state.States[0][B1states + B1gm];
            gds = state.States[0][B1states + B1gds];
            gmbs = state.States[0][B1states + B1gmbs];
            gbd = state.States[0][B1states + B1gbd];
            gbs = state.States[0][B1states + B1gbs];
            capbd = state.States[0][B1states + B1capbd];
            capbs = state.States[0][B1states + B1capbs];
            /* 
			* charge oriented model parameters
			*/

            cggb = state.States[0][B1states + B1cggb];
            cgsb = state.States[0][B1states + B1cgsb];
            cgdb = state.States[0][B1states + B1cgdb];

            cbgb = state.States[0][B1states + B1cbgb];
            cbsb = state.States[0][B1states + B1cbsb];
            cbdb = state.States[0][B1states + B1cbdb];

            cdgb = state.States[0][B1states + B1cdgb];
            cdsb = state.States[0][B1states + B1cdsb];
            cddb = state.States[0][B1states + B1cddb];

            xcdgb = (cdgb - B1GDoverlapCap) * cstate.Laplace;
            xcddb = (cddb + capbd + B1GDoverlapCap) * cstate.Laplace;
            xcdsb = cdsb * cstate.Laplace;
            xcsgb = -(cggb + cbgb + cdgb + B1GSoverlapCap) * cstate.Laplace;
            xcsdb = -(cgdb + cbdb + cddb) * cstate.Laplace;
            xcssb = (capbs + B1GSoverlapCap - (cgsb + cbsb + cdsb)) * cstate.Laplace;
            xcggb = (cggb + B1GDoverlapCap + B1GSoverlapCap + B1GBoverlapCap) * cstate.Laplace;
            xcgdb = (cgdb - B1GDoverlapCap) * cstate.Laplace;
            xcgsb = (cgsb - B1GSoverlapCap) * cstate.Laplace;
            xcbgb = (cbgb - B1GBoverlapCap) * cstate.Laplace;
            xcbdb = (cbdb - capbd) * cstate.Laplace;
            xcbsb = (cbsb - capbs) * cstate.Laplace;

            cstate.Matrix[B1gNode, B1gNode] += xcggb;
            cstate.Matrix[B1bNode, B1bNode] += -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[B1dNodePrime, B1dNodePrime] += xcddb;
            cstate.Matrix[B1sNodePrime, B1sNodePrime] += xcssb;
            cstate.Matrix[B1gNode, B1bNode] += -xcggb - xcgdb - xcgsb;
            cstate.Matrix[B1gNode, B1dNodePrime] += xcgdb;
            cstate.Matrix[B1gNode, B1sNodePrime] += xcgsb;
            cstate.Matrix[B1bNode, B1gNode] += xcbgb;
            cstate.Matrix[B1bNode, B1dNodePrime] += xcbdb;
            cstate.Matrix[B1bNode, B1sNodePrime] += xcbsb;
            cstate.Matrix[B1dNodePrime, B1gNode] += xcdgb;
            cstate.Matrix[B1dNodePrime, B1bNode] += -xcdgb - xcddb - xcdsb;
            cstate.Matrix[B1dNodePrime, B1sNodePrime] += xcdsb;
            cstate.Matrix[B1sNodePrime, B1gNode] += xcsgb;
            cstate.Matrix[B1sNodePrime, B1bNode] += -xcsgb - xcsdb - xcssb;
            cstate.Matrix[B1sNodePrime, B1dNodePrime] += xcsdb;
            cstate.Matrix[B1dNode, B1dNode] += gdpr;
            cstate.Matrix[B1sNode, B1sNode] += gspr;
            cstate.Matrix[B1bNode, B1bNode] += gbd + gbs + -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[B1dNodePrime, B1dNodePrime] += gdpr + gds + gbd + xrev * (gm + gmbs) + xcddb;
            cstate.Matrix[B1sNodePrime, B1sNodePrime] += gspr + gds + gbs + xnrm * (gm + gmbs) + xcssb;
            cstate.Matrix[B1dNode, B1dNodePrime] -= gdpr;
            cstate.Matrix[B1sNode, B1sNodePrime] -= gspr;
            cstate.Matrix[B1bNode, B1dNodePrime] -= gbd - xcbdb;
            cstate.Matrix[B1bNode, B1sNodePrime] -= gbs - xcbsb;
            cstate.Matrix[B1dNodePrime, B1dNode] -= gdpr;
            cstate.Matrix[B1dNodePrime, B1gNode] += (xnrm - xrev) * gm + xcdgb;
            cstate.Matrix[B1dNodePrime, B1bNode] += -gbd + (xnrm - xrev) * gmbs + -xcdgb - xcddb - xcdsb;
            cstate.Matrix[B1dNodePrime, B1sNodePrime] += -gds - xnrm * (gm + gmbs) + xcdsb;
            cstate.Matrix[B1sNodePrime, B1gNode] += -(xnrm - xrev) * gm + xcsgb;
            cstate.Matrix[B1sNodePrime, B1sNode] -= gspr;
            cstate.Matrix[B1sNodePrime, B1bNode] += -gbs - (xnrm - xrev) * gmbs + -xcsgb - xcsdb - xcssb;
            cstate.Matrix[B1sNodePrime, B1dNodePrime] += -gds - xrev * (gm + gmbs) + xcsdb;
        }

        /* This routine evaluates the drain current, its derivatives and the
         * charges associated with the gate,bulk and drain terminal
         * using the B1 (Berkeley Short-Channel IGFET Model) Equations.
         */
        private void B1evaluate(double vds, double vbs, double vgs, out double gmPointer, out double gdsPointer, out double gmbsPointer,
                out double qgPointer, out double qbPointer, out double qdPointer, out double cggbPointer, out double cgdbPointer, out double cgsbPointer,
                out double cbgbPointer, out double cbdbPointer, out double cbsbPointer, out double cdgbPointer, out double cddbPointer,
                out double cdsbPointer, out double cdrainPointer, out double vonPointer, out double vdsatPointer, Circuit ckt)
        {
            var model = Model as BSIM1Model;
            double gm, gds, gmbs, qg = 0.0, qb = 0.0, qd = 0.0, cggb = 0, cgdb = 0, cgsb = 0, cbgb = 0, cbdb = 0, cbsb = 0, cdgb = 0, cddb = 0, cdsb = 0, Vfb, Phi, K1, K2, Vdd, Ugs, Uds, dUgsdVbs,
                Leff, dUdsdVbs, dUdsdVds, Eta, dEtadVds, dEtadVbs, Vpb, SqrtVpb, Von, Vth, dVthdVbs, dVthdVds, Vgs_Vth, DrainCurrent,
                G, A, Arg, dGdVbs, dAdVbs, Beta, Beta_Vds_0, BetaVdd, dBetaVdd_dVds, Beta0, dBeta0dVds, dBeta0dVbs, VddSquare, C1, C2,
                dBetaVdd_dVbs, dBeta_Vds_0_dVbs, dC1dVbs, dC2dVbs, dBetadVgs, dBetadVds, dBetadVbs, VdsSat = 0, Argl1, Argl2, Vc, Term1, K,
                Args1, dVcdVgs, dVcdVds, dVcdVbs, dKdVc, dKdVgs, dKdVds, dKdVbs, Args2, Args3, Warg1, Vcut, N, N0, NB, ND, Warg2, Wds,
                Wgs, Ilimit, Iexp, Temp1, Vth0, Arg1, Arg2, Arg3, Arg5, Ent, Vcom, Vgb, Vgb_Vfb, VdsPinchoff, EntSquare, Vgs_VthSquare,
                Argl3, Argl4, Argl5, Argl6, Argl7, Argl8, Argl9, dEntdVds, dEntdVbs, cgbb, cdbb, cbbb, WLCox, Vtsquare, Temp3;
            bool ChargeComputationNeeded;
            double co4v15;

            if (ckt.Method != null)
            {
                ChargeComputationNeeded = true;
            }
            else if (ckt.State.UseSmallSignal)
            {
                ChargeComputationNeeded = true;
            }
            else if (ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC && ckt.State.UseIC)
            {
                ChargeComputationNeeded = true;
            }
            else
            {
                ChargeComputationNeeded = false;
            }

            Vfb = B1vfb;
            Phi = B1phi;
            K1 = B1K1;
            K2 = B1K2;
            Vdd = model.B1vdd;
            if ((Ugs = B1ugs + B1ugsB * vbs) <= 0)
            {
                Ugs = 0;
                dUgsdVbs = 0.0;
            }
            else
            {
                dUgsdVbs = B1ugsB;
            }
            if ((Uds = B1uds + B1udsB * vbs +
                    B1udsD * (vds - Vdd)) <= 0)
            {
                Uds = 0.0;
                dUdsdVbs = dUdsdVds = 0.0;
            }
            else
            {
                Leff = B1l * 1.0e6 - model.B1deltaL; /* Leff in um */
                Uds = Uds / Leff;
                dUdsdVbs = B1udsB / Leff;
                dUdsdVds = B1udsD / Leff;
            }
            Eta = B1eta + B1etaB * vbs + B1etaD *
                (vds - Vdd);
            if (Eta <= 0)
            {
                Eta = 0;
                dEtadVds = dEtadVbs = 0.0;
            }
            else if (Eta > 1)
            {
                Eta = 1;
                dEtadVds = dEtadVbs = 0;
            }
            else
            {
                dEtadVds = B1etaD;
                dEtadVbs = B1etaB;
            }
            if (vbs < 0)
            {
                Vpb = Phi - vbs;
            }
            else
            {
                Vpb = Phi;
            }
            SqrtVpb = Math.Sqrt(Vpb);
            Von = Vfb + Phi + K1 * SqrtVpb - K2 * Vpb - Eta * vds;
            Vth = Von;
            dVthdVds = -Eta - dEtadVds * vds;
            dVthdVbs = K2 - 0.5 * K1 / SqrtVpb - dEtadVbs * vds;
            Vgs_Vth = vgs - Vth;

            G = 1.0 - 1.0 / (1.744 + 0.8364 * Vpb);
            A = 1.0 + 0.5 * G * K1 / SqrtVpb;
            A = Math.Max(A, 1.0);   /* Modified */
            Arg = Math.Max((1 + Ugs * Vgs_Vth), 1.0);
            dGdVbs = -0.8364 * (1 - G) * (1 - G);
            dAdVbs = 0.25 * K1 / SqrtVpb * (2 * dGdVbs + G / Vpb);

            if (Vgs_Vth < 0)
            {
                /* cutoff */
                DrainCurrent = 0;
                gm = 0;
                gds = 0;
                gmbs = 0;
                goto SubthresholdComputation;
            }

            /* Quadratic Interpolation for Beta0 (Beta at vgs  =  0, vds=Vds) */

            Beta_Vds_0 = (B1betaZero + B1betaZeroB * vbs);
            BetaVdd = (B1betaVdd + B1betaVddB * vbs);
            dBetaVdd_dVds = Math.Max(B1betaVddD, 0.0); /* Modified */
            if (vds > Vdd)
            {
                Beta0 = BetaVdd + dBetaVdd_dVds * (vds - Vdd);
                dBeta0dVds = dBetaVdd_dVds;
                dBeta0dVbs = B1betaVddB;
            }
            else
            {
                VddSquare = Vdd * Vdd;
                C1 = (-BetaVdd + Beta_Vds_0 + dBetaVdd_dVds * Vdd) / VddSquare;
                C2 = 2 * (BetaVdd - Beta_Vds_0) / Vdd - dBetaVdd_dVds;
                dBeta_Vds_0_dVbs = B1betaZeroB;
                dBetaVdd_dVbs = B1betaVddB;
                dC1dVbs = (dBeta_Vds_0_dVbs - dBetaVdd_dVbs) / VddSquare;
                dC2dVbs = dC1dVbs * (-2) * Vdd;
                Beta0 = (C1 * vds + C2) * vds + Beta_Vds_0;
                dBeta0dVds = 2 * C1 * vds + C2;
                dBeta0dVbs = dC1dVbs * vds * vds + dC2dVbs * vds + dBeta_Vds_0_dVbs;
            }

            /*Beta  =  Beta0 / ( 1 + Ugs * Vgs_Vth );*/

            Beta = Beta0 / Arg;
            dBetadVgs = -Beta * Ugs / Arg;
            dBetadVds = dBeta0dVds / Arg - dBetadVgs * dVthdVds;
            dBetadVbs = dBeta0dVbs / Arg + Beta * Ugs * dVthdVbs / Arg -
                    Beta * Vgs_Vth * dUgsdVbs / Arg;

            /*VdsSat  = Math.Max( Vgs_Vth / ( A + Uds * Vgs_Vth ),  0.0);*/

            if ((Vc = Uds * Vgs_Vth / A) < 0.0) Vc = 0.0;
            Term1 = Math.Sqrt(1 + 2 * Vc);
            K = 0.5 * (1 + Vc + Term1);
            VdsSat = Math.Max(Vgs_Vth / (A * Math.Sqrt(K)), 0.0);

            if (vds < VdsSat)
            {
                /* Triode Region */
                /*Argl1  =  1 + Uds * vds;*/
                Argl1 = Math.Max((1 + Uds * vds), 1.0);
                Argl2 = Vgs_Vth - 0.5 * A * vds;
                DrainCurrent = Beta * Argl2 * vds / Argl1;
                gm = (dBetadVgs * Argl2 * vds + Beta * vds) / Argl1;
                gds = (dBetadVds * Argl2 * vds + Beta *
                    (Vgs_Vth - vds * dVthdVds - A * vds) -
                    DrainCurrent * (vds * dUdsdVds + Uds)) / Argl1;
                gmbs = (dBetadVbs * Argl2 * vds + Beta * vds *
                    (-dVthdVbs - 0.5 * vds * dAdVbs) -
                    DrainCurrent * vds * dUdsdVbs) / Argl1;
            }
            else
            {
                /* Pinchoff (Saturation) Region */
                Args1 = 1.0 + 1.0 / Term1;
                dVcdVgs = Uds / A;
                dVcdVds = Vgs_Vth * dUdsdVds / A - dVcdVgs * dVthdVds;
                dVcdVbs = (Vgs_Vth * dUdsdVbs - Uds *
                            (dVthdVbs + Vgs_Vth * dAdVbs / A)) / A;
                dKdVc = 0.5 * Args1;
                dKdVgs = dKdVc * dVcdVgs;
                dKdVds = dKdVc * dVcdVds;
                dKdVbs = dKdVc * dVcdVbs;
                Args2 = Vgs_Vth / A / K;
                Args3 = Args2 * Vgs_Vth;
                DrainCurrent = 0.5 * Beta * Args3;
                gm = 0.5 * Args3 * dBetadVgs + Beta * Args2 -
                            DrainCurrent * dKdVgs / K;
                gds = 0.5 * Args3 * dBetadVds - Beta * Args2 * dVthdVds -
                    DrainCurrent * dKdVds / K;
                gmbs = 0.5 * dBetadVbs * Args3 - Beta * Args2 * dVthdVbs -
                    DrainCurrent * (dAdVbs / A + dKdVbs / K);
            }

            SubthresholdComputation:

            N0 = B1subthSlope;
            Vcut = -40.0 * N0 * Circuit.CONSTvt0;

            /* The following 'if' statement has been modified so that subthreshold  *
             * current computation is always executed unless N0 >= 200. This should *
             * get rid of the Ids kink seen on Ids-Vgs plots at low Vds.            *
             *                                                Peter M. Lee          *
             *                                                6/8/90                *
             *  Old 'if' statement:                                                 *
             *  if( (N0 >=  200) || (Vgs_Vth < Vcut ) || (Vgs_Vth > (-0.5*Vcut)))   */

            if (N0 >= 200)
            {
                goto ChargeComputation;
            }

            NB = B1subthSlopeB;
            ND = B1subthSlopeD;
            N = N0 + NB * vbs + ND * vds; /* subthreshold slope */
            if (N < 0.5) N = 0.5;
            Warg1 = Math.Exp(-vds / Circuit.CONSTvt0);
            Wds = 1 - Warg1;
            Wgs = Math.Exp(Vgs_Vth / (N * Circuit.CONSTvt0));
            Vtsquare = Circuit.CONSTvt0 * Circuit.CONSTvt0;
            Warg2 = 6.04965 * Vtsquare * B1betaZero;
            Ilimit = 4.5 * Vtsquare * B1betaZero;
            Iexp = Warg2 * Wgs * Wds;
            DrainCurrent = DrainCurrent + Ilimit * Iexp / (Ilimit + Iexp);
            Temp1 = Ilimit / (Ilimit + Iexp);
            Temp1 = Temp1 * Temp1;
            Temp3 = Ilimit / (Ilimit + Wgs * Warg2);
            Temp3 = Temp3 * Temp3 * Warg2 * Wgs;
            /*    if ( Temp3 > Ilimit ) Temp3=Ilimit;*/
            gm = gm + Temp1 * Iexp / (N * Circuit.CONSTvt0);
            /* gds term has been modified to prevent blow up at Vds=0 */
            gds = gds + Temp3 * (-Wds / N / Circuit.CONSTvt0 * (dVthdVds +
                Vgs_Vth * ND / N) + Warg1 / Circuit.CONSTvt0);
            gmbs = gmbs - Temp1 * Iexp * (dVthdVbs + Vgs_Vth * NB / N) /
                (N * Circuit.CONSTvt0);

            ChargeComputation:

            /* Some Limiting of DC Parameters */
            if (DrainCurrent < 0.0) DrainCurrent = 0.0;
            if (gm < 0.0) gm = 0.0;
            if (gds < 0.0) gds = 0.0;
            if (gmbs < 0.0) gmbs = 0.0;

            WLCox = model.B1Cox *
                (B1l - model.B1deltaL * 1.0e-6) *
                (B1w - model.B1deltaW * 1.0e-6) * 1.0e4;   /* F */

            if (!ChargeComputationNeeded)
            {
                qg = 0;
                qd = 0;
                qb = 0;
                cggb = 0;
                cgsb = 0;
                cgdb = 0;
                cdgb = 0;
                cdsb = 0;
                cddb = 0;
                cbgb = 0;
                cbsb = 0;
                cbdb = 0;
                goto finished;
            }
            G = 1.0 - 1.0 / (1.744 + 0.8364 * Vpb);
            A = 1.0 + 0.5 * G * K1 / SqrtVpb;
            A = Math.Max(A, 1.0);   /* Modified */
                                    /*Arg  =  1 + Ugs * Vgs_Vth;*/
            dGdVbs = -0.8364 * (1 - G) * (1 - G);
            dAdVbs = 0.25 * K1 / SqrtVpb * (2 * dGdVbs + G / Vpb);
            Phi = Math.Max(0.1, Phi);

            if (model.B1channelChargePartitionFlag >= 1)
            {

                /*0/100 partitioning for drain/source chArges at the saturation region*/
                Vth0 = Vfb + Phi + K1 * SqrtVpb;
                Vgs_Vth = vgs - Vth0;
                Arg1 = A * vds;
                Arg2 = Vgs_Vth - 0.5 * Arg1;
                Arg3 = vds - Arg1;
                Arg5 = Arg1 * Arg1;
                dVthdVbs = -0.5 * K1 / SqrtVpb;
                dAdVbs = 0.5 * K1 * (0.5 * G / Vpb - 0.8364 * (1 - G) * (1 - G)) /
                    SqrtVpb;
                Ent = Math.Max(Arg2, 1.0e-8);
                dEntdVds = -0.5 * A;
                dEntdVbs = -dVthdVbs - 0.5 * vds * dAdVbs;
                Vcom = Vgs_Vth * Vgs_Vth / 6.0 - 1.25e-1 * Arg1 *
                            Vgs_Vth + 2.5e-2 * Arg5;
                VdsPinchoff = Math.Max(Vgs_Vth / A, 0.0);
                Vgb = vgs - vbs;
                Vgb_Vfb = Vgb - Vfb;

                if (Vgb_Vfb < 0)
                {
                    /* Accumulation Region */
                    qg = WLCox * Vgb_Vfb;
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox;
                    cgdb = 0.0;
                    cgsb = 0.0;
                    cbgb = -WLCox;
                    cbdb = 0.0;
                    cbsb = 0.0;
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }
                else if (vgs < Vth0)
                {
                    /* Subthreshold Region */
                    qg = 0.5 * WLCox * K1 * K1 * (-1 +
                        Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1)));
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox / Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1));
                    cgdb = cgsb = 0.0;
                    cbgb = -cggb;
                    cbdb = cbsb = cdgb = cddb = cdsb = 0.0;
                    goto finished;
                }
                else if (vds < VdsPinchoff)
                {    /* triode region  */
                     /*Vgs_Vth2 = Vgs_Vth*Vgs_Vth;*/
                    EntSquare = Ent * Ent;
                    Argl1 = 1.2e1 * EntSquare;
                    Argl2 = 1.0 - A;
                    Argl3 = Arg1 * vds;
                    /*Argl4 = Vcom/Ent/EntSquare;*/
                    if (Ent > 1.0e-8)
                    {
                        Argl5 = Arg1 / Ent;
                        /*Argl6 = Vcom/EntSquare;*/
                    }
                    else
                    {
                        Argl5 = 2.0;
                        Argl6 = 4.0 / 1.5e1;
                    }
                    Argl7 = Argl5 / 1.2e1;
                    Argl8 = 6.0 * Ent;
                    Argl9 = 0.125 * Argl5 * Argl5;
                    qg = WLCox * (vgs - Vfb - Phi - 0.5 * vds + vds * Argl7);
                    qb = WLCox * (-Vth0 + Vfb + Phi + 0.5 * Arg3 - Arg3 * Argl7);
                    qd = -WLCox * (0.5 * Vgs_Vth - 0.75 * Arg1 +
                        0.125 * Arg1 * Argl5);
                    cggb = WLCox * (1.0 - Argl3 / Argl1);
                    cgdb = WLCox * (-0.5 + Arg1 / Argl8 - Argl3 * dEntdVds /
                        Argl1);
                    cgbb = WLCox * (vds * vds * dAdVbs * Ent - Argl3 * dEntdVbs) /
                        Argl1;
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * Argl3 * Argl2 / Argl1;
                    cbdb = WLCox * Argl2 * (0.5 - Arg1 / Argl8 + Argl3 * dEntdVds /
                        Argl1);
                    cbbb = -WLCox * (dVthdVbs + 0.5 * vds * dAdVbs + vds *
                        vds * ((1.0 - 2.0 * A) * dAdVbs * Ent - Argl2 *
                        A * dEntdVbs) / Argl1);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -WLCox * (0.5 - Argl9);
                    cddb = WLCox * (0.75 * A - 0.25 * A * Arg1 / Ent +
                        Argl9 * dEntdVds);
                    cdbb = WLCox * (0.5 * dVthdVbs + vds * dAdVbs *
                        (0.75 - 0.25 * Argl5) + Argl9 * dEntdVbs);
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
                else if (vds >= VdsPinchoff)
                {    /* saturation region   */
                    Args1 = 1.0 / (3.0 * A);
                    qg = WLCox * (vgs - Vfb - Phi - Vgs_Vth * Args1);
                    qb = WLCox * (Vfb + Phi - Vth0 + (1.0 - A) * Vgs_Vth * Args1);
                    qd = 0.0;
                    cggb = WLCox * (1.0 - Args1);
                    cgdb = 0.0;
                    cgbb = WLCox * Args1 * (dVthdVbs + Vgs_Vth * dAdVbs / A);
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * (Args1 - 1.0 / 3.0);
                    cbdb = 0.0;
                    cbbb = -WLCox * ((2.0 / 3.0 + Args1) * dVthdVbs +
                        Vgs_Vth * Args1 * dAdVbs / A);      /* Modified */
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }

                goto finished;

            }
            else
            {
                /* ChannelChargePartionFlag  < = 0 */

                /*40/60 partitioning for drain/source chArges at the saturation region*/
                co4v15 = 4.0 / 15.0;
                Vth0 = Vfb + Phi + K1 * SqrtVpb;
                Vgs_Vth = vgs - Vth0;
                Arg1 = A * vds;
                Arg2 = Vgs_Vth - 0.5 * Arg1;
                Arg3 = vds - Arg1;
                Arg5 = Arg1 * Arg1;
                dVthdVbs = -0.5 * K1 / SqrtVpb;
                dAdVbs = 0.5 * K1 * (0.5 * G / Vpb - 0.8364 * (1 - G) * (1 - G)) / SqrtVpb;
                Ent = Math.Max(Arg2, 1.0e-8);
                dEntdVds = -0.5 * A;
                dEntdVbs = -dVthdVbs - 0.5 * vds * dAdVbs;
                Vcom = Vgs_Vth * Vgs_Vth / 6.0 - 1.25e-1 * Arg1 * Vgs_Vth + 2.5e-2 * Arg5;
                VdsPinchoff = Math.Max(Vgs_Vth / A, 0.0);
                Vgb = vgs - vbs;
                Vgb_Vfb = Vgb - Vfb;

                if (Vgb_Vfb < 0)
                {           /* Accumulation Region */
                    qg = WLCox * Vgb_Vfb;
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox;
                    cgdb = 0.0;
                    cgsb = 0.0;
                    cbgb = -WLCox;
                    cbdb = 0.0;
                    cbsb = 0.0;
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }
                else if (vgs < Vth0)
                {    /* Subthreshold Region */
                    qg = 0.5 * WLCox * K1 * K1 * (-1 + Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1)));
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox / Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1));
                    cgdb = cgsb = 0.0;
                    cbgb = -cggb;
                    cbdb = cbsb = cdgb = cddb = cdsb = 0.0;
                    goto finished;
                }
                else if (vds < VdsPinchoff)
                {      /* triode region */

                    Vgs_VthSquare = Vgs_Vth * Vgs_Vth;
                    EntSquare = Ent * Ent;
                    Argl1 = 1.2e1 * EntSquare;
                    Argl2 = 1.0 - A;
                    Argl3 = Arg1 * vds;
                    Argl4 = Vcom / Ent / EntSquare;
                    if (Ent > 1.0e-8)
                    {
                        Argl5 = Arg1 / Ent;
                        Argl6 = Vcom / EntSquare;
                    }
                    else
                    {
                        Argl5 = 2.0;
                        Argl6 = 4.0 / 1.5e1;
                    }
                    Argl7 = Argl5 / 1.2e1;
                    Argl8 = 6.0 * Ent;
                    qg = WLCox * (vgs - Vfb - Phi - 0.5 * vds + vds * Argl7);
                    qb = WLCox * (-Vth0 + Vfb + Phi + 0.5 * Arg3 - Arg3 * Argl7);
                    qd = -WLCox * (0.5 * (Vgs_Vth - Arg1) + Arg1 * Argl6);
                    cggb = WLCox * (1.0 - Argl3 / Argl1);
                    cgdb = WLCox * (-0.5 + Arg1 / Argl8 - Argl3 * dEntdVds / Argl1);
                    cgbb = WLCox * (vds * vds * dAdVbs * Ent - Argl3 * dEntdVbs) / Argl1;
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * Argl3 * Argl2 / Argl1;
                    cbdb = WLCox * Argl2 * (0.5 - Arg1 / Argl8 + Argl3 * dEntdVds / Argl1);
                    cbbb = -WLCox * (dVthdVbs + 0.5 * vds * dAdVbs + vds * vds * ((1.0 - 2.0 * A)
                        * dAdVbs * Ent - Argl2 * A * dEntdVbs) / Argl1);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -WLCox * (0.5 + Arg1 * (4.0 * Vgs_Vth - 1.5 * Arg1) / Argl1 -
                        2.0 * Arg1 * Argl4);
                    cddb = WLCox * (0.5 * A + 2.0 * Arg1 * dEntdVds * Argl4 - A * (2.0 * Vgs_VthSquare
                        - 3.0 * Arg1 * Vgs_Vth + 0.9 * Arg5) / Argl1);
                    cdbb = WLCox * (0.5 * dVthdVbs + 0.5 * vds * dAdVbs + 2.0 * Arg1 * dEntdVbs
                        * Argl4 - vds * (2.0 * Vgs_VthSquare * dAdVbs - 4.0 * A * Vgs_Vth * dVthdVbs - 3.0
                        * Arg1 * Vgs_Vth * dAdVbs + 1.5 * A * Arg1 * dVthdVbs + 0.9 * Arg5 * dAdVbs)
                        / Argl1);
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
                else if (vds >= VdsPinchoff)
                {      /* saturation region */

                    Args1 = 1.0 / (3.0 * A);
                    qg = WLCox * (vgs - Vfb - Phi - Vgs_Vth * Args1);
                    qb = WLCox * (Vfb + Phi - Vth0 + (1.0 - A) * Vgs_Vth * Args1);
                    qd = -co4v15 * WLCox * Vgs_Vth;
                    cggb = WLCox * (1.0 - Args1);
                    cgdb = 0.0;
                    cgbb = WLCox * Args1 * (dVthdVbs + Vgs_Vth * dAdVbs / A);
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * (Args1 - 1.0 / 3.0);
                    cbdb = 0.0;
                    cbbb = -WLCox * ((2.0 / 3.0 + Args1) * dVthdVbs + Vgs_Vth * Args1 * dAdVbs / A);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -co4v15 * WLCox;
                    cddb = 0.0;
                    cdbb = co4v15 * WLCox * dVthdVbs;
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
            }

            finished:       /* returning Values to Calling Routine */


            gmPointer = Math.Max(gm, 0.0);
            gdsPointer = Math.Max(gds, 0.0);
            gmbsPointer = Math.Max(gmbs, 0.0);
            qgPointer = qg;
            qbPointer = qb;
            qdPointer = qd;
            cggbPointer = cggb;
            cgdbPointer = cgdb;
            cgsbPointer = cgsb;
            cbgbPointer = cbgb;
            cbdbPointer = cbdb;
            cbsbPointer = cbsb;
            cdgbPointer = cdgb;
            cddbPointer = cddb;
            cdsbPointer = cdsb;
            cdrainPointer = Math.Max(DrainCurrent, 0.0);
            vonPointer = Von;
            vdsatPointer = VdsSat;
        }

        /* routine to calculate equivalent conductance and total terminal 
         * charges
         */
        private void B1mosCap(Circuit ckt, double vgd, double vgs, double vgb, double[] args,
            double cbgb, double cbdb, double cbsb, double cdgb, double cddb, double cdsb,
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
            gcssbPointer = (args[4] + args[1] - (args[7] + cbsb + cdsb)) * ag0;
            gcggbPointer = (args[5] + args[0] + args[1] + args[2]) * ag0;
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
    }
}
