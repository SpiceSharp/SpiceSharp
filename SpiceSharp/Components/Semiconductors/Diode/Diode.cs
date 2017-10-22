using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode
    /// </summary>
    [SpicePins("D+", "D-")]
    public class Diode : CircuitComponent<Diode>
    {
        /// <summary>
        /// Register diode behaviours
        /// </summary>
        static Diode()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(Diode), typeof(ComponentBehaviours.DiodeLoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(Diode), typeof(ComponentBehaviours.DiodeAcBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(Diode), typeof(ComponentBehaviours.DiodeNoiseBehaviour));
        }

        /// <summary>
        /// Set the model for the diode
        /// </summary>
        public void SetModel(DiodeModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();
        [SpiceName("off"), SpiceInfo("Initially off")]
        public bool DIOoff { get; set; }
        [SpiceName("ic"), SpiceInfo("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [SpiceName("sens_area"), SpiceInfo("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [SpiceName("cd"), SpiceInfo("Diode capacitance")]
        public double DIOcap { get; set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("vd"), SpiceInfo("Diode voltage")]
        public double GetDIO_VOLTAGE(Circuit ckt) => ckt.State.States[0][DIOstate + DIOvoltage];
        [SpiceName("id"), SpiceName("c"), SpiceInfo("Diode current")]
        public double GetDIO_CURRENT(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcurrent];
        [SpiceName("charge"), SpiceInfo("Diode capacitor charge")]
        public double GetDIO_CHARGE(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCharge];
        [SpiceName("capcur"), SpiceInfo("Diode capacitor current")]
        public double GetDIO_CAPCUR(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCurrent];
        [SpiceName("gd"), SpiceInfo("Diode conductance")]
        public double GetDIO_CONDUCT(Circuit ckt) => ckt.State.States[0][DIOstate + DIOconduct];
        [SpiceName("p"), SpiceInfo("Diode power")]

        /// <summary>
        /// Extra variables
        /// </summary>
        public double DIOtJctCap { get; private set; }
        public double DIOtJctPot { get; private set; }
        public double DIOtSatCur { get; private set; }
        public double DIOtF1 { get; private set; }
        public double DIOtDepCap { get; private set; }
        public double DIOtVcrit { get; private set; }
        public double DIOtBrkdwnV { get; private set; }
        public int DIOposNode { get; private set; }
        public int DIOposPrimeNode { get; private set; }
        public int DIOnegNode { get; private set; }
        public int DIOstate { get; private set; }


        /// <summary>
        /// Constants
        /// </summary>
        public const int DIOvoltage = 0;
        public const int DIOcurrent = 1;
        public const int DIOconduct = 2;
        public const int DIOcapCharge = 3;
        public const int DIOcapCurrent = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as DiodeModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            DIOposNode = nodes[0].Index;
            DIOnegNode = nodes[1].Index;

            // Allocate states
            DIOstate = ckt.State.GetState(5);

            if (model.DIOresist.Value == 0)
                DIOposPrimeNode = DIOposNode;
            else
                DIOposPrimeNode = CreateNode(ckt, Name.Grow("#pos")).Index;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as DiodeModel;
            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            /* loop through all the instances */
            if (!DIOtemp.Given)
                DIOtemp.Value = ckt.State.Temperature;
            vt = Circuit.CONSTKoverQ * DIOtemp;
            /* this part gets really ugly - I won't even try to
			* explain these equations */
            fact2 = DIOtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * DIOtemp * DIOtemp) / (DIOtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * DIOtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            egfet1 = 1.16 - (7.02e-4 * model.DIOnomTemp * model.DIOnomTemp) / (model.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * model.DIOnomTemp) + 1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = model.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (model.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (model.DIOjunctionPot - pbo) / pbo;
            DIOtJctCap = model.DIOjunctionCap / (1 + model.DIOgradingCoeff * (400e-6 * (model.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (DIOtJctPot - pbo) / pbo;
            DIOtJctCap *= 1 + model.DIOgradingCoeff * (400e-6 * (DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            DIOtSatCur = model.DIOsatCur * Math.Exp(((DIOtemp / model.DIOnomTemp) - 1) * model.DIOactivationEnergy /
                (model.DIOemissionCoeff * vt) + model.DIOsaturationCurrentExp / model.DIOemissionCoeff * Math.Log(DIOtemp / model.DIOnomTemp));
            /* the defintion of f1, just recompute after temperature adjusting
			* all the variables used in it */
            DIOtF1 = DIOtJctPot * (1 - Math.Exp((1 - model.DIOgradingCoeff) * model.xfc)) / (1 - model.DIOgradingCoeff);
            /* same for Depletion Capacitance */
            DIOtDepCap = model.DIOdepletionCapCoeff * DIOtJctPot;
            /* and Vcrit */
            vte = model.DIOemissionCoeff * vt;
            DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * DIOtSatCur));
            /* and now to copute the breakdown voltage, again, using
			* temperature adjusted basic parameters */
            if (model.DIObreakdownVoltage.Given)
            {
                cbv = model.DIObreakdownCurrent;
                if (cbv < DIOtSatCur * model.DIObreakdownVoltage / vt)
                {
                    cbv = DIOtSatCur * model.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{Name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = model.DIObreakdownVoltage;
                }
                else
                {
                    tol = ckt.Simulation.Config.RelTol * cbv;
                    xbv = model.DIObreakdownVoltage - vt * Math.Log(1 + cbv / DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = model.DIObreakdownVoltage - vt * Math.Log(cbv / DIOtSatCur + 1 - xbv / vt);
                        xcbv = DIOtSatCur * (Math.Exp((model.DIObreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol) goto matched;
                    }
                    CircuitWarning.Warning(this, $"{Name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                matched:
                DIOtBrkdwnV = xbv;
            }
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            ckt.Method.Terr(DIOstate + DIOcapCharge, ckt, ref timeStep);
        }
    }
}
