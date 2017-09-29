using System;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Model for a MESFET
    /// </summary>
    public class MESModel : CircuitModel<MESModel>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vt0"), SpiceName("vto"), SpiceInfo("Pinch-off voltage")]
        public Parameter MESthreshold { get; } = new Parameter(-2);
        [SpiceName("alpha"), SpiceInfo("Saturation voltage parameter")]
        public Parameter MESalpha { get; } = new Parameter(2);
        [SpiceName("beta"), SpiceInfo("Transconductance parameter")]
        public Parameter MESbeta { get; } = new Parameter(2.5e-3);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation parm.")]
        public Parameter MESlModulation { get; } = new Parameter();
        [SpiceName("b"), SpiceInfo("Doping tail extending parameter")]
        public Parameter MESb { get; } = new Parameter(0.3);
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MESdrainResist { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MESsourceResist { get; } = new Parameter();
        [SpiceName("cgs"), SpiceInfo("G-S junction capacitance")]
        public Parameter MEScapGS { get; } = new Parameter();
        [SpiceName("cgd"), SpiceInfo("G-D junction capacitance")]
        public Parameter MEScapGD { get; } = new Parameter();
        [SpiceName("pb"), SpiceInfo("Gate junction potential")]
        public Parameter MESgatePotential { get; } = new Parameter(1);
        [SpiceName("is"), SpiceInfo("Junction saturation current")]
        public Parameter MESgateSatCurrent { get; } = new Parameter(1e-14);
        [SpiceName("fc"), SpiceInfo("Forward bias junction fit parm.")]
        public Parameter MESdepletionCapCoeff { get; } = new Parameter(.5);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MESfNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MESfNexp { get; } = new Parameter(1);
        [SpiceName("gd"), SpiceInfo("Drain conductance")]
        public double MESdrainConduct { get; private set; }
        [SpiceName("gs"), SpiceInfo("Source conductance")]
        public double MESsourceConduct { get; private set; }
        [SpiceName("depl_cap"), SpiceInfo("Depletion capacitance")]
        public double MESdepletionCap { get; private set; }
        [SpiceName("vcrit"), SpiceInfo("Critical voltage")]
        public double MESvcrit { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmf"), SpiceInfo("N type MESfet model")]
        public void SetNMF(bool value)
        {
            if (value)
                MEStype = NMF;
        }
        [SpiceName("pmf"), SpiceInfo("P type MESfet model")]
        public void SetPMF(bool value)
        {
            if (value)
                MEStype = PMF;
        }
        [SpiceName("type"), SpiceInfo("N-type or P-type MESfet model")]
        public string GetTYPE(Circuit ckt)
        {
            if (MEStype == NMF)
                return "nmf";
            else
                return "pmf";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MEStype { get; private set; }
        public double MESf1 { get; private set; }
        public double MESf2 { get; private set; }
        public double MESf3 { get; private set; }

        public const double NMF = 1.0;
        public const double PMF = -1.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MESModel(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if ((MEStype != NMF) && (MEStype != PMF))
                MEStype = NMF;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double xfc, temp;

            if (MESdrainResist != 0)
                MESdrainConduct = 1 / MESdrainResist;
            else
                MESdrainConduct = 0;
            if (MESsourceResist != 0)
                MESsourceConduct = 1 / MESsourceResist;
            else
                MESsourceConduct = 0;

            MESdepletionCap = MESdepletionCapCoeff * MESgatePotential;
            xfc = (1 - MESdepletionCapCoeff);
            temp = Math.Sqrt(xfc);
            MESf1 = MESgatePotential * (1 - temp) / (1 - .5);
            MESf2 = temp * temp * temp;
            MESf3 = 1 - MESdepletionCapCoeff * (1 + .5);
            MESvcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * MESgateSatCurrent));
        }
    }
}
