using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using System;
using Transistor = SpiceSharp.Components.Mosfets.Transistor;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Biasing behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    [BehaviorFor(typeof(JFET)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Temperature,
        IBiasingBehavior
    {
        private readonly IIntegrationMethod _method;
        private readonly int _drainNode, _gateNode, _sourceNode, _drainPrimeNode, _sourcePrimeNode;
        private readonly IIterationSimulationState _iteration;
        private readonly ITimeSimulationState _time;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the biasing parameters.
        /// </summary>
        /// <value>
        /// The biasing parameters.
        /// </value>
        protected BiasingParameters BiasingParameters { get; }

        /// <summary>
        /// Gets the internal drain node.
        /// </summary>
        /// <value>
        /// The internal drain node.
        /// </value>
        protected IVariable<double> DrainPrime { get; }

        /// <summary>
        /// Gets the internal source node.
        /// </summary>
        /// <value>
        /// The internal source node.
        /// </value>
        protected IVariable<double> SourcePrime { get; }

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        /// <value>
        /// The gate-source voltage.
        /// </value>
        [ParameterName("vgs"), ParameterInfo("Voltage G-S")]
        public double Vgs { get; private set; }

        /// <summary>
        /// Gets the gate-drain voltage.
        /// </summary>
        /// <value>
        /// The gate-drain voltage.
        /// </value>
        [ParameterName("vgd"), ParameterInfo("Voltage G-D")]
        public double Vgd { get; private set; }

        /// <summary>
        /// Gets the gate current.
        /// </summary>
        /// <value>
        /// The gate current.
        /// </value>
        [ParameterName("ig"), ParameterName("cg"), ParameterInfo("Current at gate node")]
        public double Cg { get; private set; }

        /// <summary>
        /// Gets the drain current.
        /// </summary>
        /// <value>
        /// The drain current.
        /// </value>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Current at drain node")]
        public double Cd { get; private set; }

        /// <summary>
        /// Gets the gate-drain current.
        /// </summary>
        /// <value>
        /// The gate-drain current.
        /// </value>
        [ParameterName("igd"), ParameterInfo("Current G-D")]
        public double Cgd { get; private set; }

        /// <summary>
        /// Gets the small-signal transconductance.
        /// </summary>
        /// <value>
        /// The small-signal transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        /// <summary>
        /// Gets the small-signal drain-source conductance.
        /// </summary>
        /// <value>
        /// The small-signal drain-source conductance.
        /// </value>
        [ParameterName("gds"), ParameterInfo("Conductance D-S")]
        public double Gds { get; private set; }

        /// <summary>
        /// Gets the small-signal gate-source conductance.
        /// </summary>
        /// <value>
        /// The small-signal gate-source conductance.
        /// </value>
        [ParameterName("ggs"), ParameterName("Conductance G-S")]
        public double Ggs { get; private set; }

        /// <summary>
        /// Gets the small-signal gate-drain conductance.
        /// </summary>
        /// <value>
        /// The small-signal gate-drain conductance.
        /// </value>
        [ParameterName("ggd"), ParameterInfo("Conductance G-D")]
        public double Ggd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.Nodes.CheckNodes(3);

            BiasingParameters = context.GetSimulationParameterSet<BiasingParameters>();
            _iteration = context.GetState<IIterationSimulationState>();
            context.TryGetState(out _time);
            context.TryGetState(out _method);

            DrainPrime = BiasingState.GetSharedVariable(context.Nodes[0]);
            _drainNode = BiasingState.Map[DrainPrime];
            _gateNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            SourcePrime = BiasingState.GetSharedVariable(context.Nodes[2]);
            _sourceNode = BiasingState.Map[SourcePrime];

            if (ModelParameters.DrainResistance > 0)
                DrainPrime = BiasingState.CreatePrivateVariable(Name.Combine("drain"), Units.Volt);
            _drainPrimeNode = BiasingState.Map[DrainPrime];

            if (ModelParameters.SourceResistance > 0)
                SourcePrime = BiasingState.CreatePrivateVariable(Name.Combine("source"), Units.Volt);
            _sourcePrimeNode = BiasingState.Map[SourcePrime];

            _elements = new ElementSet<double>(BiasingState.Solver, [
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
            ], [_gateNode, _drainPrimeNode, _sourcePrimeNode]);
        }

        /// <inheritdoc/>
        protected virtual void Load()
        {
            // DC model parameters
            double beta = ModelParameters.Beta * Parameters.Area;
            double gdpr = ModelParameters.DrainConductance * Parameters.Area;
            double gspr = ModelParameters.SourceConductance * Parameters.Area;
            double csat = TempSaturationCurrent * Parameters.Area;

            double ggs, cg;
            double ggd, cgd;
            double cdrain, gm, gds, betap, bfac;

            // Get the current voltages
            Initialize(out double vgs, out double vgd, out bool check);
            double vds = vgs - vgd;

            // Determine dc current and derivatives 
            if (vgs <= -5 * Parameters.Temperature * Constants.KOverQ)
            {
                ggs = -csat / vgs + BiasingParameters.Gmin;
                cg = ggs * vgs;
            }
            else
            {
                double evgs = Math.Exp(vgs / (Parameters.Temperature * Constants.KOverQ));
                ggs = csat * evgs / (Parameters.Temperature * Constants.KOverQ) + BiasingParameters.Gmin;
                cg = csat * (evgs - 1) + BiasingParameters.Gmin * vgs;
            }

            if (vgd <= -5 * (Parameters.Temperature * Constants.KOverQ))
            {
                ggd = -csat / vgd + BiasingParameters.Gmin;
                cgd = ggd * vgd;
            }
            else
            {
                double evgd = Math.Exp(vgd / (Parameters.Temperature * Constants.KOverQ));
                ggd = csat * evgd / (Parameters.Temperature * Constants.KOverQ) + BiasingParameters.Gmin;
                cgd = csat * (evgd - 1) + BiasingParameters.Gmin * vgd;
            }

            cg += cgd;

            // Modification for Sydney University JFET model
            double vto = ModelParameters.Threshold;
            if (vds >= 0)
            {
                double vgst = vgs - vto;

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
                        double apart = 2 * ModelParameters.B + 3 * bfac * (vgst - vds);
                        double cpart = vds * (vds * (bfac * vds - ModelParameters.B) + vgst * apart);
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
                        double cpart = vgst * vgst * (ModelParameters.B + bfac);
                        cdrain = betap * cpart;
                        gds = ModelParameters.LModulation * beta * cpart;
                    }
                }
            }
            else
            {
                double vgdt = vgd - vto;

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
                        double apart = 2 * ModelParameters.B + 3 * bfac * (vgdt + vds);
                        double cpart = vds * (-vds * (-bfac * vds - ModelParameters.B) + vgdt * apart);
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
                        double cpart = vgdt * vgdt * (ModelParameters.B + bfac);
                        cdrain = -betap * cpart;
                        gds = ModelParameters.LModulation * beta * cpart - gm;
                    }
                }
            }

            double cd = cdrain - cgd;
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
            double ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd);
            double ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs);
            double cdreq = ModelParameters.JFETType * (cd + cgd - gds * vds - gm * vgs);

            double m = Parameters.ParallelMultiplier;
            _elements.Add(
                // Y-matrix
                -gdpr * m,
                -ggd * m,
                -ggs * m,
                -gspr * m,
                -gdpr * m,
                (gm - ggd) * m,
                (-gds - gm) * m,
                (-ggs - gm) * m,
                -gspr * m,
                -gds * m,
                gdpr * m,
                (ggd + ggs) * m,
                gspr * m,
                (gdpr + gds + ggd) * m,
                (gspr + gds + gm + ggs) * m,
                // RHS
                (-ceqgs - ceqgd) * m,
                (-cdreq + ceqgd) * m,
                (cdreq + ceqgs) * m
                );
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initializes the voltages for the current iteration.
        /// </summary>
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vgd">The gate-drain voltage.</param>
        /// <param name="check">If set to <c>true</c>, the voltages were limited to avoid blowing up of the currents.</param>
        protected void Initialize(out double vgs, out double vgd, out bool check)
        {
            var state = BiasingState;

            // Initialization
            check = true;
            if (_iteration.Mode == IterationModes.Junction && _method != null && (_time != null && _time.UseDc && _time.UseIc))
            {
                double vds = ModelParameters.JFETType * Parameters.InitialVds;
                vgs = ModelParameters.JFETType * Parameters.InitialVgs;
                vgd = vgs - vds;
            }
            else if (_iteration.Mode == IterationModes.Junction && !Parameters.Off)
            {
                vgs = -1;
                vgd = -1;
            }
            else if (_iteration.Mode == IterationModes.Junction || _iteration.Mode == IterationModes.Fix && Parameters.Off)
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
                    Parameters.Temperature * Constants.KOverQ, Vcrit, ref check);
                vgd = Semiconductor.LimitJunction(vgd, Vgd,
                    Parameters.Temperature * Constants.KOverQ, Vcrit, ref check);
                vgs = Transistor.LimitFet(vgs, Vgs, ModelParameters.Threshold);
                vgd = Transistor.LimitFet(vgd, Vgd, ModelParameters.Threshold);
            }
        }
    }
}
