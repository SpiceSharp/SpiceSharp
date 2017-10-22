using System;
using System.Numerics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [SpicePins("V+", "V-"), VoltageDriver(0, 1), IndependentSource]
    public class Voltagesource : CircuitComponent<Voltagesource>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        public IWaveform VSRCwaveform { get; set; }
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter VSRCdcValue { get; } = new Parameter();
        [SpiceName("acmag"), SpiceInfo("A.C. Magnitude")]
        public Parameter VSRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. Phase")]
        public Parameter VSRCacPhase { get; } = new Parameter();
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
        /// Get the complex current through the voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public Complex GetComplexCurrent(Circuit ckt) => ckt.State.Complex.Solution[VSRCbranch];
        
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
        public Complex VSRCac;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public Voltagesource(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Voltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double dc) : base(name)
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
        public Voltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, IWaveform w) : base(name)
        {
            Connect(pos, neg);
            VSRCwaveform = w;
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
            VSRCbranch = CreateNode(ckt, Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;

            // Setup the waveform if specified
            VSRCwaveform?.Setup(ckt);
        }

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
                if (VSRCwaveform != null)
                    CircuitWarning.Warning(this, $"{Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name}: No value, DC 0 assumed");
            }
            double radians = VSRCacPhase * Circuit.CONSTPI / 180.0;
            VSRCac = new Complex(VSRCacMag * Math.Cos(radians), VSRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            if (VSRCwaveform != null)
                VSRCwaveform.Accept(ckt);
        }
    }
}
