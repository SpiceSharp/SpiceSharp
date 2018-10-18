using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// A common loading behavior for mosfets
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
    public abstract class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseConfiguration _baseConfig;
        private ModelBaseParameters _mbp;
        private BaseParameters _bp;

        /// <summary>
        /// Gets or sets the drain current.
        /// </summary>
        /// <value>
        /// The drain current.
        /// </value>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double DrainCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-source current.
        /// </summary>
        /// <value>
        /// The bulk-source current.
        /// </value>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double BsCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-drain current.
        /// </summary>
        /// <value>
        /// The bulk-drain current.
        /// </value>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double BdCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Transconductance { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk transconductance.
        /// </summary>
        /// <value>
        /// The bulk transconductance.
        /// </value>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double TransconductanceBs { get; protected set; }

        /// <summary>
        /// Gets or sets the output conductance.
        /// </summary>
        /// <value>
        /// The output conductance.
        /// </value>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double CondDs { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-source conductance.
        /// </summary>
        /// <value>
        /// The bulk-source conductance.
        /// </value>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double CondBs { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-drain conductance.
        /// </summary>
        /// <value>
        /// The bulk-drain conductance.
        /// </value>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double CondBd { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Mode { get; protected set; }
        public double VoltageGs { get; protected set; }
        public double VoltageDs { get; protected set; }
        public double VoltageBs { get; protected set; }
        public double VoltageBd { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int DrainNode { get; private set; }
        protected int GateNode { get; private set; }
        protected int SourceNode { get; private set; }
        protected int BulkNode { get; private set; }
        public int DrainNodePrime { get; private set; }
        public int SourceNodePrime { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<double> DrainDrainPtr { get; private set; }
        protected MatrixElement<double> GateGatePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePtr { get; private set; }
        protected MatrixElement<double> BulkBulkPtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateBulkPtr { get; private set; }
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<double> BulkDrainPrimePtr { get; private set; }
        protected MatrixElement<double> BulkSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<double> BulkGatePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }
        protected VectorElement<double> BulkPtr { get; private set; }
        protected VectorElement<double> DrainPrimePtr { get; private set; }
        protected VectorElement<double> SourcePrimePtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected LoadBehavior(string name) : base(name)
        {
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

            DrainNode = pins[0];
            GateNode = pins[1];
            SourceNode = pins[2];
            BulkNode = pins[3];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add series drain node if necessary
            if (_mbp.DrainResistance > 0 || _mbp.SheetResistance > 0 && _bp.DrainSquares > 0)
                DrainNodePrime = variables.Create(Name.Combine("drain")).Index;
            else
                DrainNodePrime = DrainNode;

            // Add series source node if necessary
            if (_mbp.SourceResistance > 0 || _mbp.SheetResistance > 0 && _bp.SourceSquares > 0)
                SourceNodePrime = variables.Create(Name.Combine("source")).Index;
            else
                SourceNodePrime = SourceNode;

            // Get matrix pointers
            DrainDrainPtr = solver.GetMatrixElement(DrainNode, DrainNode);
            GateGatePtr = solver.GetMatrixElement(GateNode, GateNode);
            SourceSourcePtr = solver.GetMatrixElement(SourceNode, SourceNode);
            BulkBulkPtr = solver.GetMatrixElement(BulkNode, BulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainNodePrime, DrainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourceNodePrime, SourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(DrainNode, DrainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(GateNode, BulkNode);
            GateDrainPrimePtr = solver.GetMatrixElement(GateNode, DrainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(GateNode, SourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(SourceNode, SourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(BulkNode, DrainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(BulkNode, SourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainNodePrime, SourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(DrainNodePrime, DrainNode);
            BulkGatePtr = solver.GetMatrixElement(BulkNode, GateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(DrainNodePrime, GateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(SourceNodePrime, GateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(SourceNodePrime, SourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(DrainNodePrime, BulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(SourceNodePrime, BulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourceNodePrime, DrainNodePrime);

            // Get rhs pointers
            BulkPtr = solver.GetRhsElement(BulkNode);
            DrainPrimePtr = solver.GetRhsElement(DrainNodePrime);
            SourcePrimePtr = solver.GetRhsElement(SourceNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _mbp = null;

            // Remove references
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            BulkBulkPtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateBulkPtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            BulkDrainPrimePtr = null;
            BulkSourcePrimePtr = null;
            DrainPrimeSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            BulkGatePtr = null;
            DrainPrimeGatePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            DrainPrimeBulkPtr = null;
            SourcePrimeBulkPtr = null;
            SourcePrimeDrainPrimePtr = null;
        }

        public override void Load(BaseSimulation simulation)
        {
            
        }

        /// <summary>
        /// Check convergence
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double cdhat;

            var vbs = _mbp.MosfetType * (state.Solution[BulkNode] - state.Solution[SourceNodePrime]);
            var vgs = _mbp.MosfetType * (state.Solution[GateNode] - state.Solution[SourceNodePrime]);
            var vds = _mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
            var vbd = vbs - vds;
            var vgd = vgs - vds;
            var vgdo = VoltageGs - VoltageDs;
            var delvbs = vbs - VoltageBs;
            var delvbd = vbd - VoltageBd;
            var delvgs = vgs - VoltageGs;
            var delvds = vds - VoltageDs;
            var delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: Cd does not include contributions for transient simulations... Should check for a way to include them!
            if (Mode >= 0)
            {
                cdhat = DrainCurrent - CondBd * delvbd + TransconductanceBs * delvbs +
                    Transconductance * delvgs + CondDs * delvds;
            }
            else
            {
                cdhat = DrainCurrent - (CondBd - TransconductanceBs) * delvbd -
                    Transconductance * delvgd + CondDs * delvds;
            }
            var cbhat = BsCurrent + BdCurrent + CondBd * delvbd + CondBs * delvbs;

            /*
             *  check convergence
             */
            var tol = _baseConfig.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + _baseConfig.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = _baseConfig.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + _baseConfig.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
