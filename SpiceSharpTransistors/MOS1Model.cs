using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    public class MOS1Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature (in Kelvin)")]
        public Parameter<double> MOS1tnom = new Parameter<double>();
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter<double> MOS1vt0 { get; } = new Parameter<double>();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter<double> MOS1transconductance { get; } = new Parameter<double>(2e-5);
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter<double> MOS1gamma { get; } = new Parameter<double>();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter<double> MOS1phi { get; } = new Parameter<double>(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation")]
        public Parameter<double> MOS1lambda { get; } = new Parameter<double>();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter<double> MOS1drainResistance { get; } = new Parameter<double>();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter<double> MOS1sourceResistance { get; } = new Parameter<double>();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter<double> MOS1capBD { get; } = new Parameter<double>();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter<double> MOS1capBS { get; } = new Parameter<double>();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter<double> MOS1jctSatCur { get; } = new Parameter<double>(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter<double> MOS1bulkJctPotential { get; } = new Parameter<double>(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter<double> MOS1gateSourceOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter<double> MOS1gateDrainOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter<double> MOS1gateBulkOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter<double> MOS1bulkCapFactor { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter<double> MOS1bulkJctBotGradingCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter<double> MOS1sideWallCapFactor { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter<double> MOS1bulkJctSideGradingCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter<double> MOS1jctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter<double> MOS1oxideThickness { get; } = new Parameter<double>();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter<double> MOS1latDiff { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter<double> MOS1sheetResistance { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter<double> MOS1surfaceMobility { get; } = new Parameter<double>();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter<double> MOS1fwdCapDepCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter<double> MOS1surfaceStateDensity { get; } = new Parameter<double>();
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter<double> MOS1substrateDoping { get; } = new Parameter<double>();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public int MOS1type { get; private set; } = NMOS;
        [SpiceName("nmos"), SpiceInfo("NMOS type")]
        public void SetNMOS() { MOS1type = NMOS; }
        [SpiceName("pmos"), SpiceInfo("PMOS type")]
        public void SetPMOS() { MOS1type = PMOS; }
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter<double> MOS1fNcoef { get; } = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter<double> MOS1fNexp { get; } = new Parameter<double>(1);

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
        public double MOS1oxideCapFactor { get; private set; }

        private const int NMOS = 1;
        private const int PMOS = -1;

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
            double kt1;
            double arg1;
            double fermis;
            double wkfng;
            double fermig;
            double wkfngs;
            double vfb;

            /* perform model defaulting */
            if (!MOS1tnom.Given)
            {
                MOS1tnom.Value = ckt.State.NominalTemperature;
            }

            fact1 = MOS1tnom / Circuit.CONSTRefTemp;
            vtnom = MOS1tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS1tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS1tnom * MOS1tnom) /
            (MOS1tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            /* now model parameter preprocessing */

            if (!MOS1oxideThickness.Given || MOS1oxideThickness == 0)
            {
                MOS1oxideCapFactor = 0;
            }
            else
            {
                MOS1oxideCapFactor = 3.9 * 8.854214871e-12 /
                MOS1oxideThickness;
                if (!MOS1transconductance.Given)
                {
                    if (!MOS1surfaceMobility.Given)
                    {
                        MOS1surfaceMobility.Value = 600;
                    }
                    MOS1transconductance.Value = MOS1surfaceMobility *
                    MOS1oxideCapFactor * 1e-4 /*(m**2/cm**2)*/;
                }
                if (MOS1substrateDoping.Given)
                {
                    if (MOS1substrateDoping * 1e6 /*(cm**3/m**3)*/ > 1.45e16)
                    {
                        if (!MOS1phi.Given)
                        {
                            MOS1phi.Value = 2 * vtnom *
                            Math.Log(MOS1substrateDoping *
                            1e6/*(cm**3/m**3)*// 1.45e16);
                            MOS1phi.Value = Math.Max(.1, MOS1phi);
                        }
                        fermis = MOS1type * .5 * MOS1phi;
                        wkfng = 3.2;
                        if (MOS1type != 0)
                        {
                            fermig = MOS1type * MOS1type * .5 * egfet1;
                            wkfng = 3.25 + .5 * egfet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                        if (!MOS1gamma.Given)
                        {
                            MOS1gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 *
                            Circuit.CHARGE * MOS1substrateDoping *
                            1e6/*(cm**3/m**3)*/) /
                            MOS1oxideCapFactor;
                        }
                        if (!MOS1vt0.Given)
                        {
                            if (!MOS1surfaceStateDensity.Given)
                                MOS1surfaceStateDensity.Value = 0;
                            vfb = wkfngs -
                            MOS1surfaceStateDensity *
                            1e4 /*(cm**2/m**2)*/ *
                            Circuit.CHARGE / MOS1oxideCapFactor;
                            MOS1vt0.Value = vfb + MOS1type *
                            (MOS1gamma * Math.Sqrt(MOS1phi) +
                            MOS1phi);
                        }
                    }
                    else
                    {
                        MOS1substrateDoping.Value = 0;
                        throw new CircuitException($"{Name}: Nsub < Ni");
                    }
                }
            }

            /* loop through all instances of the model */
        }
    }
}
