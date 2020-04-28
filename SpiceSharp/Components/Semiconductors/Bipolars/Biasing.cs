﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// DC biasing behavior for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    public class Biasing : Temperature, 
        IBiasingBehavior, 
        IConvergenceBehavior
    {
        private readonly int _collectorNode, _baseNode, _emitterNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the base configuration of the simulation.
        /// </summary>
        protected BiasingParameters BaseConfiguration { get; private set; }

        /// <summary>
        /// Gets the base-emitter voltage.
        /// </summary>
        /// <value>
        /// The base-emitter voltage.
        /// </value>
        [ParameterName("vbe"), ParameterInfo("B-E voltage")]
        public double VoltageBe { get; private set; }

        /// <summary>
        /// Gets the base-collector voltage.
        /// </summary>
        /// <value>
        /// The base-collector voltage.
        /// </value>
        [ParameterName("vbc"), ParameterInfo("B-C voltage")]
        public double VoltageBc { get; private set; }

        /// <summary>
        /// Gets the collector current.
        /// </summary>
        /// <value>
        /// The collector current.
        /// </value>
        [ParameterName("cc"), ParameterName("ic"), ParameterInfo("Current at collector node")]
        public double CollectorCurrent { get; protected set; }

        /// <summary>
        /// Gets the base current.
        /// </summary>
        /// <value>
        /// The base current.
        /// </value>
        [ParameterName("cb"), ParameterName("ib"), ParameterInfo("Current at base node")]
        public double BaseCurrent { get; protected set; }

        /// <summary>
        /// Gets the small signal input conductance - pi.
        /// </summary>
        /// <value>
        /// The small signal input conductance - pi.
        /// </value>
        [ParameterName("gpi"), ParameterInfo("Small signal input conductance - pi")]
        public double ConductancePi { get; protected set; }

        /// <summary>
        /// Gets the small signal conductance mu.
        /// </summary>
        /// <value>
        /// The small signal conductance mu.
        /// </value>
        [ParameterName("gmu"), ParameterInfo("Small signal conductance - mu")]
        public double ConductanceMu { get; protected set; }

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Small signal transconductance")]
        public double Transconductance { get; protected set; }

        /// <summary>
        /// Gets the output conductance.
        /// </summary>
        /// <value>
        /// The output conductance.
        /// </value>
        [ParameterName("go"), ParameterInfo("Small signal output conductance")]
        public double OutputConductance { get; protected set; }

        /// <summary>
        /// Gets the conductance - X.
        /// </summary>
        /// <value>
        /// The conductance - X.
        /// </value>
        public double ConductanceX { get; protected set; }

        /// <summary>
        /// Gets the instantaneously dissipated power.
        /// </summary>
        /// <value>
        /// The instantaneously dissipated power.
        /// </value>
        [ParameterName("p"), ParameterInfo("Power dissipation")]
        public virtual double Power
        {
            get
            {
                var value = CollectorCurrent * BiasingState.Solution[_collectorNode];
                value += BaseCurrent * BiasingState.Solution[_baseNode];
                value -= (CollectorCurrent + BaseCurrent) * BiasingState.Solution[_emitterNode];
                return value;
            }
        }

        /// <summary>
        /// Gets the internal collector node.
        /// </summary>
        /// <value>
        /// The internal collector node.
        /// </value>
        protected IVariable<double> CollectorPrime { get; private set; }

        /// <summary>
        /// Gets the internal base node.
        /// </summary>
        /// <value>
        /// The internal base node.
        /// </value>
        protected IVariable<double> BasePrime { get; private set; }

        /// <summary>
        /// Gets the internal emitter node.
        /// </summary>
        /// <value>
        /// The internal emitter node.
        /// </value>
        protected IVariable<double> EmitterPrime { get; private set; }

        /// <summary>
        /// Gets the base-emitter current.
        /// </summary>
        /// <value>
        /// The base-emitter current.
        /// </value>
        public virtual double CurrentBe { get; protected set; }

        /// <summary>
        /// Gets the base-collector current.
        /// </summary>
        /// <value>
        /// The base-collector current.
        /// </value>
        public virtual double CurrentBc { get; protected set; }

        /// <summary>
        /// Gets the base-emitter conductance.
        /// </summary>
        /// <value>
        /// The base-emitter conductance.
        /// </value>
        public double CondBe { get; protected set; }

        /// <summary>
        /// Gets the base-collector conductance.
        /// </summary>
        /// <value>
        /// The base-collector conductance.
        /// </value>
        public double CondBc { get; protected set; }

        /// <summary>
        /// Gets the base charge.
        /// </summary>
        /// <value>
        /// The base charge.
        /// </value>
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

        /// <summary>
        /// Gets the iteration.
        /// </summary>
        /// <value>
        /// The iteration.
        /// </value>
        protected IIterationSimulationState Iteration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Biasing(string name, ComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.CheckNodes(4);

            // Get configurations
            BaseConfiguration = context.GetSimulationParameterSet<BiasingParameters>();

            // Get states
            Iteration = context.GetState<IIterationSimulationState>();
            CollectorPrime = BiasingState.GetSharedVariable(context.Nodes[0]);
            BasePrime = BiasingState.GetSharedVariable(context.Nodes[1]);
            EmitterPrime = BiasingState.GetSharedVariable(context.Nodes[2]);
            _collectorNode = BiasingState.Map[CollectorPrime];
            _baseNode = BiasingState.Map[BasePrime];
            _emitterNode = BiasingState.Map[EmitterPrime];

            // Add a series collector node if necessary
            if (ModelParameters.CollectorResistance > 0)
                CollectorPrime = BiasingState.CreatePrivateVariable(Name.Combine("col"), Units.Volt);
            _collectorPrimeNode = BiasingState.Map[CollectorPrime];

            // Add a series base node if necessary
            if (ModelParameters.BaseResist > 0)
                BasePrime = BiasingState.CreatePrivateVariable(Name.Combine("base"), Units.Volt);
            _basePrimeNode = BiasingState.Map[BasePrime];

            // Add a series emitter node if necessary
            if (ModelParameters.EmitterResistance > 0)
                EmitterPrime = BiasingState.CreatePrivateVariable(Name.Combine("emit"), Units.Volt);
            _emitterPrimeNode = BiasingState.Map[EmitterPrime];

            // Get solver pointers
            _elements = new ElementSet<double>(BiasingState.Solver, new[] {
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
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        protected virtual void Load()
        {
            double gben;
            double cben;
            double gbcn;
            double cbcn;

            // DC model parameters
            var csat = TempSaturationCurrent * Parameters.Area;
            var rbpr = ModelParameters.MinimumBaseResistance / Parameters.Area;
            var rbpi = ModelParameters.BaseResist / Parameters.Area - rbpr;
            var gcpr = ModelTemperature.CollectorConduct * Parameters.Area;
            var gepr = ModelTemperature.EmitterConduct * Parameters.Area;
            var oik = ModelTemperature.InverseRollOffForward / Parameters.Area;
            var c2 = TempBeLeakageCurrent * Parameters.Area;
            var vte = ModelParameters.LeakBeEmissionCoefficient * Vt;
            var oikr = ModelTemperature.InverseRollOffReverse / Parameters.Area;
            var c4 = TempBcLeakageCurrent * Parameters.Area;
            var vtc = ModelParameters.LeakBcEmissionCoefficient * Vt;
            var xjrb = ModelParameters.BaseCurrentHalfResist * Parameters.Area;

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

            _elements.Add(
                // Y-matrix
                gcpr, gx, gepr, gmu + go + gcpr, gx + gpi + gmu, gpi + gepr + gm + go,
                -gcpr, -gx, -gepr, -gcpr, -gmu + gm, -gm - go, -gx, -gmu, -gpi, -gepr, -go, 
                -gpi - gm,
                // RHS vector
                ceqbc, -ceqbe - ceqbc, ceqbe);
        }

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
        /// <param name="vbe">The base-emitter voltage.</param>
        /// <param name="vbc">The base-collector voltage.</param>
        protected virtual void Initialize(out double vbe, out double vbc)
        {
            var state = BiasingState;

            // Initialization
            if (Iteration.Mode == IterationModes.Junction && !Parameters.Off)
            {
                vbe = TempVCritical;
                vbc = 0;
            }
            else if (Iteration.Mode == IterationModes.Junction || Iteration.Mode == IterationModes.Fix && Parameters.Off)
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
                    Iteration.IsConvergent = false;
            }
        }

        // TODO: I believe this method of checking convergence can be improved. These calculations seem to be common for multiple behaviors.
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
                Iteration.IsConvergent = false;
                return false;
            }

            tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cbhat - cb) > tol)
            {
                Iteration.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
