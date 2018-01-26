using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
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
        /// Properties
        /// </summary>
        [PropertyName("i"), PropertyInfo("Voltage source current")]
        public double GetCurrent(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [PropertyName("p"), PropertyInfo("Instantaneous power")]
        public double GetPower(State state)
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
        protected int posNode, negNode;
        public int BranchEq { get; protected set; }
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrIbrPtr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Setup the waveform
            bp.Waveform?.Setup();

            // Calculate the voltage source's complex value
            if (!bp.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (bp.Waveform != null)
                    CircuitWarning.Warning(this, $"{Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name}: No value, DC 0 assumed");
            }
        }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="property">Parameter</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            // Avoid reflection for common components
            switch (property)
            {
                case "i": return GetCurrent;
                case "v": return (State state) => Voltage;
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
                throw new CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            BranchEq = nodes.Create(Name?.Grow("#branch"), Node.NodeType.Current).Index;
            PosIbrPtr = matrix.GetElement(posNode, BranchEq);
            IbrPosPtr = matrix.GetElement(BranchEq, posNode);
            NegIbrPtr = matrix.GetElement(negNode, BranchEq);
            IbrNegPtr = matrix.GetElement(BranchEq, negNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosIbrPtr = null;
            IbrPosPtr = null;
            NegIbrPtr = null;
            IbrNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double time = 0.0;
            double value = 0.0;

            PosIbrPtr.Value.Real += 1.0;
            IbrPosPtr.Value.Real += 1.0;
            NegIbrPtr.Value.Real -= 1.0;
            IbrNegPtr.Value.Real -= 1.0;

            if (state.Domain == State.DomainTypes.Time)
            {
                if (sim is TimeSimulation tsim)
                    time = tsim.Method.Time;

                // Use the waveform if possible
                if (bp.Waveform != null)
                    value = bp.Waveform.At(time);
                else
                    value = bp.DcValue * state.SrcFact;
            }
            else
            {
                value = bp.DcValue * state.SrcFact;
            }
            state.Rhs[BranchEq] += value;
        }
    }
}
