using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Components.DIO;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// General behavior for <see cref="Diode"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        ModelTemperatureBehavior modeltemp;
        TemperatureBehavior temp;
        BaseParameters bp;
        ModelBaseParameters mbp;

        /// <summary>
        /// Nodes
        /// </summary>
        int DIOposNode, DIOnegNode;
        public int DIOposPrimeNode { get; private set; }
        protected MatrixElement DIOposPosPrimePtr { get; private set; }
        protected MatrixElement DIOnegPosPrimePtr { get; private set; }
        protected MatrixElement DIOposPrimePosPtr { get; private set; }
        protected MatrixElement DIOposPrimeNegPtr { get; private set; }
        protected MatrixElement DIOposPosPtr { get; private set; }
        protected MatrixElement DIOnegNegPtr { get; private set; }
        protected MatrixElement DIOposPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double DIOvoltage { get; protected set; }
        public double DIOcurrent { get; protected set; }
        public double DIOconduct { get; protected set; }
        public int DIOstate { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Create a getter
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(State state, string parameter)
        {
            switch (parameter)
            {
                case "vd": return () => DIOvoltage;
                case "v": return () => state.Solution[DIOposNode] - state.Solution[DIOnegNode];
                case "i":
                case "id": return () => DIOcurrent;
                case "gd": return () => DIOconduct;
                case "p": return () => (state.Solution[DIOposNode] - state.Solution[DIOnegNode]) * -DIOcurrent;
                case "pd": return () => -DIOvoltage * DIOcurrent;
                default: return null;
            }
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            DIOposNode = pins[0];
            DIOnegNode = pins[1];
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Create
            if (mbp.DIOresist.Value == 0)
                DIOposPrimeNode = DIOposNode;
            else
                DIOposPrimeNode = nodes.Create(Name.Grow("#pos")).Index;

            // Get matrix pointers
            DIOposPosPrimePtr = matrix.GetElement(DIOposNode, DIOposPrimeNode);
            DIOnegPosPrimePtr = matrix.GetElement(DIOnegNode, DIOposPrimeNode);
            DIOposPrimePosPtr = matrix.GetElement(DIOposPrimeNode, DIOposNode);
            DIOposPrimeNegPtr = matrix.GetElement(DIOposPrimeNode, DIOnegNode);
            DIOposPosPtr = matrix.GetElement(DIOposNode, DIOposNode);
            DIOnegNegPtr = matrix.GetElement(DIOnegNode, DIOnegNode);
            DIOposPrimePosPrimePtr = matrix.GetElement(DIOposPrimeNode, DIOposPrimeNode);
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            DIOposPosPrimePtr = null;
            DIOnegPosPrimePtr = null;
            DIOposPrimePosPtr = null;
            DIOposPrimeNegPtr = null;
            DIOposPosPtr = null;
            DIOnegNegPtr = null;
            DIOposPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var method = ckt.Method;
            bool Check;
            double csat, gspr, vt, vte, vd, vdtemp, evd, cd, gd, arg, evrev, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            csat = temp.DIOtSatCur * bp.DIOarea;
            gspr = modeltemp.DIOconductance * bp.DIOarea;
            vt = Circuit.CONSTKoverQ * bp.DIOtemp;
            vte = mbp.DIOemissionCoeff * vt;

            // Initialization
            Check = false;
            if (state.Init == State.InitFlags.InitJct)
            {
                if (bp.DIOoff)
                    vd = 0.0;
                else
                    vd = temp.DIOtVcrit;
            }
            else if (state.Init == State.InitFlags.InitFix && bp.DIOoff)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

                // limit new junction voltage
                if ((mbp.DIObreakdownVoltage.Given) && (vd < Math.Min(0, -temp.DIOtBrkdwnV + 10 * vte)))
                {
                    vdtemp = -(vd + temp.DIOtBrkdwnV);
                    vdtemp = Semiconductor.DEVpnjlim(vdtemp, -(DIOvoltage + temp.DIOtBrkdwnV), vte, temp.DIOtVcrit, ref Check);
                    vd = -(vdtemp + temp.DIOtBrkdwnV);
                }
                else
                {
                    vd = Semiconductor.DEVpnjlim(vd, DIOvoltage, vte, temp.DIOtVcrit, ref Check);
                }
            }

            // compute dc current and derivatives
            if (vd >= -3 * vte)
            {
                // Forward bias
                evd = Math.Exp(vd / vte);
                cd = csat * (evd - 1) + state.Gmin * vd;
                gd = csat * evd / vte + state.Gmin;
            }
            else if (temp.DIOtBrkdwnV == 0.0 || vd >= -temp.DIOtBrkdwnV)
            {
                // Reverse bias
                arg = 3 * vte / (vd * Circuit.CONSTE);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                // Reverse breakdown
                evrev = Math.Exp(-(temp.DIOtBrkdwnV + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }

            // Check convergence
            if ((state.Init != State.InitFlags.InitFix) || !bp.DIOoff)
            {
                if (Check)
                    ckt.State.IsCon = false;
            }

            // Store for next time
            DIOvoltage = vd;
            DIOcurrent = cd;
            DIOconduct = gd;

            // Load Rhs vector
            cdeq = cd - gd * vd;
            state.Rhs[DIOnegNode] += cdeq;
            state.Rhs[DIOposPrimeNode] -= cdeq;

            // Load Y-matrix
            DIOposPosPtr.Add(gspr);
            DIOnegNegPtr.Add(gd);
            DIOposPrimePosPrimePtr.Add(gd + gspr);
            DIOposPosPrimePtr.Sub(gspr);
            DIOposPrimePosPtr.Sub(gspr);
            DIOnegPosPrimePtr.Sub(gd);
            DIOposPrimeNegPtr.Sub(gd);
        }

        /// <summary>
        /// Check convergence for the diode
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var state = ckt.State;

            double delvd, cdhat, cd;

            double vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

            delvd = vd - DIOvoltage;
            cdhat = DIOcurrent + DIOconduct * delvd;
            cd = DIOcurrent;

            // check convergence
            // TODO: Find a way to pass configuration RELTOL and ABSTOL, defaults are used here
            double tol = 1e-3 * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + 1e-12;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
