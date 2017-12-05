using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [SpicePins("I+", "I-"), IndependentSource, ConnectedPins()]
    public class Currentsource : CircuitComponent
    {
        /// <summary>
        /// Register default behaviors
        /// </summary>
        static Currentsource()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(Currentsource), typeof(ComponentBehaviors.CurrentsourceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Currentsource), typeof(ComponentBehaviors.CurrentsourceAcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Currentsource), typeof(ComponentBehaviors.CurrentsourceAcceptBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("waveform"), SpiceInfo("The waveform object for this source")]
        public Waveform ISRCwaveform { get; set; } = null;
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter ISRCdcValue { get; } = new Parameter();
        [SpiceName("acmag"), SpiceInfo("A.C. magnitude value")]
        public Parameter ISRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. phase value")]
        public Parameter ISRCacPhase { get; } = new Parameter();
        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(Circuit ckt) => (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCnegNode]);
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(Circuit ckt) => (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCposNode]) * -ISRCdcValue;
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            switch (ac.Length)
            {
                case 2: ISRCacPhase.Set(ac[1]); goto case 1;
                case 1: ISRCacMag.Set(ac[0]); break;
                case 0: ISRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }
        [SpiceName("c"), SpiceInfo("Current through current source")]
        public double Current { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public int ISRCposNode { get; private set; }
        public int ISRCnegNode { get; private set; }
        public Complex ISRCac;

        /// <summary>
        /// Get the complex voltage across the current source
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public Complex GetComplexVoltage(Circuit ckt) => ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCnegNode];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public Currentsource(CircuitIdentifier name) : base(name, 2)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Currentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double dc) : base(name, 2)
        {
            Connect(pos, neg);
            ISRCdcValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The Waveform-object</param>
        public Currentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, Waveform w) : base(name, 2)
        {
            Connect(pos, neg);
            ISRCwaveform = w;
        }

        /// <summary>
        /// Setup the current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            ISRCposNode = nodes[0].Index;
            ISRCnegNode = nodes[1].Index;

            // Setup waveform
            ISRCwaveform?.Setup(ckt);
        }
    }
}
