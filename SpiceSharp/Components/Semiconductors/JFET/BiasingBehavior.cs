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
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the base configuration.
        /// </summary>
        /// <value>
        /// The base configuration.
        /// </value>
        protected BaseConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int DrainNode { get; private set; }
        protected int GateNode { get; private set; }
        protected int SourceNode { get; private set; }
        public int DrainPrimeNode { get; private set; }
        public int SourcePrimeNode { get; private set; }
        protected VectorElement<double> GateNodePtr { get; private set; }
        protected VectorElement<double> DrainPrimeNodePtr { get; private set; }
        protected VectorElement<double> SourcePrimeNodePtr { get; private set; }
        protected MatrixElement<double> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> DrainDrainPtr { get; private set; }
        protected MatrixElement<double> GateGatePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }

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
        /// Gets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        /// <summary>
        /// Gets the drain-source conductance.
        /// </summary>
        /// <value>
        /// The drain-source conductance.
        /// </value>
        [ParameterName("gds"), ParameterInfo("Conductance D-S")]
        public double Gds { get; private set; }

        /// <summary>
        /// Gets the gate-source conductance.
        /// </summary>
        /// <value>
        /// The GGS.
        /// </value>
        [ParameterName("ggs"), ParameterName("Conductance G-S")]
        public double Ggs { get; private set; }

        /// <summary>
        /// Gets the gate-drain conductance.
        /// </summary>
        /// <value>
        /// The gate-drain conductance.
        /// </value>
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
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        /// <exception cref="ArgumentNullException">pins</exception>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 3);
            DrainNode = pins[0];
            GateNode = pins[1];
            SourceNode = pins[2];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            simulation.ThrowIfNull(nameof(simulation));

            // Get configuration
            BaseConfiguration = simulation.Configurations.Get<BaseConfiguration>();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            variables.ThrowIfNull(nameof(variables));
            solver.ThrowIfNull(nameof(solver));
            
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
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public void Load(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var state = simulation.RealState;

            // DC model parameters
            var beta = ModelParameters.Beta * BaseParameters.Area;
            var gdpr = ModelParameters.DrainConductance * BaseParameters.Area;
            var gspr = ModelParameters.SourceConductance * BaseParameters.Area;
            var csat = TempSaturationCurrent * BaseParameters.Area;

            double ggs, cg;
            double ggd, cgd;
            double cdrain, gm, gds, betap, bfac;

            // Get the current voltages
            Initialize(simulation, out double vgs, out double vgd, out bool check);
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
        /// <param name="simulation">The simulation.</param>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vgd">The VGD.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        protected void Initialize(BaseSimulation simulation, out double vgs, out double vgd, out bool check)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var state = simulation.RealState;

            // Initialization
            check = true;
            if (state.Init == InitializationModes.Junction && simulation is TimeSimulation && state.UseDc && state.UseIc)
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
        /// <param name="simulation">The base simulation.</param>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent(BaseSimulation simulation) => true;
    }
}
