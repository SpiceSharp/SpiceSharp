using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    public class Voltagesource : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("waveform"), SpiceInfo("The waveform function")]
        public Parameter<Waveform> VSRCwaveform { get; } = new Parameter<Waveform>();
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter<double> VSRCdcValue { get; } = new Parameter<double>();
        [SpiceName("acmag"), SpiceInfo("A.C. Magnitude")]
        public Parameter<double> VSRCacMag { get; } = new Parameter<double>();
        [SpiceName("acphase"), SpiceInfo("A.C. Phase")]
        public Parameter<double> VSRCacPhase { get; } = new Parameter<double>();
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(Circuit ckt, double[] ac)
        {
            switch (ac?.Length ?? -1)
            {
                case 2: VSRCacPhase.Set(ac[1]); goto case 1;
                case 1: VSRCacMag.Set(ac[0]); break;
                case 0: VSRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }
        [SpiceName("acreal"), SpiceInfo("A.C. real part")]
        public double GetAcReal(Circuit ckt) => VSRCac.Real;
        [SpiceName("acimag"), SpiceInfo("A.C. imaginary part")]
        public double GetAcImag(Circuit ckt) => VSRCac.Imaginary;
        [SpiceName("i"), SpiceInfo("Voltage source current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Real.Solution[VSRCbranch];
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(Circuit ckt) => (ckt.State.Real.Solution[VSRCposNode] - ckt.State.Real.Solution[VSRCnegNode]) * -ckt.State.Real.Solution[VSRCbranch];
        
        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node")]
        public int VSRCposNode { get; private set; }
        [SpiceName("neg_node")]
        public int VSRCnegNode { get; private set; }
        public int VSRCbranch { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Complex VSRCac;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Voltagesource(string name) : base(name, 2) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value of the source</param>
        public Voltagesource(string name, string pos, string neg, double dc) : base(name, 2)
        {
            Connect(pos, neg);
            VSRCdcValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The waveform</param>
        public Voltagesource(string name, string pos, string neg, Waveform w) : base(name, 2)
        {
            Connect(pos, neg);
            VSRCwaveform.Set(w);
        }

        /// <summary>
        /// Setup the voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Bind the nodes
            var nodes = BindNodes(ckt);
            VSRCposNode = nodes[0].Index;
            VSRCnegNode = nodes[1].Index;
            VSRCbranch = CreateNode(ckt, CircuitNode.NodeType.Current).Index;

            // Setup the waveform if specified
            VSRCwaveform.Value?.Setup(ckt);
        }

        /// <summary>
        /// Get the model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Calculate the voltage source's complex value
            if (!VSRCdcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (VSRCwaveform.Given)
                    CircuitWarning.Warning(this, $"{Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name}: No value, DC 0 assumed");
            }
            double radians = VSRCacPhase * Circuit.CONSTPI / 180.0;
            VSRCac = new Complex(VSRCacMag * Math.Cos(radians), VSRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Load the device in the circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;
            double time = 0.0;
            double value = 0.0;

            rstate.Matrix[VSRCposNode, VSRCbranch] += 1.0;
            rstate.Matrix[VSRCbranch, VSRCposNode] += 1.0;
            rstate.Matrix[VSRCnegNode, VSRCbranch] -= 1.0;
            rstate.Matrix[VSRCbranch, VSRCnegNode] -= 1.0;

            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (VSRCwaveform.Given)
                    value = VSRCwaveform.Value?.At(time) ?? VSRCdcValue.Value;
                else
                    value = VSRCdcValue * state.SrcFact;
            }
            else
            {
                value = VSRCdcValue * state.SrcFact;
            }
            rstate.Rhs[VSRCbranch] += value;
        }

        /// <summary>
        /// Load the voltage source for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Matrix[VSRCposNode, VSRCbranch] += 1.0;
            cstate.Matrix[VSRCnegNode, VSRCbranch] -= 1.0;
            cstate.Matrix[VSRCbranch, VSRCnegNode] -= 1.0;
            cstate.Matrix[VSRCbranch, VSRCposNode] += 1.0;
            cstate.Rhs[VSRCbranch] += VSRCac;
        }

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            if (VSRCwaveform.Given)
                VSRCwaveform.Value?.Accept(ckt);
        }
    }
}
