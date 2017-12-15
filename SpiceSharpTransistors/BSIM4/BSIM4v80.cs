using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;
using static SpiceSharp.Components.Transistors.BSIM4v80Helpers;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The BSIM4v80 device
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public partial class BSIM4v80 : Component
    {
        /// <summary>
        /// Register default behaviors
        /// </summary>
        static BSIM4v80()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80), typeof(ComponentBehaviors.BSIM4v80TemperatureBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80), typeof(ComponentBehaviors.BSIM4v80LoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80), typeof(ComponentBehaviors.BSIM4v80AcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80), typeof(ComponentBehaviors.BSIM4v80NoiseBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80), typeof(ComponentBehaviors.BSIM4v80TruncateBehavior));
        }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM4v80Model model) => Model = model;

        /// <summary>
        /// Size-dependent parameters
        /// </summary>
        internal BSIM4SizeDependParam pParam = null;

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
        public double BSIM4cd { get; internal set; }
        [SpiceName("ibs"), SpiceInfo("Ibs")]
        public double BSIM4cbs { get; internal set; }
        [SpiceName("ibd"), SpiceInfo("Ibd")]
        public double BSIM4cbd { get; internal set; }
        [SpiceName("isub"), SpiceInfo("Isub")]
        public double BSIM4csub { get; internal set; }
        [SpiceName("igidl"), SpiceInfo("Igidl")]
        public double BSIM4Igidl { get; internal set; }
        [SpiceName("igisl"), SpiceInfo("Igisl")]
        public double BSIM4Igisl { get; internal set; }
        [SpiceName("igs"), SpiceInfo("Igs")]
        public double BSIM4Igs { get; internal set; }
        [SpiceName("igd"), SpiceInfo("Igd")]
        public double BSIM4Igd { get; internal set; }
        [SpiceName("igb"), SpiceInfo("Igb")]
        public double BSIM4Igb { get; internal set; }
        [SpiceName("igcs"), SpiceInfo("Igcs")]
        public double BSIM4Igcs { get; internal set; }
        [SpiceName("igcd"), SpiceInfo("Igcd")]
        public double BSIM4Igcd { get; internal set; }
        [SpiceName("gm"), SpiceInfo("Gm")]
        public double BSIM4gm { get; internal set; }
        [SpiceName("gds"), SpiceInfo("Gds")]
        public double BSIM4gds { get; internal set; }
        [SpiceName("gmbs"), SpiceInfo("Gmb")]
        public double BSIM4gmbs { get; internal set; }
        [SpiceName("gbd"), SpiceInfo("gbd")]
        public double BSIM4gbd { get; internal set; }
        [SpiceName("gbs"), SpiceInfo("gbs")]
        public double BSIM4gbs { get; internal set; }
        [SpiceName("qb"), SpiceInfo("Qbulk")]
        public double BSIM4qbulk { get; internal set; }
        [SpiceName("qg"), SpiceInfo("Qgate")]
        public double BSIM4qgate { get; internal set; }
        [SpiceName("qs"), SpiceInfo("Qsource")]
        public double BSIM4qsrc { get; internal set; }
        [SpiceName("qd"), SpiceInfo("Qdrain")]
        public double BSIM4qdrn { get; internal set; }
        [SpiceName("gcrg"), SpiceInfo("Gcrg")]
        public double BSIM4gcrg { get; internal set; }
        [SpiceName("gtau"), SpiceInfo("Gtau")]
        public double BSIM4gtau { get; internal set; }
        [SpiceName("cgg"), SpiceInfo("Cggb")]
        public double BSIM4cggb { get; internal set; }
        [SpiceName("cgd"), SpiceInfo("Cgdb")]
        public double BSIM4cgdb { get; internal set; }
        [SpiceName("cgs"), SpiceInfo("Cgsb")]
        public double BSIM4cgsb { get; internal set; }
        [SpiceName("cdg"), SpiceInfo("Cdgb")]
        public double BSIM4cdgb { get; internal set; }
        [SpiceName("cdd"), SpiceInfo("Cddb")]
        public double BSIM4cddb { get; internal set; }
        [SpiceName("cds"), SpiceInfo("Cdsb")]
        public double BSIM4cdsb { get; internal set; }
        [SpiceName("cbg"), SpiceInfo("Cbgb")]
        public double BSIM4cbgb { get; internal set; }
        [SpiceName("cbd"), SpiceInfo("Cbdb")]
        public double BSIM4cbdb { get; internal set; }
        [SpiceName("cbs"), SpiceInfo("Cbsb")]
        public double BSIM4cbsb { get; internal set; }
        [SpiceName("csg"), SpiceInfo("Csgb")]
        public double BSIM4csgb { get; internal set; }
        [SpiceName("csd"), SpiceInfo("Csdb")]
        public double BSIM4csdb { get; internal set; }
        [SpiceName("css"), SpiceInfo("Cssb")]
        public double BSIM4cssb { get; internal set; }
        [SpiceName("cgb"), SpiceInfo("Cgbb")]
        public double BSIM4cgbb { get; internal set; }
        [SpiceName("cdb"), SpiceInfo("Cdbb")]
        public double BSIM4cdbb { get; internal set; }
        [SpiceName("csb"), SpiceInfo("Csbb")]
        public double BSIM4csbb { get; internal set; }
        [SpiceName("cbb"), SpiceInfo("Cbbb")]
        public double BSIM4cbbb { get; internal set; }
        [SpiceName("capbd"), SpiceInfo("Capbd")]
        public double BSIM4capbd { get; internal set; }
        [SpiceName("capbs"), SpiceInfo("Capbs")]
        public double BSIM4capbs { get; internal set; }
        [SpiceName("vth"), SpiceInfo("Vth")]
        public double BSIM4von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Vdsat")]
        public double BSIM4vdsat { get; internal set; }

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
        public double BSIM4u0temp { get; internal set; }
        public double BSIM4vsattemp { get; internal set; }
        public double BSIM4vth0 { get; internal set; }
        public double BSIM4eta0 { get; internal set; }
        public double BSIM4k2 { get; internal set; }
        public double BSIM4vfb { get; internal set; }
        public double BSIM4vtfbphi1 { get; internal set; }
        public double BSIM4vtfbphi2 { get; internal set; }
        public double BSIM4vbsc { get; internal set; }
        public double BSIM4k2ox { get; internal set; }
        public double BSIM4vfbzb { get; internal set; }
        public double BSIM4cgso { get; internal set; }
        public double BSIM4cgdo { get; internal set; }
        public double BSIM4grbdb { get; internal set; }
        public double BSIM4grbpb { get; internal set; }
        public double BSIM4grbps { get; internal set; }
        public double BSIM4grbsb { get; internal set; }
        public double BSIM4grbpd { get; internal set; }
        public double BSIM4grgeltd { get; internal set; }
        public double BSIM4Pseff { get; internal set; }
        public double BSIM4Pdeff { get; internal set; }
        public double BSIM4Aseff { get; internal set; }
        public double BSIM4Adeff { get; internal set; }
        public double BSIM4sourceConductance { get; internal set; }
        public double BSIM4drainConductance { get; internal set; }
        public double BSIM4XExpBVS { get; internal set; }
        public double BSIM4vjsmFwd { get; internal set; }
        public double BSIM4IVjsmFwd { get; internal set; }
        public double BSIM4SslpFwd { get; internal set; }
        public double BSIM4vjsmRev { get; internal set; }
        public double BSIM4IVjsmRev { get; internal set; }
        public double BSIM4SslpRev { get; internal set; }
        public double BSIM4XExpBVD { get; internal set; }
        public double BSIM4vjdmFwd { get; internal set; }
        public double BSIM4IVjdmFwd { get; internal set; }
        public double BSIM4DslpFwd { get; internal set; }
        public double BSIM4vjdmRev { get; internal set; }
        public double BSIM4IVjdmRev { get; internal set; }
        public double BSIM4DslpRev { get; internal set; }
        public double BSIM4SjctTempRevSatCur { get; internal set; }
        public double BSIM4DjctTempRevSatCur { get; internal set; }
        public double BSIM4SswTempRevSatCur { get; internal set; }
        public double BSIM4DswTempRevSatCur { get; internal set; }
        public double BSIM4SswgTempRevSatCur { get; internal set; }
        public double BSIM4DswgTempRevSatCur { get; internal set; }
        public double BSIM4toxp { get; internal set; }
        public double BSIM4coxp { get; internal set; }
        public double BSIM4mode { get; internal set; }
        public double BSIM4gbbs { get; internal set; }
        public double BSIM4ggidlb { get; internal set; }
        public double BSIM4gbgs { get; internal set; }
        public double BSIM4ggidlg { get; internal set; }
        public double BSIM4gbds { get; internal set; }
        public double BSIM4ggidld { get; internal set; }
        public double BSIM4ggisls { get; internal set; }
        public double BSIM4ggislg { get; internal set; }
        public double BSIM4ggislb { get; internal set; }
        public double BSIM4gIgsg { get; internal set; }
        public double BSIM4gIgcsg { get; internal set; }
        public double BSIM4gIgcsd { get; internal set; }
        public double BSIM4gIgcsb { get; internal set; }
        public double BSIM4gIgdg { get; internal set; }
        public double BSIM4gIgcdg { get; internal set; }
        public double BSIM4gIgcdd { get; internal set; }
        public double BSIM4gIgcdb { get; internal set; }
        public double BSIM4gIgbg { get; internal set; }
        public double BSIM4gIgbd { get; internal set; }
        public double BSIM4gIgbb { get; internal set; }
        public double BSIM4ggidls { get; internal set; }
        public double BSIM4ggisld { get; internal set; }
        public double BSIM4gstot { get; internal set; }
        public double BSIM4gstotd { get; internal set; }
        public double BSIM4gstotg { get; internal set; }
        public double BSIM4gstotb { get; internal set; }
        public double BSIM4gdtot { get; internal set; }
        public double BSIM4gdtotd { get; internal set; }
        public double BSIM4gdtotg { get; internal set; }
        public double BSIM4gdtotb { get; internal set; }
        public double BSIM4thetavth { get; internal set; }
        public double BSIM4nstar { get; internal set; }
        public double BSIM4vgs_eff { get; internal set; }
        public double BSIM4vgd_eff { get; internal set; }
        public double BSIM4dvgs_eff_dvg { get; internal set; }
        public double BSIM4dvgd_eff_dvg { get; internal set; }
        public double BSIM4Vgsteff { get; internal set; }
        public double BSIM4grdsw { get; internal set; }
        public double BSIM4Abulk { get; internal set; }
        public double BSIM4ueff { get; internal set; }
        public double BSIM4EsatL { get; internal set; }
        public double BSIM4Vdseff { get; internal set; }
        public double BSIM4Coxeff { get; internal set; }
        public double BSIM4AbovVgst2Vtm { get; internal set; }
        public double BSIM4IdovVds { get; internal set; }
        public double BSIM4gcrgd { get; internal set; }
        public double BSIM4gcrgb { get; internal set; }
        public double BSIM4gcrgg { get; internal set; }
        public double BSIM4gcrgs { get; internal set; }
        public double BSIM4gstots { get; internal set; }
        public double BSIM4gdtots { get; internal set; }
        public double BSIM4gIgss { get; internal set; }
        public double BSIM4gIgdd { get; internal set; }
        public double BSIM4gIgbs { get; internal set; }
        public double BSIM4gIgcss { get; internal set; }
        public double BSIM4gIgcds { get; internal set; }
        public double BSIM4qinv { get; internal set; }
        public double BSIM4noiGd0 { get; internal set; }
        public double BSIM4cqdb { get; internal set; }
        public double BSIM4cqsb { get; internal set; }
        public double BSIM4cqgb { get; internal set; }
        public double BSIM4cqbb { get; internal set; }
        public double BSIM4qchqs { get; internal set; }
        public double BSIM4taunet { get; internal set; }
        public double BSIM4qgdo { get; internal set; }
        public double BSIM4qgso { get; internal set; }
        public double BSIM4gtg { get; internal set; }
        public double BSIM4gtd { get; internal set; }
        public double BSIM4gts { get; internal set; }
        public double BSIM4gtb { get; internal set; }
        public int BSIM4dNodePrime { get; internal set; }
        public int BSIM4bNodePrime { get; internal set; }
        public int BSIM4gNodePrime { get; internal set; }
        public int BSIM4sNodePrime { get; internal set; }
        public int BSIM4dNode { get; internal set; }
        public int BSIM4sNode { get; internal set; }
        public int BSIM4qNode { get; internal set; }
        public int BSIM4gNodeExt { get; internal set; }
        public int BSIM4gNodeMid { get; internal set; }
        public int BSIM4dbNode { get; internal set; }
        public int BSIM4sbNode { get; internal set; }
        public int BSIM4bNode { get; internal set; }
        public int BSIM4states { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BSIM4vbd = 0;
        public const int BSIM4vbs = 1;
        public const int BSIM4vgs = 2;
        public const int BSIM4vds = 3;
        public const int BSIM4vdbs = 4;
        public const int BSIM4vdbd = 5;
        public const int BSIM4vsbs = 6;
        public const int BSIM4vges = 7;
        public const int BSIM4vgms = 8;
        public const int BSIM4vses = 9;
        public const int BSIM4vdes = 10;
        public const int BSIM4qb = 11;
        public const int BSIM4cqb = 12;
        public const int BSIM4qg = 13;
        public const int BSIM4cqg = 14;
        public const int BSIM4qd = 15;
        public const int BSIM4cqd = 16;
        public const int BSIM4qgmid = 17;
        public const int BSIM4cqgmid = 18;
        public const int BSIM4qbs = 19;
        public const int BSIM4cqbs = 20;
        public const int BSIM4qbd = 21;
        public const int BSIM4cqbd = 22;
        public const int BSIM4qcheq = 23;
        public const int BSIM4cqcheq = 24;
        public const int BSIM4qcdump = 25;
        public const int BSIM4cqcdump = 26;
        public const int BSIM4qdef = 27;
        public const int BSIM4qs = 28;
        public const int BSIM4pinCount = 4;

        public const double NMOS = 1.0;
        public const double PMOS = -1.0;
        public const double ScalingFactor = 1e-9;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM4v80(CircuitIdentifier name) : base(name, BSIM4pinCount)
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
            BSIM4dNode = nodes[0].Index;
            BSIM4gNodeExt = nodes[1].Index;
            BSIM4sNode = nodes[2].Index;
            BSIM4bNode = nodes[3].Index;

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
                    this.BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                    BSIM4rgeoMod, BSIM4min,
                    BSIM4w, model.BSIM4sheetResistance,
                    model.DMCGeff, model.DMCIeff, model.DMDGeff, 0, out Rtot);
                    if (Rtot > 0)
                        createNode = 1;
                }
            }
            if (createNode != 0)
                BSIM4dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
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
                    this.BSIM4RdseffGeo(BSIM4nf, BSIM4geoMod,
                    BSIM4rgeoMod, BSIM4min,
                    BSIM4w, model.BSIM4sheetResistance,
                    model.DMCGeff, model.DMCIeff, model.DMDGeff, 1, out Rtot);
                    if (Rtot > 0)
                        createNode = 1;
                }
            }
            if (createNode != 0)
                BSIM4sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                BSIM4sNodePrime = BSIM4sNode;

            if (BSIM4rgateMod > 0)
                BSIM4gNodePrime = CreateNode(ckt, Name.Grow("#gate")).Index;
            else
                BSIM4gNodePrime = BSIM4gNodeExt;

            if (BSIM4rgateMod.Value == 3)
            {
                BSIM4gNodeMid = CreateNode(ckt, Name.Grow("#gmid")).Index;
            }
            else
                BSIM4gNodeMid = BSIM4gNodeExt;

            /* internal body nodes for body resistance model */
            if ((BSIM4rbodyMod.Value == 1) || (BSIM4rbodyMod.Value == 2))
            {
                BSIM4dbNode = CreateNode(ckt, Name.Grow("#db")).Index;
                BSIM4bNodePrime = CreateNode(ckt, Name.Grow("#bulk")).Index;
                BSIM4sbNode = CreateNode(ckt, Name.Grow("#sb")).Index;
            }
            else
                BSIM4dbNode = BSIM4bNodePrime = BSIM4sbNode = BSIM4bNode;

            /* NQS node */
            if (BSIM4trnqsMod != 0)
                BSIM4qNode = CreateNode(ckt, Name.Grow("#q")).Index;
            else
                BSIM4qNode = 0;
        }
    }
}
