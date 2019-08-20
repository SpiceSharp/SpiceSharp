using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
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
        protected BaseConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Gets the external drain node.
        /// </summary>
        protected int DrainNode { get; private set; }

        /// <summary>
        /// Gets the external gate node.
        /// </summary>
        protected int GateNode { get; private set; }

        /// <summary>
        /// Gets the external source node.
        /// </summary>
        protected int SourceNode { get; private set; }

        /// <summary>
        /// Gets the drain node.
        /// </summary>
        public int DrainPrimeNode { get; private set; }

        /// <summary>
        /// Gets the source node.
        /// </summary>
        public int SourcePrimeNode { get; private set; }

        /// <summary>
        /// Gets the gate RHS element.
        /// </summary>
        protected VectorElement<double> GateNodePtr { get; private set; }

        /// <summary>
        /// Gets the drain RHS element.
        /// </summary>
        protected VectorElement<double> DrainPrimeNodePtr { get; private set; }

        /// <summary>
        /// Gets the source RHS element.
        /// </summary>
        protected VectorElement<double> SourcePrimeNodePtr { get; private set; }

        /// <summary>
        /// Gets the (external drain, drain) element.
        /// </summary>
        protected MatrixElement<double> DrainDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (gate, drain) element.
        /// </summary>
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (gate, source) element.
        /// </summary>
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external source, source) element.
        /// </summary>
        protected MatrixElement<double> SourceSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external drain, drain) element.
        /// </summary>
        protected MatrixElement<double> DrainPrimeDrainPtr { get; private set; }

        /// <summary>
        /// Gets the (drain, gate) element.
        /// </summary>
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, source) element.
        /// </summary>
        protected MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (source, gate) element.
        /// </summary>
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }

        /// <summary>
        /// Gets the (source, external source) element.
        /// </summary>
        protected MatrixElement<double> SourcePrimeSourcePtr { get; private set; }

        /// <summary>
        /// Gets the (source, drain) element.
        /// </summary>
        protected MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external drain, drain) element.
        /// </summary>
        protected MatrixElement<double> DrainDrainPtr { get; private set; }

        /// <summary>
        /// Gets the (gate, gate) element.
        /// </summary>
        protected MatrixElement<double> GateGatePtr { get; private set; }

        /// <summary>
        /// Gets the (external source, source) element.
        /// </summary>
        protected MatrixElement<double> SourceSourcePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, drain) element.
        /// </summary>
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (source, source) element.
        /// </summary>
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public BiasingBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get configuration
            BaseConfiguration = simulation.Configurations.Get<BaseConfiguration>();

            if (context is ComponentBindingContext cc)
            {
                DrainNode = cc.Pins[0];
                GateNode = cc.Pins[1];
                SourceNode = cc.Pins[2];
            }

            var solver = State.Solver;
            var variables = simulation.Variables;
            SourcePrimeNode = ModelParameters.SourceResistance > 0 ? variables.Create(Name.Combine("source")).Index : SourceNode;
            DrainPrimeNode = ModelParameters.DrainResistance > 0 ? variables.Create(Name.Combine("drain")).Index : DrainNode;

            GateNodePtr = solver.GetRhsElement(GateNode);
            DrainPrimeNodePtr = solver.GetRhsElement(DrainPrimeNode);
            SourcePrimeNodePtr = solver.GetRhsElement(SourcePrimeNode);
            DrainDrainPrimePtr = solver.GetMatrixElement(DrainNode, DrainPrimeNode);
            GateDrainPrimePtr = solver.GetMatrixElement(GateNode, DrainPrimeNode);
            GateSourcePrimePtr = solver.GetMatrixElement(GateNode, SourcePrimeNode);
            SourceSourcePrimePtr = solver.GetMatrixElement(SourceNode, SourcePrimeNode);
            DrainPrimeDrainPtr = solver.GetMatrixElement(DrainPrimeNode, DrainNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(DrainPrimeNode, GateNode);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainPrimeNode, SourcePrimeNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(SourcePrimeNode, GateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(SourcePrimeNode, SourceNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourcePrimeNode, DrainPrimeNode);
            DrainDrainPtr = solver.GetMatrixElement(DrainNode, DrainNode);
            GateGatePtr = solver.GetMatrixElement(GateNode, GateNode);
            SourceSourcePtr = solver.GetMatrixElement(SourceNode, SourceNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainPrimeNode, DrainPrimeNode);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourcePrimeNode, SourcePrimeNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            GateNodePtr = null;
            DrainPrimeNodePtr = null;
            SourcePrimeNodePtr = null;
            DrainDrainPrimePtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            DrainPrimeGatePtr = null;
            DrainPrimeSourcePrimePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            SourcePrimeDrainPrimePtr = null;
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var state = State;

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
            if (state.Init != InitializationModes.Fix || !state.UseIc)
            {
                if (check)
                    state.IsConvergent = false;
            }

            // Load current vector
            var ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd);
            var ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = ModelParameters.JFETType * (cd + cgd - gds * vds - gm * vgs);
            GateNodePtr.Value += -ceqgs - ceqgd;
            DrainPrimeNodePtr.Value += -cdreq + ceqgd;
            SourcePrimeNodePtr.Value += cdreq + ceqgs;

            // Load Y-matrix
            DrainDrainPrimePtr.Value += -gdpr;
            GateDrainPrimePtr.Value += -ggd;
            GateSourcePrimePtr.Value += -ggs;
            SourceSourcePrimePtr.Value += -gspr;
            DrainPrimeDrainPtr.Value += -gdpr;
            DrainPrimeGatePtr.Value += gm - ggd;
            DrainPrimeSourcePrimePtr.Value += -gds - gm;
            SourcePrimeGatePtr.Value += -ggs - gm;
            SourcePrimeSourcePtr.Value += -gspr;
            SourcePrimeDrainPrimePtr.Value += -gds;
            DrainDrainPtr.Value += gdpr;
            GateGatePtr.Value += ggd + ggs;
            SourceSourcePtr.Value += gspr;
            DrainPrimeDrainPrimePtr.Value += gdpr + gds + ggd;
            SourcePrimeSourcePrimePtr.Value += gspr + gds + gm + ggs;
        }

        /// <summary>
        /// Initializes the voltages for the current iteration.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vgd">The VGD.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        protected void Initialize(out double vgs, out double vgd, out bool check)
        {
            var state = State;

            // Initialization
            check = true;
            if (state.Init == InitializationModes.Junction && Simulation is TimeSimulation && state.UseDc && state.UseIc)
            {
                var vds = ModelParameters.JFETType * BaseParameters.InitialVds;
                vgs = ModelParameters.JFETType * BaseParameters.InitialVgs;
                vgd = vgs - vds;
            }
            else if (state.Init == InitializationModes.Junction && !BaseParameters.Off)
            {
                vgs = -1;
                vgd = -1;
            }
            else if (state.Init == InitializationModes.Junction || state.Init == InitializationModes.Fix && BaseParameters.Off)
            {
                vgs = 0;
                vgd = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vgs = ModelParameters.JFETType * (state.Solution[GateNode] - state.Solution[SourcePrimeNode]);
                vgd = ModelParameters.JFETType * (state.Solution[GateNode] - state.Solution[DrainPrimeNode]);

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

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
