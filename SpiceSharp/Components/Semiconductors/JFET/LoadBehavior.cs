using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using Transistor = SpiceSharp.Components.MosfetBehaviors.Common.Transistor;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        private BaseConfiguration _baseConfig;
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drain, _gate, _source;
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

        [ParameterName("vgs"), ParameterInfo("Voltage G-S")]
        public double Vgs { get; private set; }

        [ParameterName("vgd"), ParameterInfo("Voltage G-D")]
        public double Vgd { get; private set; }

        [ParameterName("ig"), ParameterName("cg"), ParameterInfo("Current at gate node")]
        public double Cg { get; private set; }

        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Current at drain node")]
        public double Cd { get; private set; }

        [ParameterName("igd"), ParameterInfo("Current G-D")]
        public double Cgd { get; private set; }

        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        [ParameterName("gds"), ParameterInfo("Conductance D-S")]
        public double Gds { get; private set; }

        [ParameterName("ggs"), ParameterName("Conductance G-S")]
        public double Ggs { get; private set; }

        [ParameterName("ggd"), ParameterInfo("Conductance G-D")]
        public double Ggd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public LoadBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        /// <exception cref="ArgumentNullException">pins</exception>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            _drain = pins[0];
            _gate = pins[1];
            _source = pins[2];
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
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get configuration
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            SourcePrimeNode = _mbp.SourceResistance > 0 ? variables.Create(Name.Combine("source")).Index : _source;
            DrainPrimeNode = _mbp.DrainResistance > 0 ? variables.Create(Name.Combine("drain")).Index : _drain;

            GateNodePtr = solver.GetRhsElement(_gate);
            DrainPrimeNodePtr = solver.GetRhsElement(DrainPrimeNode);
            SourcePrimeNodePtr = solver.GetRhsElement(SourcePrimeNode);
            DrainDrainPrimePtr = solver.GetMatrixElement(_drain, DrainPrimeNode);
            GateDrainPrimePtr = solver.GetMatrixElement(_gate, DrainPrimeNode);
            GateSourcePrimePtr = solver.GetMatrixElement(_gate, SourcePrimeNode);
            SourceSourcePrimePtr = solver.GetMatrixElement(_source, SourcePrimeNode);
            DrainPrimeDrainPtr = solver.GetMatrixElement(DrainPrimeNode, _drain);
            DrainPrimeGatePtr = solver.GetMatrixElement(DrainPrimeNode, _gate);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainPrimeNode, SourcePrimeNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(SourcePrimeNode, _gate);
            SourcePrimeSourcePtr = solver.GetMatrixElement(SourcePrimeNode, _source);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourcePrimeNode, DrainPrimeNode);
            DrainDrainPtr = solver.GetMatrixElement(_drain, _drain);
            GateGatePtr = solver.GetMatrixElement(_gate, _gate);
            SourceSourcePtr = solver.GetMatrixElement(_source, _source);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainPrimeNode, DrainPrimeNode);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourcePrimeNode, SourcePrimeNode);
        }
        
        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public override void Load(BaseSimulation simulation)
        {
            var state = simulation.RealState;

            // DC model parameters
            var beta = _mbp.Beta * _bp.Area;
            var gdpr = _mbp.DrainConductance * _bp.Area;
            var gspr = _mbp.SourceConductance * _bp.Area;
            var csat = _temp.TempSaturationCurrent * _bp.Area;

            double vgs, vgd, vds;
            double ggs, cg;
            double ggd, cgd;
            double cdrain, gm, gds, betap, bfac;

            // Initialization
            var check = true;
            if (state.Init == InitializationModes.Junction && simulation is TimeSimulation && state.UseDc && state.UseIc)
            {
                vds = _mbp.JFETType * _bp.InitialVds;
                vgs = _mbp.JFETType * _bp.InitialVgs;
                vgd = vgs - vds;
            }
            else if (state.Init == InitializationModes.Junction && !_bp.Off)
            {
                vgs = -1;
                vgd = -1;
            }
            else if (state.Init == InitializationModes.Junction || state.Init == InitializationModes.Fix && _bp.Off)
            {
                vgs = 0;
                vgd = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vgs = _mbp.JFETType * (state.Solution[_gate] - state.Solution[SourcePrimeNode]);
                vgd = _mbp.JFETType * (state.Solution[_gate] - state.Solution[DrainPrimeNode]);

                // Limit nonlinear branch voltages
                check = false;
                vgs = Semiconductor.LimitJunction(vgs, Vgs,
                    _bp.Temperature * Circuit.KOverQ, _temp.Vcrit, ref check);
                vgd = Semiconductor.LimitJunction(vgd, Vgd,
                    _bp.Temperature * Circuit.KOverQ, _temp.Vcrit, ref check);
                vgs = Transistor.LimitFet(vgs, Vgs, _mbp.Threshold);
                vgd = Transistor.LimitFet(vgd, Vgd, _mbp.Threshold);
            }

            // Determine dc current and derivatives 
            vds = vgs - vgd;
            if (vgs <= -5 * _bp.Temperature * Circuit.KOverQ)
            {
                ggs = -csat / vgs + _baseConfig.Gmin;
                cg = ggs * vgs;
            }
            else
            {
                var evgs = Math.Exp(vgs / (_bp.Temperature * Circuit.KOverQ));
                ggs = csat * evgs / (_bp.Temperature * Circuit.KOverQ) + _baseConfig.Gmin;
                cg = csat * (evgs - 1) + _baseConfig.Gmin * vgs;
            }

            if (vgd <= -5 * (_bp.Temperature * Circuit.KOverQ))
            {
                ggd = -csat / vgd + _baseConfig.Gmin;
                cgd = ggd * vgd;
            }
            else
            {
                var evgd = Math.Exp(vgd / (_bp.Temperature * Circuit.KOverQ));
                ggd = csat * evgd / (_bp.Temperature * Circuit.KOverQ) + _baseConfig.Gmin;
                cgd = csat * (evgd - 1) + _baseConfig.Gmin * vgd;
            }

            cg = cg + cgd;

            // Modification for Sydney University JFET model
            var vto = _mbp.Threshold;
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
                    betap = beta * (1 + _mbp.LModulation * vds);
                    bfac = _modeltemp.BFactor;
                    if (vgst >= vds)
                    {
                        // Normal mode, linear region
                        var apart = 2 * _mbp.B + 3 * bfac * (vgst - vds);
                        var cpart = vds * (vds * (bfac * vds - _mbp.B) + vgst * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * bfac * vgst);
                        gds = betap * (vgst - vds) * apart
                              + beta * _mbp.LModulation * cpart;
                    }
                    else
                    {
                        bfac = vgst * bfac;
                        gm = betap * vgst * (2 * _mbp.B + 3 * bfac);

                        // Normal mode, saturation region
                        var cpart = vgst * vgst * (_mbp.B + bfac);
                        cdrain = betap * cpart;
                        gds = _mbp.LModulation * beta * cpart;
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
                    betap = beta * (1 - _mbp.LModulation * vds);
                    bfac = _modeltemp.BFactor;
                    if (vgdt + vds >= 0)
                    {
                        // Inverse mode, linear region
                        var apart = 2 * _mbp.B + 3 * bfac * (vgdt + vds);
                        var cpart = vds * (-vds * (-bfac * vds - _mbp.B) + vgdt * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * bfac * vgdt);
                        gds = betap * (vgdt + vds) * apart
                              - beta * _mbp.LModulation * cpart - gm;
                    }
                    else
                    {
                        bfac = vgdt * bfac;
                        gm = -betap * vgdt * (2 * _mbp.B + 3 * bfac);

                        // Inverse mode, saturation region
                        var cpart = vgdt * vgdt * (_mbp.B + bfac);
                        cdrain = -betap * cpart;
                        gds = _mbp.LModulation * beta * cpart - gm;
                    }
                }
            }

            var cd = cdrain - cgd;

            // Check convergence
            if (state.Init != InitializationModes.Fix || !state.UseIc)
            {
                if (check)
                    state.IsConvergent = false;
            }

            Vgs = vgs;
            Vgd = vgd;
            Cg = cg;
            Cd = cd;
            Cgd = cgd;
            Gm = gm;
            Gds = gds;
            Ggs = ggs;
            Ggd = ggd;

            // Load current vector
            var ceqgd = _mbp.JFETType * (cgd - ggd * vgd);
            var ceqgs = _mbp.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = _mbp.JFETType * (cd + cgd - gds * vds - gm * vgs);
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
    }
}
