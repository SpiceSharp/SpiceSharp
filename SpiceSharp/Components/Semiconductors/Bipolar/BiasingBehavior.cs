using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConvergenceBehavior
    {
        /// <summary>
        /// Gets the base configuration of the simulation.
        /// </summary>
        protected BiasingConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Gets the base-emitter voltage.
        /// </summary>
        [ParameterName("vbe"), ParameterInfo("B-E voltage")]
        public double VoltageBe { get; private set; }

        /// <summary>
        /// Gets the base-collector voltage.
        /// </summary>
        [ParameterName("vbc"), ParameterInfo("B-C voltage")]
        public double VoltageBc { get; private set; }

        /// <summary>
        /// Gets or modifies the collector current.
        /// </summary>
        [ParameterName("cc"), ParameterName("ic"), ParameterInfo("Current at collector node")]
        public double CollectorCurrent { get; protected set; }

        /// <summary>
        /// Gets or modifies the base current.
        /// </summary>
        [ParameterName("cb"), ParameterName("ib"), ParameterInfo("Current at base node")]
        public double BaseCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the small signal input conductance - pi.
        /// </summary>
        [ParameterName("gpi"), ParameterInfo("Small signal input conductance - pi")]
        public double ConductancePi { get; protected set; }

        /// <summary>
        /// Gets or sets the small signal conductance mu.
        /// </summary>
        [ParameterName("gmu"), ParameterInfo("Small signal conductance - mu")]
        public double ConductanceMu { get; protected set; }

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        [ParameterName("gm"), ParameterInfo("Small signal transconductance")]
        public double Transconductance { get; protected set; }

        /// <summary>
        /// Gets or sets the output conductance.
        /// </summary>
        [ParameterName("go"), ParameterInfo("Small signal output conductance")]
        public double OutputConductance { get; protected set; }

        /// <summary>
        /// Gets or sets the conductance - X.
        /// </summary>
        public double ConductanceX { get; protected set; }

        /// <summary>
        /// Gets the dissipated power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power dissipation")]
        public virtual double GetPower(IBiasingSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var value = CollectorCurrent * state.Solution[_collectorNode];
            value += BaseCurrent * state.Solution[_baseNode];
            value -= (CollectorCurrent + BaseCurrent) * state.Solution[_emitterNode];
            return value;
        }

        /// <summary>
        /// Gets the collector prime node index.
        /// </summary>
        public Variable CollectorPrime { get; private set; }

        /// <summary>
        /// Gets the base prime node index.
        /// </summary>
        public Variable BasePrime { get; private set; }

        /// <summary>
        /// Gets the emitter prime node index.
        /// </summary>
        public Variable EmitterPrime { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets or modifies the base-emitter current.
        /// </summary>
        public virtual double CurrentBe { get; protected set; }

        /// <summary>
        /// Gets or modifies the base-collector current.
        /// </summary>
        public virtual double CurrentBc { get; protected set; }

        /// <summary>
        /// Gets or modifies the base-emitter conductance.
        /// </summary>
        public double CondBe { get; protected set; }

        /// <summary>
        /// Gets or modifies the base-collector conductance.
        /// </summary>
        public double CondBc { get; protected set; }

        /// <summary>
        /// Gets or sets the base charge.
        /// </summary>
        public double BaseCharge { get; protected set; }

        /// <summary>
        /// TODO: Try to factor out this part of the biasing behavior.
        /// Gets or sets the charge to collector voltage derivative.
        /// </summary>
        public double Dqbdvc { get; protected set; }

        /// <summary>
        /// TODO: Try to factor our this part of the biasing behavior.
        /// Gets or sets the charge to emitter voltage derivative.
        /// </summary>
        public double Dqbdve { get; protected set; }

        private IIntegrationMethod _method;
        private int _collectorNode, _baseNode, _emitterNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.CheckNodes(4);

            // Get configurations
            BaseConfiguration = context.Configurations.GetValue<BiasingConfiguration>();

            // Get states
            context.TryGetState(out _method);
            _collectorNode = BiasingState.Map[context.Nodes[0]];
            _baseNode = BiasingState.Map[context.Nodes[1]];
            _emitterNode = BiasingState.Map[context.Nodes[2]];
            var variables = context.Variables;

            // Add a series collector node if necessary
            CollectorPrime = ModelParameters.CollectorResistance.Value > 0 ?
                variables.Create(Name.Combine("col"), VariableType.Voltage) :
                context.Nodes[0];
            _collectorPrimeNode = BiasingState.Map[CollectorPrime];

            // Add a series base node if necessary
            BasePrime = ModelParameters.BaseResist.Value > 0 ?
                variables.Create(Name.Combine("base"), VariableType.Voltage) :
                context.Nodes[1];
            _basePrimeNode = BiasingState.Map[BasePrime];

            // Add a series emitter node if necessary
            EmitterPrime = ModelParameters.EmitterResistance.Value > 0 ?
                variables.Create(Name.Combine("emit"), VariableType.Voltage) :
                context.Nodes[2];
            _emitterPrimeNode = BiasingState.Map[EmitterPrime];

            // Get solver pointers
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_collectorNode, _collectorNode),
                new MatrixLocation(_baseNode, _baseNode),
                new MatrixLocation(_emitterNode, _emitterNode),
                new MatrixLocation(_collectorPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _basePrimeNode),
                new MatrixLocation(_emitterPrimeNode, _emitterPrimeNode),
                new MatrixLocation(_collectorNode, _collectorPrimeNode),
                new MatrixLocation(_baseNode, _basePrimeNode),
                new MatrixLocation(_emitterNode, _emitterPrimeNode),
                new MatrixLocation(_collectorPrimeNode, _collectorNode),
                new MatrixLocation(_collectorPrimeNode, _basePrimeNode),
                new MatrixLocation(_collectorPrimeNode, _emitterPrimeNode),
                new MatrixLocation(_basePrimeNode, _baseNode),
                new MatrixLocation(_basePrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _emitterPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _emitterNode),
                new MatrixLocation(_emitterPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _basePrimeNode)
            }, new[] { _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode });
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        protected virtual void Load()
        {
            double gben;
            double cben;
            double gbcn;
            double cbcn;

            // DC model parameters
            var csat = TempSaturationCurrent * BaseParameters.Area;
            var rbpr = ModelParameters.MinimumBaseResistance / BaseParameters.Area;
            var rbpi = ModelParameters.BaseResist / BaseParameters.Area - rbpr;
            var gcpr = ModelTemperature.CollectorConduct * BaseParameters.Area;
            var gepr = ModelTemperature.EmitterConduct * BaseParameters.Area;
            var oik = ModelTemperature.InverseRollOffForward / BaseParameters.Area;
            var c2 = TempBeLeakageCurrent * BaseParameters.Area;
            var vte = ModelParameters.LeakBeEmissionCoefficient * Vt;
            var oikr = ModelTemperature.InverseRollOffReverse / BaseParameters.Area;
            var c4 = TempBcLeakageCurrent * BaseParameters.Area;
            var vtc = ModelParameters.LeakBcEmissionCoefficient * Vt;
            var xjrb = ModelParameters.BaseCurrentHalfResist * BaseParameters.Area;

            // Get the current voltages
            Initialize(out var vbe, out var vbc);

            // Determine dc current and derivitives
            var vtn = Vt * ModelParameters.EmissionCoefficientForward;
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

            vtn = Vt * ModelParameters.EmissionCoefficientReverse;
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
            var cc = 0.0;
            var cex = CurrentBe;
            var gex = CondBe;
            ExcessPhaseCalculation(ref cc, ref cex, ref gex);

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

            Elements.Add(
                // Y-matrix
                gcpr, gx, gepr, gmu + go + gcpr, gx + gpi + gmu, gpi + gepr + gm + go,
                -gcpr, -gx, -gepr, -gcpr, -gmu + gm, -gm - go, -gx, -gmu, -gpi, -gepr, -go, 
                -gpi - gm,
                // RHS vector
                ceqbc, -ceqbe - ceqbc, ceqbe);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Excess phase calculation.
        /// </summary>
        /// <param name="cc">The collector current.</param>
        /// <param name="cex">The excess phase current.</param>
        /// <param name="gex">The excess phase conductance.</param>
        protected virtual void ExcessPhaseCalculation(ref double cc, ref double cex, ref double gex)
        {
            // This is a time-dependent effect. Not implemented here.
        }

        /// <summary>
        /// Initializes the voltages for the current iteration.
        /// </summary>
        /// <param name="vbe">The VBE.</param>
        /// <param name="vbc">The VBC.</param>
        protected void Initialize(out double vbe, out double vbc)
        {
            var state = BiasingState;

            // Initialization
            if (state.Init == InitializationModes.Junction && (_method != null) && state.UseDc && state.UseIc)
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
                vbe = ModelParameters.BipolarType * (state.Solution[_basePrimeNode] - state.Solution[_emitterPrimeNode]);
                vbc = ModelParameters.BipolarType * (state.Solution[_basePrimeNode] - state.Solution[_collectorPrimeNode]);

                // Limit nonlinear branch voltages
                var limited = false;
                vbe = Semiconductor.LimitJunction(vbe, VoltageBe, Vt, TempVCritical, ref limited);
                vbc = Semiconductor.LimitJunction(vbc, VoltageBc, Vt, TempVCritical, ref limited);
                if (limited)
                    state.IsConvergent = false;
            }
        }

        // TODO: I believe this method of checking convergence can be improved. These calculations seem to be common for multiple behaviors.
        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <returns></returns>
        bool IConvergenceBehavior.IsConvergent()
        {
            var state = BiasingState;
            var vbe = ModelParameters.BipolarType * (state.Solution[_basePrimeNode] - state.Solution[_emitterPrimeNode]);
            var vbc = ModelParameters.BipolarType * (state.Solution[_basePrimeNode] - state.Solution[_collectorPrimeNode]);
            var delvbe = vbe - VoltageBe;
            var delvbc = vbc - VoltageBc;
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
