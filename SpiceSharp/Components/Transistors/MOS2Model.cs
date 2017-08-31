using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The MOS2 model for MOSFETs
    /// </summary>
    public class MOS2Model : CircuitModel<MOS2Model>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double MOS2_TNOM
        {
            get => MOS2tnom - Circuit.CONSTCtoK;
            set => MOS2tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2tnom { get; } = new Parameter();
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS2vt0 { get; } = new Parameter();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter MOS2transconductance { get; } = new Parameter();
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS2gamma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS2phi { get; } = new Parameter(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation")]
        public Parameter MOS2lambda { get; } = new Parameter();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS2drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS2sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS2capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS2capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS2jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS2bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS2gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS2gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS2gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS2bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS2bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS2sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS2bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS2jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS2oxideThickness { get; } = new Parameter();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS2latDiff { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS2sheetResistance { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS2surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS2fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS2substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS2gateType { get; } = new Parameter();
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS2surfaceStateDensity { get; } = new Parameter();
        [SpiceName("nfs"), SpiceInfo("Fast surface state density")]
        public Parameter MOS2fastSurfaceStateDensity { get; } = new Parameter();
        [SpiceName("delta"), SpiceInfo("Width effect on threshold")]
        public Parameter MOS2narrowFactor { get; } = new Parameter();
        [SpiceName("uexp"), SpiceInfo("Crit. field exp for mob. deg.")]
        public Parameter MOS2critFieldExp { get; } = new Parameter();
        [SpiceName("vmax"), SpiceInfo("Maximum carrier drift velocity")]
        public Parameter MOS2maxDriftVel { get; } = new Parameter();
        [SpiceName("xj"), SpiceInfo("Junction depth")]
        public Parameter MOS2junctionDepth { get; } = new Parameter();
        [SpiceName("neff"), SpiceInfo("Total channel charge coeff.")]
        public Parameter MOS2channelCharge { get; } = new Parameter(1);
        [SpiceName("ucrit"), SpiceInfo("Crit. field for mob. degradation")]
        public Parameter MOS2critField { get; } = new Parameter(1e4);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS2fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS2fNexp { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS2type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS2type = -1.0;
        }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS2type > 0)
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
        public double MOS2type { get; private set; } = 1.0;
        public double MOS2oxideCapFactor { get; private set; }
        public double MOS2xd { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* perform model defaulting */

            /* now model parameter preprocessing */
            if (!MOS2tnom.Given)
                MOS2tnom.Value = ckt.State.NominalTemperature;
            fact1 = MOS2tnom / Circuit.CONSTRefTemp;
            vtnom = MOS2tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS2tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS2tnom * MOS2tnom) / (MOS2tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            if (!MOS2oxideThickness.Given)
            {
                MOS2oxideThickness.Value = 1e-7;
            }
            MOS2oxideCapFactor = 3.9 * 8.854214871e-12 / MOS2oxideThickness;

            if (!MOS2surfaceMobility.Given)
                MOS2surfaceMobility.Value = 600;
            if (!MOS2transconductance.Given)
            {
                MOS2transconductance.Value = MOS2surfaceMobility * 1e-4 * MOS2oxideCapFactor;
            }
            if (MOS2substrateDoping.Given)
            {
                if (MOS2substrateDoping * 1e6 > 1.45e16)
                {
                    if (!MOS2phi.Given)
                    {
                        MOS2phi.Value = 2 * vtnom * Math.Log(MOS2substrateDoping * 1e6 / 1.45e16);
                        MOS2phi.Value = Math.Max(.1, MOS2phi);
                    }
                    fermis = MOS2type * .5 * MOS2phi;
                    wkfng = 3.2;
                    if (!MOS2gateType.Given)
                        MOS2gateType.Value = 1;
                    if (MOS2gateType != 0)
                    {
                        fermig = MOS2type * MOS2gateType * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!MOS2gamma.Given)
                    {
                        MOS2gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.CHARGE * MOS2substrateDoping * 1e6) / MOS2oxideCapFactor;
					}
					if (!MOS2vt0.Given)
					{
						if (!MOS2surfaceStateDensity.Given)
						    MOS2surfaceStateDensity.Value = 0;
						vfb = wkfngs - MOS2surfaceStateDensity * 1e4 * Circuit.CHARGE / MOS2oxideCapFactor;
                        MOS2vt0.Value = vfb + MOS2type * (MOS2gamma * Math.Sqrt(MOS2phi) + MOS2phi);
                    }
                    else
                    {
                        vfb = MOS2vt0 - MOS2type * (MOS2gamma * Math.Sqrt(MOS2phi) + MOS2phi);
                    }
                    MOS2xd = Math.Sqrt((Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.CHARGE * MOS2substrateDoping * 1e6));
                }
                else
                {
                    MOS2substrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            if (!MOS2bulkCapFactor.Given)
            {
                MOS2bulkCapFactor.Value = Math.Sqrt(Transistor.EPSSIL * Circuit.CHARGE * MOS2substrateDoping * 1e6 /* cm *  * 3 / m *  * 3 */  / (2 *
                    MOS2bulkJctPotential));
            }
        }
    }
}
