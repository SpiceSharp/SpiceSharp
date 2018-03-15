using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Diode"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior _modeltemp;
        private TemperatureBehavior _temp;
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        public int PosPrimeNode { get; private set; }
        protected MatrixElement<double> PosPosPrimePtr { get; private set; }
        protected MatrixElement<double> NegPosPrimePtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPtr { get; private set; }
        protected MatrixElement<double> PosPrimeNegPtr { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPrimePtr { get; private set; }
        protected VectorElement<double> PosPrimePtr { get; private set; }
        protected VectorElement<double> NegPtr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        [ParameterName("vd"), ParameterInfo("Voltage across the internal diode")]
        public double InternalVoltage { get; protected set; }
        [ParameterName("v"), ParameterInfo("Voltage across the diode")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Current through the diode")]
        public double Current { get; protected set; }
        [ParameterName("gd"), ParameterInfo("Small-signal conductance")]
        public double Conduct { get; protected set; }
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public double GetPower(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return (state.Solution[_posNode] - state.Solution[_negNode]) * -Current;
        }

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
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Get equation pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(NodeMap nodes, Solver<double> solver)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Create
            PosPrimeNode = _mbp.Resistance > 0 ? nodes.Create(Name.Grow("#pos")).Index : _posNode;

            // Get matrix elements
            PosPosPrimePtr = solver.GetMatrixElement(_posNode, PosPrimeNode);
            NegPosPrimePtr = solver.GetMatrixElement(_negNode, PosPrimeNode);
            PosPrimePosPtr = solver.GetMatrixElement(PosPrimeNode, _posNode);
            PosPrimeNegPtr = solver.GetMatrixElement(PosPrimeNode, _negNode);
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            PosPrimePosPrimePtr = solver.GetMatrixElement(PosPrimeNode, PosPrimeNode);
            
            // Get RHS elements
            NegPtr = solver.GetRhsElement(_negNode);
            PosPrimePtr = solver.GetRhsElement(PosPrimeNode);
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
            double vd;
            double cd, gd;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            var csat = _temp.TempSaturationCurrent * _bp.Area;
            var gspr = _modeltemp.Conductance * _bp.Area;
            var vt = Circuit.KOverQ * _bp.Temperature;
            var vte = _mbp.EmissionCoefficient * vt;

            // Initialization
            var check = false;
            if (state.Init == RealState.InitializationStates.InitJunction)
            {
                vd = _bp.Off ? 0.0 : _temp.TempVCritical;
            }
            else if (state.Init == RealState.InitializationStates.InitFix && _bp.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[PosPrimeNode] - state.Solution[_negNode];

                // limit new junction voltage
                if (_mbp.BreakdownVoltage.Given && vd < Math.Min(0, -_temp.TempBreakdownVoltage + 10 * vte))
                {
                    var vdtemp = -(vd + _temp.TempBreakdownVoltage);
                    vdtemp = Semiconductor.LimitJunction(vdtemp, -(InternalVoltage + _temp.TempBreakdownVoltage), vte, _temp.TempVCritical, ref check);
                    vd = -(vdtemp + _temp.TempBreakdownVoltage);
                }
                else
                {
                    vd = Semiconductor.LimitJunction(vd, InternalVoltage, vte, _temp.TempVCritical, ref check);
                }
            }

            // compute dc current and derivatives
            if (vd >= -3 * vte)
            {
                // Forward bias
                var evd = Math.Exp(vd / vte);
                cd = csat * (evd - 1) + state.Gmin * vd;
                gd = csat * evd / vte + state.Gmin;
            }
            else if (!_mbp.BreakdownVoltage.Given || vd >= -_temp.TempBreakdownVoltage)
            {
                // Reverse bias
                var arg = 3 * vte / (vd * Math.E);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                // Reverse breakdown
                var evrev = Math.Exp(-(_temp.TempBreakdownVoltage + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }

            // Check convergence
            if (state.Init != RealState.InitializationStates.InitFix || !_bp.Off)
            {
                if (check)
                    state.IsConvergent = false;
            }

            // Store for next time
            InternalVoltage = vd;
            Current = cd;
            Conduct = gd;

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
            double vd = state.Solution[PosPrimeNode] - state.Solution[_negNode];

            var delvd = vd - InternalVoltage;
            var cdhat = Current + Conduct * delvd;
            var cd = Current;

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
