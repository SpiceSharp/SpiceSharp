using System;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components.DiodeBehaviors
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
        int posourceNode, negateNode;
        public int PosPrimeNode { get; private set; }
        protected MatrixElement PosPosPrimePtr { get; private set; }
        protected MatrixElement NegPosPrimePtr { get; private set; }
        protected MatrixElement PosPrimePosPtr { get; private set; }
        protected MatrixElement PosPrimeNegPtr { get; private set; }
        protected MatrixElement PosPosPtr { get; private set; }
        protected MatrixElement NegNegPtr { get; private set; }
        protected MatrixElement PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Voltage { get; protected set; }
        public double Current { get; protected set; }
        public double Conduct { get; protected set; }

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="propertyName">Parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string propertyName)
        {
            switch (propertyName)
            {
                case "vd": return (State state) => Voltage;
                case "v": return (State state) => state.Solution[posourceNode] - state.Solution[negateNode];
                case "i":
                case "id": return (State state) => Current;
                case "gd": return (State state) => Conduct;
                case "p": return (State state) => (state.Solution[posourceNode] - state.Solution[negateNode]) * -Current;
                case "pd": return (State state) => -Voltage * Current;
                default: return null;
            }
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posourceNode = pins[0];
            negateNode = pins[1];
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            
            // Create
            if (mbp.Resistance.Value == 0)
                PosPrimeNode = posourceNode;
            else
                PosPrimeNode = nodes.Create(Name.Grow("#pos")).Index;

            // Get matrix pointers
            PosPosPrimePtr = matrix.GetElement(posourceNode, PosPrimeNode);
            NegPosPrimePtr = matrix.GetElement(negateNode, PosPrimeNode);
            PosPrimePosPtr = matrix.GetElement(PosPrimeNode, posourceNode);
            PosPrimeNegPtr = matrix.GetElement(PosPrimeNode, negateNode);
            PosPosPtr = matrix.GetElement(posourceNode, posourceNode);
            NegNegPtr = matrix.GetElement(negateNode, negateNode);
            PosPrimePosPrimePtr = matrix.GetElement(PosPrimeNode, PosPrimeNode);
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            PosPosPrimePtr = null;
            NegPosPrimePtr = null;
            PosPrimePosPtr = null;
            PosPrimeNegPtr = null;
            PosPosPtr = null;
            NegNegPtr = null;
            PosPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            bool Check;
            double csat, gspr, vt, vte, vd, vdtemp, evd, cd, gd, arg, evrev, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            csat = temp.TSatCur * bp.Area;
            gspr = modeltemp.Conductance * bp.Area;
            vt = Circuit.KOverQ * bp.Temperature;
            vte = mbp.EmissionCoefficient * vt;

            // Initialization
            Check = false;
            if (state.Init == State.InitializationStates.InitJct)
            {
                if (bp.Off)
                    vd = 0.0;
                else
                    vd = temp.TVcrit;
            }
            else if (state.Init == State.InitializationStates.InitFix && bp.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[PosPrimeNode] - state.Solution[negateNode];

                // limit new junction voltage
                if ((mbp.BreakdownVoltage.Given) && (vd < Math.Min(0, -temp.TBrkdwnV + 10 * vte)))
                {
                    vdtemp = -(vd + temp.TBrkdwnV);
                    vdtemp = Semiconductor.DEVpnjlim(vdtemp, -(Voltage + temp.TBrkdwnV), vte, temp.TVcrit, ref Check);
                    vd = -(vdtemp + temp.TBrkdwnV);
                }
                else
                {
                    vd = Semiconductor.DEVpnjlim(vd, Voltage, vte, temp.TVcrit, ref Check);
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
            else if (temp.TBrkdwnV == 0.0 || vd >= -temp.TBrkdwnV)
            {
                // Reverse bias
                arg = 3 * vte / (vd * Math.E);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                // Reverse breakdown
                evrev = Math.Exp(-(temp.TBrkdwnV + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }

            // Check convergence
            if ((state.Init != State.InitializationStates.InitFix) || !bp.Off)
            {
                if (Check)
                    state.IsCon = false;
            }

            // Store for next time
            Voltage = vd;
            Current = cd;
            Conduct = gd;

            // Load Rhs vector
            cdeq = cd - gd * vd;
            state.Rhs[negateNode] += cdeq;
            state.Rhs[PosPrimeNode] -= cdeq;

            // Load Y-matrix
            PosPosPtr.Add(gspr);
            NegNegPtr.Add(gd);
            PosPrimePosPrimePtr.Add(gd + gspr);
            PosPosPrimePtr.Sub(gspr);
            PosPrimePosPtr.Sub(gspr);
            NegPosPrimePtr.Sub(gd);
            PosPrimeNegPtr.Sub(gd);
        }

        /// <summary>
        /// Check convergence for the diode
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var config = simulation.BaseConfiguration;
            double delvd, cdhat, cd;
            double vd = state.Solution[PosPrimeNode] - state.Solution[negateNode];

            delvd = vd - Voltage;
            cdhat = Current + Conduct * delvd;
            cd = Current;

            // check convergence
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + config.AbsTol;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
