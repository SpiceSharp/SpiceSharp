using System;
using SpiceSharp.Attributes;
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
        int posNode, negNode;
        public int PosPrimeNode { get; private set; }
        protected Element<double> PosPosPrimePtr { get; private set; }
        protected Element<double> NegPosPrimePtr { get; private set; }
        protected Element<double> PosPrimePosPtr { get; private set; }
        protected Element<double> PosPrimeNegPtr { get; private set; }
        protected Element<double> PosPosPtr { get; private set; }
        protected Element<double> NegNegPtr { get; private set; }
        protected Element<double> PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        [PropertyName("vd"), PropertyInfo("Voltage across the internal diode")]
        public double Voltage { get; protected set; }
        [PropertyName("v"), PropertyInfo("Voltage across the diode")]
        public double GetVoltage(RealState state) => state.Solution[posNode] - state.Solution[negNode];
        [PropertyName("i"), PropertyName("id"), PropertyInfo("Current through the diode")]
        public double Current { get; protected set; }
        [PropertyName("gd"), PropertyInfo("Small-signal conductance")]
        public double Conduct { get; protected set; }
        [PropertyName("p"), PropertyName("pd"), PropertyInfo("Power")]
        public double GetPower(RealState state) => (state.Solution[posNode] - state.Solution[negNode]) * -Current;

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
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }
        
        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Nodes nodes, Matrix<double> matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Create
            if (mbp.Resistance > 0)
                PosPrimeNode = nodes.Create(Name.Grow("#pos")).Index;
            else
                PosPrimeNode = posNode;

            // Get matrix elements
            PosPosPrimePtr = matrix.GetElement(posNode, PosPrimeNode);
            NegPosPrimePtr = matrix.GetElement(negNode, PosPrimeNode);
            PosPrimePosPtr = matrix.GetElement(PosPrimeNode, posNode);
            PosPrimeNegPtr = matrix.GetElement(PosPrimeNode, negNode);
            PosPosPtr = matrix.GetElement(posNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
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

            var state = simulation.RealState;
            bool Check;
            double csat, gspr, vt, vte, vd, vdtemp, evd, cd, gd, arg, evrev, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            csat = temp.TempSaturationCurrent * bp.Area;
            gspr = modeltemp.Conductance * bp.Area;
            vt = Circuit.KOverQ * bp.Temperature;
            vte = mbp.EmissionCoefficient * vt;

            // Initialization
            Check = false;
            if (state.Init == RealState.InitializationStates.InitJunction)
            {
                if (bp.Off)
                    vd = 0.0;
                else
                    vd = temp.TempVCritical;
            }
            else if (state.Init == RealState.InitializationStates.InitFix && bp.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[PosPrimeNode] - state.Solution[negNode];

                // limit new junction voltage
                if ((mbp.BreakdownVoltage.Given) && (vd < Math.Min(0, -temp.TempBreakdownVoltage + 10 * vte)))
                {
                    vdtemp = -(vd + temp.TempBreakdownVoltage);
                    vdtemp = Semiconductor.LimitJunction(vdtemp, -(Voltage + temp.TempBreakdownVoltage), vte, temp.TempVCritical, ref Check);
                    vd = -(vdtemp + temp.TempBreakdownVoltage);
                }
                else
                {
                    vd = Semiconductor.LimitJunction(vd, Voltage, vte, temp.TempVCritical, ref Check);
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
            else if (!mbp.BreakdownVoltage.Given || vd >= -temp.TempBreakdownVoltage)
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
                evrev = Math.Exp(-(temp.TempBreakdownVoltage + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }

            // Check convergence
            if ((state.Init != RealState.InitializationStates.InitFix) || !bp.Off)
            {
                if (Check)
                    state.IsConvergent = false;
            }

            // Store for next time
            Voltage = vd;
            Current = cd;
            Conduct = gd;

            // Load Rhs vector
            cdeq = cd - gd * vd;
            state.Rhs[negNode] += cdeq;
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

            var state = simulation.RealState;
            var config = simulation.BaseConfiguration;
            double delvd, cdhat, cd;
            double vd = state.Solution[PosPrimeNode] - state.Solution[negNode];

            delvd = vd - Voltage;
            cdhat = Current + Conduct * delvd;
            cd = Current;

            // check convergence
            double tol = config.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + config.AbsoluteTolerance;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
