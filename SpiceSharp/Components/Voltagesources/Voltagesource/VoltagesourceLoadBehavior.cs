using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using System;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltagesourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        public Waveform VSRCwaveform { get; set; }
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter VSRCdcValue { get; } = new Parameter();
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
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var vsrc = component as Voltagesource;

            // Get nodes
            VSRCposNode = vsrc.VSRCposNode;
            VSRCnegNode = vsrc.VSRCnegNode;
            VSRCbranch = CreateNode(ckt, component.Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            VSRCposIbrptr = matrix.GetElement(VSRCposNode, VSRCbranch);
            VSRCibrPosptr = matrix.GetElement(VSRCbranch, VSRCposNode);
            VSRCnegIbrptr = matrix.GetElement(VSRCnegNode, VSRCbranch);
            VSRCibrNegptr = matrix.GetElement(VSRCbranch, VSRCnegNode);

            // Setup the waveform if specified
            VSRCwaveform?.Setup(ckt);

            // Calculate the voltage source's complex value
            if (!VSRCdcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (VSRCwaveform != null)
                    CircuitWarning.Warning(this, $"{component.Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{component.Name}: No value, DC 0 assumed");
            }
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
        /// Execute DC or Transient behaviour
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

            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (VSRCwaveform != null)
                    value = VSRCwaveform.At(time);
                else
                    value = VSRCdcValue * state.SrcFact;
            }
            else
            {
                value = VSRCdcValue * state.SrcFact;
            }
            rstate.Rhs[VSRCbranch] += value;
        }
    }
}
