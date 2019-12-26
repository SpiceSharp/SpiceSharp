﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using Transistor = SpiceSharp.Components.MosfetBehaviors.Transistor;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="JFET" />.
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base configuration.
        /// </summary>
        protected BiasingConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Gets the drain node.
        /// </summary>
        public Variable DrainPrime { get; private set; }

        /// <summary>
        /// Gets the source node.
        /// </summary>
        public Variable SourcePrime { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        [ParameterName("vgs"), ParameterInfo("Voltage G-S")]
        public double Vgs { get; private set; }

        /// <summary>
        /// Gets the gate-drain voltage.
        /// </summary>
        [ParameterName("vgd"), ParameterInfo("Voltage G-D")]
        public double Vgd { get; private set; }

        /// <summary>
        /// Gets the gate current.
        /// </summary>
        [ParameterName("ig"), ParameterName("cg"), ParameterInfo("Current at gate node")]
        public double Cg { get; private set; }

        /// <summary>
        /// Gets the drain current.
        /// </summary>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Current at drain node")]
        public double Cd { get; private set; }

        /// <summary>
        /// Gets the gate-drain current.
        /// </summary>
        [ParameterName("igd"), ParameterInfo("Current G-D")]
        public double Cgd { get; private set; }

        /// <summary>
        /// Gets the small-signal transconductance.
        /// </summary>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        /// <summary>
        /// Gets the small-signal drain-source conductance.
        /// </summary>
        [ParameterName("gds"), ParameterInfo("Conductance D-S")]
        public double Gds { get; private set; }

        /// <summary>
        /// Gets the small-signal gate-source conductance.
        /// </summary>
        [ParameterName("ggs"), ParameterName("Conductance G-S")]
        public double Ggs { get; private set; }

        /// <summary>
        /// Gets the small-signal gate-drain conductance.
        /// </summary>
        [ParameterName("ggd"), ParameterInfo("Conductance G-D")]
        public double Ggd { get; private set; }

        private readonly IIntegrationMethod _method;
        private readonly int _drainNode, _gateNode, _sourceNode, _drainPrimeNode, _sourcePrimeNode;
        private readonly IIterationSimulationState _iteration;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(3);

            BaseConfiguration = context.Configurations.GetValue<BiasingConfiguration>();
            _iteration = context.GetState<IIterationSimulationState>();
            context.TryGetState(out _time);
            context.TryGetState(out _method);
            _drainNode = BiasingState.Map[context.Nodes[0]];
            _gateNode = BiasingState.Map[context.Nodes[1]];
            _sourceNode = BiasingState.Map[context.Nodes[2]];
            var variables = context.Variables;
            DrainPrime = ModelParameters.DrainResistance > 0 ?
                variables.Create(Name.Combine("drain"), VariableType.Voltage) :
                context.Nodes[0];
            _drainPrimeNode = BiasingState.Map[DrainPrime];
            SourcePrime = ModelParameters.SourceResistance > 0 ? 
                variables.Create(Name.Combine("source"), VariableType.Voltage) :
                context.Nodes[2];
            _sourcePrimeNode = BiasingState.Map[SourcePrime];
            
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_drainNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_sourceNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _drainNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _sourcePrimeNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _sourceNode),
                new MatrixLocation(_sourcePrimeNode, _drainPrimeNode),
                new MatrixLocation(_drainNode, _drainNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_sourceNode, _sourceNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode)
            }, new[] { _gateNode, _drainPrimeNode, _sourcePrimeNode });
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        protected virtual void Load()
        {
            var state = BiasingState;

            // DC model parameters
            var beta = ModelParameters.Beta * BaseParameters.Area;
            var gdpr = ModelParameters.DrainConductance * BaseParameters.Area;
            var gspr = ModelParameters.SourceConductance * BaseParameters.Area;
            var csat = TempSaturationCurrent * BaseParameters.Area;

            double ggs, cg;
            double ggd, cgd;
            double cdrain, gm, gds, betap, bfac;

            // Get the current voltages
            Initialize(out double vgs, out double vgd, out bool check);
            var vds = vgs - vgd;

            // Determine dc current and derivatives 
            if (vgs <= -5 * BaseParameters.Temperature * Constants.KOverQ)
            {
                ggs = -csat / vgs + BaseConfiguration.Gmin;
                cg = ggs * vgs;
            }
            else
            {
                var evgs = Math.Exp(vgs / (BaseParameters.Temperature * Constants.KOverQ));
                ggs = csat * evgs / (BaseParameters.Temperature * Constants.KOverQ) + BaseConfiguration.Gmin;
                cg = csat * (evgs - 1) + BaseConfiguration.Gmin * vgs;
            }

            if (vgd <= -5 * (BaseParameters.Temperature * Constants.KOverQ))
            {
                ggd = -csat / vgd + BaseConfiguration.Gmin;
                cgd = ggd * vgd;
            }
            else
            {
                var evgd = Math.Exp(vgd / (BaseParameters.Temperature * Constants.KOverQ));
                ggd = csat * evgd / (BaseParameters.Temperature * Constants.KOverQ) + BaseConfiguration.Gmin;
                cgd = csat * (evgd - 1) + BaseConfiguration.Gmin * vgd;
            }

            cg += cgd;

            // Modification for Sydney University JFET model
            var vto = ModelParameters.Threshold;
            if (vds >= 0)
            {
                var vgst = vgs - vto;

                // Compute drain current and derivatives for normal mode
                if (vgst <= 0)
                {
                    // Normal mode, cutoff region
                    cdrain = 0;
                    gm = 0;
                    gds = 0;
                }
                else
                {
                    betap = beta * (1 + ModelParameters.LModulation * vds);
                    bfac = ModelTemperature.BFactor;
                    if (vgst >= vds)
                    {
                        // Normal mode, linear region
                        var apart = 2 * ModelParameters.B + 3 * bfac * (vgst - vds);
                        var cpart = vds * (vds * (bfac * vds - ModelParameters.B) + vgst * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * bfac * vgst);
                        gds = betap * (vgst - vds) * apart
                              + beta * ModelParameters.LModulation * cpart;
                    }
                    else
                    {
                        bfac = vgst * bfac;
                        gm = betap * vgst * (2 * ModelParameters.B + 3 * bfac);

                        // Normal mode, saturation region
                        var cpart = vgst * vgst * (ModelParameters.B + bfac);
                        cdrain = betap * cpart;
                        gds = ModelParameters.LModulation * beta * cpart;
                    }
                }
            }
            else
            {
                var vgdt = vgd - vto;

                // Compute drain current and derivatives for inverse mode
                if (vgdt <= 0)
                {
                    // Inverse mode, cutoff region
                    cdrain = 0;
                    gm = 0;
                    gds = 0;
                }
                else
                {
                    betap = beta * (1 - ModelParameters.LModulation * vds);
                    bfac = ModelTemperature.BFactor;
                    if (vgdt + vds >= 0)
                    {
                        // Inverse mode, linear region
                        var apart = 2 * ModelParameters.B + 3 * bfac * (vgdt + vds);
                        var cpart = vds * (-vds * (-bfac * vds - ModelParameters.B) + vgdt * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * bfac * vgdt);
                        gds = betap * (vgdt + vds) * apart
                              - beta * ModelParameters.LModulation * cpart - gm;
                    }
                    else
                    {
                        bfac = vgdt * bfac;
                        gm = -betap * vgdt * (2 * ModelParameters.B + 3 * bfac);

                        // Inverse mode, saturation region
                        var cpart = vgdt * vgdt * (ModelParameters.B + bfac);
                        cdrain = -betap * cpart;
                        gds = ModelParameters.LModulation * beta * cpart - gm;
                    }
                }
            }

            var cd = cdrain - cgd;
            Vgs = vgs;
            Vgd = vgd;
            Cg = cg;
            Cd = cd;
            Cgd = cgd;
            Gm = gm;
            Gds = gds;
            Ggs = ggs;
            Ggd = ggd;

            // Check convergence
            if (_iteration.Mode != IterationModes.Fix || (_time != null && !_time.UseIc))
            {
                if (check)
                    _iteration.IsConvergent = false;
            }

            // Load current vector
            var ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd);
            var ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = ModelParameters.JFETType * (cd + cgd - gds * vds - gm * vgs);

            Elements.Add(
                // Y-matrix
                -gdpr,
                -ggd,
                -ggs,
                -gspr,
                -gdpr,
                gm - ggd,
                -gds - gm,
                -ggs - gm,
                -gspr,
                -gds,
                gdpr,
                ggd + ggs,
                gspr,
                gdpr + gds + ggd,
                gspr + gds + gm + ggs,
                // RHS
                -ceqgs - ceqgd, 
                -cdreq + ceqgd,
                cdreq + ceqgs
                );
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initializes the voltages for the current iteration.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vgd">The VGD.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        protected void Initialize(out double vgs, out double vgd, out bool check)
        {
            var state = BiasingState;

            // Initialization
            check = true;
            if (_iteration.Mode == IterationModes.Junction && _method != null && (_time != null && _time.UseDc && _time.UseIc))
            {
                var vds = ModelParameters.JFETType * BaseParameters.InitialVds;
                vgs = ModelParameters.JFETType * BaseParameters.InitialVgs;
                vgd = vgs - vds;
            }
            else if (_iteration.Mode == IterationModes.Junction && !BaseParameters.Off)
            {
                vgs = -1;
                vgd = -1;
            }
            else if (_iteration.Mode == IterationModes.Junction || _iteration.Mode == IterationModes.Fix && BaseParameters.Off)
            {
                vgs = 0;
                vgd = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vgs = ModelParameters.JFETType * (state.Solution[_gateNode] - state.Solution[_sourcePrimeNode]);
                vgd = ModelParameters.JFETType * (state.Solution[_gateNode] - state.Solution[_drainPrimeNode]);

                // Limit nonlinear branch voltages
                check = false;
                vgs = Semiconductor.LimitJunction(vgs, Vgs,
                    BaseParameters.Temperature * Constants.KOverQ, Vcrit, ref check);
                vgd = Semiconductor.LimitJunction(vgd, Vgd,
                    BaseParameters.Temperature * Constants.KOverQ, Vcrit, ref check);
                vgs = Transistor.LimitFet(vgs, Vgs, ModelParameters.Threshold);
                vgd = Transistor.LimitFet(vgd, Vgd, ModelParameters.Threshold);
            }
        }
    }
}
