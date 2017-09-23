using System.Numerics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [SpicePins("C+", "C-"), ConnectedPins()]
    public class Capacitor : CircuitComponent<Capacitor>
    {
        /// <summary>
        /// Set the model for the capacitor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(CapacitorModel model) => Model = model;

        /// <summary>
        /// Capacitance
        /// </summary>
        [SpiceName("capacitance"), SpiceInfo("Device capacitance", IsPrincipal = true)]
        public Parameter CAPcapac { get; } = new Parameter();
        [SpiceName("ic"), SpiceInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter CAPinitCond { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Device width", Interesting = false)]
        public Parameter CAPwidth { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Device length", Interesting = false)]
        public Parameter CAPlength { get; } = new Parameter();
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(string name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(string name, string pos, string neg, double cap) : base(name)
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
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            CapacitorModel model = Model as CapacitorModel;
            if (model == null)
                return;

            /* Default Value Processing for Capacitor Instance */
            if (!CAPwidth.Given)
                CAPwidth.Value = model.CAPdefWidth;
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
            var method = ckt.Method;

            bool cond1 = (state.UseDC && state.Init == Circuits.CircuitState.InitFlags.InitJct) || state.UseIC;

            if (cond1)
                vcap = CAPinitCond;
            else
                vcap = rstate.OldSolution[CAPposNode] - rstate.OldSolution[CAPnegNode];

            // Fill the matrix
            state.States[0][CAPstate + CAPqcap] = CAPcapac * vcap;

            // Without integration, a capacitor cannot do anything
            if (method != null)
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

        /// <summary>
        /// Load the capacitance for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            Complex val = cstate.Laplace * CAPcapac.Value;

            // Load the matrix
            cstate.Matrix[CAPposNode, CAPposNode] += val;
            cstate.Matrix[CAPposNode, CAPnegNode] -= val;
            cstate.Matrix[CAPnegNode, CAPposNode] -= val;
            cstate.Matrix[CAPnegNode, CAPnegNode] += val;
        }

        /// <summary>
        /// Accept a timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Copy DC states when accepting the first timepoint
            var method = ckt.Method;
            if (method != null && method.SavedTime == 0.0)
            {
                ckt.State.CopyDC(CAPstate + CAPqcap);
            }
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            ckt.Method.Terr(CAPstate + CAPqcap, ckt, ref timeStep);
        }
    }
}
