using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Diode" />.
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the positive internal node.
        /// </summary>
        public int PosPrimeNode { get; private set; }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the (external positive, positive) element.
        /// </summary>
        protected MatrixElement<double> PosPosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<double> NegPosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (positive, external positive) element.
        /// </summary>
        protected MatrixElement<double> PosPrimePosPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<double> PosPrimeNegPtr { get; private set; }

        /// <summary>
        /// Gets the external (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the positive RHS element.
        /// </summary>
        protected VectorElement<double> PosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the negative RHS element.
        /// </summary>
        protected VectorElement<double> NegPtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("vd"), ParameterInfo("Diode voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Diode current")]
        public double Current { get; protected set; }

        /// <summary>
        /// Gets the small-signal conductance.
        /// </summary>
        [ParameterName("gd"), ParameterInfo("Small-signal conductance")]
        public double Conductance { get; protected set; }

        /// <summary>
        /// Gets the power dissipated.
        /// </summary>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return Current * Voltage;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Get equation pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            variables.ThrowIfNull(nameof(variables));
            solver.ThrowIfNull(nameof(solver));

            // Create
            PosPrimeNode = ModelParameters.Resistance > 0 ? variables.Create(Name.Combine("pos")).Index : PosNode;

            // Get matrix elements
            PosPosPrimePtr = solver.GetMatrixElement(PosNode, PosPrimeNode);
            NegPosPrimePtr = solver.GetMatrixElement(NegNode, PosPrimeNode);
            PosPrimePosPtr = solver.GetMatrixElement(PosPrimeNode, PosNode);
            PosPrimeNegPtr = solver.GetMatrixElement(PosPrimeNode, NegNode);
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            PosPrimePosPrimePtr = solver.GetMatrixElement(PosPrimeNode, PosPrimeNode);
            
            // Get RHS elements
            NegPtr = solver.GetRhsElement(NegNode);
            PosPrimePtr = solver.GetRhsElement(PosPrimeNode);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;
            double cd, gd;

            // Get the current voltages
            Initialize(simulation, out double vd, out bool check);

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            var csat = TempSaturationCurrent * BaseParameters.Area;
            var gspr = ModelTemperature.Conductance * BaseParameters.Area;

            // compute dc current and derivatives
            if (vd >= -3 * Vte)
            {
                // Forward bias
                var evd = Math.Exp(vd / Vte);
                cd = csat * (evd - 1) + BaseConfiguration.Gmin * vd;
                gd = csat * evd / Vte + BaseConfiguration.Gmin;
            }
            else if (!ModelParameters.BreakdownVoltage.Given || vd >= -TempBreakdownVoltage)
            {
                // Reverse bias
                var arg = 3 * Vte / (vd * Math.E);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + BaseConfiguration.Gmin * vd;
                gd = csat * 3 * arg / vd + BaseConfiguration.Gmin;
            }
            else
            {
                // Reverse breakdown
                var evrev = Math.Exp(-(TempBreakdownVoltage + vd) / Vte);
                cd = -csat * evrev + BaseConfiguration.Gmin * vd;
                gd = csat * evrev / Vte + BaseConfiguration.Gmin;
            }

            // Check convergence
            if (state.Init != InitializationModes.Fix || !BaseParameters.Off)
            {
                if (check)
                    state.IsConvergent = false;
            }

            // Store for next time
            Voltage = vd;
            Current = cd;
            Conductance = gd;

            // Load Rhs vector
            var cdeq = cd - gd * vd;
            NegPtr.Value += cdeq;
            PosPrimePtr.Value -= cdeq;

            // Load Y-matrix
            PosPosPtr.Value += gspr;
            NegNegPtr.Value += gd;
            PosPrimePosPrimePtr.Value += gd + gspr;
            PosPosPrimePtr.Value -= gspr;
            PosPrimePosPtr.Value -= gspr;
            NegPosPrimePtr.Value -= gd;
            PosPrimeNegPtr.Value -= gd;
        }

        /// <summary>
        /// Initialize the device based on the current iteration state.
        /// </summary>
        protected void Initialize(BaseSimulation simulation, out double vd, out bool check)
        {
            var state = simulation.RealState;
            check = false;
            if (state.Init == InitializationModes.Junction)
            {
                vd = BaseParameters.Off ? 0.0 : TempVCritical;
            }
            else if (state.Init == InitializationModes.Fix && BaseParameters.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];

                // limit new junction voltage
                if (ModelParameters.BreakdownVoltage.Given && vd < Math.Min(0, -TempBreakdownVoltage + 10 * Vte))
                {
                    var vdtemp = -(vd + TempBreakdownVoltage);
                    vdtemp = Semiconductor.LimitJunction(vdtemp, -(Voltage + TempBreakdownVoltage), Vte, TempVCritical, ref check);
                    vd = -(vdtemp + TempBreakdownVoltage);
                }
                else
                {
                    vd = Semiconductor.LimitJunction(vd, Voltage, Vte, TempVCritical, ref check);
                }
            }
        }

        /// <summary>
        /// Check convergence for the diode
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public bool IsConvergent(BaseSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];

            var delvd = vd - Voltage;
            var cdhat = Current + Conductance * delvd;
            var cd = Current;

            // check convergence
            var tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
