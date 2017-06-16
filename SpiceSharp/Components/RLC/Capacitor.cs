using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a capacitor
    /// </summary>
    public class Capacitor : CircuitComponent
    {
        /// <summary>
        /// A standard model if no model is given
        /// </summary>
        protected static CapacitorModel defaultmodel = new CapacitorModel(null);

        /// <summary>
        /// Gets or sets the model used by this capacitor
        /// </summary>
        public CapacitorModel Model { get; set; }

        /// <summary>
        /// Capacitance
        /// </summary>
        [SpiceName("capacitance"), SpiceInfo("Device capacitance", IsPrincipal = true)]
        public Parameter<double> CAPcapac { get; } = new Parameter<double>();
        [SpiceName("ic"), SpiceInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter<double> CAPinitCond { get; } = new Parameter<double>();
        [SpiceName("w"), SpiceInfo("Device width", Interesting = false)]
        public Parameter<double> CAPwidth { get; } = new Parameter<double>();
        [SpiceName("l"), SpiceInfo("Device length", Interesting = false)]
        public Parameter<double> CAPlength { get; } = new Parameter<double>();
        [SpiceName("i"), SpiceInfo("Device current")]
        public double GetCurrent(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap];
        [SpiceName("p"), SpiceInfo("Instantaneous device power")]
        public double GetPower(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap] * (ckt.State.Real.Solution[CAPposNode] - ckt.State.Real.Solution[CAPnegNode]);

        /// <summary>
        /// Nodes and states
        /// </summary>
        public int CAPstate { get; private set; }
        public int CAPposNode { get; private set; }
        public int CAPnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int CAPqcap = 0;
        private const int CAPccap = 1;
        private const int CAPsensxp = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(string name) : base(name, 2) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(string name, string pos, string neg, double cap) : base(name, 2)
        {
            Connect(pos, neg);
            CAPcapac.Set(cap);
        }
        
        /// <summary>
        /// Setup the capacitor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CAPposNode = nodes[0].Index;
            CAPnegNode = nodes[1].Index;

            // Create to states for integration
            CAPstate = ckt.State.GetState(2);
        }

        /// <summary>
        /// Get the model of the capacitor
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => Model;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            CapacitorModel model = Model as CapacitorModel ?? defaultmodel;

            /* Default Value Processing for Capacitor Instance */
            if (!CAPwidth.Given)
            {
                CAPwidth.Value = model.CAPdefWidth;
            }
            if (!CAPcapac.Given)
            {
                CAPcapac.Value =
                        model.CAPcj *
                            (CAPwidth - model.CAPnarrow) *
                            (CAPlength - model.CAPnarrow) +
                        model.CAPcjsw * 2 * (
                            (CAPlength - model.CAPnarrow) +
                            (CAPwidth - model.CAPnarrow));
            }
        }

        /// <summary>
        /// Load the capacitance
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            double vcap;
            var state = ckt.State;
            var rstate = state.Real;

            if (state.UseIC && CAPinitCond.Given)
                vcap = CAPinitCond;
            else
                vcap = rstate.OldSolution[CAPposNode] - rstate.OldSolution[CAPnegNode];

            // Fill the matrix
            state.States[0][CAPstate + CAPqcap] = CAPcapac * vcap;

            // Without integration, a capacitor cannot do anything
            if (ckt.Method != null)
            {
                var result = ckt.Method.Integrate(state, CAPstate + CAPqcap, CAPcapac);

                rstate.Matrix[CAPposNode, CAPposNode] += result.Geq;
                rstate.Matrix[CAPnegNode, CAPnegNode] += result.Geq;
                rstate.Matrix[CAPposNode, CAPnegNode] -= result.Geq;
                rstate.Matrix[CAPnegNode, CAPposNode] -= result.Geq;
                rstate.Rhs[CAPposNode] -= result.Ceq;
                rstate.Rhs[CAPnegNode] += result.Ceq;
            }
        }
    }
}
