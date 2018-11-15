using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// General behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the base configuration of the simulation.
        /// </summary>
        /// <value>
        /// The base configuration.
        /// </value>
        protected BaseConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("vbe"), ParameterInfo("B-E voltage")]
        public double VoltageBe { get; protected set; }
        [ParameterName("vbc"), ParameterInfo("B-C voltage")]
        public double VoltageBc { get; protected set; }
        [ParameterName("cc"), ParameterInfo("Current at collector node")]
        public double CollectorCurrent { get; protected set; }
        [ParameterName("cb"), ParameterInfo("Current at base node")]
        public double BaseCurrent { get; protected set; }
        [ParameterName("gpi"), ParameterInfo("Small signal input conductance - pi")]
        public double ConductancePi { get; protected set; }
        [ParameterName("gmu"), ParameterInfo("Small signal conductance - mu")]
        public double ConductanceMu { get; protected set; }
        [ParameterName("gm"), ParameterInfo("Small signal transconductance")]
        public double Transconductance { get; protected set; }
        [ParameterName("go"), ParameterInfo("Small signal output conductance")]
        public double OutputConductance { get; protected set; }
        public double ConductanceX { get; protected set; }

        /// <summary>
        /// Gets the collector prime node index.
        /// </summary>
        /// <value>
        /// The collector prime node index.
        /// </value>
        public int CollectorPrimeNode { get; private set; }

        /// <summary>
        /// Gets the base prime node index.
        /// </summary>
        /// <value>
        /// The base prime nodeindex .
        /// </value>
        public int BasePrimeNode { get; private set; }

        /// <summary>
        /// Gets the emitter prime node index.
        /// </summary>
        /// <value>
        /// The emitter prime node index.
        /// </value>
        public int EmitterPrimeNode { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int CollectorNode { get; private set; }
        protected int BaseNode { get; private set; }
        protected int EmitterNode { get; private set; } 
        protected int SubstrateNode { get; private set; }
        protected MatrixElement<double> CollectorCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BaseBasePrimePtr { get; private set; }
        protected MatrixElement<double> EmitterEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeCollectorPtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeBasePtr { get; private set; }
        protected MatrixElement<double> BasePrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeEmitterPtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> CollectorCollectorPtr { get; private set; }
        protected MatrixElement<double> BaseBasePtr { get; private set; }
        protected MatrixElement<double> EmitterEmitterPtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> SubstrateSubstratePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeSubstratePtr { get; private set; }
        protected MatrixElement<double> SubstrateCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BaseCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeBasePtr { get; private set; }
        protected VectorElement<double> CollectorPrimePtr { get; private set; }
        protected VectorElement<double> BasePrimePtr { get; private set; }
        protected VectorElement<double> EmitterPrimePtr { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public virtual double CurrentBe { get; protected set; }
        public double CondBe { get; protected set; }
        public virtual double CurrentBc { get; protected set; }
        public double CondBc { get; protected set; }
        public double BaseCharge { get; protected set; }
        public double Dqbdvc { get; protected set; }
        public double Dqbdve { get; protected set; }

        /// <summary>
        /// Event called when excess phase calculation is needed
        /// </summary>
        public event EventHandler<ExcessPhaseEventArgs> ExcessPhaseCalculation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get configurations
            BaseConfiguration = simulation.Configurations.Get<BaseConfiguration>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            CollectorNode = pins[0];
            BaseNode = pins[1];
            EmitterNode = pins[2];
            SubstrateNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add a series collector node if necessary
            CollectorPrimeNode = ModelParameters.CollectorResistance.Value > 0 ? variables.Create(Name.Combine("col")).Index : CollectorNode;

            // Add a series base node if necessary
            BasePrimeNode = ModelParameters.BaseResist.Value > 0 ? variables.Create(Name.Combine("base")).Index : BaseNode;

            // Add a series emitter node if necessary
            EmitterPrimeNode = ModelParameters.EmitterResistance.Value > 0 ? variables.Create(Name.Combine("emit")).Index : EmitterNode;

            // Get solver pointers
            CollectorCollectorPrimePtr = solver.GetMatrixElement(CollectorNode, CollectorPrimeNode);
            BaseBasePrimePtr = solver.GetMatrixElement(BaseNode, BasePrimeNode);
            EmitterEmitterPrimePtr = solver.GetMatrixElement(EmitterNode, EmitterPrimeNode);
            CollectorPrimeCollectorPtr = solver.GetMatrixElement(CollectorPrimeNode, CollectorNode);
            CollectorPrimeBasePrimePtr = solver.GetMatrixElement(CollectorPrimeNode, BasePrimeNode);
            CollectorPrimeEmitterPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, EmitterPrimeNode);
            BasePrimeBasePtr = solver.GetMatrixElement(BasePrimeNode, BaseNode);
            BasePrimeCollectorPrimePtr = solver.GetMatrixElement(BasePrimeNode, CollectorPrimeNode);
            BasePrimeEmitterPrimePtr = solver.GetMatrixElement(BasePrimeNode, EmitterPrimeNode);
            EmitterPrimeEmitterPtr = solver.GetMatrixElement(EmitterPrimeNode, EmitterNode);
            EmitterPrimeCollectorPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, CollectorPrimeNode);
            EmitterPrimeBasePrimePtr = solver.GetMatrixElement(EmitterPrimeNode, BasePrimeNode);
            CollectorCollectorPtr = solver.GetMatrixElement(CollectorNode, CollectorNode);
            BaseBasePtr = solver.GetMatrixElement(BaseNode, BaseNode);
            EmitterEmitterPtr = solver.GetMatrixElement(EmitterNode, EmitterNode);
            CollectorPrimeCollectorPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, CollectorPrimeNode);
            BasePrimeBasePrimePtr = solver.GetMatrixElement(BasePrimeNode, BasePrimeNode);
            EmitterPrimeEmitterPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, EmitterPrimeNode);
            SubstrateSubstratePtr = solver.GetMatrixElement(SubstrateNode, SubstrateNode);
            CollectorPrimeSubstratePtr = solver.GetMatrixElement(CollectorPrimeNode, SubstrateNode);
            SubstrateCollectorPrimePtr = solver.GetMatrixElement(SubstrateNode, CollectorPrimeNode);
            BaseCollectorPrimePtr = solver.GetMatrixElement(BaseNode, CollectorPrimeNode);
            CollectorPrimeBasePtr = solver.GetMatrixElement(CollectorPrimeNode, BaseNode);

            // Get RHS pointers
            CollectorPrimePtr = solver.GetRhsElement(CollectorPrimeNode);
            BasePrimePtr = solver.GetRhsElement(BasePrimeNode);
            EmitterPrimePtr = solver.GetRhsElement(EmitterPrimeNode);
        }


        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vbe;
            double vbc;
            double gben;
            double cben;
            double gbcn;
            double cbcn;

            var vt = BaseParameters.Temperature * Circuit.KOverQ;

            // DC model parameters
            var csat = TempSaturationCurrent * BaseParameters.Area;
            var rbpr = ModelParameters.MinimumBaseResistance / BaseParameters.Area;
            var rbpi = ModelParameters.BaseResist / BaseParameters.Area - rbpr;
            var gcpr = ModelTemperature.CollectorConduct * BaseParameters.Area;
            var gepr = ModelTemperature.EmitterConduct * BaseParameters.Area;
            var oik = ModelTemperature.InverseRollOffForward / BaseParameters.Area;
            var c2 = TempBeLeakageCurrent * BaseParameters.Area;
            var vte = ModelParameters.LeakBeEmissionCoefficient * vt;
            var oikr = ModelTemperature.InverseRollOffReverse / BaseParameters.Area;
            var c4 = TempBcLeakageCurrent * BaseParameters.Area;
            var vtc = ModelParameters.LeakBcEmissionCoefficient * vt;
            var xjrb = ModelParameters.BaseCurrentHalfResist * BaseParameters.Area;

            // Initialization
            if (state.Init == InitializationModes.Junction && (simulation is TimeSimulation) && state.UseDc && state.UseIc)
            {
                vbe = ModelParameters.BipolarType * BaseParameters.InitialVoltageBe;
                var vce = ModelParameters.BipolarType * BaseParameters.InitialVoltageCe;
                vbc = vbe - vce;
            }
            else if (state.Init == InitializationModes.Junction && !BaseParameters.Off)
            {
                vbe = TempVCritical;
                vbc = 0;
            }
            else if (state.Init == InitializationModes.Junction || state.Init == InitializationModes.Fix && BaseParameters.Off)
            {
                vbe = 0;
                vbc = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vbe = ModelParameters.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
                vbc = ModelParameters.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);

                // Limit nonlinear branch voltages
                var limited = false;
                vbe = Semiconductor.LimitJunction(vbe, VoltageBe, vt, TempVCritical, ref limited);
                vbc = Semiconductor.LimitJunction(vbc, VoltageBc, vt, TempVCritical, ref limited);
                if (limited)
                    state.IsConvergent = false;
            }

            // Determine dc current and derivitives
            var vtn = vt * ModelParameters.EmissionCoefficientForward;
            if (vbe > -5 * vtn)
            {
                var evbe = Math.Exp(vbe / vtn);
                CurrentBe = csat * (evbe - 1) + BaseConfiguration.Gmin * vbe;
                CondBe = csat * evbe / vtn + BaseConfiguration.Gmin;
                if (c2.Equals(0)) // Avoid Exp()
                {
                    cben = 0;
                    gben = 0;
                }
                else
                {
                    var evben = Math.Exp(vbe / vte);
                    cben = c2 * (evben - 1);
                    gben = c2 * evben / vte;
                }
            }
            else
            {
                CondBe = -csat / vbe + BaseConfiguration.Gmin;
                CurrentBe = CondBe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }

            vtn = vt * ModelParameters.EmissionCoefficientReverse;
            if (vbc > -5 * vtn)
            {
                var evbc = Math.Exp(vbc / vtn);
                CurrentBc = csat * (evbc - 1) + BaseConfiguration.Gmin * vbc;
                CondBc = csat * evbc / vtn + BaseConfiguration.Gmin;
                if (c4.Equals(0)) // Avoid Exp()
                {
                    cbcn = 0;
                    gbcn = 0;
                }
                else
                {
                    var evbcn = Math.Exp(vbc / vtc);
                    cbcn = c4 * (evbcn - 1);
                    gbcn = c4 * evbcn / vtc;
                }
            }
            else
            {
                CondBc = -csat / vbc + BaseConfiguration.Gmin;
                CurrentBc = CondBc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }

            // Determine base charge terms
            var q1 = 1 / (1 - ModelTemperature.InverseEarlyVoltForward * vbc - ModelTemperature.InverseEarlyVoltReverse * vbe);
            if (oik.Equals(0) && oikr.Equals(0)) // Avoid computations
            {
                BaseCharge = q1;
                Dqbdve = q1 * BaseCharge * ModelTemperature.InverseEarlyVoltReverse;
                Dqbdvc = q1 * BaseCharge * ModelTemperature.InverseEarlyVoltForward;
            }
            else
            {
                var q2 = oik * CurrentBe + oikr * CurrentBc;
                var arg = Math.Max(0, 1 + 4 * q2);
                double sqarg = 1;
                if (!arg.Equals(0)) // Avoid Sqrt()
                    sqarg = Math.Sqrt(arg);
                BaseCharge = q1 * (1 + sqarg) / 2;
                Dqbdve = q1 * (BaseCharge * ModelTemperature.InverseEarlyVoltReverse + oik * CondBe / sqarg);
                Dqbdvc = q1 * (BaseCharge * ModelTemperature.InverseEarlyVoltForward + oikr * CondBc / sqarg);
            }

            // Excess phase calculation
            var ep = new ExcessPhaseEventArgs
            {
                CollectorCurrent = 0.0,
                ExcessPhaseCurrent = CurrentBe,
                ExcessPhaseConduct = CondBe,
                BaseCharge = BaseCharge
            };
            ExcessPhaseCalculation?.Invoke(this, ep);
            var cc = ep.CollectorCurrent;
            var cex = ep.ExcessPhaseCurrent;
            var gex = ep.ExcessPhaseConduct;

            // Determine dc incremental conductances
            cc = cc + (cex - CurrentBc) / BaseCharge - CurrentBc / TempBetaReverse - cbcn;
            var cb = CurrentBe / TempBetaForward + cben + CurrentBc / TempBetaReverse + cbcn;
            var gx = rbpr + rbpi / BaseCharge;
            if (!xjrb.Equals(0)) // Avoid calculations
            {
                var arg1 = Math.Max(cb / xjrb, 1e-9);
                var arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (!gx.Equals(0)) // Do not divide by 0
                gx = 1 / gx;
            var gpi = CondBe / TempBetaForward + gben;
            var gmu = CondBc / TempBetaReverse + gbcn;
            var go = (CondBc + (cex - CurrentBc) * Dqbdvc / BaseCharge) / BaseCharge;
            var gm = (gex - (cex - CurrentBc) * Dqbdve / BaseCharge) / BaseCharge - go;

            VoltageBe = vbe;
            VoltageBc = vbc;
            CollectorCurrent = cc;
            BaseCurrent = cb;
            ConductancePi = gpi;
            ConductanceMu = gmu;
            Transconductance = gm;
            OutputConductance = go;
            ConductanceX = gx;

            // Load current excitation vector
            var ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * (gm + go + gpi) + vbc * go);
            var ceqbc = ModelParameters.BipolarType * (-cc + vbe * (gm + go) - vbc * (gmu + go));
            CollectorPrimePtr.Value += ceqbc;
            BasePrimePtr.Value += -ceqbe - ceqbc;
            EmitterPrimePtr.Value += ceqbe;

            // Load y matrix
            CollectorCollectorPtr.Value += gcpr;
            BaseBasePtr.Value += gx;
            EmitterEmitterPtr.Value += gepr;
            CollectorPrimeCollectorPrimePtr.Value += gmu + go + gcpr;
            BasePrimeBasePrimePtr.Value += gx + gpi + gmu;
            EmitterPrimeEmitterPrimePtr.Value += gpi + gepr + gm + go;
            CollectorCollectorPrimePtr.Value += -gcpr;
            BaseBasePrimePtr.Value += -gx;
            EmitterEmitterPrimePtr.Value += -gepr;
            CollectorPrimeCollectorPtr.Value += -gcpr;
            CollectorPrimeBasePrimePtr.Value += -gmu + gm;
            CollectorPrimeEmitterPrimePtr.Value += -gm - go;
            BasePrimeBasePtr.Value += -gx;
            BasePrimeCollectorPrimePtr.Value += -gmu;
            BasePrimeEmitterPrimePtr.Value += -gpi;
            EmitterPrimeEmitterPtr.Value += -gepr;
            EmitterPrimeCollectorPrimePtr.Value += -go;
            EmitterPrimeBasePrimePtr.Value += -gpi - gm;
        }

        // TODO: I believe this method of checking convergence can be improved. These calculations seem to be common for multiple behaviors.
        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var vbe = ModelParameters.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
            var vbc = ModelParameters.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);
            var delvbe = vbe - VoltageBe;
            var delvbc = vbc - VoltageBe;
            var cchat = CollectorCurrent + (Transconductance + OutputConductance) * delvbe - (OutputConductance + ConductanceMu) * delvbc;
            var cbhat = BaseCurrent + ConductancePi * delvbe + ConductanceMu * delvbc;
            var cc = CollectorCurrent;
            var cb = BaseCurrent;

            // Check convergence
            var tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cbhat - cb) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
