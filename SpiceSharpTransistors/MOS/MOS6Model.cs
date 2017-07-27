using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class MOS6Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public ParameterMethod<double> MOS6tnom { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter<double> MOS6vt0 { get; } = new Parameter<double>();
        [SpiceName("kv"), SpiceInfo("Saturation voltage factor")]
        public Parameter<double> MOS6kv { get; } = new Parameter<double>(2);
        [SpiceName("nv"), SpiceInfo("Saturation voltage coeff.")]
        public Parameter<double> MOS6nv { get; } = new Parameter<double>();
        [SpiceName("kc"), SpiceInfo("Saturation current factor")]
        public Parameter<double> MOS6kc { get; } = new Parameter<double>(5e-5);
        [SpiceName("nc"), SpiceInfo("Saturation current coeff.")]
        public Parameter<double> MOS6nc { get; } = new Parameter<double>(1);
        [SpiceName("nvth"), SpiceInfo("Threshold voltage coeff.")]
        public Parameter<double> MOS6nvth { get; } = new Parameter<double>();
        [SpiceName("ps"), SpiceInfo("Sat. current modification  par.")]
        public Parameter<double> MOS6ps { get; } = new Parameter<double>();
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter<double> MOS6gamma { get; } = new Parameter<double>();
        [SpiceName("gamma1"), SpiceInfo("Bulk threshold parameter 1")]
        public Parameter<double> MOS6gamma1 { get; } = new Parameter<double>();
        [SpiceName("sigma"), SpiceInfo("Static feedback effect par.")]
        public Parameter<double> MOS6sigma { get; } = new Parameter<double>();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter<double> MOS6phi { get; } = new Parameter<double>(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation param.")]
        public Parameter<double> MOS6lambda { get; } = new Parameter<double>();
        [SpiceName("lambda0"), SpiceInfo("Channel length modulation param. 0")]
        public Parameter<double> MOS6lamda0 { get; } = new Parameter<double>();
        [SpiceName("lambda1"), SpiceInfo("Channel length modulation param. 1")]
        public Parameter<double> MOS6lamda1 { get; } = new Parameter<double>();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter<double> MOS6drainResistance { get; } = new Parameter<double>();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter<double> MOS6sourceResistance { get; } = new Parameter<double>();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter<double> MOS6capBD { get; } = new Parameter<double>();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter<double> MOS6capBS { get; } = new Parameter<double>();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter<double> MOS6jctSatCur { get; } = new Parameter<double>(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter<double> MOS6bulkJctPotential { get; } = new Parameter<double>(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter<double> MOS6gateSourceOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter<double> MOS6gateDrainOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter<double> MOS6gateBulkOverlapCapFactor { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter<double> MOS6bulkCapFactor { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter<double> MOS6bulkJctBotGradingCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter<double> MOS6sideWallCapFactor { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter<double> MOS6bulkJctSideGradingCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter<double> MOS6jctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter<double> MOS6oxideThickness { get; } = new Parameter<double>();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter<double> MOS6latDiff { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter<double> MOS6sheetResistance { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter<double> MOS6surfaceMobility { get; } = new Parameter<double>();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter<double> MOS6fwdCapDepCoeff { get; } = new Parameter<double>(.5);
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter<double> MOS6surfaceStateDensity { get; } = new Parameter<double>();
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter<double> MOS6substrateDoping { get; } = new Parameter<double>();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter<int> MOS6gateType { get; } = new Parameter<int>();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value) { MOS6type = 1; }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value) { MOS6type = -1; }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS6type > 0)
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
        public int MOS6type { get; private set; } = 1;
        public double MOS6oxideCapFactor { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS6Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if (!MOS6lamda0.Given)
            {
                MOS6lamda0.Value = 0;
                if (MOS6lambda.Given)
                {
                    MOS6lamda0.Value = MOS6lambda;
                }
            }

            /* loop through all the instances of the model */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb;

            /* perform model defaulting */
            if (!MOS6tnom.Given)
                MOS6tnom.Value = ckt.State.NominalTemperature;

            fact1 = MOS6tnom / Circuit.CONSTRefTemp;
            vtnom = MOS6tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * MOS6tnom;
            egfet1 = 1.16 - (7.02e-4 * MOS6tnom * MOS6tnom) / (MOS6tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);

            /* now model parameter preprocessing */

            if (!MOS6oxideThickness.Given || MOS6oxideThickness == 0)
            {
                MOS6oxideCapFactor = 0;
            }
            else
            {
                MOS6oxideCapFactor = 3.9 * 8.854214871e-12 / MOS6oxideThickness;
                if (!MOS6kc.Given)
                {
                    if (!MOS6surfaceMobility.Given)
                    {
                        MOS6surfaceMobility.Value = 600;
                    }
                    MOS6kc.Value = 0.5 * MOS6surfaceMobility * MOS6oxideCapFactor * 1e-4 /*(m**2/cm**2)*/;
                }
                if (MOS6substrateDoping.Given)
                {
                    if (MOS6substrateDoping * 1e6 /*(cm**3/m**3)*/ > 1.45e16)
                    {
                        if (!MOS6phi.Given)
                        {
                            MOS6phi.Value = 2 * vtnom * Math.Log(MOS6substrateDoping * 1e6/*(cm**3/m**3)*// 1.45e16);
                            MOS6phi.Value = Math.Max(.1, MOS6phi);
                        }
                        fermis = MOS6type * .5 * MOS6phi;
                        wkfng = 3.2;
                        if (!MOS6gateType.Given)
                            MOS6gateType.Value = 1;
                        if (MOS6gateType != 0)
                        {
                            fermig = MOS6type * MOS6gateType * .5 * egfet1;
                            wkfng = 3.25 + .5 * egfet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                        if (!MOS6gamma.Given)
                        {
                            MOS6gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.CHARGE * MOS6substrateDoping * 1e6/*(cm**3/m**3)*/) / MOS6oxideCapFactor;
                        }
                        if (!MOS6gamma1.Given)
                            MOS6gamma1.Value = 0.0;
                        if (!MOS6vt0.Given)
                        {
                            if (!MOS6surfaceStateDensity.Given)
                                MOS6surfaceStateDensity.Value = 0;
                            vfb = wkfngs - MOS6surfaceStateDensity * 1e4 /*(cm**2/m**2)*/ * Circuit.CHARGE / MOS6oxideCapFactor;
                            MOS6vt0.Value = vfb + MOS6type * (MOS6gamma * Math.Sqrt(MOS6phi) + MOS6phi);
                        }
                    }
                    else
                    {
                        MOS6substrateDoping.Value = 0;
                        throw new CircuitException($"{Name}: Nsub < Ni");
                    }
                }
            }
        }
    }
}
