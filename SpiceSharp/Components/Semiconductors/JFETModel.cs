using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    public class JFETModel : CircuitModel<JFETModel>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("parameter measurement temperature")]
        public double JFET_TNOM
        {
            get => JFETtnom - Circuit.CONSTCtoK;
            set => JFETtnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter JFETtnom { get; } = new Parameter();
        [SpiceName("vt0"), SpiceName("vto"), SpiceInfo("Threshold voltage")]
        public Parameter JFETthreshold { get; } = new Parameter(-2);
        [SpiceName("beta"), SpiceInfo("Transconductance parameter")]
        public Parameter JFETbeta { get; } = new Parameter(1e-4);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation param.")]
        public Parameter JFETlModulation { get; } = new Parameter();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter JFETdrainResist { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter JFETsourceResist { get; } = new Parameter();
        [SpiceName("cgs"), SpiceInfo("G-S junction capactance")]
        public Parameter JFETcapGS { get; } = new Parameter();
        [SpiceName("cgd"), SpiceInfo("G-D junction cap")]
        public Parameter JFETcapGD { get; } = new Parameter();
        [SpiceName("pb"), SpiceInfo("Gate junction potential")]
        public Parameter JFETgatePotential { get; } = new Parameter(1);
        [SpiceName("is"), SpiceInfo("Gate junction saturation current")]
        public Parameter JFETgateSatCurrent { get; } = new Parameter(1e-14);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parm.")]
        public Parameter JFETdepletionCapCoeff { get; } = new Parameter(.5);
        [SpiceName("kf"), SpiceInfo("Flicker Noise Coefficient")]
        public Parameter JFETfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker Noise Exponent")]
        public Parameter JFETfNexp { get; } = new Parameter(1);
        [SpiceName("b"), SpiceInfo("Doping tail parameter")]
        public Parameter JFETb { get; } = new Parameter(1.0);
        [SpiceName("gd"), SpiceInfo("Drain conductance")]
        public double JFETdrainConduct { get; private set; }
        [SpiceName("gs"), SpiceInfo("Source conductance")]
        public double JFETsourceConduct { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("njf"), SpiceInfo("N type JFET model")]
        public void SetNJF(bool value)
        {
            if (value)
                JFETtype = NJF;
        }
        [SpiceName("pjf"), SpiceInfo("P type JFET model")]
        public void SetPJF(bool value)
        {
            if (value)
                JFETtype = PJF;
        }
        [SpiceName("type"), SpiceInfo("N-type or P-type JFET model")]
        public string GetTYPE(Circuit ckt)
        {
            if (JFETtype == NJF)
                return "njf";
            else
                return "pjf";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double fact1 { get; private set; }
        public double pbo { get; private set; }
        public double cjfact { get; private set; }
        public double xfc { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double JFETtype { get; private set; }
        public double JFETmodName { get; private set; }
        public double JFETf2 { get; private set; }
        public double JFETf3 { get; private set; }
        public double JFETbFac { get; private set; }

        public const double NJF = 1.0;
        public const double PJF = -1.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public JFETModel(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if ((JFETtype != NJF) && (JFETtype != PJF))
                JFETtype = NJF;

            if (JFETdrainResist != 0)
                JFETdrainConduct = 1 / JFETdrainResist;
            else
                JFETdrainConduct = 0;
            if (JFETsourceResist != 0)
                JFETsourceConduct = 1 / JFETsourceResist;
            else
                JFETsourceConduct = 0;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vtnom, kt1, egfet1, arg1, pbfact1, gmaold;

            if (!JFETtnom.Given)
                JFETtnom.Value = ckt.State.NominalTemperature;
            vtnom = Circuit.CONSTKoverQ * JFETtnom;
            fact1 = JFETtnom / Circuit.CONSTRefTemp;
            kt1 = Circuit.CONSTBoltz * JFETtnom;
            egfet1 = 1.16 - (7.02e-4 * JFETtnom * JFETtnom) / (JFETtnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (JFETgatePotential - pbfact1) / fact1;
            gmaold = (JFETgatePotential - pbo) / pbo;
            cjfact = 1 / (1 + .5 * (4e-4 * (JFETtnom - Circuit.CONSTRefTemp) - gmaold));

            if (JFETdrainResist != 0)
                JFETdrainConduct = 1 / JFETdrainResist;
            else
                JFETdrainConduct = 0;
            if (JFETsourceResist != 0)
                JFETsourceConduct = 1 / JFETsourceResist;
            else
                JFETsourceConduct = 0;
            if (JFETdepletionCapCoeff > .95)
            {
                CircuitWarning.Warning(this, $"{Name}: Depletion cap. coefficient too large, limited to .95");
                JFETdepletionCapCoeff.Value = .95;
            }

            xfc = Math.Log(1 - JFETdepletionCapCoeff);
            JFETf2 = Math.Exp((1 + .5) * xfc);
            JFETf3 = 1 - JFETdepletionCapCoeff * (1 + .5);
            /* Modification for Sydney University JFET model */
            JFETbFac = (1 - JFETb) / (JFETgatePotential - JFETthreshold);
            /* end Sydney University mod */
        }
    }
}
