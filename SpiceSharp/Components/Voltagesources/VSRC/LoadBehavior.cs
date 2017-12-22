using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using System;
using SpiceSharp.Components.VSRC;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// General behavior for <see cref="Voltagesource"/>
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
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[VSRCbranch];
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[VSRCposNode] - ckt.State.Solution[VSRCnegNode]) * -ckt.State.Solution[VSRCbranch];

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
        /// Create a getter
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="parameter">Parameter</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(Circuit ckt, string parameter)
        {
            switch (parameter)
            {
                case "i": return () => ckt.State.Solution[VSRCbranch];
                case "p": return () => (ckt.State.Solution[VSRCposNode] - ckt.State.Solution[VSRCnegNode]) * -ckt.State.Solution[VSRCbranch];
                default:
                    return base.CreateGetter(ckt, parameter);
            }
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="pool"></param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            bp = parameters.Get<BaseParameters>();

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
        /// Execute DC or Transient behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;
            double time = 0.0;
            double value = 0.0;

            VSRCposIbrptr.Value.Real += 1.0;
            VSRCibrPosptr.Value.Real += 1.0;
            VSRCnegIbrptr.Value.Real -= 1.0;
            VSRCibrNegptr.Value.Real -= 1.0;

            if (state.Domain == State.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

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
            rstate.Rhs[VSRCbranch] += value;
        }
    }
}
