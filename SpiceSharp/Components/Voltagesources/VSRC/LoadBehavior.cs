using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.VoltagesourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("i"), PropertyInfo("Voltage source current")]
        public double GetCurrent(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [PropertyName("p"), PropertyInfo("Instantaneous power")]
        public double GetPower(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) * -state.Solution[BranchEq];
        }
        [PropertyName("v"), PropertyInfo("Instantaneous voltage")]
        public double Voltage { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;
        public int BranchEq { get; protected set; }
        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }
        protected MatrixElement<double> BranchBranchPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>("entity");

            // Setup the waveform
            bp.Waveform?.Setup();

            // Calculate the voltage source's complex value
            if (!bp.DCValue.Given)
            {
                // No DC value: either have a transient value or none
                if (bp.Waveform != null)
                    CircuitWarning.Warning(this, "{0}: No DC value, transient time 0 value used".FormatString(Name));
                else
                    CircuitWarning.Warning(this, "{0}: No value, DC 0 assumed".FormatString(Name));
            }
        }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="propertyName">Parameter</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(string propertyName)
        {
            // Avoid reflection for common components
            switch (propertyName)
            {
                case "i": return GetCurrent;
                case "v": return (RealState state) => Voltage;
                case "p": return GetPower;
                default: return null;
            }
        }
        
        /// <summary>
        /// Connect the load behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Nodes nodes, Solver<double> solver)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            BranchEq = nodes.Create(Name?.Grow("#branch"), Node.NodeType.Current).Index;

            // Get matrix elements
            PosBranchPtr = solver.GetMatrixElement(posNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, posNode);
            NegBranchPtr = solver.GetMatrixElement(negNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, negNode);

            // Get rhs elements
            BranchPtr = solver.GetRhsElement(BranchEq);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosBranchPtr = null;
            BranchPosPtr = null;
            NegBranchPtr = null;
            BranchNegPtr = null;
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
            double time = 0.0;
            double value = 0.0;

            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            if (state.Domain == RealState.DomainType.Time)
            {
                if (simulation is TimeSimulation tsim)
                    time = tsim.Method.Time;

                // Use the waveform if possible
                if (bp.Waveform != null)
                    value = bp.Waveform.At(time);
                else
                    value = bp.DCValue * state.SourceFactor;
            }
            else
            {
                value = bp.DCValue * state.SourceFactor;
            }
            BranchPtr.Value += value;
        }
    }
}
