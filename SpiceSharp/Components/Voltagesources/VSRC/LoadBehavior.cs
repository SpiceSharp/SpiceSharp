using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using System;
using SpiceSharp.Components.VSRC;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// General behavior for <see cref="Components.Voltagesource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("i"), SpiceInfo("Voltage source current")]
        public double GetCurrent(State state) => state.Solution[VSRCbranch];
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(State state) => (state.Solution[VSRCposNode] - state.Solution[VSRCnegNode]) * -state.Solution[VSRCbranch];
        [SpiceName("v"), SpiceInfo("Instantaneous voltage")]
        public double VSRCvoltage { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int VSRCposNode, VSRCnegNode;
        public int VSRCbranch { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement VSRCposIbrptr { get; private set; }
        protected MatrixElement VSRCnegIbrptr { get; private set; }
        protected MatrixElement VSRCibrPosptr { get; private set; }
        protected MatrixElement VSRCibrNegptr { get; private set; }
        protected MatrixElement VSRCibrIbrptr { get; private set; }

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
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Setup the waveform
            bp.VSRCwaveform?.Setup();

            // Calculate the voltage source's complex value
            if (!bp.VSRCdcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (bp.VSRCwaveform != null)
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
            switch (property)
            {
                case "i": return GetCurrent;
                case "v": return (State state) => VSRCvoltage;
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
            // Get nodes
            VSRCposNode = pins[0];
            VSRCnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            VSRCbranch = nodes.Create(Name?.Grow("#branch"), Node.NodeType.Current).Index;
            VSRCposIbrptr = matrix.GetElement(VSRCposNode, VSRCbranch);
            VSRCibrPosptr = matrix.GetElement(VSRCbranch, VSRCposNode);
            VSRCnegIbrptr = matrix.GetElement(VSRCnegNode, VSRCbranch);
            VSRCibrNegptr = matrix.GetElement(VSRCbranch, VSRCnegNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            VSRCposIbrptr = null;
            VSRCibrPosptr = null;
            VSRCnegIbrptr = null;
            VSRCibrNegptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            var state = sim.State;
            double time = 0.0;
            double value = 0.0;

            VSRCposIbrptr.Value.Real += 1.0;
            VSRCibrPosptr.Value.Real += 1.0;
            VSRCnegIbrptr.Value.Real -= 1.0;
            VSRCibrNegptr.Value.Real -= 1.0;

            if (state.Domain == State.DomainTypes.Time)
            {
                if (sim is TimeSimulation tsim)
                    time = tsim.Method.Time;

                // Use the waveform if possible
                if (bp.VSRCwaveform != null)
                    value = bp.VSRCwaveform.At(time);
                else
                    value = bp.VSRCdcValue * state.SrcFact;
            }
            else
            {
                value = bp.VSRCdcValue * state.SrcFact;
            }
            state.Rhs[VSRCbranch] += value;
        }
    }
}
