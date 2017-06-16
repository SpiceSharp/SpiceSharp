using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents an independent current source
    /// </summary>
    public class Currentsource : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("waveform"), SpiceInfo("The waveform object for this source")]
        public Parameter<Waveform> ISRCwaveform { get; } = new Parameter<Waveform>();
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter<double> ISRCdcValue { get; } = new Parameter<double>();
        [SpiceName("acmag"), SpiceInfo("A.C. magnitude value")]
        public Parameter<double> ISRCacMag { get; } = new Parameter<double>();
        [SpiceName("acphase"), SpiceInfo("A.C. phase value")]
        public Parameter<double> ISRCacPhase { get; } = new Parameter<double>();
        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(Circuit ckt) => (ckt.State.Real.Solution[ISRCposNode] - ckt.State.Real.Solution[ISRCnegNode]);
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(Circuit ckt) => (ckt.State.Real.Solution[ISRCposNode] - ckt.State.Real.Solution[ISRCposNode]) * -ISRCdcValue;
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(Circuit ckt, double[] ac)
        {
            switch (ac?.Length ?? -1)
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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public Currentsource(string name) : base(name, 2)
        {
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
            if (ISRCwaveform.Given)
                ISRCwaveform.Value?.Setup(ckt);
        }

        /// <summary>
        /// No model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            if (!ISRCdcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (ISRCwaveform.Given)
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
                if (ISRCwaveform.Given)
                    value = ISRCwaveform.Value?.At(time) ?? ISRCdcValue.Value;
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
    }
}
