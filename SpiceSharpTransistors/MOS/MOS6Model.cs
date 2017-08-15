using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class MOS6Model : CircuitModel<MOS6Model>
    {
        /// <summary>
        /// Register our par
        /// </summary>
        static MOS6Model()
        {
            Register();
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature in Kelvin")]
        public Parameter MOS6tnom { get; } = new Parameter(300.15);
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS6vt0 { get; } = new Parameter();
        [SpiceName("kv"), SpiceInfo("Saturation voltage factor")]
        public Parameter MOS6kv { get; } = new Parameter(2);
        [SpiceName("nv"), SpiceInfo("Saturation voltage coeff.")]
        public Parameter MOS6nv { get; } = new Parameter();
        [SpiceName("kc"), SpiceInfo("Saturation current factor")]
        public Parameter MOS6kc { get; } = new Parameter(5e-5);
        [SpiceName("nc"), SpiceInfo("Saturation current coeff.")]
        public Parameter MOS6nc { get; } = new Parameter(1);
        [SpiceName("nvth"), SpiceInfo("Threshold voltage coeff.")]
        public Parameter MOS6nvth { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Sat. current modification  par.")]
        public Parameter MOS6ps { get; } = new Parameter();
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS6gamma { get; } = new Parameter();
        [SpiceName("gamma1"), SpiceInfo("Bulk threshold parameter 1")]
        public Parameter MOS6gamma1 { get; } = new Parameter();
        [SpiceName("sigma"), SpiceInfo("Static feedback effect par.")]
        public Parameter MOS6sigma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS6phi { get; } = new Parameter(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation param.")]
        public Parameter MOS6lambda { get; } = new Parameter();
        [SpiceName("lambda0"), SpiceInfo("Channel length modulation param. 0")]
        public Parameter MOS6lamda0 { get; } = new Parameter();
        [SpiceName("lambda1"), SpiceInfo("Channel length modulation param. 1")]
        public Parameter MOS6lamda1 { get; } = new Parameter();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS6drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS6sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS6capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS6capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS6jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS6bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS6gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS6gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS6gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS6bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS6bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS6sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS6bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS6jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS6oxideThickness { get; } = new Parameter();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS6latDiff { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS6sheetResistance { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS6surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS6fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS6surfaceStateDensity { get; } = new Parameter();
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS6substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS6gateType { get; } = new Parameter();

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
