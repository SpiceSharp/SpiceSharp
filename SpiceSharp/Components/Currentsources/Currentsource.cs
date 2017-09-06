using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents an independent current source
    /// </summary>
    [SpicePins("I+", "I-"), IndependentSource, ConnectedPins()]
    public class Currentsource : CircuitComponent<Currentsource>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("waveform"), SpiceInfo("The waveform object for this source")]
        public IWaveform ISRCwaveform { get; set; } = null;
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter ISRCdcValue { get; } = new Parameter();
        [SpiceName("acmag"), SpiceInfo("A.C. magnitude value")]
        public Parameter ISRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. phase value")]
        public Parameter ISRCacPhase { get; } = new Parameter();
        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(Circuit ckt) => (ckt.State.Real.Solution[ISRCposNode] - ckt.State.Real.Solution[ISRCnegNode]);
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(Circuit ckt) => (ckt.State.Real.Solution[ISRCposNode] - ckt.State.Real.Solution[ISRCposNode]) * -ISRCdcValue;
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

        public int ISRCposNode { get; private set; }
        public int ISRCnegNode { get; private set; }
        private Complex ISRCac;

        /// <summary>
        /// Get the complex voltage across the current source
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public Complex GetComplexVoltage(Circuit ckt) => ckt.State.Complex.Solution[ISRCposNode] - ckt.State.Complex.Solution[ISRCnegNode];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public Currentsource(string name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Currentsource(string name, string pos, string neg, double dc) : base(name)
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
        public Currentsource(string name, string pos, string neg, IWaveform w) : base(name)
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

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            if (!ISRCdcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (ISRCwaveform != null)
                    CircuitWarning.Warning(this, $"{Name} has no DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name} has no value, DC 0 assumed");
            }
            double radians = ISRCacPhase * Circuit.CONSTPI / 180.0;
            ISRCac = new Complex(ISRCacMag * Math.Cos(radians), ISRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Load the current source in the circuit
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (ISRCwaveform != null)
                    value = ISRCwaveform.At(time);
                else
                    value = ISRCdcValue * state.SrcFact;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = ISRCdcValue * state.SrcFact;
            }

            rstate.Rhs[ISRCposNode] += value;
            rstate.Rhs[ISRCnegNode] -= value;
        }

        /// <summary>
        /// Load the current source in the circuit for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Rhs[ISRCposNode] += ISRCac;
            cstate.Rhs[ISRCnegNode] -= ISRCac;
        }
    }
}
