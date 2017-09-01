using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    [SpiceNodes("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public partial class BSIM4v80 : CircuitComponent<BSIM4v80>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM4v80Model model) => Model = model;

        /// <summary>
        /// Size-dependent parameters
        /// </summary>
        private BSIM4SizeDependParam pParam = null;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter BSIM4w { get; } = new Parameter(5.0e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter BSIM4l { get; } = new Parameter(5.0e-6);
        [SpiceName("nf"), SpiceInfo("Number of fingers")]
        public Parameter BSIM4nf { get; } = new Parameter(1.0);
        [SpiceName("min"), SpiceInfo("Minimize either D or S")]
        public Parameter BSIM4min { get; } = new Parameter();
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter BSIM4sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter BSIM4drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter BSIM4sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter BSIM4drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter BSIM4sourceSquares { get; } = new Parameter(1.0);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter BSIM4drainSquares { get; } = new Parameter(1.0);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool BSIM4off { get; set; }
        [SpiceName("sa"), SpiceInfo("distance between  OD edge to poly of one side ")]
        public Parameter BSIM4sa { get; } = new Parameter();
        [SpiceName("sb"), SpiceInfo("distance between  OD edge to poly of the other side")]
        public Parameter BSIM4sb { get; } = new Parameter();
        [SpiceName("sd"), SpiceInfo("distance between neighbour fingers")]
        public Parameter BSIM4sd { get; } = new Parameter();
        [SpiceName("sca"), SpiceInfo("Integral of the first distribution function for scattered well dopant")]
        public Parameter BSIM4sca { get; } = new Parameter();
        [SpiceName("scb"), SpiceInfo("Integral of the second distribution function for scattered well dopant")]
        public Parameter BSIM4scb { get; } = new Parameter();
        [SpiceName("scc"), SpiceInfo("Integral of the third distribution function for scattered well dopant")]
        public Parameter BSIM4scc { get; } = new Parameter();
        [SpiceName("sc"), SpiceInfo("Distance to a single well edge ")]
        public Parameter BSIM4sc { get; } = new Parameter();
        [SpiceName("rbsb"), SpiceInfo("Body resistance")]
        public Parameter BSIM4rbsb { get; } = new Parameter();
        [SpiceName("rbdb"), SpiceInfo("Body resistance")]
        public Parameter BSIM4rbdb { get; } = new Parameter();
        [SpiceName("rbpb"), SpiceInfo("Body resistance")]
        public Parameter BSIM4rbpb { get; } = new Parameter();
        [SpiceName("rbps"), SpiceInfo("Body resistance")]
        public Parameter BSIM4rbps { get; } = new Parameter();
        [SpiceName("rbpd"), SpiceInfo("Body resistance")]
        public Parameter BSIM4rbpd { get; } = new Parameter();
        [SpiceName("delvto"), SpiceInfo("Zero bias threshold voltage variation")]
        public Parameter BSIM4delvto { get; } = new Parameter();
        [SpiceName("xgw"), SpiceInfo("Distance from gate contact center to device edge")]
        public Parameter BSIM4xgw { get; } = new Parameter();
        [SpiceName("ngcon"), SpiceInfo("Number of gate contacts")]
        public Parameter BSIM4ngcon { get; } = new Parameter();
        [SpiceName("trnqsmod"), SpiceInfo("Transient NQS model selector")]
        public Parameter BSIM4trnqsMod { get; } = new Parameter();
        [SpiceName("acnqsmod"), SpiceInfo("AC NQS model selector")]
        public Parameter BSIM4acnqsMod { get; } = new Parameter();
        [SpiceName("rbodymod"), SpiceInfo("Distributed body R model selector")]
        public Parameter BSIM4rbodyMod { get; } = new Parameter();
        [SpiceName("rgatemod"), SpiceInfo("Gate resistance model selector")]
        public Parameter BSIM4rgateMod { get; } = new Parameter();
        [SpiceName("geomod"), SpiceInfo("Geometry dependent parasitics model selector")]
        public Parameter BSIM4geoMod { get; } = new Parameter();
        [SpiceName("rgeomod"), SpiceInfo("S/D resistance and contact model selector")]
        public Parameter BSIM4rgeoMod { get; } = new Parameter();
        [SpiceName("id"), SpiceInfo("Ids")]
        public double BSIM4cd { get; private set; }
        [SpiceName("ibs"), SpiceInfo("Ibs")]
        public double BSIM4cbs { get; private set; }
        [SpiceName("ibd"), SpiceInfo("Ibd")]
        public double BSIM4cbd { get; private set; }
        [SpiceName("isub"), SpiceInfo("Isub")]
        public double BSIM4csub { get; private set; }
        [SpiceName("igidl"), SpiceInfo("Igidl")]
        public double BSIM4Igidl { get; private set; }
        [SpiceName("igisl"), SpiceInfo("Igisl")]
        public double BSIM4Igisl { get; private set; }
        [SpiceName("igs"), SpiceInfo("Igs")]
        public double BSIM4Igs { get; private set; }
        [SpiceName("igd"), SpiceInfo("Igd")]
        public double BSIM4Igd { get; private set; }
        [SpiceName("igb"), SpiceInfo("Igb")]
        public double BSIM4Igb { get; private set; }
        [SpiceName("igcs"), SpiceInfo("Igcs")]
        public double BSIM4Igcs { get; private set; }
        [SpiceName("igcd"), SpiceInfo("Igcd")]
        public double BSIM4Igcd { get; private set; }
        [SpiceName("gm"), SpiceInfo("Gm")]
        public double BSIM4gm { get; private set; }
        [SpiceName("gds"), SpiceInfo("Gds")]
        public double BSIM4gds { get; private set; }
        [SpiceName("gmbs"), SpiceInfo("Gmb")]
        public double BSIM4gmbs { get; private set; }
        [SpiceName("gbd"), SpiceInfo("gbd")]
        public double BSIM4gbd { get; private set; }
        [SpiceName("gbs"), SpiceInfo("gbs")]
        public double BSIM4gbs { get; private set; }
        [SpiceName("qb"), SpiceInfo("Qbulk")]
        public double BSIM4qbulk { get; private set; }
        [SpiceName("qg"), SpiceInfo("Qgate")]
        public double BSIM4qgate { get; private set; }
        [SpiceName("qs"), SpiceInfo("Qsource")]
        public double BSIM4qsrc { get; private set; }
        [SpiceName("qd"), SpiceInfo("Qdrain")]
        public double BSIM4qdrn { get; private set; }
        [SpiceName("gcrg"), SpiceInfo("Gcrg")]
        public double BSIM4gcrg { get; private set; }
        [SpiceName("gtau"), SpiceInfo("Gtau")]
        public double BSIM4gtau { get; private set; }
        [SpiceName("cgg"), SpiceInfo("Cggb")]
        public double BSIM4cggb { get; private set; }
        [SpiceName("cgd"), SpiceInfo("Cgdb")]
        public double BSIM4cgdb { get; private set; }
        [SpiceName("cgs"), SpiceInfo("Cgsb")]
        public double BSIM4cgsb { get; private set; }
        [SpiceName("cdg"), SpiceInfo("Cdgb")]
        public double BSIM4cdgb { get; private set; }
        [SpiceName("cdd"), SpiceInfo("Cddb")]
        public double BSIM4cddb { get; private set; }
        [SpiceName("cds"), SpiceInfo("Cdsb")]
        public double BSIM4cdsb { get; private set; }
        [SpiceName("cbg"), SpiceInfo("Cbgb")]
        public double BSIM4cbgb { get; private set; }
        [SpiceName("cbd"), SpiceInfo("Cbdb")]
        public double BSIM4cbdb { get; private set; }
        [SpiceName("cbs"), SpiceInfo("Cbsb")]
        public double BSIM4cbsb { get; private set; }
        [SpiceName("csg"), SpiceInfo("Csgb")]
        public double BSIM4csgb { get; private set; }
        [SpiceName("csd"), SpiceInfo("Csdb")]
        public double BSIM4csdb { get; private set; }
        [SpiceName("css"), SpiceInfo("Cssb")]
        public double BSIM4cssb { get; private set; }
        [SpiceName("cgb"), SpiceInfo("Cgbb")]
        public double BSIM4cgbb { get; private set; }
        [SpiceName("cdb"), SpiceInfo("Cdbb")]
        public double BSIM4cdbb { get; private set; }
        [SpiceName("csb"), SpiceInfo("Csbb")]
        public double BSIM4csbb { get; private set; }
        [SpiceName("cbb"), SpiceInfo("Cbbb")]
        public double BSIM4cbbb { get; private set; }
        [SpiceName("capbd"), SpiceInfo("Capbd")]
        public double BSIM4capbd { get; private set; }
        [SpiceName("capbs"), SpiceInfo("Capbs")]
        public double BSIM4capbs { get; private set; }
        [SpiceName("vth"), SpiceInfo("Vth")]
        public double BSIM4von { get; private set; }
        [SpiceName("vdsat"), SpiceInfo("Vdsat")]
        public double BSIM4vdsat { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: BSIM4icVBS.Set(value[2]); goto case 2;
                case 2: BSIM4icVGS.Set(value[1]); goto case 1;
                case 1: BSIM4icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("vbs"), SpiceInfo("Vbs")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][BSIM4states + BSIM4vbs];
        [SpiceName("vgs"), SpiceInfo("Vgs")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][BSIM4states + BSIM4vgs];
        [SpiceName("vds"), SpiceInfo("Vds")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][BSIM4states + BSIM4vds];
        [SpiceName("qdef"), SpiceInfo("Qdef")]
        public double GetQDEF(Circuit ckt) => ckt.State.States[0][BSIM4states + BSIM4qdef];

        /// <summary>
        /// Extra variables
        /// </summary>
        public Parameter BSIM4icVDS { get; } = new Parameter();
        public Parameter BSIM4icVGS { get; } = new Parameter();
        public Parameter BSIM4icVBS { get; } = new Parameter();
        public double BSIM4u0temp { get; private set; }
        public double BSIM4vsattemp { get; private set; }
        public double BSIM4vth0 { get; private set; }
        public double BSIM4eta0 { get; private set; }
        public double BSIM4k2 { get; private set; }
        public double BSIM4vfb { get; private set; }
        public double BSIM4vtfbphi1 { get; private set; }
        public double BSIM4vtfbphi2 { get; private set; }
        public double BSIM4vbsc { get; private set; }
        public double BSIM4k2ox { get; private set; }
        public double BSIM4vfbzb { get; private set; }
        public double BSIM4cgso { get; private set; }
        public double BSIM4cgdo { get; private set; }
        public double BSIM4grbdb { get; private set; }
        public double BSIM4grbpb { get; private set; }
        public double BSIM4grbps { get; private set; }
        public double BSIM4grbsb { get; private set; }
        public double BSIM4grbpd { get; private set; }
        public double BSIM4grgeltd { get; private set; }
        public double BSIM4Pseff { get; private set; }
        public double BSIM4Pdeff { get; private set; }
        public double BSIM4Aseff { get; private set; }
        public double BSIM4Adeff { get; private set; }
        public double BSIM4sourceConductance { get; private set; }
        public double BSIM4drainConductance { get; private set; }
        public double BSIM4XExpBVS { get; private set; }
        public double BSIM4vjsmFwd { get; private set; }
        public double BSIM4IVjsmFwd { get; private set; }
        public double BSIM4SslpFwd { get; private set; }
        public double BSIM4vjsmRev { get; private set; }
        public double BSIM4IVjsmRev { get; private set; }
        public double BSIM4SslpRev { get; private set; }
        public double BSIM4XExpBVD { get; private set; }
        public double BSIM4vjdmFwd { get; private set; }
        public double BSIM4IVjdmFwd { get; private set; }
        public double BSIM4DslpFwd { get; private set; }
        public double BSIM4vjdmRev { get; private set; }
        public double BSIM4IVjdmRev { get; private set; }
        public double BSIM4DslpRev { get; private set; }
        public double BSIM4SjctTempRevSatCur { get; private set; }
        public double BSIM4DjctTempRevSatCur { get; private set; }
        public double BSIM4SswTempRevSatCur { get; private set; }
        public double BSIM4DswTempRevSatCur { get; private set; }
        public double BSIM4SswgTempRevSatCur { get; private set; }
        public double BSIM4DswgTempRevSatCur { get; private set; }
        public double BSIM4toxp { get; private set; }
        public double BSIM4coxp { get; private set; }
        public double BSIM4mode { get; private set; }
        public double BSIM4gbbs { get; private set; }
        public double BSIM4ggidlb { get; private set; }
        public double BSIM4gbgs { get; private set; }
        public double BSIM4ggidlg { get; private set; }
        public double BSIM4gbds { get; private set; }
        public double BSIM4ggidld { get; private set; }
        public double BSIM4ggisls { get; private set; }
        public double BSIM4ggislg { get; private set; }
        public double BSIM4ggislb { get; private set; }
        public double BSIM4gIgsg { get; private set; }
        public double BSIM4gIgcsg { get; private set; }
        public double BSIM4gIgcsd { get; private set; }
        public double BSIM4gIgcsb { get; private set; }
        public double BSIM4gIgdg { get; private set; }
        public double BSIM4gIgcdg { get; private set; }
        public double BSIM4gIgcdd { get; private set; }
        public double BSIM4gIgcdb { get; private set; }
        public double BSIM4gIgbg { get; private set; }
        public double BSIM4gIgbd { get; private set; }
        public double BSIM4gIgbb { get; private set; }
        public double BSIM4ggidls { get; private set; }
        public double BSIM4ggisld { get; private set; }
        public double BSIM4gstot { get; private set; }
        public double BSIM4gstotd { get; private set; }
        public double BSIM4gstotg { get; private set; }
        public double BSIM4gstotb { get; private set; }
        public double BSIM4gdtot { get; private set; }
        public double BSIM4gdtotd { get; private set; }
        public double BSIM4gdtotg { get; private set; }
        public double BSIM4gdtotb { get; private set; }
        public double BSIM4thetavth { get; private set; }
        public double BSIM4nstar { get; private set; }
        public double BSIM4vgs_eff { get; private set; }
        public double BSIM4vgd_eff { get; private set; }
        public double BSIM4dvgs_eff_dvg { get; private set; }
        public double BSIM4dvgd_eff_dvg { get; private set; }
        public double BSIM4Vgsteff { get; private set; }
        public double BSIM4grdsw { get; private set; }
        public double BSIM4Abulk { get; private set; }
        public double BSIM4ueff { get; private set; }
        public double BSIM4EsatL { get; private set; }
        public double BSIM4Vdseff { get; private set; }
        public double BSIM4Coxeff { get; private set; }
        public double BSIM4AbovVgst2Vtm { get; private set; }
        public double BSIM4IdovVds { get; private set; }
        public double BSIM4gcrgd { get; private set; }
        public double BSIM4gcrgb { get; private set; }
        public double BSIM4gcrgg { get; private set; }
        public double BSIM4gcrgs { get; private set; }
        public double BSIM4gstots { get; private set; }
        public double BSIM4gdtots { get; private set; }
        public double BSIM4gIgss { get; private set; }
        public double BSIM4gIgdd { get; private set; }
        public double BSIM4gIgbs { get; private set; }
        public double BSIM4gIgcss { get; private set; }
        public double BSIM4gIgcds { get; private set; }
        public double BSIM4qinv { get; private set; }
        public double BSIM4noiGd0 { get; private set; }
        public double BSIM4cqdb { get; private set; }
        public double BSIM4cqsb { get; private set; }
        public double BSIM4cqgb { get; private set; }
        public double BSIM4cqbb { get; private set; }
        public double BSIM4qchqs { get; private set; }
        public double BSIM4taunet { get; private set; }
        public double BSIM4qgdo { get; private set; }
        public double BSIM4qgso { get; private set; }
        public double BSIM4gtg { get; private set; }
        public double BSIM4gtd { get; private set; }
        public double BSIM4gts { get; private set; }
        public double BSIM4gtb { get; private set; }
        public int BSIM4dNodePrime { get; private set; }
        public int BSIM4bNodePrime { get; private set; }
        public int BSIM4gNodePrime { get; private set; }
        public int BSIM4sNodePrime { get; private set; }
        public int BSIM4dNode { get; private set; }
        public int BSIM4sNode { get; private set; }
        public int BSIM4qNode { get; private set; }
        public int BSIM4gNodeExt { get; private set; }
        public int BSIM4gNodeMid { get; private set; }
        public int BSIM4dbNode { get; private set; }
        public int BSIM4sbNode { get; private set; }
        public int BSIM4bNode { get; private set; }
        public int BSIM4states { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int BSIM4vbd = 0;
        private const int BSIM4vbs = 1;
        private const int BSIM4vgs = 2;
        private const int BSIM4vds = 3;
        private const int BSIM4vdbs = 4;
        private const int BSIM4vdbd = 5;
        private const int BSIM4vsbs = 6;
        private const int BSIM4vges = 7;
        private const int BSIM4vgms = 8;
        private const int BSIM4vses = 9;
        private const int BSIM4vdes = 10;
        private const int BSIM4qb = 11;
        private const int BSIM4cqb = 12;
        private const int BSIM4qg = 13;
        private const int BSIM4cqg = 14;
        private const int BSIM4qd = 15;
        private const int BSIM4cqd = 16;
        private const int BSIM4qgmid = 17;
        private const int BSIM4cqgmid = 18;
        private const int BSIM4qbs = 19;
        private const int BSIM4cqbs = 20;
        private const int BSIM4qbd = 21;
        private const int BSIM4cqbd = 22;
        private const int BSIM4qcheq = 23;
        private const int BSIM4cqcheq = 24;
        private const int BSIM4qcdump = 25;
        private const int BSIM4cqcdump = 26;
        private const int BSIM4qdef = 27;
        private const int BSIM4qs = 28;

        private const double NMOS = 1.0;
        private const double PMOS = -1.0;
        private const double ScalingFactor = 1e-9;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM4v80(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM4v80Model;
            int createNode;
            bool noiseAnalGiven = true;
            double Rtot;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            BSIM4dNodePrime = nodes[0].Index;
            BSIM4bNodePrime = nodes[1].Index;
            BSIM4gNodePrime = nodes[2].Index;
            BSIM4sNodePrime = nodes[3].Index;

            // Allocate states
            BSIM4states = ckt.State.GetState(29);

            if (!BSIM4rbdb.Given)
                BSIM4rbdb.Value = model.BSIM4rbdb; /* in ohm */
            if (!BSIM4rbsb.Given)
                BSIM4rbsb.Value = model.BSIM4rbsb;
            if (!BSIM4rbpb.Given)
                BSIM4rbpb.Value = model.BSIM4rbpb;
            if (!BSIM4rbps.Given)
                BSIM4rbps.Value = model.BSIM4rbps;
            if (!BSIM4rbpd.Given)
                BSIM4rbpd.Value = model.BSIM4rbpd;
            if (!BSIM4xgw.Given)
                BSIM4xgw.Value = model.BSIM4xgw;
            if (!BSIM4ngcon.Given)
                BSIM4ngcon.Value = model.BSIM4ngcon;

            /* Process instance model selectors, some
			* may override their global counterparts
			*/
            if (!BSIM4rbodyMod.Given)
                BSIM4rbodyMod.Value = model.BSIM4rbodyMod;
            else if ((BSIM4rbodyMod != 0) && (BSIM4rbodyMod != 1) && (BSIM4rbodyMod != 2))
            {
                BSIM4rbodyMod.Value = model.BSIM4rbodyMod;
                CircuitWarning.Warning(this, $"Warning: rbodyMod has been set to its global value {model.BSIM4rbodyMod}.");
            }

            if (!BSIM4rgateMod.Given)
                BSIM4rgateMod.Value = model.BSIM4rgateMod;
            else if ((BSIM4rgateMod != 0) && (BSIM4rgateMod != 1) && (BSIM4rgateMod != 2) && (BSIM4rgateMod != 3))
            {
                BSIM4rgateMod.Value = model.BSIM4rgateMod;
                CircuitWarning.Warning(this, $"Warning: rgateMod has been set to its global value {model.BSIM4rgateMod}.");
            }

            if (!BSIM4geoMod.Given)
                BSIM4geoMod.Value = model.BSIM4geoMod;
            if (!BSIM4trnqsMod.Given)
                BSIM4trnqsMod.Value = model.BSIM4trnqsMod;
            else if ((BSIM4trnqsMod != 0) && (BSIM4trnqsMod != 1))
            {
                BSIM4trnqsMod.Value = model.BSIM4trnqsMod;
                CircuitWarning.Warning(this, $"Warning: trnqsMod has been set to its global value {model.BSIM4trnqsMod}.");
            }

            if (!BSIM4acnqsMod.Given)
                BSIM4acnqsMod.Value = model.BSIM4acnqsMod;
            else if ((BSIM4acnqsMod != 0) && (BSIM4acnqsMod != 1))
            {
                BSIM4acnqsMod.Value = model.BSIM4acnqsMod;
                CircuitWarning.Warning(this, $"Warning: acnqsMod has been set to its global value {model.BSIM4acnqsMod}.");
            }

            /* stress effect */
            if (!BSIM4sd.Given)
                BSIM4sd.Value = 2 * model.BSIM4dmcg;

            /* process drain series resistance */
            createNode = 0;
            if ((model.BSIM4rdsMod != 0) || (model.BSIM4tnoiMod.Value == 1 && noiseAnalGiven))
                createNode = 1;
            else if (model.BSIM4sheetResistance > 0)
            {
                if (BSIM4drainSquares.Given && BSIM4drainSquares > 0)
                {
                    createNode = 1;
                }
                else if (!BSIM4drainSquares.Given && (BSIM4rgeoMod != 0))
                {
                    BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                    BSIM4rgeoMod, BSIM4min,
                    BSIM4w, model.BSIM4sheetResistance,
                    model.DMCGeff, model.DMCIeff, model.DMDGeff, 0, out Rtot);
                    if (Rtot > 0)
                        createNode = 1;
                }
            }
            if (createNode != 0)
                BSIM4dNodePrime = CreateNode(ckt).Index;
            else
                BSIM4dNodePrime = BSIM4dNode;

            /* process source series resistance */
            createNode = 0;
            if ((model.BSIM4rdsMod != 0) || (model.BSIM4tnoiMod.Value == 1 && noiseAnalGiven))
                createNode = 1;
            else if (model.BSIM4sheetResistance > 0)
            {
                if (BSIM4sourceSquares.Given && BSIM4sourceSquares > 0)
                    createNode = 1;
                else if (!BSIM4sourceSquares.Given && (BSIM4rgeoMod != 0))
                {
                    BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                    BSIM4rgeoMod, BSIM4min,
                    BSIM4w, model.BSIM4sheetResistance,
                    model.DMCGeff, model.DMCIeff, model.DMDGeff, 1, out Rtot);
                    if (Rtot > 0)
                        createNode = 1;
                }
            }
            if (createNode != 0)
                BSIM4sNodePrime = CreateNode(ckt).Index;
            else
                BSIM4sNodePrime = BSIM4sNode;

            if (BSIM4rgateMod > 0)
                BSIM4gNodePrime = CreateNode(ckt).Index;
            else
                BSIM4gNodePrime = BSIM4gNodeExt;

            if (BSIM4rgateMod.Value == 3)
            {
                BSIM4gNodeMid = CreateNode(ckt).Index;
            }
            else
                BSIM4gNodeMid = BSIM4gNodeExt;

            /* internal body nodes for body resistance model */
            if ((BSIM4rbodyMod.Value == 1) || (BSIM4rbodyMod.Value == 2))
            {
                BSIM4dbNode = CreateNode(ckt).Index;
                BSIM4bNodePrime = CreateNode(ckt).Index;
                BSIM4sbNode = CreateNode(ckt).Index;
            }
            else
                BSIM4dbNode = BSIM4bNodePrime = BSIM4sbNode = BSIM4bNode;

            /* NQS node */
            if (BSIM4trnqsMod != 0)
                BSIM4qNode = CreateNode(ckt).Index;
            else
                BSIM4qNode = 0;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as BSIM4v80Model;
            double Ldrn, Wdrn, Lnew = 0, Wnew, T0, T1, tmp1, tmp2, T2, T3, Inv_L, Inv_W, Inv_LW,
                PowWeffWr, T10, T4, T5, tmp, T8, T9, wlod, W_tmp, Inv_saref, Inv_sbref, Theta0, tmp3, n0, Inv_sa,
                Inv_sb, kvsat, i, Inv_ODeff, rho, OD_offset, dvth0_lod, dk2_lod, deta0_lod, sceff, lnl, lnw,
                lnnf, bodymode, rbsby, rbsbx, rbdbx, rbdby, rbpbx, rbpby, DMCGeff, DMCIeff, DMDGeff, Nvtms, SourceSatCurrent, Nvtmd,
                DrainSatCurrent, T7, T11, Vtmeot, vbieot, phieot, vddeot, T6, Vgs_eff, lt1, ltw, Delt_vth, TempRatioeot, Vth_NarrowW,
                Lpe_Vb, Vth, n, Vgsteff, vtfbphi2eot, niter, toxpf, toxpi, Tcen;
            double dumPs, dumPd, dumAs, dumAd;

            /* stress effect */
            Ldrn = BSIM4l;
            Wdrn = BSIM4w / BSIM4nf;
            Tuple<double, double, double> size = new Tuple<double, double, double>(BSIM4w, BSIM4l, BSIM4nf);
            if (model.Sizes.ContainsKey(size))
                pParam = model.Sizes[size];
            else
            {
                pParam = new BSIM4SizeDependParam();
                model.Sizes.Add(size, pParam);

                pParam.NFinger = BSIM4nf;
                Lnew = BSIM4l + model.BSIM4xl;
                Wnew = BSIM4w / BSIM4nf + model.BSIM4xw;

                T0 = Math.Pow(Lnew, model.BSIM4Lln);
                T1 = Math.Pow(Wnew, model.BSIM4Lwn);
                tmp1 = model.BSIM4Ll / T0 + model.BSIM4Lw / T1 + model.BSIM4Lwl / (T0 * T1);
                pParam.BSIM4dl = model.BSIM4Lint + tmp1;
                tmp2 = model.BSIM4Llc / T0 + model.BSIM4Lwc / T1 + model.BSIM4Lwlc / (T0 * T1);
                pParam.BSIM4dlc = model.BSIM4dlc + tmp2;

                T2 = Math.Pow(Lnew, model.BSIM4Wln);
                T3 = Math.Pow(Wnew, model.BSIM4Wwn);
                tmp1 = model.BSIM4Wl / T2 + model.BSIM4Ww / T3 + model.BSIM4Wwl / (T2 * T3);
                pParam.BSIM4dw = model.BSIM4Wint + tmp1;
                tmp2 = model.BSIM4Wlc / T2 + model.BSIM4Wwc / T3 + model.BSIM4Wwlc / (T2 * T3);
                pParam.BSIM4dwc = model.BSIM4dwc + tmp2;
                pParam.BSIM4dwj = model.BSIM4dwj + tmp2;

                pParam.BSIM4leff = Lnew - 2.0 * pParam.BSIM4dl;
                if (pParam.BSIM4leff <= 0.0)
                    throw new CircuitException($"BSIM4v80: mosfet {Name}, model {model.Name}: Effective channel length <= 0");
                pParam.BSIM4weff = Wnew - 2.0 * pParam.BSIM4dw;
                if (pParam.BSIM4weff <= 0.0)
                    throw new CircuitException($"BSIM4v80: mosfet {Name}, model {model.Name}: Effective channel width <= 0");
                pParam.BSIM4leffCV = Lnew - 2.0 * pParam.BSIM4dlc;
                if (pParam.BSIM4leffCV <= 0.0)
                    throw new CircuitException($"BSIM4v80: mosfet {Name}, model {model.Name}: Effective channel length for C-V <= 0");
                pParam.BSIM4weffCV = Wnew - 2.0 * pParam.BSIM4dwc;
                if (pParam.BSIM4weffCV <= 0.0)
                    throw new CircuitException($"BSIM4v80: mosfet {Name}, model {model.Name}: Effective channel width for C-V <= 0");
                pParam.BSIM4weffCJ = Wnew - 2.0 * pParam.BSIM4dwj;
                if (pParam.BSIM4weffCJ <= 0.0)
                    throw new CircuitException($"BSIM4v80: mosfet {Name}, model {model.Name}: Effective channel width for S/D junctions <= 0");

                if (model.BSIM4binUnit.Value == 1)
                {
                    Inv_L = 1.0e-6 / pParam.BSIM4leff;
                    Inv_W = 1.0e-6 / pParam.BSIM4weff;
                    Inv_LW = 1.0e-12 / (pParam.BSIM4leff * pParam.BSIM4weff);
                }
                else
                {
                    Inv_L = 1.0 / pParam.BSIM4leff;
                    Inv_W = 1.0 / pParam.BSIM4weff;
                    Inv_LW = 1.0 / (pParam.BSIM4leff * pParam.BSIM4weff);
                }
                pParam.BSIM4cdsc = model.BSIM4cdsc + model.BSIM4lcdsc * Inv_L + model.BSIM4wcdsc * Inv_W + model.BSIM4pcdsc * Inv_LW;
                pParam.BSIM4cdscb = model.BSIM4cdscb + model.BSIM4lcdscb * Inv_L + model.BSIM4wcdscb * Inv_W + model.BSIM4pcdscb * Inv_LW;

                pParam.BSIM4cdscd = model.BSIM4cdscd + model.BSIM4lcdscd * Inv_L + model.BSIM4wcdscd * Inv_W + model.BSIM4pcdscd * Inv_LW;

                pParam.BSIM4cit = model.BSIM4cit + model.BSIM4lcit * Inv_L + model.BSIM4wcit * Inv_W + model.BSIM4pcit * Inv_LW;
                pParam.BSIM4nfactor = model.BSIM4nfactor + model.BSIM4lnfactor * Inv_L + model.BSIM4wnfactor * Inv_W + model.BSIM4pnfactor *
                  Inv_LW;
                pParam.BSIM4tnfactor = model.BSIM4tnfactor /* v4.7 */  + model.BSIM4ltnfactor * Inv_L + model.BSIM4wtnfactor * Inv_W +
                                    model.BSIM4ptnfactor * Inv_LW;
                pParam.BSIM4xj = model.BSIM4xj + model.BSIM4lxj * Inv_L + model.BSIM4wxj * Inv_W + model.BSIM4pxj * Inv_LW;
                pParam.BSIM4vsat = model.BSIM4vsat + model.BSIM4lvsat * Inv_L + model.BSIM4wvsat * Inv_W + model.BSIM4pvsat * Inv_LW;
                pParam.BSIM4at = model.BSIM4at + model.BSIM4lat * Inv_L + model.BSIM4wat * Inv_W + model.BSIM4pat * Inv_LW;
                pParam.BSIM4a0 = model.BSIM4a0 + model.BSIM4la0 * Inv_L + model.BSIM4wa0 * Inv_W + model.BSIM4pa0 * Inv_LW;

                pParam.BSIM4ags = model.BSIM4ags + model.BSIM4lags * Inv_L + model.BSIM4wags * Inv_W + model.BSIM4pags * Inv_LW;

                pParam.BSIM4a1 = model.BSIM4a1 + model.BSIM4la1 * Inv_L + model.BSIM4wa1 * Inv_W + model.BSIM4pa1 * Inv_LW;
                pParam.BSIM4a2 = model.BSIM4a2 + model.BSIM4la2 * Inv_L + model.BSIM4wa2 * Inv_W + model.BSIM4pa2 * Inv_LW;
                pParam.BSIM4keta = model.BSIM4keta + model.BSIM4lketa * Inv_L + model.BSIM4wketa * Inv_W + model.BSIM4pketa * Inv_LW;
                pParam.BSIM4nsub = model.BSIM4nsub + model.BSIM4lnsub * Inv_L + model.BSIM4wnsub * Inv_W + model.BSIM4pnsub * Inv_LW;
                pParam.BSIM4ndep = model.BSIM4ndep + model.BSIM4lndep * Inv_L + model.BSIM4wndep * Inv_W + model.BSIM4pndep * Inv_LW;
                pParam.BSIM4nsd = model.BSIM4nsd + model.BSIM4lnsd * Inv_L + model.BSIM4wnsd * Inv_W + model.BSIM4pnsd * Inv_LW;
                pParam.BSIM4phin = model.BSIM4phin + model.BSIM4lphin * Inv_L + model.BSIM4wphin * Inv_W + model.BSIM4pphin * Inv_LW;
                pParam.BSIM4ngate = model.BSIM4ngate + model.BSIM4lngate * Inv_L + model.BSIM4wngate * Inv_W + model.BSIM4pngate * Inv_LW;
                pParam.BSIM4gamma1 = model.BSIM4gamma1 + model.BSIM4lgamma1 * Inv_L + model.BSIM4wgamma1 * Inv_W + model.BSIM4pgamma1 * Inv_LW;
                pParam.BSIM4gamma2 = model.BSIM4gamma2 + model.BSIM4lgamma2 * Inv_L + model.BSIM4wgamma2 * Inv_W + model.BSIM4pgamma2 * Inv_LW;
                pParam.BSIM4vbx = model.BSIM4vbx + model.BSIM4lvbx * Inv_L + model.BSIM4wvbx * Inv_W + model.BSIM4pvbx * Inv_LW;
                pParam.BSIM4vbm = model.BSIM4vbm + model.BSIM4lvbm * Inv_L + model.BSIM4wvbm * Inv_W + model.BSIM4pvbm * Inv_LW;
                pParam.BSIM4xt = model.BSIM4xt + model.BSIM4lxt * Inv_L + model.BSIM4wxt * Inv_W + model.BSIM4pxt * Inv_LW;
                pParam.BSIM4vfb = model.BSIM4vfb + model.BSIM4lvfb * Inv_L + model.BSIM4wvfb * Inv_W + model.BSIM4pvfb * Inv_LW;
                pParam.BSIM4k1 = model.BSIM4k1 + model.BSIM4lk1 * Inv_L + model.BSIM4wk1 * Inv_W + model.BSIM4pk1 * Inv_LW;
                pParam.BSIM4kt1 = model.BSIM4kt1 + model.BSIM4lkt1 * Inv_L + model.BSIM4wkt1 * Inv_W + model.BSIM4pkt1 * Inv_LW;
                pParam.BSIM4kt1l = model.BSIM4kt1l + model.BSIM4lkt1l * Inv_L + model.BSIM4wkt1l * Inv_W + model.BSIM4pkt1l * Inv_LW;
                pParam.BSIM4k2 = model.BSIM4k2 + model.BSIM4lk2 * Inv_L + model.BSIM4wk2 * Inv_W + model.BSIM4pk2 * Inv_LW;
                pParam.BSIM4kt2 = model.BSIM4kt2 + model.BSIM4lkt2 * Inv_L + model.BSIM4wkt2 * Inv_W + model.BSIM4pkt2 * Inv_LW;
                pParam.BSIM4k3 = model.BSIM4k3 + model.BSIM4lk3 * Inv_L + model.BSIM4wk3 * Inv_W + model.BSIM4pk3 * Inv_LW;
                pParam.BSIM4k3b = model.BSIM4k3b + model.BSIM4lk3b * Inv_L + model.BSIM4wk3b * Inv_W + model.BSIM4pk3b * Inv_LW;
                pParam.BSIM4w0 = model.BSIM4w0 + model.BSIM4lw0 * Inv_L + model.BSIM4ww0 * Inv_W + model.BSIM4pw0 * Inv_LW;
                pParam.BSIM4lpe0 = model.BSIM4lpe0 + model.BSIM4llpe0 * Inv_L + model.BSIM4wlpe0 * Inv_W + model.BSIM4plpe0 * Inv_LW;
                pParam.BSIM4lpeb = model.BSIM4lpeb + model.BSIM4llpeb * Inv_L + model.BSIM4wlpeb * Inv_W + model.BSIM4plpeb * Inv_LW;
                pParam.BSIM4dvtp0 = model.BSIM4dvtp0 + model.BSIM4ldvtp0 * Inv_L + model.BSIM4wdvtp0 * Inv_W + model.BSIM4pdvtp0 * Inv_LW;
                pParam.BSIM4dvtp1 = model.BSIM4dvtp1 + model.BSIM4ldvtp1 * Inv_L + model.BSIM4wdvtp1 * Inv_W + model.BSIM4pdvtp1 * Inv_LW;
                pParam.BSIM4dvtp2 = model.BSIM4dvtp2 /* v4.7 */  + model.BSIM4ldvtp2 * Inv_L + model.BSIM4wdvtp2 * Inv_W + model.BSIM4pdvtp2 *
                  Inv_LW;
                pParam.BSIM4dvtp3 = model.BSIM4dvtp3 /* v4.7 */  + model.BSIM4ldvtp3 * Inv_L + model.BSIM4wdvtp3 * Inv_W + model.BSIM4pdvtp3 *
                  Inv_LW;
                pParam.BSIM4dvtp4 = model.BSIM4dvtp4 /* v4.7 */  + model.BSIM4ldvtp4 * Inv_L + model.BSIM4wdvtp4 * Inv_W + model.BSIM4pdvtp4 *
                  Inv_LW;
                pParam.BSIM4dvtp5 = model.BSIM4dvtp5 /* v4.7 */  + model.BSIM4ldvtp5 * Inv_L + model.BSIM4wdvtp5 * Inv_W + model.BSIM4pdvtp5 *
                  Inv_LW;
                pParam.BSIM4dvt0 = model.BSIM4dvt0 + model.BSIM4ldvt0 * Inv_L + model.BSIM4wdvt0 * Inv_W + model.BSIM4pdvt0 * Inv_LW;
                pParam.BSIM4dvt1 = model.BSIM4dvt1 + model.BSIM4ldvt1 * Inv_L + model.BSIM4wdvt1 * Inv_W + model.BSIM4pdvt1 * Inv_LW;
                pParam.BSIM4dvt2 = model.BSIM4dvt2 + model.BSIM4ldvt2 * Inv_L + model.BSIM4wdvt2 * Inv_W + model.BSIM4pdvt2 * Inv_LW;
                pParam.BSIM4dvt0w = model.BSIM4dvt0w + model.BSIM4ldvt0w * Inv_L + model.BSIM4wdvt0w * Inv_W + model.BSIM4pdvt0w * Inv_LW;
                pParam.BSIM4dvt1w = model.BSIM4dvt1w + model.BSIM4ldvt1w * Inv_L + model.BSIM4wdvt1w * Inv_W + model.BSIM4pdvt1w * Inv_LW;
                pParam.BSIM4dvt2w = model.BSIM4dvt2w + model.BSIM4ldvt2w * Inv_L + model.BSIM4wdvt2w * Inv_W + model.BSIM4pdvt2w * Inv_LW;
                pParam.BSIM4drout = model.BSIM4drout + model.BSIM4ldrout * Inv_L + model.BSIM4wdrout * Inv_W + model.BSIM4pdrout * Inv_LW;
                pParam.BSIM4dsub = model.BSIM4dsub + model.BSIM4ldsub * Inv_L + model.BSIM4wdsub * Inv_W + model.BSIM4pdsub * Inv_LW;
                pParam.BSIM4vth0 = model.BSIM4vth0 + model.BSIM4lvth0 * Inv_L + model.BSIM4wvth0 * Inv_W + model.BSIM4pvth0 * Inv_LW;
                pParam.BSIM4ua = model.BSIM4ua + model.BSIM4lua * Inv_L + model.BSIM4wua * Inv_W + model.BSIM4pua * Inv_LW;
                pParam.BSIM4ua1 = model.BSIM4ua1 + model.BSIM4lua1 * Inv_L + model.BSIM4wua1 * Inv_W + model.BSIM4pua1 * Inv_LW;
                pParam.BSIM4ub = model.BSIM4ub + model.BSIM4lub * Inv_L + model.BSIM4wub * Inv_W + model.BSIM4pub * Inv_LW;
                pParam.BSIM4ub1 = model.BSIM4ub1 + model.BSIM4lub1 * Inv_L + model.BSIM4wub1 * Inv_W + model.BSIM4pub1 * Inv_LW;
                pParam.BSIM4uc = model.BSIM4uc + model.BSIM4luc * Inv_L + model.BSIM4wuc * Inv_W + model.BSIM4puc * Inv_LW;
                pParam.BSIM4uc1 = model.BSIM4uc1 + model.BSIM4luc1 * Inv_L + model.BSIM4wuc1 * Inv_W + model.BSIM4puc1 * Inv_LW;
                pParam.BSIM4ud = model.BSIM4ud + model.BSIM4lud * Inv_L + model.BSIM4wud * Inv_W + model.BSIM4pud * Inv_LW;
                pParam.BSIM4ud1 = model.BSIM4ud1 + model.BSIM4lud1 * Inv_L + model.BSIM4wud1 * Inv_W + model.BSIM4pud1 * Inv_LW;
                pParam.BSIM4up = model.BSIM4up + model.BSIM4lup * Inv_L + model.BSIM4wup * Inv_W + model.BSIM4pup * Inv_LW;
                pParam.BSIM4lp = model.BSIM4lp + model.BSIM4llp * Inv_L + model.BSIM4wlp * Inv_W + model.BSIM4plp * Inv_LW;
                pParam.BSIM4eu = model.BSIM4eu + model.BSIM4leu * Inv_L + model.BSIM4weu * Inv_W + model.BSIM4peu * Inv_LW;
                pParam.BSIM4u0 = model.BSIM4u0 + model.BSIM4lu0 * Inv_L + model.BSIM4wu0 * Inv_W + model.BSIM4pu0 * Inv_LW;
                pParam.BSIM4ute = model.BSIM4ute + model.BSIM4lute * Inv_L + model.BSIM4wute * Inv_W + model.BSIM4pute * Inv_LW;
                /* high k mobility */
                pParam.BSIM4ucs = model.BSIM4ucs + model.BSIM4lucs * Inv_L + model.BSIM4wucs * Inv_W + model.BSIM4pucs * Inv_LW;
                pParam.BSIM4ucste = model.BSIM4ucste + model.BSIM4lucste * Inv_L + model.BSIM4wucste * Inv_W + model.BSIM4pucste * Inv_LW;

                pParam.BSIM4voff = model.BSIM4voff + model.BSIM4lvoff * Inv_L + model.BSIM4wvoff * Inv_W + model.BSIM4pvoff * Inv_LW;
                pParam.BSIM4tvoff = model.BSIM4tvoff + model.BSIM4ltvoff * Inv_L + model.BSIM4wtvoff * Inv_W + model.BSIM4ptvoff * Inv_LW;
                pParam.BSIM4minv = model.BSIM4minv + model.BSIM4lminv * Inv_L + model.BSIM4wminv * Inv_W + model.BSIM4pminv * Inv_LW;
                pParam.BSIM4minvcv = model.BSIM4minvcv + model.BSIM4lminvcv * Inv_L + model.BSIM4wminvcv * Inv_W + model.BSIM4pminvcv * Inv_LW;
                pParam.BSIM4fprout = model.BSIM4fprout + model.BSIM4lfprout * Inv_L + model.BSIM4wfprout * Inv_W + model.BSIM4pfprout * Inv_LW;
                pParam.BSIM4pdits = model.BSIM4pdits + model.BSIM4lpdits * Inv_L + model.BSIM4wpdits * Inv_W + model.BSIM4ppdits * Inv_LW;
                pParam.BSIM4pditsd = model.BSIM4pditsd + model.BSIM4lpditsd * Inv_L + model.BSIM4wpditsd * Inv_W + model.BSIM4ppditsd * Inv_LW;
                pParam.BSIM4delta = model.BSIM4delta + model.BSIM4ldelta * Inv_L + model.BSIM4wdelta * Inv_W + model.BSIM4pdelta * Inv_LW;
                pParam.BSIM4rdsw = model.BSIM4rdsw + model.BSIM4lrdsw * Inv_L + model.BSIM4wrdsw * Inv_W + model.BSIM4prdsw * Inv_LW;
                pParam.BSIM4rdw = model.BSIM4rdw + model.BSIM4lrdw * Inv_L + model.BSIM4wrdw * Inv_W + model.BSIM4prdw * Inv_LW;
                pParam.BSIM4rsw = model.BSIM4rsw + model.BSIM4lrsw * Inv_L + model.BSIM4wrsw * Inv_W + model.BSIM4prsw * Inv_LW;
                pParam.BSIM4prwg = model.BSIM4prwg + model.BSIM4lprwg * Inv_L + model.BSIM4wprwg * Inv_W + model.BSIM4pprwg * Inv_LW;
                pParam.BSIM4prwb = model.BSIM4prwb + model.BSIM4lprwb * Inv_L + model.BSIM4wprwb * Inv_W + model.BSIM4pprwb * Inv_LW;
                pParam.BSIM4prt = model.BSIM4prt + model.BSIM4lprt * Inv_L + model.BSIM4wprt * Inv_W + model.BSIM4pprt * Inv_LW;
                pParam.BSIM4eta0 = model.BSIM4eta0 + model.BSIM4leta0 * Inv_L + model.BSIM4weta0 * Inv_W + model.BSIM4peta0 * Inv_LW;
                pParam.BSIM4teta0 = model.BSIM4teta0 /* v4.7 */  + model.BSIM4lteta0 * Inv_L + model.BSIM4wteta0 * Inv_W + model.BSIM4pteta0 *
                  Inv_LW;
                pParam.BSIM4tvoffcv = model.BSIM4tvoffcv /* v4.8.0 */  + model.BSIM4ltvoffcv * Inv_L + model.BSIM4wtvoffcv * Inv_W +
                                    model.BSIM4ptvoffcv * Inv_LW;
                pParam.BSIM4etab = model.BSIM4etab + model.BSIM4letab * Inv_L + model.BSIM4wetab * Inv_W + model.BSIM4petab * Inv_LW;
                pParam.BSIM4pclm = model.BSIM4pclm + model.BSIM4lpclm * Inv_L + model.BSIM4wpclm * Inv_W + model.BSIM4ppclm * Inv_LW;
                pParam.BSIM4pdibl1 = model.BSIM4pdibl1 + model.BSIM4lpdibl1 * Inv_L + model.BSIM4wpdibl1 * Inv_W + model.BSIM4ppdibl1 * Inv_LW;
                pParam.BSIM4pdibl2 = model.BSIM4pdibl2 + model.BSIM4lpdibl2 * Inv_L + model.BSIM4wpdibl2 * Inv_W + model.BSIM4ppdibl2 * Inv_LW;
                pParam.BSIM4pdiblb = model.BSIM4pdiblb + model.BSIM4lpdiblb * Inv_L + model.BSIM4wpdiblb * Inv_W + model.BSIM4ppdiblb * Inv_LW;
                pParam.BSIM4pscbe1 = model.BSIM4pscbe1 + model.BSIM4lpscbe1 * Inv_L + model.BSIM4wpscbe1 * Inv_W + model.BSIM4ppscbe1 * Inv_LW;
                pParam.BSIM4pscbe2 = model.BSIM4pscbe2 + model.BSIM4lpscbe2 * Inv_L + model.BSIM4wpscbe2 * Inv_W + model.BSIM4ppscbe2 * Inv_LW;
                pParam.BSIM4pvag = model.BSIM4pvag + model.BSIM4lpvag * Inv_L + model.BSIM4wpvag * Inv_W + model.BSIM4ppvag * Inv_LW;
                pParam.BSIM4wr = model.BSIM4wr + model.BSIM4lwr * Inv_L + model.BSIM4wwr * Inv_W + model.BSIM4pwr * Inv_LW;
                pParam.BSIM4dwg = model.BSIM4dwg + model.BSIM4ldwg * Inv_L + model.BSIM4wdwg * Inv_W + model.BSIM4pdwg * Inv_LW;
                pParam.BSIM4dwb = model.BSIM4dwb + model.BSIM4ldwb * Inv_L + model.BSIM4wdwb * Inv_W + model.BSIM4pdwb * Inv_LW;
                pParam.BSIM4b0 = model.BSIM4b0 + model.BSIM4lb0 * Inv_L + model.BSIM4wb0 * Inv_W + model.BSIM4pb0 * Inv_LW;
                pParam.BSIM4b1 = model.BSIM4b1 + model.BSIM4lb1 * Inv_L + model.BSIM4wb1 * Inv_W + model.BSIM4pb1 * Inv_LW;
                pParam.BSIM4alpha0 = model.BSIM4alpha0 + model.BSIM4lalpha0 * Inv_L + model.BSIM4walpha0 * Inv_W + model.BSIM4palpha0 * Inv_LW;
                pParam.BSIM4alpha1 = model.BSIM4alpha1 + model.BSIM4lalpha1 * Inv_L + model.BSIM4walpha1 * Inv_W + model.BSIM4palpha1 * Inv_LW;
                pParam.BSIM4beta0 = model.BSIM4beta0 + model.BSIM4lbeta0 * Inv_L + model.BSIM4wbeta0 * Inv_W + model.BSIM4pbeta0 * Inv_LW;
                pParam.BSIM4agidl = model.BSIM4agidl + model.BSIM4lagidl * Inv_L + model.BSIM4wagidl * Inv_W + model.BSIM4pagidl * Inv_LW;
                pParam.BSIM4bgidl = model.BSIM4bgidl + model.BSIM4lbgidl * Inv_L + model.BSIM4wbgidl * Inv_W + model.BSIM4pbgidl * Inv_LW;
                pParam.BSIM4cgidl = model.BSIM4cgidl + model.BSIM4lcgidl * Inv_L + model.BSIM4wcgidl * Inv_W + model.BSIM4pcgidl * Inv_LW;
                pParam.BSIM4egidl = model.BSIM4egidl + model.BSIM4legidl * Inv_L + model.BSIM4wegidl * Inv_W + model.BSIM4pegidl * Inv_LW;
                pParam.BSIM4rgidl = model.BSIM4rgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lrgidl * Inv_L + model.BSIM4wrgidl * Inv_W +
                                    model.BSIM4prgidl * Inv_LW;
                pParam.BSIM4kgidl = model.BSIM4kgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lkgidl * Inv_L + model.BSIM4wkgidl * Inv_W +
                                    model.BSIM4pkgidl * Inv_LW;
                pParam.BSIM4fgidl = model.BSIM4fgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lfgidl * Inv_L + model.BSIM4wfgidl * Inv_W +
                                    model.BSIM4pfgidl * Inv_LW;
                pParam.BSIM4agisl = model.BSIM4agisl + model.BSIM4lagisl * Inv_L + model.BSIM4wagisl * Inv_W + model.BSIM4pagisl * Inv_LW;
                pParam.BSIM4bgisl = model.BSIM4bgisl + model.BSIM4lbgisl * Inv_L + model.BSIM4wbgisl * Inv_W + model.BSIM4pbgisl * Inv_LW;
                pParam.BSIM4cgisl = model.BSIM4cgisl + model.BSIM4lcgisl * Inv_L + model.BSIM4wcgisl * Inv_W + model.BSIM4pcgisl * Inv_LW;
                pParam.BSIM4egisl = model.BSIM4egisl + model.BSIM4legisl * Inv_L + model.BSIM4wegisl * Inv_W + model.BSIM4pegisl * Inv_LW;
                pParam.BSIM4rgisl = model.BSIM4rgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lrgisl * Inv_L + model.BSIM4wrgisl * Inv_W +
                                    model.BSIM4prgisl * Inv_LW;
                pParam.BSIM4kgisl = model.BSIM4kgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lkgisl * Inv_L + model.BSIM4wkgisl * Inv_W +
                                    model.BSIM4pkgisl * Inv_LW;
                pParam.BSIM4fgisl = model.BSIM4fgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lfgisl * Inv_L + model.BSIM4wfgisl * Inv_W +
                                    model.BSIM4pfgisl * Inv_LW;
                pParam.BSIM4aigc = model.BSIM4aigc + model.BSIM4laigc * Inv_L + model.BSIM4waigc * Inv_W + model.BSIM4paigc * Inv_LW;
                pParam.BSIM4bigc = model.BSIM4bigc + model.BSIM4lbigc * Inv_L + model.BSIM4wbigc * Inv_W + model.BSIM4pbigc * Inv_LW;
                pParam.BSIM4cigc = model.BSIM4cigc + model.BSIM4lcigc * Inv_L + model.BSIM4wcigc * Inv_W + model.BSIM4pcigc * Inv_LW;
                pParam.BSIM4aigsd = model.BSIM4aigsd + model.BSIM4laigsd * Inv_L + model.BSIM4waigsd * Inv_W + model.BSIM4paigsd * Inv_LW;
                pParam.BSIM4bigsd = model.BSIM4bigsd + model.BSIM4lbigsd * Inv_L + model.BSIM4wbigsd * Inv_W + model.BSIM4pbigsd * Inv_LW;
                pParam.BSIM4cigsd = model.BSIM4cigsd + model.BSIM4lcigsd * Inv_L + model.BSIM4wcigsd * Inv_W + model.BSIM4pcigsd * Inv_LW;
                pParam.BSIM4aigs = model.BSIM4aigs + model.BSIM4laigs * Inv_L + model.BSIM4waigs * Inv_W + model.BSIM4paigs * Inv_LW;
                pParam.BSIM4bigs = model.BSIM4bigs + model.BSIM4lbigs * Inv_L + model.BSIM4wbigs * Inv_W + model.BSIM4pbigs * Inv_LW;
                pParam.BSIM4cigs = model.BSIM4cigs + model.BSIM4lcigs * Inv_L + model.BSIM4wcigs * Inv_W + model.BSIM4pcigs * Inv_LW;
                pParam.BSIM4aigd = model.BSIM4aigd + model.BSIM4laigd * Inv_L + model.BSIM4waigd * Inv_W + model.BSIM4paigd * Inv_LW;
                pParam.BSIM4bigd = model.BSIM4bigd + model.BSIM4lbigd * Inv_L + model.BSIM4wbigd * Inv_W + model.BSIM4pbigd * Inv_LW;
                pParam.BSIM4cigd = model.BSIM4cigd + model.BSIM4lcigd * Inv_L + model.BSIM4wcigd * Inv_W + model.BSIM4pcigd * Inv_LW;
                pParam.BSIM4aigbacc = model.BSIM4aigbacc + model.BSIM4laigbacc * Inv_L + model.BSIM4waigbacc * Inv_W + model.BSIM4paigbacc *
                  Inv_LW;
                pParam.BSIM4bigbacc = model.BSIM4bigbacc + model.BSIM4lbigbacc * Inv_L + model.BSIM4wbigbacc * Inv_W + model.BSIM4pbigbacc *
                  Inv_LW;
                pParam.BSIM4cigbacc = model.BSIM4cigbacc + model.BSIM4lcigbacc * Inv_L + model.BSIM4wcigbacc * Inv_W + model.BSIM4pcigbacc *
                  Inv_LW;
                pParam.BSIM4aigbinv = model.BSIM4aigbinv + model.BSIM4laigbinv * Inv_L + model.BSIM4waigbinv * Inv_W + model.BSIM4paigbinv *
                  Inv_LW;
                pParam.BSIM4bigbinv = model.BSIM4bigbinv + model.BSIM4lbigbinv * Inv_L + model.BSIM4wbigbinv * Inv_W + model.BSIM4pbigbinv *
                  Inv_LW;
                pParam.BSIM4cigbinv = model.BSIM4cigbinv + model.BSIM4lcigbinv * Inv_L + model.BSIM4wcigbinv * Inv_W + model.BSIM4pcigbinv *
                  Inv_LW;
                pParam.BSIM4nigc = model.BSIM4nigc + model.BSIM4lnigc * Inv_L + model.BSIM4wnigc * Inv_W + model.BSIM4pnigc * Inv_LW;
                pParam.BSIM4nigbacc = model.BSIM4nigbacc + model.BSIM4lnigbacc * Inv_L + model.BSIM4wnigbacc * Inv_W + model.BSIM4pnigbacc *
                  Inv_LW;
                pParam.BSIM4nigbinv = model.BSIM4nigbinv + model.BSIM4lnigbinv * Inv_L + model.BSIM4wnigbinv * Inv_W + model.BSIM4pnigbinv *
                  Inv_LW;
                pParam.BSIM4ntox = model.BSIM4ntox + model.BSIM4lntox * Inv_L + model.BSIM4wntox * Inv_W + model.BSIM4pntox * Inv_LW;
                pParam.BSIM4eigbinv = model.BSIM4eigbinv + model.BSIM4leigbinv * Inv_L + model.BSIM4weigbinv * Inv_W + model.BSIM4peigbinv *
                  Inv_LW;
                pParam.BSIM4pigcd = model.BSIM4pigcd + model.BSIM4lpigcd * Inv_L + model.BSIM4wpigcd * Inv_W + model.BSIM4ppigcd * Inv_LW;
                pParam.BSIM4poxedge = model.BSIM4poxedge + model.BSIM4lpoxedge * Inv_L + model.BSIM4wpoxedge * Inv_W + model.BSIM4ppoxedge *
                  Inv_LW;
                pParam.BSIM4xrcrg1 = model.BSIM4xrcrg1 + model.BSIM4lxrcrg1 * Inv_L + model.BSIM4wxrcrg1 * Inv_W + model.BSIM4pxrcrg1 * Inv_LW;
                pParam.BSIM4xrcrg2 = model.BSIM4xrcrg2 + model.BSIM4lxrcrg2 * Inv_L + model.BSIM4wxrcrg2 * Inv_W + model.BSIM4pxrcrg2 * Inv_LW;
                pParam.BSIM4lambda = model.BSIM4lambda + model.BSIM4llambda * Inv_L + model.BSIM4wlambda * Inv_W + model.BSIM4plambda * Inv_LW;
                pParam.BSIM4vtl = model.BSIM4vtl + model.BSIM4lvtl * Inv_L + model.BSIM4wvtl * Inv_W + model.BSIM4pvtl * Inv_LW;
                pParam.BSIM4xn = model.BSIM4xn + model.BSIM4lxn * Inv_L + model.BSIM4wxn * Inv_W + model.BSIM4pxn * Inv_LW;
                pParam.BSIM4vfbsdoff = model.BSIM4vfbsdoff + model.BSIM4lvfbsdoff * Inv_L + model.BSIM4wvfbsdoff * Inv_W +
                                    model.BSIM4pvfbsdoff * Inv_LW;
                pParam.BSIM4tvfbsdoff = model.BSIM4tvfbsdoff + model.BSIM4ltvfbsdoff * Inv_L + model.BSIM4wtvfbsdoff * Inv_W +
                                    model.BSIM4ptvfbsdoff * Inv_LW;

                pParam.BSIM4cgsl = model.BSIM4cgsl + model.BSIM4lcgsl * Inv_L + model.BSIM4wcgsl * Inv_W + model.BSIM4pcgsl * Inv_LW;
                pParam.BSIM4cgdl = model.BSIM4cgdl + model.BSIM4lcgdl * Inv_L + model.BSIM4wcgdl * Inv_W + model.BSIM4pcgdl * Inv_LW;
                pParam.BSIM4ckappas = model.BSIM4ckappas + model.BSIM4lckappas * Inv_L + model.BSIM4wckappas * Inv_W + model.BSIM4pckappas *
                  Inv_LW;
                pParam.BSIM4ckappad = model.BSIM4ckappad + model.BSIM4lckappad * Inv_L + model.BSIM4wckappad * Inv_W + model.BSIM4pckappad *
                  Inv_LW;
                pParam.BSIM4cf = model.BSIM4cf + model.BSIM4lcf * Inv_L + model.BSIM4wcf * Inv_W + model.BSIM4pcf * Inv_LW;
                pParam.BSIM4clc = model.BSIM4clc + model.BSIM4lclc * Inv_L + model.BSIM4wclc * Inv_W + model.BSIM4pclc * Inv_LW;
                pParam.BSIM4cle = model.BSIM4cle + model.BSIM4lcle * Inv_L + model.BSIM4wcle * Inv_W + model.BSIM4pcle * Inv_LW;
                pParam.BSIM4vfbcv = model.BSIM4vfbcv + model.BSIM4lvfbcv * Inv_L + model.BSIM4wvfbcv * Inv_W + model.BSIM4pvfbcv * Inv_LW;
                pParam.BSIM4acde = model.BSIM4acde + model.BSIM4lacde * Inv_L + model.BSIM4wacde * Inv_W + model.BSIM4pacde * Inv_LW;
                pParam.BSIM4moin = model.BSIM4moin + model.BSIM4lmoin * Inv_L + model.BSIM4wmoin * Inv_W + model.BSIM4pmoin * Inv_LW;
                pParam.BSIM4noff = model.BSIM4noff + model.BSIM4lnoff * Inv_L + model.BSIM4wnoff * Inv_W + model.BSIM4pnoff * Inv_LW;
                pParam.BSIM4voffcv = model.BSIM4voffcv + model.BSIM4lvoffcv * Inv_L + model.BSIM4wvoffcv * Inv_W + model.BSIM4pvoffcv * Inv_LW;
                pParam.BSIM4kvth0we = model.BSIM4kvth0we + model.BSIM4lkvth0we * Inv_L + model.BSIM4wkvth0we * Inv_W + model.BSIM4pkvth0we *
                  Inv_LW;
                pParam.BSIM4k2we = model.BSIM4k2we + model.BSIM4lk2we * Inv_L + model.BSIM4wk2we * Inv_W + model.BSIM4pk2we * Inv_LW;
                pParam.BSIM4ku0we = model.BSIM4ku0we + model.BSIM4lku0we * Inv_L + model.BSIM4wku0we * Inv_W + model.BSIM4pku0we * Inv_LW;

                pParam.BSIM4abulkCVfactor = 1.0 + Math.Pow((pParam.BSIM4clc / pParam.BSIM4leffCV), pParam.BSIM4cle);

                T0 = (model.TRatio - 1.0);

                PowWeffWr = Math.Pow(pParam.BSIM4weffCJ * 1.0e6, pParam.BSIM4wr) * BSIM4nf;

                T1 = T2 = T3 = T4 = 0.0;
                pParam.BSIM4ucs = pParam.BSIM4ucs * Math.Pow(model.TRatio, pParam.BSIM4ucste);
                if (model.BSIM4tempMod.Value == 0)
                {
                    pParam.BSIM4ua = pParam.BSIM4ua + pParam.BSIM4ua1 * T0;
                    pParam.BSIM4ub = pParam.BSIM4ub + pParam.BSIM4ub1 * T0;
                    pParam.BSIM4uc = pParam.BSIM4uc + pParam.BSIM4uc1 * T0;
                    pParam.BSIM4ud = pParam.BSIM4ud + pParam.BSIM4ud1 * T0;
                    pParam.BSIM4vsattemp = pParam.BSIM4vsat - pParam.BSIM4at * T0;
                    T10 = pParam.BSIM4prt * T0;
                    if (model.BSIM4rdsMod != 0)
                    {
                        /* External Rd(V) */
                        T1 = pParam.BSIM4rdw + T10;
                        T2 = model.BSIM4rdwmin + T10;
                        /* External Rs(V) */
                        T3 = pParam.BSIM4rsw + T10;
                        T4 = model.BSIM4rswmin + T10;
                    }
                    /* Internal Rds(V) in IV */
                    pParam.BSIM4rds0 = (pParam.BSIM4rdsw + T10) * BSIM4nf / PowWeffWr;
                    pParam.BSIM4rdswmin = (model.BSIM4rdswmin + T10) * BSIM4nf / PowWeffWr;
                }
                else
                {
                    if (model.BSIM4tempMod.Value == 3)
                    {
                        pParam.BSIM4ua = pParam.BSIM4ua * Math.Pow(model.TRatio, pParam.BSIM4ua1);
                        pParam.BSIM4ub = pParam.BSIM4ub * Math.Pow(model.TRatio, pParam.BSIM4ub1);
                        pParam.BSIM4uc = pParam.BSIM4uc * Math.Pow(model.TRatio, pParam.BSIM4uc1);
                        pParam.BSIM4ud = pParam.BSIM4ud * Math.Pow(model.TRatio, pParam.BSIM4ud1);
                    }
                    else
                    {
                        /* tempMod = 1, 2 */
                        pParam.BSIM4ua = pParam.BSIM4ua * (1.0 + pParam.BSIM4ua1 * model.delTemp);
                        pParam.BSIM4ub = pParam.BSIM4ub * (1.0 + pParam.BSIM4ub1 * model.delTemp);
                        pParam.BSIM4uc = pParam.BSIM4uc * (1.0 + pParam.BSIM4uc1 * model.delTemp);
                        pParam.BSIM4ud = pParam.BSIM4ud * (1.0 + pParam.BSIM4ud1 * model.delTemp);
                    }
                    pParam.BSIM4vsattemp = pParam.BSIM4vsat * (1.0 - pParam.BSIM4at * model.delTemp);
                    T10 = 1.0 + pParam.BSIM4prt * model.delTemp;
                    if (model.BSIM4rdsMod != 0)
                    {
                        /* External Rd(V) */
                        T1 = pParam.BSIM4rdw * T10;
                        T2 = model.BSIM4rdwmin * T10;

                        /* External Rs(V) */
                        T3 = pParam.BSIM4rsw * T10;
                        T4 = model.BSIM4rswmin * T10;
                    }
                    /* Internal Rds(V) in IV */
                    pParam.BSIM4rds0 = pParam.BSIM4rdsw * T10 * BSIM4nf / PowWeffWr;
                    pParam.BSIM4rdswmin = model.BSIM4rdswmin * T10 * BSIM4nf / PowWeffWr;
                }

                if (T1 < 0.0)
                {
                    T1 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rdw at current temperature is negative; set to 0.");
                }
                if (T2 < 0.0)
                {
                    T2 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rdwmin at current temperature is negative; set to 0.");
                }
                pParam.BSIM4rd0 = T1 / PowWeffWr;
                pParam.BSIM4rdwmin = T2 / PowWeffWr;
                if (T3 < 0.0)
                {
                    T3 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rsw at current temperature is negative; set to 0.");
                }
                if (T4 < 0.0)
                {
                    T4 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rswmin at current temperature is negative; set to 0.");
                }
                pParam.BSIM4rs0 = T3 / PowWeffWr;
                pParam.BSIM4rswmin = T4 / PowWeffWr;

                if (pParam.BSIM4u0 > 1.0)
                    pParam.BSIM4u0 = pParam.BSIM4u0 / 1.0e4;

                /* mobility channel length dependence */
                T5 = 1.0 - pParam.BSIM4up * Math.Exp(-pParam.BSIM4leff / pParam.BSIM4lp);
                pParam.BSIM4u0temp = pParam.BSIM4u0 * T5 * Math.Pow(model.TRatio, pParam.BSIM4ute);
                if (pParam.BSIM4eu < 0.0)
                {
                    pParam.BSIM4eu = 0.0;

                    CircuitWarning.Warning(this, "Warning: eu has been negative; reset to 0.0.");
                }
                if (pParam.BSIM4ucs < 0.0)
                {
                    pParam.BSIM4ucs = 0.0;

                    CircuitWarning.Warning(this, "Warning: ucs has been negative; reset to 0.0.");
                }

                pParam.BSIM4vfbsdoff = pParam.BSIM4vfbsdoff * (1.0 + pParam.BSIM4tvfbsdoff * model.delTemp);
                pParam.BSIM4voff = pParam.BSIM4voff * (1.0 + pParam.BSIM4tvoff * model.delTemp);

                pParam.BSIM4nfactor = pParam.BSIM4nfactor + pParam.BSIM4tnfactor * model.delTemp / model.Tnom; /*
					v4.7 temp dep of leakage currents */
                pParam.BSIM4voffcv = pParam.BSIM4voffcv * (1.0 + pParam.BSIM4tvoffcv * model.delTemp); /* v4.7 temp dep of leakage currents */
                pParam.BSIM4eta0 = pParam.BSIM4eta0 + pParam.BSIM4teta0 * model.delTemp / model.Tnom; /* v4.7 temp dep of leakage currents */

                /* Source End Velocity Limit */
                if ((model.BSIM4vtl.Given) && (model.BSIM4vtl > 0.0))
                {
                    if (model.BSIM4lc < 0.0)

                        pParam.BSIM4lc = 0.0;
                    else pParam.BSIM4lc = model.BSIM4lc;
                    T0 = pParam.BSIM4leff / (pParam.BSIM4xn * pParam.BSIM4leff + pParam.BSIM4lc);
                    pParam.BSIM4tfactor = (1.0 - T0) / (1.0 + T0);
                }

                pParam.BSIM4cgdo = (model.BSIM4cgdo + pParam.BSIM4cf) * pParam.BSIM4weffCV;
                pParam.BSIM4cgso = (model.BSIM4cgso + pParam.BSIM4cf) * pParam.BSIM4weffCV;
                pParam.BSIM4cgbo = model.BSIM4cgbo * pParam.BSIM4leffCV * BSIM4nf;

                if (!model.BSIM4ndep.Given && model.BSIM4gamma1.Given)
                {
                    T0 = pParam.BSIM4gamma1 * model.BSIM4coxe;
                    pParam.BSIM4ndep = 3.01248e22 * T0 * T0;
                }

                pParam.BSIM4phi = model.Vtm0 * Math.Log(pParam.BSIM4ndep / model.ni) + pParam.BSIM4phin + 0.4;

                pParam.BSIM4sqrtPhi = Math.Sqrt(pParam.BSIM4phi);
                pParam.BSIM4phis3 = pParam.BSIM4sqrtPhi * pParam.BSIM4phi;

                pParam.BSIM4Xdep0 = Math.Sqrt(2.0 * model.epssub / (Transistor.Charge_q * pParam.BSIM4ndep * 1.0e6)) * pParam.BSIM4sqrtPhi;
                pParam.BSIM4sqrtXdep0 = Math.Sqrt(pParam.BSIM4Xdep0);

                if (model.BSIM4mtrlMod.Value == 0)
                    pParam.BSIM4litl = Math.Sqrt(3.0 * 3.9 / model.epsrox * pParam.BSIM4xj * model.toxe);
                else
                    pParam.BSIM4litl = Math.Sqrt(model.BSIM4epsrsub / model.epsrox * pParam.BSIM4xj * model.toxe);

                pParam.BSIM4vbi = model.Vtm0 * Math.Log(pParam.BSIM4nsd * pParam.BSIM4ndep / (model.ni * model.ni));

                if (model.BSIM4mtrlMod.Value == 0)
                {
                    if (pParam.BSIM4ngate > 0.0)
                    {
                        pParam.BSIM4vfbsd = model.Vtm0 * Math.Log(pParam.BSIM4ngate / pParam.BSIM4nsd);
                    }
                    else
                        pParam.BSIM4vfbsd = 0.0;
                }
                else
                {
                    T0 = model.Vtm0 * Math.Log(pParam.BSIM4nsd / model.ni);
                    T1 = 0.5 * model.Eg0;
                    if (T0 > T1)
                        T0 = T1;
                    T2 = model.BSIM4easub + T1 - model.BSIM4type * T0;
                    pParam.BSIM4vfbsd = model.BSIM4phig - T2;
                }

                pParam.BSIM4cdep0 = Math.Sqrt(Transistor.Charge_q * model.epssub * pParam.BSIM4ndep * 1.0e6 / 2.0 / pParam.BSIM4phi);

                pParam.BSIM4ToxRatio = Math.Exp(pParam.BSIM4ntox * Math.Log(model.BSIM4toxref / model.toxe)) / model.toxe / model.toxe;
                pParam.BSIM4ToxRatioEdge = Math.Exp(pParam.BSIM4ntox * Math.Log(model.BSIM4toxref / (model.toxe * pParam.BSIM4poxedge))) /
                    model.toxe / model.toxe / pParam.BSIM4poxedge / pParam.BSIM4poxedge;
                pParam.BSIM4Aechvb = (model.BSIM4type == NMOS) ? 4.97232e-7 : 3.42537e-7;
                pParam.BSIM4Bechvb = (model.BSIM4type == NMOS) ? 7.45669e11 : 1.16645e12;
                pParam.BSIM4AechvbEdgeS = pParam.BSIM4Aechvb * pParam.BSIM4weff * model.BSIM4dlcig * pParam.BSIM4ToxRatioEdge;
                pParam.BSIM4AechvbEdgeD = pParam.BSIM4Aechvb * pParam.BSIM4weff * model.BSIM4dlcigd * pParam.BSIM4ToxRatioEdge;
                pParam.BSIM4BechvbEdge = -pParam.BSIM4Bechvb * model.toxe * pParam.BSIM4poxedge;
                pParam.BSIM4Aechvb *= pParam.BSIM4weff * pParam.BSIM4leff * pParam.BSIM4ToxRatio;
                pParam.BSIM4Bechvb *= -model.toxe;

                pParam.BSIM4mstar = 0.5 + Math.Atan(pParam.BSIM4minv) / Circuit.CONSTPI;
                pParam.BSIM4mstarcv = 0.5 + Math.Atan(pParam.BSIM4minvcv) / Circuit.CONSTPI;
                pParam.BSIM4voffcbn = pParam.BSIM4voff + model.BSIM4voffl / pParam.BSIM4leff;
                pParam.BSIM4voffcbncv = pParam.BSIM4voffcv + model.BSIM4voffcvl / pParam.BSIM4leff;

                pParam.BSIM4ldeb = Math.Sqrt(model.epssub * model.Vtm0 / (Transistor.Charge_q * pParam.BSIM4ndep * 1.0e6)) / 3.0;
                pParam.BSIM4acde *= Math.Pow((pParam.BSIM4ndep / 2.0e16), -0.25);

                if (model.BSIM4k1.Given || model.BSIM4k2.Given)
                {
                    if (!model.BSIM4k1.Given)
                    {

                        CircuitWarning.Warning(this, "Warning: k1 should be specified with k2.");
                        pParam.BSIM4k1 = 0.53;
                    }
                    if (!model.BSIM4k2.Given)
                    {

                        CircuitWarning.Warning(this, "Warning: k2 should be specified with k1.");
                        pParam.BSIM4k2 = -0.0186;
                    }
                    if (model.BSIM4nsub.Given)
                        CircuitWarning.Warning(this, "Warning: nsub is ignored because k1 or k2 is given.");
                    if (model.BSIM4xt.Given)
                        CircuitWarning.Warning(this, "Warning: xt is ignored because k1 or k2 is given.");
                    if (model.BSIM4vbx.Given)
                        CircuitWarning.Warning(this, "Warning: vbx is ignored because k1 or k2 is given.");
                    if (model.BSIM4gamma1.Given)
                        CircuitWarning.Warning(this, "Warning: gamma1 is ignored because k1 or k2 is given.");
                    if (model.BSIM4gamma2.Given)
                        CircuitWarning.Warning(this, "Warning: gamma2 is ignored because k1 or k2 is given.");
                }
                else
                {
                    if (!model.BSIM4vbx.Given)
                        pParam.BSIM4vbx = pParam.BSIM4phi - 7.7348e-4 * pParam.BSIM4ndep * pParam.BSIM4xt * pParam.BSIM4xt;
                    if (pParam.BSIM4vbx > 0.0)
                        pParam.BSIM4vbx = -pParam.BSIM4vbx;
                    if (pParam.BSIM4vbm > 0.0)
                        pParam.BSIM4vbm = -pParam.BSIM4vbm;

                    if (!model.BSIM4gamma1.Given)
                        pParam.BSIM4gamma1 = 5.753e-12 * Math.Sqrt(pParam.BSIM4ndep) / model.BSIM4coxe;
                    if (!model.BSIM4gamma2.Given)
                        pParam.BSIM4gamma2 = 5.753e-12 * Math.Sqrt(pParam.BSIM4nsub) / model.BSIM4coxe;

                    T0 = pParam.BSIM4gamma1 - pParam.BSIM4gamma2;
                    T1 = Math.Sqrt(pParam.BSIM4phi - pParam.BSIM4vbx) - pParam.BSIM4sqrtPhi;
                    T2 = Math.Sqrt(pParam.BSIM4phi * (pParam.BSIM4phi - pParam.BSIM4vbm)) - pParam.BSIM4phi;
                    pParam.BSIM4k2 = T0 * T1 / (2.0 * T2 + pParam.BSIM4vbm);
                    pParam.BSIM4k1 = pParam.BSIM4gamma2 - 2.0 * pParam.BSIM4k2 * Math.Sqrt(pParam.BSIM4phi - pParam.BSIM4vbm);
                }

                if (!model.BSIM4vfb.Given)
                {
                    if (model.BSIM4vth0.Given)
                    {
                        pParam.BSIM4vfb = model.BSIM4type * pParam.BSIM4vth0 - pParam.BSIM4phi - pParam.BSIM4k1 * pParam.BSIM4sqrtPhi;
                    }
                    else
                    {
                        if ((model.BSIM4mtrlMod != 0) && (model.BSIM4phig.Given) && (model.BSIM4nsub.Given))
                        {
                            T0 = model.Vtm0 * Math.Log(pParam.BSIM4nsub / model.ni);
                            T1 = 0.5 * model.Eg0;
                            if (T0 > T1)
                                T0 = T1;
                            T2 = model.BSIM4easub + T1 + model.BSIM4type * T0;
                            pParam.BSIM4vfb = model.BSIM4phig - T2;
                        }
                        else
                        {
                            pParam.BSIM4vfb = -1.0;
                        }
                    }
                }
                if (!model.BSIM4vth0.Given)
                {
                    pParam.BSIM4vth0 = model.BSIM4type * (pParam.BSIM4vfb + pParam.BSIM4phi + pParam.BSIM4k1 * pParam.BSIM4sqrtPhi);
                }

                pParam.BSIM4k1ox = pParam.BSIM4k1 * model.toxe / model.BSIM4toxm;

                tmp = Math.Sqrt(model.epssub / (model.epsrox * Transistor.EPS0) * model.toxe * pParam.BSIM4Xdep0);
                T0 = pParam.BSIM4dsub * pParam.BSIM4leff / tmp;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    pParam.BSIM4theta0vb0 = T1 / T4;
                }
                else
                    pParam.BSIM4theta0vb0 = 1.0 / (Transistor.MAX_EXP - 2.0);

                T0 = pParam.BSIM4drout * pParam.BSIM4leff / tmp;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T5 = T1 / T4;
                }
                else
                    T5 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                pParam.BSIM4thetaRout = pParam.BSIM4pdibl1 * T5 + pParam.BSIM4pdibl2;

                tmp = Math.Sqrt(pParam.BSIM4Xdep0);
                tmp1 = pParam.BSIM4vbi - pParam.BSIM4phi;
                tmp2 = model.BSIM4factor1 * tmp;

                T0 = pParam.BSIM4dvt1w * pParam.BSIM4weff * pParam.BSIM4leff / tmp2;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T8 = T1 / T4;
                }
                else
                    T8 = 1.0 / (Transistor.MAX_EXP - 2.0);
                T0 = pParam.BSIM4dvt0w * T8;
                T8 = T0 * tmp1;

                T0 = pParam.BSIM4dvt1 * pParam.BSIM4leff / tmp2;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T9 = T1 / T4;
                }
                else
                    T9 = 1.0 / (Transistor.MAX_EXP - 2.0);
                T9 = pParam.BSIM4dvt0 * T9 * tmp1;

                T4 = model.toxe * pParam.BSIM4phi / (pParam.BSIM4weff + pParam.BSIM4w0);

                T0 = Math.Sqrt(1.0 + pParam.BSIM4lpe0 / pParam.BSIM4leff);
                if ((model.BSIM4tempMod.Value == 1) || (model.BSIM4tempMod.Value == 0))
                    T3 = (pParam.BSIM4kt1 + pParam.BSIM4kt1l / pParam.BSIM4leff) * (model.TRatio - 1.0);
                if ((model.BSIM4tempMod.Value == 2) || (model.BSIM4tempMod.Value == 3))
                    T3 = -pParam.BSIM4kt1 * (model.TRatio - 1.0);

                T5 = pParam.BSIM4k1ox * (T0 - 1.0) * pParam.BSIM4sqrtPhi + T3;
                pParam.BSIM4vfbzbfactor = -T8 - T9 + pParam.BSIM4k3 * T4 + T5 - pParam.BSIM4phi - pParam.BSIM4k1 * pParam.BSIM4sqrtPhi;

                /* stress effect */

                wlod = model.BSIM4wlod;
                if (model.BSIM4wlod < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: WLOD = {model.BSIM4wlod} is less than 0. 0.0 is used");
                    wlod = 0.0;
                }
                T0 = Math.Pow(Lnew, model.BSIM4llodku0);
                W_tmp = Wnew + wlod;
                T1 = Math.Pow(W_tmp, model.BSIM4wlodku0);
                tmp1 = model.BSIM4lku0 / T0 + model.BSIM4wku0 / T1 + model.BSIM4pku0 / (T0 * T1);
                pParam.BSIM4ku0 = 1.0 + tmp1;

                T0 = Math.Pow(Lnew, model.BSIM4llodvth);
                T1 = Math.Pow(W_tmp, model.BSIM4wlodvth);
                tmp1 = model.BSIM4lkvth0 / T0 + model.BSIM4wkvth0 / T1 + model.BSIM4pkvth0 / (T0 * T1);
                pParam.BSIM4kvth0 = 1.0 + tmp1;
                pParam.BSIM4kvth0 = Math.Sqrt(pParam.BSIM4kvth0 * pParam.BSIM4kvth0 + Transistor.DELTA);

                T0 = (model.TRatio - 1.0);
                pParam.BSIM4ku0temp = pParam.BSIM4ku0 * (1.0 + model.BSIM4tku0 * T0) + Transistor.DELTA;

                Inv_saref = 1.0 / (model.BSIM4saref + 0.5 * Ldrn);
                Inv_sbref = 1.0 / (model.BSIM4sbref + 0.5 * Ldrn);
                pParam.BSIM4inv_od_ref = Inv_saref + Inv_sbref;
                pParam.BSIM4rho_ref = model.BSIM4ku0 / pParam.BSIM4ku0temp * pParam.BSIM4inv_od_ref;

                /* high k */
                /* Calculate VgsteffVth for mobMod = 3 */
                if (model.BSIM4mobMod.Value == 3)
                {
                    /* Calculate n @ Vbs = Vds = 0 */
                    lt1 = model.BSIM4factor1 * pParam.BSIM4sqrtXdep0;
                    T0 = pParam.BSIM4dvt1 * pParam.BSIM4leff / lt1;
                    if (T0 < Transistor.EXP_THRESHOLD)
                    {
                        T1 = Math.Exp(T0);
                        T2 = T1 - 1.0;
                        T3 = T2 * T2;
                        T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                        Theta0 = T1 / T4;
                    }
                    else
                        Theta0 = 1.0 / (Transistor.MAX_EXP - 2.0);

                    tmp1 = model.epssub / pParam.BSIM4Xdep0;
                    tmp2 = pParam.BSIM4nfactor * tmp1;
                    tmp3 = (tmp2 + pParam.BSIM4cdsc * Theta0 + pParam.BSIM4cit) / model.BSIM4coxe;
                    if (tmp3 >= -0.5)
                        n0 = 1.0 + tmp3;
                    else
                    {
                        T0 = 1.0 / (3.0 + 8.0 * tmp3);
                        n0 = (1.0 + 3.0 * tmp3) * T0;
                    }

                    T0 = n0 * model.BSIM4vtm;
                    T1 = pParam.BSIM4voffcbn;
                    T2 = T1 / T0;
                    if (T2 < -Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MIN_EXP / pParam.BSIM4cdep0;
                        T4 = pParam.BSIM4mstar + T3 * n0;
                    }
                    else if (T2 > Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MAX_EXP / pParam.BSIM4cdep0;
                        T4 = pParam.BSIM4mstar + T3 * n0;
                    }
                    else
                    {
                        T3 = Math.Exp(T2) * model.BSIM4coxe / pParam.BSIM4cdep0;
                        T4 = pParam.BSIM4mstar + T3 * n0;
                    }
                    pParam.BSIM4VgsteffVth = T0 * Math.Log(2.0) / T4;
                }

                /* New DITS term added in 4.7 */
                T0 = -pParam.BSIM4dvtp3 * Math.Log(pParam.BSIM4leff);
                T1 = Dexp(T0);
                pParam.BSIM4dvtp2factor = pParam.BSIM4dvtp5 + pParam.BSIM4dvtp2 * T1;

            } /* End of SizeNotFound */

            /* stress effect */
            if ((BSIM4sa > 0.0) && (BSIM4sb > 0.0) && ((BSIM4nf.Value == 1.0) || ((BSIM4nf > 1.0) && (BSIM4sd > 0.0))))
            {
                Inv_sa = 0;
                Inv_sb = 0;

                kvsat = model.BSIM4kvsat;
                if (model.BSIM4kvsat < -1.0)
                {

                    CircuitWarning.Warning(this, $"Warning: KVSAT = {model.BSIM4kvsat} is too small; - 1.0 is used.");
                    kvsat = -1.0;
                }
                if (model.BSIM4kvsat > 1.0)
                {

                    CircuitWarning.Warning(this, $"Warning: KVSAT = {model.BSIM4kvsat} is too big; 1.0 is used.");
                    kvsat = 1.0;
                }

                for (i = 0; i < BSIM4nf; i++)
                {
                    T0 = 1.0 / BSIM4nf / (BSIM4sa + 0.5 * Ldrn + i * (BSIM4sd + Ldrn));
                    T1 = 1.0 / BSIM4nf / (BSIM4sb + 0.5 * Ldrn + i * (BSIM4sd + Ldrn));
                    Inv_sa += T0;
                    Inv_sb += T1;
                }
                Inv_ODeff = Inv_sa + Inv_sb;
                rho = model.BSIM4ku0 / pParam.BSIM4ku0temp * Inv_ODeff;
                T0 = (1.0 + rho) / (1.0 + pParam.BSIM4rho_ref);
                BSIM4u0temp = pParam.BSIM4u0temp * T0;

                T1 = (1.0 + kvsat * rho) / (1.0 + kvsat * pParam.BSIM4rho_ref);
                BSIM4vsattemp = pParam.BSIM4vsattemp * T1;

                OD_offset = Inv_ODeff - pParam.BSIM4inv_od_ref;
                dvth0_lod = model.BSIM4kvth0 / pParam.BSIM4kvth0 * OD_offset;
                dk2_lod = model.BSIM4stk2 / Math.Pow(pParam.BSIM4kvth0, model.BSIM4lodk2) * OD_offset;
                deta0_lod = model.BSIM4steta0 / Math.Pow(pParam.BSIM4kvth0, model.BSIM4lodeta0) * OD_offset;
                BSIM4vth0 = pParam.BSIM4vth0 + dvth0_lod;

                BSIM4eta0 = pParam.BSIM4eta0 + deta0_lod;
                BSIM4k2 = pParam.BSIM4k2 + dk2_lod;
            }
            else
            {
                BSIM4u0temp = pParam.BSIM4u0temp;
                BSIM4vth0 = pParam.BSIM4vth0;
                BSIM4vsattemp = pParam.BSIM4vsattemp;
                BSIM4eta0 = pParam.BSIM4eta0;
                BSIM4k2 = pParam.BSIM4k2;
            }

            /* Well Proximity Effect */
            if (model.BSIM4wpemod != 0)
            {
                if ((!BSIM4sca.Given) && (!BSIM4scb.Given) && (!BSIM4scc.Given))
                {
                    if ((BSIM4sc.Given) && (BSIM4sc > 0.0))
                    {
                        T1 = BSIM4sc + Wdrn;
                        T2 = 1.0 / model.BSIM4scref;
                        BSIM4sca.Value = model.BSIM4scref * model.BSIM4scref / (BSIM4sc * T1);
                        BSIM4scb.Value = ((0.1 * BSIM4sc + 0.01 * model.BSIM4scref) * Math.Exp(-10.0 * BSIM4sc * T2) - (0.1 * T1 + 0.01 *
                                                    model.BSIM4scref) * Math.Exp(-10.0 * T1 * T2)) / Wdrn;
                        BSIM4scc.Value = ((0.05 * BSIM4sc + 0.0025 * model.BSIM4scref) * Math.Exp(-20.0 * BSIM4sc * T2) - (0.05 * T1 +
                            0.0025 * model.BSIM4scref) * Math.Exp(-20.0 * T1 * T2)) / Wdrn;
                    }
                    else
                    {

                        CircuitWarning.Warning(this, "Warning: No WPE as none of SCA, SCB, SCC, SC is given and / or SC not positive.");
                    }
                }

                if (BSIM4sca < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCA = {BSIM4sca} is negative. Set to 0.0.");
                    BSIM4sca.Value = 0.0;
                }
                if (BSIM4scb < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCB = {BSIM4scb} is negative. Set to 0.0.");
                    BSIM4scb.Value = 0.0;
                }
                if (BSIM4scc < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCC = {BSIM4scc} is negative. Set to 0.0.");
                    BSIM4scc.Value = 0.0;
                }
                if (BSIM4sc < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SC = {BSIM4sc} is negative. Set to 0.0.");
                    BSIM4sc.Value = 0.0;
                }
                /* 4.6.2 */
                sceff = BSIM4sca + model.BSIM4web * BSIM4scb + model.BSIM4wec * BSIM4scc;
                BSIM4vth0 += pParam.BSIM4kvth0we * sceff;
                BSIM4k2 += pParam.BSIM4k2we * sceff;
                T3 = 1.0 + pParam.BSIM4ku0we * sceff;
                if (T3 <= 0.0)
                {
                    T3 = 0.0;

                    CircuitWarning.Warning(this, $"Warning: ku0we = {pParam.BSIM4ku0we} is negatively too high. Negative mobility! ");
                }
                BSIM4u0temp *= T3;
            }

            /* adding delvto */
            BSIM4vth0 += BSIM4delvto;
            BSIM4vfb = pParam.BSIM4vfb + model.BSIM4type * BSIM4delvto;

            /* Instance variables calculation */
            T3 = model.BSIM4type * BSIM4vth0 - BSIM4vfb - pParam.BSIM4phi;
            T4 = T3 + T3;
            T5 = 2.5 * T3;
            BSIM4vtfbphi1 = (model.BSIM4type == NMOS) ? T4 : T5;
            if (BSIM4vtfbphi1 < 0.0)

                BSIM4vtfbphi1 = 0.0;

            BSIM4vtfbphi2 = 4.0 * T3;
            if (BSIM4vtfbphi2 < 0.0)

                BSIM4vtfbphi2 = 0.0;

            if (BSIM4k2 < 0.0)
            {
                T0 = 0.5 * pParam.BSIM4k1 / BSIM4k2;
                BSIM4vbsc = 0.9 * (pParam.BSIM4phi - T0 * T0);
                if (BSIM4vbsc > -3.0)
                    BSIM4vbsc = -3.0;
                else if (BSIM4vbsc < -30.0)

                    BSIM4vbsc = -30.0;
            }
            else
                BSIM4vbsc = -30.0;
            if (BSIM4vbsc > pParam.BSIM4vbm)
                BSIM4vbsc = pParam.BSIM4vbm;
            BSIM4k2ox = BSIM4k2 * model.toxe / model.BSIM4toxm;

            BSIM4vfbzb = pParam.BSIM4vfbzbfactor + model.BSIM4type * BSIM4vth0;

            BSIM4cgso = pParam.BSIM4cgso;
            BSIM4cgdo = pParam.BSIM4cgdo;

            lnl = Math.Log(pParam.BSIM4leff * 1.0e6);
            lnw = Math.Log(pParam.BSIM4weff * 1.0e6);
            lnnf = Math.Log(BSIM4nf);

            bodymode = 5;
            if ((!model.BSIM4rbps0.Given) || (!model.BSIM4rbpd0.Given))
                bodymode = 1;
            else
            if ((!model.BSIM4rbsbx0.Given && !model.BSIM4rbsby0.Given) || (!model.BSIM4rbdbx0.Given && !model.BSIM4rbdby0.Given))
                bodymode = 3;

            if (BSIM4rbodyMod.Value == 2)
            {
                if (bodymode == 5)
                {
                    /* rbsbx = Math.Exp(Math.Log(model.BSIM4rbsbx0) + model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf *
						lnnf);
					rbsby = Math.Exp(Math.Log(model.BSIM4rbsby0) + model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
						lnnf);
					*/
                    rbsbx = model.BSIM4rbsbx0 * Math.Exp(model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf * lnnf);
                    rbsby = model.BSIM4rbsby0 * Math.Exp(model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
                        lnnf);
                    BSIM4rbsb.Value = rbsbx * rbsby / (rbsbx + rbsby);

                    /* rbdbx = Math.Exp(Math.Log(model.BSIM4rbdbx0) + model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf *
						lnnf);
					rbdby = Math.Exp(Math.Log(model.BSIM4rbdby0) + model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
						lnnf);
					*/

                    rbdbx = model.BSIM4rbdbx0 * Math.Exp(model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf * lnnf);
                    rbdby = model.BSIM4rbdby0 * Math.Exp(model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf * lnnf);

                    BSIM4rbdb.Value = rbdbx * rbdby / (rbdbx + rbdby);
                }

                if ((bodymode == 3) || (bodymode == 5))
                {
                    /* BSIM4rbps.Value = Math.Exp(Math.Log(model.BSIM4rbps0) + model.BSIM4rbpsl * lnl + model.BSIM4rbpsw * lnw + model.BSIM4rbpsnf *
						lnnf);
					BSIM4rbpd.Value = Math.Exp(Math.Log(model.BSIM4rbpd0) + model.BSIM4rbpdl * lnl + model.BSIM4rbpdw * lnw + model.BSIM4rbpdnf *
						lnnf);
					*/
                    BSIM4rbps.Value = model.BSIM4rbps0 * Math.Exp(model.BSIM4rbpsl * lnl + model.BSIM4rbpsw * lnw + model.BSIM4rbpsnf * lnnf);
                    BSIM4rbpd.Value = model.BSIM4rbpd0 * Math.Exp(model.BSIM4rbpdl * lnl + model.BSIM4rbpdw * lnw + model.BSIM4rbpdnf * lnnf);

                }

                /* rbpbx = Math.Exp(Math.Log(model.BSIM4rbpbx0) + model.BSIM4rbpbxl * lnl + model.BSIM4rbpbxw * lnw + model.BSIM4rbpbxnf *
					lnnf);
				rbpby = Math.Exp(Math.Log(model.BSIM4rbpby0) + model.BSIM4rbpbyl * lnl + model.BSIM4rbpbyw * lnw + model.BSIM4rbpbynf * lnnf);
				*/
                rbpbx = model.BSIM4rbpbx0 * Math.Exp(model.BSIM4rbpbxl * lnl + model.BSIM4rbpbxw * lnw + model.BSIM4rbpbxnf * lnnf);
                rbpby = model.BSIM4rbpby0 * Math.Exp(model.BSIM4rbpbyl * lnl + model.BSIM4rbpbyw * lnw + model.BSIM4rbpbynf * lnnf);

                BSIM4rbpb.Value = rbpbx * rbpby / (rbpbx + rbpby);
            }

            if ((BSIM4rbodyMod.Value == 1) || ((BSIM4rbodyMod.Value == 2) && (bodymode == 5)))
            {
                if (BSIM4rbdb < 1.0e-3)

                    BSIM4grbdb = 1.0e3; /* in mho */
                else
                    BSIM4grbdb = model.BSIM4gbmin + 1.0 / BSIM4rbdb;
                if (BSIM4rbpb < 1.0e-3)

                    BSIM4grbpb = 1.0e3;
                else
                    BSIM4grbpb = model.BSIM4gbmin + 1.0 / BSIM4rbpb;
                if (BSIM4rbps < 1.0e-3)

                    BSIM4grbps = 1.0e3;
                else
                    BSIM4grbps = model.BSIM4gbmin + 1.0 / BSIM4rbps;
                if (BSIM4rbsb < 1.0e-3)

                    BSIM4grbsb = 1.0e3;
                else
                    BSIM4grbsb = model.BSIM4gbmin + 1.0 / BSIM4rbsb;
                if (BSIM4rbpd < 1.0e-3)

                    BSIM4grbpd = 1.0e3;
                else
                    BSIM4grbpd = model.BSIM4gbmin + 1.0 / BSIM4rbpd;

            }

            if ((BSIM4rbodyMod.Value == 2) && (bodymode == 3))
            {
                BSIM4grbdb = BSIM4grbsb = model.BSIM4gbmin;
                if (BSIM4rbpb < 1.0e-3)

                    BSIM4grbpb = 1.0e3;
                else
                    BSIM4grbpb = model.BSIM4gbmin + 1.0 / BSIM4rbpb;
                if (BSIM4rbps < 1.0e-3)

                    BSIM4grbps = 1.0e3;
                else
                    BSIM4grbps = model.BSIM4gbmin + 1.0 / BSIM4rbps;
                if (BSIM4rbpd < 1.0e-3)

                    BSIM4grbpd = 1.0e3;
                else
                    BSIM4grbpd = model.BSIM4gbmin + 1.0 / BSIM4rbpd;
            }

            if ((BSIM4rbodyMod.Value == 2) && (bodymode == 1))
            {
                BSIM4grbdb = BSIM4grbsb = model.BSIM4gbmin;
                BSIM4grbps = BSIM4grbpd = 1.0e3;
                if (BSIM4rbpb < 1.0e-3)

                    BSIM4grbpb = 1.0e3;
                else
                    BSIM4grbpb = model.BSIM4gbmin + 1.0 / BSIM4rbpb;
            }

            /* 
			* Process geomertry dependent parasitics
			*/

            BSIM4grgeltd = model.BSIM4rshg * (BSIM4xgw + pParam.BSIM4weffCJ / 3.0 / BSIM4ngcon) / (BSIM4ngcon * BSIM4nf * (Lnew -
                model.BSIM4xgl));
            if (BSIM4grgeltd > 0.0)
                BSIM4grgeltd = 1.0 / BSIM4grgeltd;
            else
            {
                BSIM4grgeltd = 1.0e3; /* mho */
                if (BSIM4rgateMod != 0) CircuitWarning.Warning(this, "Warning: The gate conductance reset to 1.0e3 mho.");
            }

            DMCGeff = model.BSIM4dmcg - model.BSIM4dmcgt;
            DMCIeff = model.BSIM4dmci;
            DMDGeff = model.BSIM4dmdg - model.BSIM4dmcgt;

            /* if (BSIM4sourcePerimeter.Given)
			{
				if (model.BSIM4perMod.Value == 0)
				BSIM4Pseff = BSIM4sourcePerimeter;
				else
				BSIM4Pseff = BSIM4sourcePerimeter - pParam.BSIM4weffCJ * BSIM4nf;
			}
			else
			BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min, 
			pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff, 
			&(BSIM4Pseff), &dumPd, &dumAs, &dumAd);
			if (BSIM4Pseff< 0.0)
			/ 4.6.2 / 
			BSIM4Pseff = 0.0; */

            /* New Diode Model v4.7 */
            if (BSIM4sourcePerimeter.Given)
            {
                /* given */
                if (BSIM4sourcePerimeter.Value == 0.0)
                    BSIM4Pseff = 0.0;
                else if (BSIM4sourcePerimeter < 0.0)
                {

                    CircuitWarning.Warning(this, "Warning: Source Perimeter is specified as negative, it is set to zero.");
                    BSIM4Pseff = 0.0;
                }
                else
                {
                    if (model.BSIM4perMod.Value == 0)
                        BSIM4Pseff = BSIM4sourcePerimeter;
                    else
                        BSIM4Pseff = BSIM4sourcePerimeter - pParam.BSIM4weffCJ * BSIM4nf;
                }
            } else
            {
                /* not given */
                double iseff;
                BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min,
                pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out iseff, out dumPd, out dumAs, out dumAd);
                BSIM4Pseff = iseff;
            }

            if (BSIM4Pseff < 0.0)
            {
                /* v4.7 final check */
                BSIM4Pseff = 0.0;

                CircuitWarning.Warning(this, "Warning: Pseff is negative, it is set to zero.");
            }
            /* if (BSIM4drainPerimeter.Given)
			{
				if (model.BSIM4perMod.Value == 0)
				BSIM4Pdeff = BSIM4drainPerimeter;
				else
				BSIM4Pdeff = BSIM4drainPerimeter - pParam.BSIM4weffCJ * BSIM4nf;
			}
			else
			BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min, 
			pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff, 
			&dumPs, &(BSIM4Pdeff), &dumAs, &dumAd);
			if (BSIM4Pdeff< 0.0)
			/ 4.6.2 / 
			BSIM4Pdeff = 0.0; */

            if (BSIM4drainPerimeter.Given)
            {
                /* given */
                if (BSIM4drainPerimeter.Value == 0.0)
                    BSIM4Pdeff = 0.0;
                else if (BSIM4drainPerimeter < 0.0)
                {

                    CircuitWarning.Warning(this, "Warning: Drain Perimeter is specified as negative, it is set to zero.");
                    BSIM4Pdeff = 0.0;
                }
                else
                {
                    if (model.BSIM4perMod.Value == 0)
                        BSIM4Pdeff = BSIM4drainPerimeter;
                    else
                        BSIM4Pdeff = BSIM4drainPerimeter - pParam.BSIM4weffCJ * BSIM4nf;
                }
            } else
            {
                /* not given */
                double ideff;
                BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min,
                    pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                    out dumPs, out ideff, out dumAs, out dumAd);
                BSIM4Pdeff = ideff;
            }

            if (BSIM4Pdeff < 0.0)
            {
                BSIM4Pdeff = 0.0; /* New Diode v4.7 */
                CircuitWarning.Warning(this, "Warning: Pdeff is negative, it is set to zero.");
            }
            if (BSIM4sourceArea.Given)
                BSIM4Aseff = BSIM4sourceArea;
            else
            {
                double iaseff;
                BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min,
                pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out dumPs, out dumPd, out iaseff, out dumAd);
                BSIM4Aseff = iaseff;
            }
            if (BSIM4Aseff < 0.0)
            {
                BSIM4Aseff = 0.0; /* v4.7 */

                CircuitWarning.Warning(this, "Warning: Aseff is negative, it is set to zero.");
            }
            if (BSIM4drainArea.Given)
                BSIM4Adeff = BSIM4drainArea;
            else
            {
                double iadeff;
                BSIM4PAeffGeo(BSIM4nf, BSIM4geoMod, BSIM4min,
                pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out dumPs, out dumPd, out dumAs, out iadeff);
                BSIM4Adeff = iadeff;
            }
            if (BSIM4Adeff < 0.0)
            {
                BSIM4Adeff = 0.0; /* v4.7 */
                CircuitWarning.Warning(this, "Warning: Adeff is negative, it is set to zero.");
            }
            /* Processing S / D resistance and conductance below */
            if (BSIM4sNodePrime != BSIM4sNode)
            {
                BSIM4sourceConductance = 0.0;
                if (BSIM4sourceSquares.Given)
                {
                    BSIM4sourceConductance = model.BSIM4sheetResistance * BSIM4sourceSquares;
                } else if (BSIM4rgeoMod > 0)
                {
                    double igsrc;
                    BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                        BSIM4rgeoMod, BSIM4min,
                        pParam.BSIM4weffCJ, model.BSIM4sheetResistance,
                        DMCGeff, DMCIeff, DMDGeff, 1, out igsrc);
                    BSIM4sourceConductance = igsrc;
                }
                else
                {
                    BSIM4sourceConductance = 0.0;
                }

                if (BSIM4sourceConductance > 0.0)
                    BSIM4sourceConductance = 1.0 / BSIM4sourceConductance;
                else
                {
                    BSIM4sourceConductance = 1.0e3; /* mho */

                    CircuitWarning.Warning(this, "Warning: Source conductance reset to 1.0e3 mho.");
                }
            }
            else
            {
                BSIM4sourceConductance = 0.0;
            }

            if (BSIM4dNodePrime != BSIM4dNode)
            {
                BSIM4drainConductance = 0.0;
                if (BSIM4drainSquares.Given)
                {
                    BSIM4drainConductance = model.BSIM4sheetResistance * BSIM4drainSquares;
                } else if (BSIM4rgeoMod > 0)
                {
                    double igdrn;
                    BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                        BSIM4rgeoMod, BSIM4min,
                        pParam.BSIM4weffCJ, model.BSIM4sheetResistance,
                        DMCGeff, DMCIeff, DMDGeff, 0, out igdrn);
                    BSIM4drainConductance = igdrn;
                }
                else
                {
                    BSIM4drainConductance = 0.0;
                }

                if (BSIM4drainConductance > 0.0)
                    BSIM4drainConductance = 1.0 / BSIM4drainConductance;
                else
                {
                    BSIM4drainConductance = 1.0e3; /* mho */
                    CircuitWarning.Warning(this, "Warning: Drain conductance reset to 1.0e3 mho.");
                }
            }
            else
            {
                BSIM4drainConductance = 0.0;
            }

            /* End of Rsd processing */

            Nvtms = model.BSIM4vtm * model.BSIM4SjctEmissionCoeff;
            if ((BSIM4Aseff <= 0.0) && (BSIM4Pseff <= 0.0))
            {
                SourceSatCurrent = 0.0; /* v4.7 */
                                        /* SourceSatCurrent = 1.0e-14; */
            }
            else
            {
                SourceSatCurrent = BSIM4Aseff * model.BSIM4SjctTempSatCurDensity + BSIM4Pseff * model.BSIM4SjctSidewallTempSatCurDensity +

                    pParam.BSIM4weffCJ * BSIM4nf * model.BSIM4SjctGateSidewallTempSatCurDensity;
            }
            if (SourceSatCurrent > 0.0)
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        if ((model.BSIM4bvs / Nvtms) > Transistor.EXP_THRESHOLD)
                            BSIM4XExpBVS = model.BSIM4xjbvs * Transistor.MIN_EXP;
                        else
                            BSIM4XExpBVS = model.BSIM4xjbvs * Math.Exp(-model.BSIM4bvs / Nvtms);
                        break;
                    case 1:
                        BSIM4vjsmFwd = BSIM4DioIjthVjmEval(Nvtms, model.BSIM4ijthsfwd, SourceSatCurrent, 0.0);
                        BSIM4IVjsmFwd = SourceSatCurrent * Math.Exp(BSIM4vjsmFwd / Nvtms);
                        break;
                    case 2:
                        if ((model.BSIM4bvs / Nvtms) > Transistor.EXP_THRESHOLD)
                        {
                            BSIM4XExpBVS = model.BSIM4xjbvs * Transistor.MIN_EXP;
                            tmp = Transistor.MIN_EXP;
                        }
                        else
                        {
                            BSIM4XExpBVS = Math.Exp(-model.BSIM4bvs / Nvtms);
                            tmp = BSIM4XExpBVS;
                            BSIM4XExpBVS *= model.BSIM4xjbvs;
                        }


                        BSIM4vjsmFwd = BSIM4DioIjthVjmEval(Nvtms, model.BSIM4ijthsfwd, SourceSatCurrent, BSIM4XExpBVS);
                        T0 = Math.Exp(BSIM4vjsmFwd / Nvtms);
                        BSIM4IVjsmFwd = SourceSatCurrent * (T0 - BSIM4XExpBVS / T0 + BSIM4XExpBVS - 1.0);
                        BSIM4SslpFwd = SourceSatCurrent * (T0 + BSIM4XExpBVS / T0) / Nvtms;

                        T2 = model.BSIM4ijthsrev / SourceSatCurrent;
                        if (T2 < 1.0)
                        {
                            T2 = 10.0;
                            CircuitWarning.Warning(this, "Warning: ijthsrev too small and set to 10 times IsbSat.");
                        }
                        BSIM4vjsmRev = -model.BSIM4bvs - Nvtms * Math.Log((T2 - 1.0) / model.BSIM4xjbvs);
                        T1 = model.BSIM4xjbvs * Math.Exp(-(model.BSIM4bvs + BSIM4vjsmRev) / Nvtms);
                        BSIM4IVjsmRev = SourceSatCurrent * (1.0 + T1);
                        BSIM4SslpRev = -SourceSatCurrent * T1 / Nvtms;
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Specified dioMod = {model.BSIM4dioMod} not matched");
                        break;
                }
            }

            Nvtmd = model.BSIM4vtm * model.BSIM4DjctEmissionCoeff;
            if ((BSIM4Adeff <= 0.0) && (BSIM4Pdeff <= 0.0))
            {
                /* DrainSatCurrent = 1.0e-14; 	v4.7 */
                DrainSatCurrent = 0.0;
            }
            else
            {
                DrainSatCurrent = BSIM4Adeff * model.BSIM4DjctTempSatCurDensity + BSIM4Pdeff * model.BSIM4DjctSidewallTempSatCurDensity +

                    pParam.BSIM4weffCJ * BSIM4nf * model.BSIM4DjctGateSidewallTempSatCurDensity;
            }
            if (DrainSatCurrent > 0.0)
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        if ((model.BSIM4bvd / Nvtmd) > Transistor.EXP_THRESHOLD)
                            BSIM4XExpBVD = model.BSIM4xjbvd * Transistor.MIN_EXP;
                        else
                            BSIM4XExpBVD = model.BSIM4xjbvd * Math.Exp(-model.BSIM4bvd / Nvtmd);
                        break;
                    case 1:
                        BSIM4vjdmFwd = BSIM4DioIjthVjmEval(Nvtmd, model.BSIM4ijthdfwd, DrainSatCurrent, 0.0);
                        BSIM4IVjdmFwd = DrainSatCurrent * Math.Exp(BSIM4vjdmFwd / Nvtmd);
                        break;
                    case 2:
                        if ((model.BSIM4bvd / Nvtmd) > Transistor.EXP_THRESHOLD)
                        {
                            BSIM4XExpBVD = model.BSIM4xjbvd * Transistor.MIN_EXP;
                            tmp = Transistor.MIN_EXP;
                        }
                        else
                        {
                            BSIM4XExpBVD = Math.Exp(-model.BSIM4bvd / Nvtmd);
                            tmp = BSIM4XExpBVD;
                            BSIM4XExpBVD *= model.BSIM4xjbvd;
                        }


                        BSIM4vjdmFwd = BSIM4DioIjthVjmEval(Nvtmd, model.BSIM4ijthdfwd, DrainSatCurrent, BSIM4XExpBVD);
                        T0 = Math.Exp(BSIM4vjdmFwd / Nvtmd);
                        BSIM4IVjdmFwd = DrainSatCurrent * (T0 - BSIM4XExpBVD / T0 + BSIM4XExpBVD - 1.0);
                        BSIM4DslpFwd = DrainSatCurrent * (T0 + BSIM4XExpBVD / T0) / Nvtmd;

                        T2 = model.BSIM4ijthdrev / DrainSatCurrent;
                        if (T2 < 1.0)
                        {
                            T2 = 10.0;

                            CircuitWarning.Warning(this, "Warning: ijthdrev too small and set to 10 times IdbSat.");
                        }
                        BSIM4vjdmRev = -model.BSIM4bvd - Nvtmd * Math.Log((T2 - 1.0) / model.BSIM4xjbvd); /* bugfix */
                        T1 = model.BSIM4xjbvd * Math.Exp(-(model.BSIM4bvd + BSIM4vjdmRev) / Nvtmd);
                        BSIM4IVjdmRev = DrainSatCurrent * (1.0 + T1);
                        BSIM4DslpRev = -DrainSatCurrent * T1 / Nvtmd;
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Specified dioMod = {model.BSIM4dioMod} not matched");
                        break;
                }
            }

            /* GEDL current reverse bias */
            T0 = (model.TRatio - 1.0);
            T7 = model.Eg0 / model.BSIM4vtm * T0;
            T9 = model.BSIM4xtss * T7;
            T1 = Dexp(T9);
            T9 = model.BSIM4xtsd * T7;
            T2 = Dexp(T9);
            T9 = model.BSIM4xtssws * T7;
            T3 = Dexp(T9);
            T9 = model.BSIM4xtsswd * T7;
            T4 = Dexp(T9);
            T9 = model.BSIM4xtsswgs * T7;
            T5 = Dexp(T9);
            T9 = model.BSIM4xtsswgd * T7;
            T6 = Dexp(T9);

            /* IBM TAT */
            if (model.BSIM4jtweff < 0.0)
            {
                model.BSIM4jtweff.Value = 0.0;
                CircuitWarning.Warning(this, "TAT width dependence effect is negative. Jtweff is clamped to zero.");
            }
            T11 = Math.Sqrt(model.BSIM4jtweff / pParam.BSIM4weffCJ) + 1.0;

            T10 = pParam.BSIM4weffCJ * BSIM4nf;
            BSIM4SjctTempRevSatCur = T1 * BSIM4Aseff * model.BSIM4jtss;
            BSIM4DjctTempRevSatCur = T2 * BSIM4Adeff * model.BSIM4jtsd;
            BSIM4SswTempRevSatCur = T3 * BSIM4Pseff * model.BSIM4jtssws;
            BSIM4DswTempRevSatCur = T4 * BSIM4Pdeff * model.BSIM4jtsswd;
            BSIM4SswgTempRevSatCur = T5 * T10 * T11 * model.BSIM4jtsswgs;
            BSIM4DswgTempRevSatCur = T6 * T10 * T11 * model.BSIM4jtsswgd;

            if (model.BSIM4mtrlMod != 0 && model.BSIM4mtrlCompatMod.Value == 0)
            {
                /* Calculate TOXP from EOT */
                /* Calculate Vgs_eff @ Vgs = VDD with Poly Depletion Effect */
                double Vtm0eot = Transistor.KboQ * model.BSIM4tempeot;
                Vtmeot = Vtm0eot;
                vbieot = Vtm0eot * Math.Log(pParam.BSIM4nsd * pParam.BSIM4ndep / (model.ni * model.ni));
                phieot = Vtm0eot * Math.Log(pParam.BSIM4ndep / model.ni) + pParam.BSIM4phin + 0.4;
                tmp2 = BSIM4vfb + phieot;
                vddeot = model.BSIM4type * model.BSIM4vddeot;
                T0 = model.BSIM4epsrgate * Transistor.EPS0;
                if ((pParam.BSIM4ngate > 1.0e18) && (pParam.BSIM4ngate < 1.0e25) && (vddeot > tmp2) && (T0 != 0))
                {
                    T1 = 1.0e6 * Circuit.CHARGE * T0 * pParam.BSIM4ngate / (model.BSIM4coxe * model.BSIM4coxe);
                    T8 = vddeot - tmp2;
                    T4 = Math.Sqrt(1.0 + 2.0 * T8 / T1);
                    T2 = 2.0 * T8 / (T4 + 1.0);
                    T3 = 0.5 * T2 * T2 / T1;
                    T7 = 1.12 - T3 - 0.05;
                    T6 = Math.Sqrt(T7 * T7 + 0.224);
                    T5 = 1.12 - 0.5 * (T7 + T6);
                    Vgs_eff = vddeot - T5;
                }
                else
                    Vgs_eff = vddeot;

                /* Calculate Vth @ Vds = Vbs = 0 */
                double V0 = vbieot - phieot;
                lt1 = model.BSIM4factor1 * pParam.BSIM4sqrtXdep0;
                ltw = lt1;
                T0 = pParam.BSIM4dvt1 * model.BSIM4leffeot / lt1;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    Theta0 = T1 / T4;
                }
                else
                    Theta0 = 1.0 / (Transistor.MAX_EXP - 2.0);
                Delt_vth = pParam.BSIM4dvt0 * Theta0 * V0;
                T0 = pParam.BSIM4dvt1w * model.BSIM4weffeot * model.BSIM4leffeot / ltw;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T5 = T1 / T4;
                }
                else
                    T5 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                T2 = pParam.BSIM4dvt0w * T5 * V0;
                TempRatioeot = model.BSIM4tempeot / model.BSIM4tnom - 1.0;
                T0 = Math.Sqrt(1.0 + pParam.BSIM4lpe0 / model.BSIM4leffeot);
                T1 = pParam.BSIM4k1ox * (T0 - 1.0) * Math.Sqrt(phieot) + (pParam.BSIM4kt1 + pParam.BSIM4kt1l / model.BSIM4leffeot) *
                   TempRatioeot;
                Vth_NarrowW = model.toxe * phieot / (model.BSIM4weffeot + pParam.BSIM4w0);
                Lpe_Vb = Math.Sqrt(1.0 + pParam.BSIM4lpeb / model.BSIM4leffeot);
                Vth = model.BSIM4type * BSIM4vth0 + (pParam.BSIM4k1ox - pParam.BSIM4k1) * Math.Sqrt(phieot) * Lpe_Vb - Delt_vth - T2 +
                    pParam.BSIM4k3 * Vth_NarrowW + T1;

                /* Calculate n */
                tmp1 = model.epssub / pParam.BSIM4Xdep0;
                tmp2 = pParam.BSIM4nfactor * tmp1;
                tmp3 = (tmp2 + pParam.BSIM4cdsc * Theta0 + pParam.BSIM4cit) / model.BSIM4coxe;
                if (tmp3 >= -0.5)
                    n = 1.0 + tmp3;
                else
                {
                    T0 = 1.0 / (3.0 + 8.0 * tmp3);
                    n = (1.0 + 3.0 * tmp3) * T0;
                }

                /* Vth correction for Pocket implant */
                if (pParam.BSIM4dvtp0 > 0.0)
                {
                    T3 = model.BSIM4leffeot + pParam.BSIM4dvtp0 * 2.0;
                    if (model.BSIM4tempMod < 2)
                        T4 = Vtmeot * Math.Log(model.BSIM4leffeot / T3);
                    else
                        T4 = Vtm0eot * Math.Log(model.BSIM4leffeot / T3);
                    Vth -= n * T4;
                }
                Vgsteff = Vgs_eff - Vth;
                /* calculating Toxp */
                T3 = model.BSIM4type * BSIM4vth0 - BSIM4vfb - phieot;
                T4 = T3 + T3;
                T5 = 2.5 * T3;

                vtfbphi2eot = 4.0 * T3;
                if (vtfbphi2eot < 0.0)

                    vtfbphi2eot = 0.0;

                niter = 0;
                toxpf = model.toxe;
                do
                {
                    toxpi = toxpf;
                    tmp2 = 2.0e8 * toxpf;
                    T0 = (Vgsteff + vtfbphi2eot) / tmp2;
                    T1 = 1.0 + Math.Exp(model.BSIM4bdos * 0.7 * Math.Log(T0));
                    Tcen = model.BSIM4ados * 1.9e-9 / T1;
                    toxpf = model.toxe - model.epsrox / model.BSIM4epsrsub * Tcen;
                    niter++;
                } while ((niter <= 4) && (Math.Abs(toxpf - toxpi) > 1e-12));
                BSIM4toxp = toxpf;
                BSIM4coxp = model.epsrox * Transistor.EPS0 / model.BSIM4toxp;
            }
            else
            {
                BSIM4toxp = model.BSIM4toxp;
                BSIM4coxp = model.BSIM4coxp;
            }

            if (BSIM4checkModel(ckt))
                throw new CircuitException($"Fatal error(s) detected during BSIM4v8.0 parameter checking for {Name} in model {model.Name}");
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as BSIM4v80Model;
            var state = ckt.State;
            var cstate = state.Complex;
            double capbd, capbs, cgso, cgdo, cgbo, Csd, Csg, Css, T0 = 0, T1, T2, T3, gmr, gmbsr, gdsr, gmi, gmbsi, gdsi, Cddr, Cdgr,
                Cdsr, Cdbr, Cddi, Cdgi, Cdsi, Cdbi, Csdr, Csgr, Cssr, Csbr, Csdi, Csgi, Cssi, Csbi, Cgdr, Cggr, Cgsr, Cgbr, Cgdi, Cggi, Cgsi,
                Cgbi, Gmr, Gmbsr, FwdSumr, RevSumr, Gmi, Gmbsi, FwdSumi, RevSumi, gbbdp, gbbsp, gbdpg, gbdpdp, gbdpb, gbdpsp, gbspdp, gbspg,
                gbspb, gbspsp, gIstotg, gIstotd, gIstots, gIstotb, gIdtotg, gIdtotd, gIdtots, gIdtotb, gIbtotg, gIbtotd, gIbtots, gIbtotb,
                gIgtotg, gIgtotd, gIgtots, gIgtotb, gcrgd, gcrgg, gcrgs, gcrgb, gcrg, xcgmgmb = 0, xcgmdb = 0, xcgmsb = 0, xcgmbb = 0, xcdgmb, xcsgmb, xcbgmb,
                xcggbr, xcgdbr, xcgsbr, xcgbbr, xcdgbr, xcsgbr, xcbgb, xcddbr, xcdsbr, xcsdbr, xcssbr, xcdbbr, xcsbbr, xcbdb, xcbsb, xcdbdb,
                xcsbsb = 0, xcbbb, xcdgbi, xcsgbi, xcddbi, xcdsbi, xcsdbi, xcssbi, xcdbbi, xcsbbi, xcggbi, xcgdbi, xcgsbi, xcgbbi, gstot, gstotd,
                gstotg, gstots, gstotb, gdtot, gdtotd, gdtotg, gdtots, gdtotb, gdpr, gspr, gjbd, gjbs, geltd, ggidld = 0, ggidlg = 0, ggidlb = 0, ggislg = 0,
                ggisls = 0, ggislb = 0;
            double omega = cstate.Laplace.Imaginary;

            capbd = BSIM4capbd;
            capbs = BSIM4capbs;
            cgso = BSIM4cgso;
            cgdo = BSIM4cgdo;
            cgbo = pParam.BSIM4cgbo;

            Csd = -(BSIM4cddb + BSIM4cgdb + BSIM4cbdb);
            Csg = -(BSIM4cdgb + BSIM4cggb + BSIM4cbgb);
            Css = -(BSIM4cdsb + BSIM4cgsb + BSIM4cbsb);

            if (BSIM4acnqsMod != 0)
            {
                T0 = omega * BSIM4taunet;
                T1 = T0 * T0;
                T2 = 1.0 / (1.0 + T1);
                T3 = T0 * T2;

                gmr = BSIM4gm * T2;
                gmbsr = BSIM4gmbs * T2;
                gdsr = BSIM4gds * T2;

                gmi = -BSIM4gm * T3;
                gmbsi = -BSIM4gmbs * T3;
                gdsi = -BSIM4gds * T3;

                Cddr = BSIM4cddb * T2;
                Cdgr = BSIM4cdgb * T2;
                Cdsr = BSIM4cdsb * T2;
                Cdbr = -(Cddr + Cdgr + Cdsr);

                /* WDLiu: Cxyi mulitplied by jomega below, and actually to be of conductance */
                Cddi = BSIM4cddb * T3 * omega;
                Cdgi = BSIM4cdgb * T3 * omega;
                Cdsi = BSIM4cdsb * T3 * omega;
                Cdbi = -(Cddi + Cdgi + Cdsi);

                Csdr = Csd * T2;
                Csgr = Csg * T2;
                Cssr = Css * T2;
                Csbr = -(Csdr + Csgr + Cssr);

                Csdi = Csd * T3 * omega;
                Csgi = Csg * T3 * omega;
                Cssi = Css * T3 * omega;
                Csbi = -(Csdi + Csgi + Cssi);

                Cgdr = -(Cddr + Csdr + BSIM4cbdb);
                Cggr = -(Cdgr + Csgr + BSIM4cbgb);
                Cgsr = -(Cdsr + Cssr + BSIM4cbsb);
                Cgbr = -(Cgdr + Cggr + Cgsr);

                Cgdi = -(Cddi + Csdi);
                Cggi = -(Cdgi + Csgi);
                Cgsi = -(Cdsi + Cssi);
                Cgbi = -(Cgdi + Cggi + Cgsi);
            }
            else /* QS */
            {
                gmr = BSIM4gm;
                gmbsr = BSIM4gmbs;
                gdsr = BSIM4gds;
                gmi = gmbsi = gdsi = 0.0;

                Cddr = BSIM4cddb;
                Cdgr = BSIM4cdgb;
                Cdsr = BSIM4cdsb;
                Cdbr = -(Cddr + Cdgr + Cdsr);
                Cddi = Cdgi = Cdsi = Cdbi = 0.0;

                Csdr = Csd;
                Csgr = Csg;
                Cssr = Css;
                Csbr = -(Csdr + Csgr + Cssr);
                Csdi = Csgi = Cssi = Csbi = 0.0;

                Cgdr = BSIM4cgdb;
                Cggr = BSIM4cggb;
                Cgsr = BSIM4cgsb;
                Cgbr = -(Cgdr + Cggr + Cgsr);
                Cgdi = Cggi = Cgsi = Cgbi = 0.0;
            }

            if (BSIM4mode >= 0)
            {
                Gmr = gmr;
                Gmbsr = gmbsr;
                FwdSumr = Gmr + Gmbsr;
                RevSumr = 0.0;
                Gmi = gmi;
                Gmbsi = gmbsi;
                FwdSumi = Gmi + Gmbsi;
                RevSumi = 0.0;

                gbbdp = -(BSIM4gbds);
                gbbsp = BSIM4gbds + BSIM4gbgs + BSIM4gbbs;
                gbdpg = BSIM4gbgs;
                gbdpdp = BSIM4gbds;
                gbdpb = BSIM4gbbs;
                gbdpsp = -(gbdpg + gbdpdp + gbdpb);

                gbspdp = 0.0;
                gbspg = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;

                if (model.BSIM4igcMod != 0)
                {
                    gIstotg = BSIM4gIgsg + BSIM4gIgcsg;
                    gIstotd = BSIM4gIgcsd;
                    gIstots = BSIM4gIgss + BSIM4gIgcss;
                    gIstotb = BSIM4gIgcsb;

                    gIdtotg = BSIM4gIgdg + BSIM4gIgcdg;
                    gIdtotd = BSIM4gIgdd + BSIM4gIgcdd;
                    gIdtots = BSIM4gIgcds;
                    gIdtotb = BSIM4gIgcdb;
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = BSIM4gIgbg;
                    gIbtotd = BSIM4gIgbd;
                    gIbtots = BSIM4gIgbs;
                    gIbtotb = BSIM4gIgbb;
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = 0.0;

                if (BSIM4rgateMod.Value == 2)
                    T0 = state.States[0][BSIM4states + BSIM4vges] - state.States[0][BSIM4states + BSIM4vgs];
                else if (BSIM4rgateMod.Value == 3)
                    T0 = state.States[0][BSIM4states + BSIM4vgms] - state.States[0][BSIM4states + BSIM4vgs];
                if (BSIM4rgateMod > 1)
                {
                    gcrgd = BSIM4gcrgd * T0;
                    gcrgg = BSIM4gcrgg * T0;
                    gcrgs = BSIM4gcrgs * T0;
                    gcrgb = BSIM4gcrgb * T0;
                    gcrgg -= BSIM4gcrg;
                    gcrg = BSIM4gcrg;
                }
                else
                    gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;

                if (BSIM4rgateMod.Value == 3)
                {
                    xcgmgmb = (cgdo + cgso + pParam.BSIM4cgbo) * omega;
                    xcgmdb = -cgdo * omega;
                    xcgmsb = -cgso * omega;
                    xcgmbb = -pParam.BSIM4cgbo * omega;

                    xcdgmb = xcgmdb;
                    xcsgmb = xcgmsb;
                    xcbgmb = xcgmbb;

                    xcggbr = Cggr * omega;
                    xcgdbr = Cgdr * omega;
                    xcgsbr = Cgsr * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = Cdgr * omega;
                    xcsgbr = Csgr * omega;
                    xcbgb = BSIM4cbgb * omega;
                }
                else
                {
                    xcggbr = (Cggr + cgdo + cgso + pParam.BSIM4cgbo) * omega;
                    xcgdbr = (Cgdr - cgdo) * omega;
                    xcgsbr = (Cgsr - cgso) * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = (Cdgr - cgdo) * omega;
                    xcsgbr = (Csgr - cgso) * omega;
                    xcbgb = (BSIM4cbgb - pParam.BSIM4cgbo) * omega;

                    xcdgmb = xcsgmb = xcbgmb = 0.0;
                }
                xcddbr = (Cddr + BSIM4capbd + cgdo) * omega;
                xcdsbr = Cdsr * omega;
                xcsdbr = Csdr * omega;
                xcssbr = (BSIM4capbs + cgso + Cssr) * omega;

                if (BSIM4rbodyMod == 0)
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb);
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb);

                    xcbdb = (BSIM4cbdb - BSIM4capbd) * omega;
                    xcbsb = (BSIM4cbsb - BSIM4capbs) * omega;
                    xcdbdb = 0.0;
                }
                else
                {
                    xcdbbr = Cdbr * omega;
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb) + BSIM4capbs * omega;

                    xcbdb = BSIM4cbdb * omega;
                    xcbsb = BSIM4cbsb * omega;

                    xcdbdb = -BSIM4capbd * omega;
                    xcsbsb = -BSIM4capbs * omega;
                }
                xcbbb = -(xcbdb + xcbgb + xcbsb + xcbgmb);

                xcdgbi = Cdgi;
                xcsgbi = Csgi;
                xcddbi = Cddi;
                xcdsbi = Cdsi;
                xcsdbi = Csdi;
                xcssbi = Cssi;
                xcdbbi = Cdbi;
                xcsbbi = Csbi;
                xcggbi = Cggi;
                xcgdbi = Cgdi;
                xcgsbi = Cgsi;
                xcgbbi = Cgbi;
            }
            else /* Reverse mode */
            {
                Gmr = -gmr;
                Gmbsr = -gmbsr;
                FwdSumr = 0.0;
                RevSumr = -(Gmr + Gmbsr);
                Gmi = -gmi;
                Gmbsi = -gmbsi;
                FwdSumi = 0.0;
                RevSumi = -(Gmi + Gmbsi);

                gbbsp = -(BSIM4gbds);
                gbbdp = BSIM4gbds + BSIM4gbgs + BSIM4gbbs;

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = BSIM4gbgs;
                gbspsp = BSIM4gbds;
                gbspb = BSIM4gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);

                if (model.BSIM4igcMod != 0)
                {
                    gIstotg = BSIM4gIgsg + BSIM4gIgcdg;
                    gIstotd = BSIM4gIgcds;
                    gIstots = BSIM4gIgss + BSIM4gIgcdd;
                    gIstotb = BSIM4gIgcdb;

                    gIdtotg = BSIM4gIgdg + BSIM4gIgcsg;
                    gIdtotd = BSIM4gIgdd + BSIM4gIgcss;
                    gIdtots = BSIM4gIgcsd;
                    gIdtotb = BSIM4gIgcsb;
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = BSIM4gIgbg;
                    gIbtotd = BSIM4gIgbs;
                    gIbtots = BSIM4gIgbd;
                    gIbtotb = BSIM4gIgbb;
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = 0.0;

                if (BSIM4rgateMod.Value == 2)
                    T0 = state.States[0][BSIM4states + BSIM4vges] - state.States[0][BSIM4states + BSIM4vgs];
                else if (BSIM4rgateMod.Value == 3)
                    T0 = state.States[0][BSIM4states + BSIM4vgms] - state.States[0][BSIM4states + BSIM4vgs];
                if (BSIM4rgateMod > 1)
                {
                    gcrgd = BSIM4gcrgs * T0;
                    gcrgg = BSIM4gcrgg * T0;
                    gcrgs = BSIM4gcrgd * T0;
                    gcrgb = BSIM4gcrgb * T0;
                    gcrgg -= BSIM4gcrg;
                    gcrg = BSIM4gcrg;
                }
                else
                    gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;

                if (BSIM4rgateMod.Value == 3)
                {
                    xcgmgmb = (cgdo + cgso + pParam.BSIM4cgbo) * omega;
                    xcgmdb = -cgdo * omega;
                    xcgmsb = -cgso * omega;
                    xcgmbb = -pParam.BSIM4cgbo * omega;

                    xcdgmb = xcgmdb;
                    xcsgmb = xcgmsb;
                    xcbgmb = xcgmbb;

                    xcggbr = Cggr * omega;
                    xcgdbr = Cgsr * omega;
                    xcgsbr = Cgdr * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = Csgr * omega;
                    xcsgbr = Cdgr * omega;
                    xcbgb = BSIM4cbgb * omega;
                }
                else
                {
                    xcggbr = (Cggr + cgdo + cgso + pParam.BSIM4cgbo) * omega;
                    xcgdbr = (Cgsr - cgdo) * omega;
                    xcgsbr = (Cgdr - cgso) * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = (Csgr - cgdo) * omega;
                    xcsgbr = (Cdgr - cgso) * omega;
                    xcbgb = (BSIM4cbgb - pParam.BSIM4cgbo) * omega;

                    xcdgmb = xcsgmb = xcbgmb = 0.0;
                }
                xcddbr = (BSIM4capbd + cgdo + Cssr) * omega;
                xcdsbr = Csdr * omega;
                xcsdbr = Cdsr * omega;
                xcssbr = (Cddr + BSIM4capbs + cgso) * omega;

                if (BSIM4rbodyMod == 0)
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb);
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb);

                    xcbdb = (BSIM4cbsb - BSIM4capbd) * omega;
                    xcbsb = (BSIM4cbdb - BSIM4capbs) * omega;
                    xcdbdb = 0.0;
                }
                else
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb) + BSIM4capbd * omega;
                    xcsbbr = Cdbr * omega;

                    xcbdb = BSIM4cbsb * omega;
                    xcbsb = BSIM4cbdb * omega;
                    xcdbdb = -BSIM4capbd * omega;
                    xcsbsb = -BSIM4capbs * omega;
                }
                xcbbb = -(xcbgb + xcbdb + xcbsb + xcbgmb);

                xcdgbi = Csgi;
                xcsgbi = Cdgi;
                xcddbi = Cssi;
                xcdsbi = Csdi;
                xcsdbi = Cdsi;
                xcssbi = Cddi;
                xcdbbi = Csbi;
                xcsbbi = Cdbi;
                xcggbi = Cggi;
                xcgdbi = Cgsi;
                xcgsbi = Cgdi;
                xcgbbi = Cgbi;
            }

            if (model.BSIM4rdsMod.Value == 1)
            {
                gstot = BSIM4gstot;
                gstotd = BSIM4gstotd;
                gstotg = BSIM4gstotg;
                gstots = BSIM4gstots - gstot;
                gstotb = BSIM4gstotb;

                gdtot = BSIM4gdtot;
                gdtotd = BSIM4gdtotd - gdtot;
                gdtotg = BSIM4gdtotg;
                gdtots = BSIM4gdtots;
                gdtotb = BSIM4gdtotb;
            }
            else
            {
                gstot = gstotd = gstotg = gstots = gstotb = 0.0;
                gdtot = gdtotd = gdtotg = gdtots = gdtotb = 0.0;
            }

            /* 
            * Loading AC matrix
            */

            if (model.BSIM4rdsMod == 0)
            {
                gdpr = BSIM4drainConductance;
                gspr = BSIM4sourceConductance;
            }
            else
                gdpr = gspr = 0.0;

            if (BSIM4rbodyMod == 0)
            {
                gjbd = BSIM4gbd;
                gjbs = BSIM4gbs;
            }
            else
                gjbd = gjbs = 0.0;

            geltd = BSIM4grgeltd;

            if (BSIM4rgateMod.Value == 1)
            {
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodeExt] += geltd;
                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodeExt] -= geltd;
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodePrime] -= geltd;

                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodePrime] += new Complex(geltd + xcggbi + gIgtotg, xcggbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4dNodePrime] += new Complex(xcgdbi + gIgtotd, xcgdbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4sNodePrime] += new Complex(xcgsbi + gIgtots, xcgsbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4bNodePrime] += new Complex(xcgbbi + gIgtotb, xcgbbr);

            } /* WDLiu: gcrg already subtracted from all gcrgg below */
            else if (BSIM4rgateMod.Value == 2)
            {
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodeExt] += gcrg;
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodePrime] += gcrgg;
                cstate.Matrix[BSIM4gNodeExt, BSIM4dNodePrime] += gcrgd;
                cstate.Matrix[BSIM4gNodeExt, BSIM4sNodePrime] += gcrgs;
                cstate.Matrix[BSIM4gNodeExt, BSIM4bNodePrime] += gcrgb;

                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodeExt] -= gcrg;
                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodePrime] -= new Complex(gcrgg - xcggbi - gIgtotg, -xcggbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4dNodePrime] -= new Complex(gcrgd - xcgdbi - gIgtotd, -xcgdbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4sNodePrime] -= new Complex(gcrgs - xcgsbi - gIgtots, -xcgsbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4bNodePrime] -= new Complex(gcrgb - xcgbbi - gIgtotb, -xcgbbr);

            }
            else if (BSIM4rgateMod.Value == 3)
            {
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodeExt] += geltd;
                cstate.Matrix[BSIM4gNodeExt, BSIM4gNodeMid] -= geltd;
                cstate.Matrix[BSIM4gNodeMid, BSIM4gNodeExt] -= geltd;
                cstate.Matrix[BSIM4gNodeMid, BSIM4gNodeMid] += new Complex(geltd + gcrg, xcgmgmb);

                cstate.Matrix[BSIM4gNodeMid, BSIM4dNodePrime] += new Complex(gcrgd, xcgmdb);

                cstate.Matrix[BSIM4gNodeMid, BSIM4gNodePrime] += gcrgg;
                cstate.Matrix[BSIM4gNodeMid, BSIM4sNodePrime] += new Complex(gcrgs, xcgmsb);

                cstate.Matrix[BSIM4gNodeMid, BSIM4bNodePrime] += new Complex(gcrgb, xcgmbb);

                cstate.Matrix[BSIM4dNodePrime, BSIM4gNodeMid] += new Complex(0.0, xcdgmb);
                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodeMid] -= gcrg;
                cstate.Matrix[BSIM4sNodePrime, BSIM4gNodeMid] += new Complex(0.0, xcsgmb);
                cstate.Matrix[BSIM4bNodePrime, BSIM4gNodeMid] += new Complex(0.0, xcbgmb);

                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodePrime] -= new Complex(gcrgg - xcggbi - gIgtotg, -xcggbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4dNodePrime] -= new Complex(gcrgd - xcgdbi - gIgtotd, -xcgdbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4sNodePrime] -= new Complex(gcrgs - xcgsbi - gIgtots, -xcgsbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4bNodePrime] -= new Complex(gcrgb - xcgbbi - gIgtotb, -xcgbbr);

            }
            else
            {
                cstate.Matrix[BSIM4gNodePrime, BSIM4gNodePrime] += new Complex(xcggbi + gIgtotg, xcggbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4dNodePrime] += new Complex(xcgdbi + gIgtotd, xcgdbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4sNodePrime] += new Complex(xcgsbi + gIgtots, xcgsbr);

                cstate.Matrix[BSIM4gNodePrime, BSIM4bNodePrime] += new Complex(xcgbbi + gIgtotb, xcgbbr);

            }

            if (model.BSIM4rdsMod != 0)
            {
                cstate.Matrix[BSIM4dNode, BSIM4gNodePrime] += gdtotg;
                cstate.Matrix[BSIM4dNode, BSIM4sNodePrime] += gdtots;
                cstate.Matrix[BSIM4dNode, BSIM4bNodePrime] += gdtotb;
                cstate.Matrix[BSIM4sNode, BSIM4dNodePrime] += gstotd;
                cstate.Matrix[BSIM4sNode, BSIM4gNodePrime] += gstotg;
                cstate.Matrix[BSIM4sNode, BSIM4bNodePrime] += gstotb;
            }

            cstate.Matrix[BSIM4dNodePrime, BSIM4dNodePrime] += new Complex(gdpr + xcddbi + gdsr + BSIM4gbd - gdtotd + RevSumr + gbdpdp -
                gIdtotd + ggidld, xcddbr + gdsi + RevSumi);

            cstate.Matrix[BSIM4dNodePrime, BSIM4dNode] -= gdpr + gdtot;
            cstate.Matrix[BSIM4dNodePrime, BSIM4gNodePrime] += new Complex(Gmr + xcdgbi - gdtotg + gbdpg - gIdtotg + ggidlg, xcdgbr + Gmi);

            cstate.Matrix[BSIM4dNodePrime, BSIM4sNodePrime] -= new Complex(gdsr - xcdsbi + FwdSumr + gdtots - gbdpsp + gIdtots + (ggidlg +
                ggidld) + ggidlb, -(xcdsbr - gdsi - FwdSumi));

            cstate.Matrix[BSIM4dNodePrime, BSIM4bNodePrime] -= new Complex(gjbd + gdtotb - xcdbbi - Gmbsr - gbdpb + gIdtotb - ggidlb, -
                (xcdbbr + Gmbsi));

            cstate.Matrix[BSIM4dNode, BSIM4dNodePrime] -= gdpr - gdtotd;
            cstate.Matrix[BSIM4dNode, BSIM4dNode] += gdpr + gdtot;

            cstate.Matrix[BSIM4sNodePrime, BSIM4dNodePrime] -= new Complex(gdsr - xcsdbi + gstotd + RevSumr - gbspdp + gIstotd + (ggisls +
                ggislg) + ggislb, -(xcsdbr - gdsi - RevSumi));

            cstate.Matrix[BSIM4sNodePrime, BSIM4gNodePrime] -= new Complex(Gmr - xcsgbi + gstotg - gbspg + gIstotg - ggislg, -(xcsgbr -
                Gmi));

            cstate.Matrix[BSIM4sNodePrime, BSIM4sNodePrime] += new Complex(gspr + xcssbi + gdsr + BSIM4gbs - gstots + FwdSumr + gbspsp -
                gIstots + ggisls, xcssbr + gdsi + FwdSumi);

            cstate.Matrix[BSIM4sNodePrime, BSIM4sNode] -= gspr + gstot;
            cstate.Matrix[BSIM4sNodePrime, BSIM4bNodePrime] -= new Complex(gjbs + gstotb - xcsbbi + Gmbsr - gbspb + gIstotb - ggislb, -
                (xcsbbr - Gmbsi));

            cstate.Matrix[BSIM4sNode, BSIM4sNodePrime] -= gspr - gstots;
            cstate.Matrix[BSIM4sNode, BSIM4sNode] += gspr + gstot;

            cstate.Matrix[BSIM4bNodePrime, BSIM4dNodePrime] -= new Complex(gjbd - gbbdp + gIbtotd + ggidld - ((ggislg + ggisls) + ggislb), -
                xcbdb);

            cstate.Matrix[BSIM4bNodePrime, BSIM4gNodePrime] -= new Complex(BSIM4gbgs + gIbtotg + ggidlg + ggislg, -xcbgb);

            cstate.Matrix[BSIM4bNodePrime, BSIM4sNodePrime] -= new Complex(gjbs - gbbsp + gIbtots - ((ggidlg + ggidld) + ggidlb) + ggisls, -
                xcbsb);

            cstate.Matrix[BSIM4bNodePrime, BSIM4bNodePrime] += new Complex(gjbd + gjbs - BSIM4gbbs - gIbtotb - ggidlb - ggislb, xcbbb);

            ggidld = BSIM4ggidld;
            ggidlg = BSIM4ggidlg;
            ggidlb = BSIM4ggidlb;
            ggislg = BSIM4ggislg;
            ggisls = BSIM4ggisls;
            ggislb = BSIM4ggislb;

            /* stamp gidl */

            /* stamp gisl */

            if (BSIM4rbodyMod != 0)
            {
                cstate.Matrix[BSIM4dNodePrime, BSIM4dbNode] -= new Complex(BSIM4gbd, -xcdbdb);

                cstate.Matrix[BSIM4sNodePrime, BSIM4sbNode] -= new Complex(BSIM4gbs, -xcsbsb);

                cstate.Matrix[BSIM4dbNode, BSIM4dNodePrime] -= new Complex(BSIM4gbd, -xcdbdb);

                cstate.Matrix[BSIM4dbNode, BSIM4dbNode] += new Complex(BSIM4gbd + BSIM4grbpd + BSIM4grbdb, -xcdbdb);

                cstate.Matrix[BSIM4dbNode, BSIM4bNodePrime] -= BSIM4grbpd;
                cstate.Matrix[BSIM4dbNode, BSIM4bNode] -= BSIM4grbdb;

                cstate.Matrix[BSIM4bNodePrime, BSIM4dbNode] -= BSIM4grbpd;
                cstate.Matrix[BSIM4bNodePrime, BSIM4bNode] -= BSIM4grbpb;
                cstate.Matrix[BSIM4bNodePrime, BSIM4sbNode] -= BSIM4grbps;
                cstate.Matrix[BSIM4bNodePrime, BSIM4bNodePrime] += BSIM4grbpd + BSIM4grbps + BSIM4grbpb;
                /* WDLiu: (-BSIM4gbbs) already added to BPbpPtr */

                cstate.Matrix[BSIM4sbNode, BSIM4sNodePrime] -= new Complex(BSIM4gbs, -xcsbsb);

                cstate.Matrix[BSIM4sbNode, BSIM4bNodePrime] -= BSIM4grbps;
                cstate.Matrix[BSIM4sbNode, BSIM4bNode] -= BSIM4grbsb;
                cstate.Matrix[BSIM4sbNode, BSIM4sbNode] += new Complex(BSIM4gbs + BSIM4grbps + BSIM4grbsb, -xcsbsb);

                cstate.Matrix[BSIM4bNode, BSIM4dbNode] -= BSIM4grbdb;
                cstate.Matrix[BSIM4bNode, BSIM4bNodePrime] -= BSIM4grbpb;
                cstate.Matrix[BSIM4bNode, BSIM4sbNode] -= BSIM4grbsb;
                cstate.Matrix[BSIM4bNode, BSIM4bNode] += BSIM4grbsb + BSIM4grbdb + BSIM4grbpb;
            }

            /* 
            * WDLiu: The internal charge node generated for transient NQS is not needed for
            * AC NQS. The following is not doing a real job, but we have to keep it;
            * otherwise a singular AC NQS matrix may occur if the transient NQS is on.
            * The charge node is isolated from the instance.
            */
            if (BSIM4trnqsMod != 0)
            {
                cstate.Matrix[BSIM4qNode, BSIM4qNode] += 1.0;
                cstate.Matrix[BSIM4qNode, BSIM4gNodePrime] += 0.0;
                cstate.Matrix[BSIM4qNode, BSIM4dNodePrime] += 0.0;
                cstate.Matrix[BSIM4qNode, BSIM4sNodePrime] += 0.0;
                cstate.Matrix[BSIM4qNode, BSIM4bNodePrime] += 0.0;

                cstate.Matrix[BSIM4dNodePrime, BSIM4qNode] += 0.0;
                cstate.Matrix[BSIM4sNodePrime, BSIM4qNode] += 0.0;
                cstate.Matrix[BSIM4gNodePrime, BSIM4qNode] += 0.0;
            }
        }
	}
}
