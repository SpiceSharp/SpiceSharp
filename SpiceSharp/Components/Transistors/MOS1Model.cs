using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This is the model for MOS1 MOSFETs
    /// </summary>
    public class MOS1Model : CircuitModel<MOS1Model>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double MOS1_TNOM
        {
            get => MOS1tnom - Circuit.CONSTCtoK;
            set => MOS1tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1tnom { get; } = new Parameter();
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS1vt0 { get; } = new Parameter();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter MOS1transconductance { get; } = new Parameter(2e-5);
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS1gamma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS1phi { get; } = new Parameter(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation")]
        public Parameter MOS1lambda { get; } = new Parameter();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS1drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS1sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS1capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS1capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS1jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS1bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS1gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS1gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS1gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS1bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS1bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS1sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS1bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS1jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS1oxideThickness { get; } = new Parameter();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS1latDiff { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS1sheetResistance { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS1surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS1fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS1surfaceStateDensity { get; } = new Parameter();
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS1substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS1gateType { get; } = new Parameter();
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS1type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS1type = -1.0;
        }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS1type > 0)
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
        public double MOS1type { get; private set; } = 1.0;
        public double MOS1modName { get; private set; }
        public double MOS1oxideCapFactor { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1Model(string name) : base(name)
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
            if (!MOS1tnom.Given)
                MOS1tnom.Value = ckt.State.NominalTemperature;

            fact1 = MOS1tnom / Circuit.CONSTRefTemp;
            vtnom = MOS1tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS1tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS1tnom * MOS1tnom) / (MOS1tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            /* now model parameter preprocessing */

            if (!MOS1oxideThickness.Given || MOS1oxideThickness.Value == 0)
            {
                MOS1oxideCapFactor = 0;
            }
            else
            {
                MOS1oxideCapFactor = 3.9 * 8.854214871e-12 / MOS1oxideThickness;
                if (!MOS1transconductance.Given)
                {
                    if (!MOS1surfaceMobility.Given)
                    {
                        MOS1surfaceMobility.Value = 600;
                    }
                    MOS1transconductance.Value = MOS1surfaceMobility * MOS1oxideCapFactor * 1e-4;
                }
                if (MOS1substrateDoping.Given)
                {
                    if (MOS1substrateDoping * 1e6 > 1.45e16)
                    {
                        if (!MOS1phi.Given)
                        {
                            MOS1phi.Value = 2 * vtnom * Math.Log(MOS1substrateDoping * 1e6 / 1.45e16);
                            MOS1phi.Value = Math.Max(.1, MOS1phi);
                        }
                        fermis = MOS1type * .5 * MOS1phi;
                        wkfng = 3.2;
                        if (!MOS1gateType.Given)
                            MOS1gateType.Value = 1;
                        if (MOS1gateType != 0)
                        {
                            fermig = MOS1type * MOS1gateType * .5 * egfet1;
                            wkfng = 3.25 + .5 * egfet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                        if (!MOS1gamma.Given)
                        {
                            MOS1gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.CHARGE * MOS1substrateDoping * 1e6) / MOS1oxideCapFactor;
						}
						if (!MOS1vt0.Given)
						{
							if (!MOS1surfaceStateDensity.Given)
							MOS1surfaceStateDensity.Value = 0;
							vfb = wkfngs - MOS1surfaceStateDensity * 1e4 * Circuit.CHARGE / MOS1oxideCapFactor;
                            MOS1vt0.Value = vfb + MOS1type * (MOS1gamma * Math.Sqrt(MOS1phi) + MOS1phi);
                        }
                    }
                    else
                    {
                        MOS1substrateDoping.Value = 0;
                        throw new CircuitException($"{Name}: Nsub < Ni");
                    }
                }
            }
        }
    }
}
