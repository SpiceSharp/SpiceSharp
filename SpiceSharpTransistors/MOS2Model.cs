using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    public class MOS2Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public Parameter<double> MOS2tnom { get; } = new Parameter<double>();
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter<double> MOS2vt0 { get; } = new Parameter<double>();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter<double> MOS2transconductance { get; } = new Parameter<double>();
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter<double> MOS2gamma { get; } = new Parameter<double>();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter<double> MOS2phi { get; } = new Parameter<double>(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation")]
        public Parameter<double> MOS2lambda { get; } = new Parameter<double>();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter<double> MOS2drainResistance { get; } = new Parameter<double>();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter<double> MOS2sourceResistance { get; } = new Parameter<double>();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter<double> MOS2capBD { get; } = new Parameter<double>();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter<double> MOS2capBS { get; } = new Parameter<double>();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter<double> MOS2jctSatCur { get; } = new Parameter<double>(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter<double> MOS2bulkJctPotential { get; } = new Parameter<double>(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter<double> MOS2gateSourceOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter<double> MOS2gateDrainOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter<double> MOS2gateBulkOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter<double> MOS2bulkCapFactor { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter<double> MOS2bulkJctBotGradingCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter<double> MOS2sideWallCapFactor { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter<double> MOS2bulkJctSideGradingCoeff { get; } = new Parameter<double>(.33);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter<double> MOS2jctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter<double> MOS2oxideThickness { get; } = new Parameter<double>(1e-7);
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter<double> MOS2latDiff { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter<double> MOS2sheetResistance { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter<double> MOS2surfaceMobility { get; } = new Parameter<double>(600);
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter<double> MOS2fwdCapDepCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter<double> MOS2substrateDoping { get; } = new Parameter<double>();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public int MOS2type { get; private set; } = NMOS;
        [SpiceName("nmos"), SpiceInfo("NMOS type")]
        public void SetNMOS() { MOS2type = NMOS; }
        [SpiceName("pmos"), SpiceInfo("PMOS type")]
        public void SetPMOS() { MOS2type = PMOS; }
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter<double> MOS2surfaceStateDensity { get; } = new Parameter<double>();
        [SpiceName("nfs"), SpiceInfo("Fast surface state density")]
        public Parameter<double> MOS2fastSurfaceStateDensity { get; } = new Parameter<double>();
        [SpiceName("delta"), SpiceInfo("Width effect on threshold")]
        public Parameter<double> MOS2narrowFactor { get; } = new Parameter<double>();
        [SpiceName("uexp"), SpiceInfo("Crit. field exp for mob. deg.")]
        public Parameter<double> MOS2critFieldExp { get; } = new Parameter<double>();
        [SpiceName("vmax"), SpiceInfo("Maximum carrier drift velocity")]
        public Parameter<double> MOS2maxDriftVel { get; } = new Parameter<double>();
        [SpiceName("xj"), SpiceInfo("Junction depth")]
        public Parameter<double> MOS2junctionDepth { get; } = new Parameter<double>();
        [SpiceName("neff"), SpiceInfo("Total channel charge coeff.")]
        public Parameter<double> MOS2channelCharge { get; } = new Parameter<double>(1);
        [SpiceName("ucrit"), SpiceInfo("Crit. field for mob. degradation")]
        public Parameter<double> MOS2critField { get; } = new Parameter<double>(1e4);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter<double> MOS2fNcoef { get; } = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter<double> MOS2fNexp { get; } = new Parameter<double>(1);

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
        public double MOS2oxideCapFactor { get; private set; }
        public double MOS2xd { get; private set; }

        private const int NMOS = 1;
        private const int PMOS = -1;
        private const double EPSSIL = 11.7 * 8.854214871e-12;

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
            double kt1;
            double arg1;
            double fermis;
            double wkfng;
            double fermig;
            double wkfngs;
            double vfb;

            /* perform model defaulting */

            /* now model parameter preprocessing */
            if (!MOS2tnom.Given)
                MOS2tnom.Value = ckt.State.NominalTemperature;
            fact1 = MOS2tnom / Circuit.CONSTRefTemp;
            vtnom = MOS2tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS2tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS2tnom * MOS2tnom) /
            (MOS2tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            MOS2oxideCapFactor = 3.9 * 8.854214871e-12 / MOS2oxideThickness;

            if (!MOS2transconductance.Given)
            {
                MOS2transconductance.Value = MOS2surfaceMobility * 1e-4 /*(m**2/cm**2) */ 
                    * MOS2oxideCapFactor;
            }
            if (MOS2substrateDoping.Given)
            {
                if (MOS2substrateDoping * 1e6 /*(cm**3/m**3)*/ > 1.45e16)
                {
                    if (!MOS2phi.Given)
                    {
                        MOS2phi.Value = 2 * vtnom *
                        Math.Log(MOS2substrateDoping *
                        1e6 /*(cm**3/m**3)*// 1.45e16);
                        MOS2phi.Value = Math.Max(.1, MOS2phi);
                    }
                    fermis = MOS2type * .5 * MOS2phi;
                    wkfng = 3.2;
                    if (MOS2type != 0)
                    {
                        fermig = MOS2type * MOS2type * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!MOS2gamma.Given)
                    {
                        MOS2gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 *
                            Circuit.CHARGE * MOS2substrateDoping *
                            1e6 /*(cm**3/m**3)*/) / MOS2oxideCapFactor;
                    }
                    if (!MOS2vt0.Given)
                    {
                        if (!MOS2surfaceStateDensity.Given)
                            MOS2surfaceStateDensity.Value = 0;
                        vfb = wkfngs -
                        MOS2surfaceStateDensity *
                        1e4 /*(cm**2/m**2)*/ * Circuit.CHARGE / MOS2oxideCapFactor;
                        MOS2vt0.Value = vfb + MOS2type *
                            (MOS2gamma * Math.Sqrt(MOS2phi) +
                            MOS2phi);
                    }
                    else
                    {
                        vfb = MOS2vt0 - MOS2type * (MOS2gamma *
                        Math.Sqrt(MOS2phi) + MOS2phi);
                    }
                    MOS2xd = Math.Sqrt((EPSSIL + EPSSIL) /
                    (Circuit.CHARGE * MOS2substrateDoping * 1e6 /*(cm**3/m**3)*/));
                }
                else
                {
                    MOS2substrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            if (!MOS2bulkCapFactor.Given)
            {
                MOS2bulkCapFactor.Value = Math.Sqrt(EPSSIL * Circuit.CHARGE *
                    MOS2substrateDoping * 1e6 /*cm**3/m**3*/
                    / (2 * MOS2bulkJctPotential));
            }

            /* loop through all instances of the model */
        }
    }
}
