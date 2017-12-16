using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Transient behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("capacitance"), SpiceInfo("Device capacitance", IsPrincipal = true)]
        public Parameter CAPcapac { get; } = new Parameter();
        [SpiceName("ic"), SpiceInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter CAPinitCond { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Device current")]
        public double GetCurrent(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap];
        [SpiceName("p"), SpiceInfo("Instantaneous device power")]
        public double GetPower(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap] * (ckt.State.Solution[CAPposNode] - ckt.State.Solution[CAPnegNode]);

        /// <summary>
        /// Nodes and states
        /// </summary>
        public int CAPstate { get; private set; }
        private int CAPposNode, CAPnegNode;
        private MatrixElement CAPposPosptr;
        private MatrixElement CAPnegNegptr;
        private MatrixElement CAPposNegptr;
        private MatrixElement CAPnegPosptr;

        /// <summary>
        /// States
        /// </summary>
        public const int CAPqcap = 0;
        public const int CAPccap = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public TransientBehavior()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cap">Capacitance</param>
        public TransientBehavior(double cap)
        {
            CAPcapac.Set(cap);
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(Entity component, Circuit ckt)
        {
            // If the capacitance is not given, try getting it from the temperature behavior
            if (!CAPcapac.Given)
            {
                var temp = component.GetBehavior(typeof(Behaviors.TemperatureBehavior)) as TemperatureBehavior;
                if (temp != null)
                    CAPcapac.Value = temp.CAPcapac;
            }

            // Allocate states
            CAPstate = ckt.State.GetState(2);

            // Get nodes
            var cap = component as Capacitor;
            CAPposNode = cap.CAPposNode;
            CAPnegNode = cap.CAPnegNode;

            // Get matrix pointers
            var matrix = ckt.State.Matrix;
            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            CAPposPosptr = null;
            CAPnegNegptr = null;
            CAPnegPosptr = null;
            CAPposNegptr = null;
        }

        /// <summary>
        /// Execute behaviour for DC and Transient analysis
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            double vcap;
            var ckt = sim.Circuit;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;

            bool cond1 = (state.UseDC && state.Init == State.InitFlags.InitJct) || state.UseIC;

            if (cond1)
                vcap = CAPinitCond;
            else
                vcap = rstate.Solution[CAPposNode] - rstate.Solution[CAPnegNode];

            // Fill the matrix
            state.States[0][CAPstate + CAPqcap] = CAPcapac * vcap;
            if (state.Init == State.InitFlags.InitTransient)
                state.States[1][CAPstate + CAPqcap] = state.States[0][CAPstate + CAPqcap];

            // Integrate
            var result = ckt.Method.Integrate(state, CAPstate + CAPqcap, CAPcapac);
            if (state.Init == State.InitFlags.InitTransient)
                state.States[1][CAPstate + CAPqcap] = state.States[0][CAPstate + CAPqcap];

            CAPposPosptr.Add(result.Geq);
            CAPnegNegptr.Add(result.Geq);
            CAPposNegptr.Sub(result.Geq);
            CAPnegPosptr.Sub(result.Geq);
            state.Rhs[CAPposNode] -= result.Ceq;
            state.Rhs[CAPnegNode] += result.Ceq;
        }
    }
}
