using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3Model : CircuitModel<MOS3Model>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS3vt0 { get; } = new Parameter();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter MOS3transconductance { get; } = new Parameter(2e-5);
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS3gamma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS3phi { get; } = new Parameter(.6);
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS3drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS3sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS3capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS3capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS3jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS3bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS3gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS3gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS3gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS3sheetResistance { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS3bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS3bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS3sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS3bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS3jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS3oxideThickness { get; } = new Parameter(1e-7);
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS3latDiff { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS3surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS3fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS3substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS3gateType { get; } = new Parameter();
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS3surfaceStateDensity { get; } = new Parameter();
        [SpiceName("eta"), SpiceInfo("Vds dependence of threshold voltage")]
        public Parameter MOS3eta { get; } = new Parameter();
        [SpiceName("nfs"), SpiceInfo("Fast surface state density")]
        public Parameter MOS3fastSurfaceStateDensity { get; } = new Parameter();
        [SpiceName("theta"), SpiceInfo("Vgs dependence on mobility")]
        public Parameter MOS3theta { get; } = new Parameter();
        [SpiceName("vmax"), SpiceInfo("Maximum carrier drift velocity")]
        public Parameter MOS3maxDriftVel { get; } = new Parameter();
        [SpiceName("kappa"), SpiceInfo("Kappa")]
        public Parameter MOS3kappa { get; } = new Parameter(.2);
        [SpiceName("xj"), SpiceInfo("Junction depth")]
        public Parameter MOS3junctionDepth { get; } = new Parameter();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double MOS3_TNOM
        {
            get => MOS3tnom - Circuit.CONSTCtoK;
            set => MOS3tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3tnom { get; } = new Parameter();
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS3fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS3fNexp { get; } = new Parameter(1);
        [SpiceName("xd"), SpiceInfo("Depletion layer width")]
        public double MOS3coeffDepLayWidth { get; private set; }
        [SpiceName("input_delta"), SpiceInfo("")]
        public double MOS3delta { get; private set; }
        [SpiceName("alpha"), SpiceInfo("Alpha")]
        public double MOS3alpha { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("delta"), SpiceInfo("Width effect on threshold")]
        public double DELTA
        {
            get => MOS3narrowFactor;
            set => MOS3delta = value;
        }
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS3type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS3type = -1.0;
        }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS3type > 0)
                return "nmos";
            else
                return "pmos";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double fact1 { get; private set; }
        public double vtnom { get; private set; }
        public double egfet1 { get; private set; }
        public double pbfact1 { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS3type { get; private set; } = 1.0;
        public double MOS3oxideCapFactor { get; private set; }
        public double MOS3narrowFactor { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3Model(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb;

            if (!MOS3tnom.Given)
            {
                MOS3tnom.Value = ckt.State.NominalTemperature;
            }
            fact1 = MOS3tnom / Circuit.CONSTRefTemp;
            vtnom = MOS3tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS3tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS3tnom * MOS3tnom) / (MOS3tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            MOS3oxideCapFactor = 3.9 * 8.854214871e-12 / MOS3oxideThickness;
            if (!MOS3surfaceMobility.Given)
                MOS3surfaceMobility.Value = 600;
            if (!MOS3transconductance.Given)
            {
                MOS3transconductance.Value = MOS3surfaceMobility * MOS3oxideCapFactor * 1e-4;
            }
            if (MOS3substrateDoping.Given)
            {
                if (MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */ > 1.45e16)
                {
                    if (!MOS3phi.Given)
                    {
                        MOS3phi.Value = 2 * vtnom * Math.Log(MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */  / 1.45e16);
                        MOS3phi.Value = Math.Max(.1, MOS3phi);
                    }
                    fermis = MOS3type * .5 * MOS3phi;
                    wkfng = 3.2;
                    if (!MOS3gateType.Given)
                        MOS3gateType.Value = 1;
                    if (MOS3gateType != 0)
                    {
                        fermig = MOS3type * MOS3gateType * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!MOS3gamma.Given)
                    {
                        MOS3gamma.Value = Math.Sqrt(2 * Transistor.EPSSIL * Circuit.CHARGE * MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */) /
                            MOS3oxideCapFactor;
                    }
                    if (!MOS3vt0.Given)
                    {
                        if (!MOS3surfaceStateDensity.Given)
                            MOS3surfaceStateDensity.Value = 0;
                        vfb = wkfngs - MOS3surfaceStateDensity * 1e4 * Circuit.CHARGE / MOS3oxideCapFactor;
                        MOS3vt0.Value = vfb + MOS3type * (MOS3gamma * Math.Sqrt(MOS3phi) + MOS3phi);
                    }
                    else
                    {
                        vfb = MOS3vt0 - MOS3type * (MOS3gamma * Math.Sqrt(MOS3phi) + MOS3phi);
                    }
                    MOS3alpha = (Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.CHARGE * MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */);
                    MOS3coeffDepLayWidth = Math.Sqrt(MOS3alpha);
                }
                else
                {
                    MOS3substrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            /* now model parameter preprocessing */
            MOS3narrowFactor = MOS3delta * 0.5 * Circuit.CONSTPI * Transistor.EPSSIL / MOS3oxideCapFactor;
        }
    }
}
