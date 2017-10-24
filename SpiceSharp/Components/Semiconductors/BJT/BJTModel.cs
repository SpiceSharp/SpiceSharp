using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BJT"/>
    /// </summary>
    public class BJTModel : CircuitModel<BJTModel>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BJTModel()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(BJTModel), typeof(ComponentBehaviors.BJTModelTemperatureBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double BJT_TNOM
        {
            get => BJTtnom - Circuit.CONSTCtoK;
            set => BJTtnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtnom { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Saturation Current")]
        public Parameter BJTsatCur { get; } = new Parameter(1e-16);
        [SpiceName("bf"), SpiceInfo("Ideal forward beta")]
        public Parameter BJTbetaF { get; } = new Parameter(100);
        [SpiceName("nf"), SpiceInfo("Forward emission coefficient")]
        public Parameter BJTemissionCoeffF { get; } = new Parameter(1);
        [SpiceName("vaf"), SpiceName("va"), SpiceInfo("Forward Early voltage")]
        public Parameter BJTearlyVoltF { get; } = new Parameter();
        [SpiceName("ikf"), SpiceName("ik"), SpiceInfo("Forward beta roll-off corner current")]
        public Parameter BJTrollOffF { get; } = new Parameter();
        [SpiceName("ise"), SpiceInfo("B-E leakage saturation current")]
        public Parameter BJTleakBEcurrent { get; } = new Parameter();
        [SpiceName("ne"), SpiceInfo("B-E leakage emission coefficient")]
        public Parameter BJTleakBEemissionCoeff { get; } = new Parameter(1.5);
        [SpiceName("br"), SpiceInfo("Ideal reverse beta")]
        public Parameter BJTbetaR { get; } = new Parameter(1);
        [SpiceName("nr"), SpiceInfo("Reverse emission coefficient")]
        public Parameter BJTemissionCoeffR { get; } = new Parameter(1);
        [SpiceName("var"), SpiceName("vb"), SpiceInfo("Reverse Early voltage")]
        public Parameter BJTearlyVoltR { get; } = new Parameter();
        [SpiceName("ikr"), SpiceInfo("reverse beta roll-off corner current")]
        public Parameter BJTrollOffR { get; } = new Parameter();
        [SpiceName("isc"), SpiceInfo("B-C leakage saturation current")]
        public Parameter BJTleakBCcurrent { get; } = new Parameter();
        [SpiceName("nc"), SpiceInfo("B-C leakage emission coefficient")]
        public Parameter BJTleakBCemissionCoeff { get; } = new Parameter(2);
        [SpiceName("rb"), SpiceInfo("Zero bias base resistance")]
        public Parameter BJTbaseResist { get; } = new Parameter();
        [SpiceName("irb"), SpiceInfo("Current for base resistance=(rb+rbm)/2")]
        public Parameter BJTbaseCurrentHalfResist { get; } = new Parameter();
        [SpiceName("rbm"), SpiceInfo("Minimum base resistance")]
        public Parameter BJTminBaseResist { get; } = new Parameter();
        [SpiceName("re"), SpiceInfo("Emitter resistance")]
        public Parameter BJTemitterResist { get; } = new Parameter();
        [SpiceName("rc"), SpiceInfo("Collector resistance")]
        public Parameter BJTcollectorResist { get; } = new Parameter();
        [SpiceName("cje"), SpiceInfo("Zero bias B-E depletion capacitance")]
        public Parameter BJTdepletionCapBE { get; } = new Parameter();
        [SpiceName("vje"), SpiceName("pe"), SpiceInfo("B-E built in potential")]
        public Parameter BJTpotentialBE { get; } = new Parameter(0.75);
        [SpiceName("mje"), SpiceName("me"), SpiceInfo("B-E junction grading coefficient")]
        public Parameter BJTjunctionExpBE { get; } = new Parameter(.33);
        [SpiceName("tf"), SpiceInfo("Ideal forward transit time")]
        public Parameter BJTtransitTimeF { get; } = new Parameter();
        [SpiceName("xtf"), SpiceInfo("Coefficient for bias dependence of TF")]
        public Parameter BJTtransitTimeBiasCoeffF { get; } = new Parameter();
        [SpiceName("vtf"), SpiceInfo("Voltage giving VBC dependence of TF")]
        public Parameter BJTtransitTimeFVBC { get; } = new Parameter();
        [SpiceName("itf"), SpiceInfo("High current dependence of TF")]
        public Parameter BJTtransitTimeHighCurrentF { get; } = new Parameter();
        [SpiceName("ptf"), SpiceInfo("Excess phase")]
        public Parameter BJTexcessPhase { get; } = new Parameter();
        [SpiceName("cjc"), SpiceInfo("Zero bias B-C depletion capacitance")]
        public Parameter BJTdepletionCapBC { get; } = new Parameter();
        [SpiceName("vjc"), SpiceName("pc"), SpiceInfo("B-C built in potential")]
        public Parameter BJTpotentialBC { get; } = new Parameter(0.75);
        [SpiceName("mjc"), SpiceName("mc"), SpiceInfo("B-C junction grading coefficient")]
        public Parameter BJTjunctionExpBC { get; } = new Parameter(.33);
        [SpiceName("xcjc"), SpiceInfo("Fraction of B-C cap to internal base")]
        public Parameter BJTbaseFractionBCcap { get; } = new Parameter(1);
        [SpiceName("tr"), SpiceInfo("Ideal reverse transit time")]
        public Parameter BJTtransitTimeR { get; } = new Parameter();
        [SpiceName("cjs"), SpiceName("ccs"), SpiceInfo("Zero bias C-S capacitance")]
        public Parameter BJTcapCS { get; } = new Parameter();
        [SpiceName("vjs"), SpiceName("ps"), SpiceInfo("Substrate junction built in potential")]
        public Parameter BJTpotentialSubstrate { get; } = new Parameter(.75);
        [SpiceName("mjs"), SpiceName("ms"), SpiceInfo("Substrate junction grading coefficient")]
        public Parameter BJTexponentialSubstrate { get; } = new Parameter();
        [SpiceName("xtb"), SpiceInfo("Forward and reverse beta temp. exp.")]
        public Parameter BJTbetaExp { get; } = new Parameter();
        [SpiceName("eg"), SpiceInfo("Energy gap for IS temp. dependency")]
        public Parameter BJTenergyGap { get; } = new Parameter(1.11);
        [SpiceName("xti"), SpiceInfo("Temp. exponent for IS")]
        public Parameter BJTtempExpIS { get; } = new Parameter(3);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parameter")]
        public Parameter BJTdepletionCapCoeff { get; } = new Parameter();
        [SpiceName("kf"), SpiceInfo("Flicker Noise Coefficient")]
        public Parameter BJTfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker Noise Exponent")]
        public Parameter BJTfNexp { get; } = new Parameter(1);
        [SpiceName("invearlyvoltf"), SpiceInfo("Inverse early voltage:forward")]
        public double BJTinvEarlyVoltF { get; internal set; }
        [SpiceName("invearlyvoltr"), SpiceInfo("Inverse early voltage:reverse")]
        public double BJTinvEarlyVoltR { get; internal set; }
        [SpiceName("invrollofff"), SpiceInfo("Inverse roll off - forward")]
        public double BJTinvRollOffF { get; internal set; }
        [SpiceName("invrolloffr"), SpiceInfo("Inverse roll off - reverse")]
        public double BJTinvRollOffR { get; internal set; }
        [SpiceName("collectorconduct"), SpiceInfo("Collector conductance")]
        public double BJTcollectorConduct { get; internal set; }
        [SpiceName("emitterconduct"), SpiceInfo("Emitter conductance")]
        public double BJTemitterConduct { get; internal set; }
        [SpiceName("transtimevbcfact"), SpiceInfo("Transit time VBC factor")]
        public double BJTtransitTimeVBCFactor { get; internal set; }
        [SpiceName("excessphasefactor"), SpiceInfo("Excess phase fact.")]
        public double BJTexcessPhaseFactor { get; internal set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("npn"), SpiceInfo("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [SpiceName("pnp"), SpiceInfo("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [SpiceName("type"), SpiceInfo("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        internal double fact1;
        internal double xfc;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double BJTtype { get; internal set; }
        public Parameter BJTc2 { get; } = new Parameter();
        public Parameter BJTc4 { get; } = new Parameter();
        public double BJTf2 { get; internal set; }
        public double BJTf3 { get; internal set; }
        public double BJTf6 { get; internal set; }
        public double BJTf7 { get; internal set; }

        private const int NPN = 1;
        private const int PNP = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if (BJTtype != NPN && BJTtype != PNP)
                BJTtype = NPN;
        }
    }
}
