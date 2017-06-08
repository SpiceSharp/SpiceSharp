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
        /// The parameters for voltage sources
        /// </summary>
        private static Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>()
        {
            { "dc", new ParameterInfo(ParameterAccess.IOPP, typeof(double), "D.C. source value", "DcValue") },
            { "acmag", new ParameterInfo(ParameterAccess.IOPPA, typeof(double), "A.C. Magnitude", "AcMag") },
            { "acphase", new ParameterInfo(ParameterAccess.IOPAAU, typeof(double), "A.C. Phase", "AcPhase") },
            { "waveform", new ParameterInfo(ParameterAccess.IOP, typeof(Waveform), "The waveform of the source", "Waveform") },
            { "pos_node", new ParameterInfo(ParameterAccess.OPU, typeof(int), "Positive node of source", "PosNode") },
            { "neg_node", new ParameterInfo(ParameterAccess.OPU, typeof(int), "Negative node of source", "NegNode") },
            { "acreal", new ParameterInfo(ParameterAccess.OPU, typeof(double), "AC real part") },
            { "acimag", new ParameterInfo(ParameterAccess.OPU, typeof(double), "AC imaginary part") },
            { "ac", new ParameterInfo(ParameterAccess.IP, typeof(double[]), "AC magnitude, phase vector") },
            { "i", new ParameterInfo(ParameterAccess.OP, typeof(double), "Voltage source current") },
            { "p", new ParameterInfo(ParameterAccess.OP, typeof(double), "Instantaneous power") },
            { "distof1", new ParameterInfo(ParameterAccess.IP, typeof(double[]), "f1 input for distortion") },
            { "distof2", new ParameterInfo(ParameterAccess.IP, typeof(double[]), "f2 input for distortion") }
        };

        /// <summary>
        /// Parameters
        /// </summary>
        public Parameter<Waveform> Waveform { get; } = new Parameter<Waveform>();
        public Parameter<double> DcValue { get; } = new Parameter<double>();
        public Parameter<double> AcMag { get; } = new Parameter<double>();
        public Parameter<double> AcPhase { get; } = new Parameter<double>();
        public Parameter<double> DF1mag { get; } = new Parameter<double>();
        public Parameter<double> DF2mag { get; } = new Parameter<double>();
        public Parameter<double> DF1phase { get; } = new Parameter<double>();
        public Parameter<double> DF2phase { get; } = new Parameter<double>();
        private Complex ac;
        
        /// <summary>
        /// Nodes
        /// </summary>
        public int PosNode { get; private set; }
        public int NegNode { get; private set; }
        public int Branch { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Voltagesource(string name) : base(name, 2)
        {
        }

        /// <summary>
        /// Set a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        /// <param name="ckt">The circuit if applicable</param>
        public override void Set(string name, object value, Circuit ckt = null)
        {
            base.Set(name, value, ckt);

            double[] v;
            switch (name)
            {
                case "ac":
                    v = (double[])value;
                    switch (v.Length)
                    {
                        case 2: AcPhase.Set(v[1]); goto case 1;
                        case 1: AcMag.Set(v[0]); break;
                        case 0: AcMag.Set(AcMag.Value); break;
                    }
                    break;
                case "distof1":
                    v = (double[])value;
                    switch (v.Length)
                    {
                        case 2: DF1phase.Set(v[1]); DF1mag.Set(v[0]); break;
                        case 1: DF1mag.Set(v[0]); DF1phase.Set(0.0); break;
                        case 0: DF1mag.Set(1.0); DF1phase.Set(0.0); break;
                    }
                    break;
                case "distof2":
                    v = (double[])value;
                    switch (v.Length)
                    {
                        case 2: DF2phase.Set(v[1]); DF2mag.Set(v[0]); break;
                        case 1: DF2mag.Set(v[0]); DF2phase.Set(0.0); break;
                        case 0: DF2mag.Set(1.0); DF2phase.Set(0.0); break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Ask a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="ckt">The circuit if applicable</param>
        /// <returns></returns>
        public override object Ask(string name, Circuit ckt = null)
        {
            object result = base.Ask(name, ckt);
            if (result != null)
                return result;

            // Special
            switch (name)
            {
                case "acreal": return ac.Real;
                case "acimag": return ac.Imaginary;
                case "i":
                    return ckt.State.Solution[Branch];
                case "p":
                    return (ckt.State.Solution[PosNode] - ckt.State.Solution[NegNode]) * -ckt.State.Solution[Branch];
                default: return null;
            }
        }

        /// <summary>
        /// Get all parameters
        /// </summary>
        public override Dictionary<string, ParameterInfo> Parameters => parameters;

        /// <summary>
        /// Setup the voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Bind the nodes
            var nodes = BindNodes(ckt, CircuitNode.NodeType.Current);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
            Branch = nodes[2].Index;

            // Setup the waveform if specified
            if (Waveform.Value != null)
                Waveform.Value.Setup(ckt);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Calculate the voltage source's complex value
            if (!DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (Waveform.Given)
                    CircuitWarning.Warning(this, $"{Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name}: No value, DC 0 assumed");
            }
            double radians = AcPhase * Circuit.CONSTPI / 180.0;
            ac = new Complex(AcMag * Math.Cos(radians), AcMag * Math.Sin(radians));
        }

        /// <summary>
        /// Load the device in the circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            double time = 0.0;

            state.Matrix[PosNode, Branch] += 1.0;
            state.Matrix[Branch, PosNode] += 1.0;
            state.Matrix[NegNode, Branch] -= 1.0;
            state.Matrix[Branch, NegNode] -= 1.0;

            if (state.IsDc && DcValue.Given)
            {
                // Grab DC value
                state.Rhs[Branch] += state.SrcFact * DcValue;
            }
            else
            {
                if (state.IsDc)
                    time = 0.0;
                else if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the transient functions
                if (Waveform.Given && Waveform.Value != null)
                    state.Rhs[Branch] += Waveform.Value.At(time);
                else
                    state.Rhs[Branch] += DcValue;
            }
        }

        public override void Accept(Circuit ckt)
        {
            if (Waveform.Given)
                Waveform.Value?.Accept(ckt);
        }
    }
}
