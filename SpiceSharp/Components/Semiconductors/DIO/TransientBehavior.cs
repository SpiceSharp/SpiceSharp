using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Components.DIO;
using SpiceSharp.Simulations;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.Diode"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;
        BaseParameters bp;
        ModelBaseParameters mbp;

        /// <summary>
        /// Diode capacitance
        /// </summary>
        [PropertyName("cd"), PropertyInfo("Diode capacitance")]
        public double DIOcap { get; protected set; }
        public double DIOcurrent { get; protected set; }
        public double DIOconduct { get; protected set; }

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        public StateDerivative DIOcapCharge { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int DIOposNode, DIOnegNode, DIOposPrimeNode;
        protected MatrixElement DIOposPosPrimePtr { get; private set; }
        protected MatrixElement DIOnegPosPrimePtr { get; private set; }
        protected MatrixElement DIOposPrimePosPtr { get; private set; }
        protected MatrixElement DIOposPrimeNegPtr { get; private set; }
        protected MatrixElement DIOposPosPtr { get; private set; }
        protected MatrixElement DIOnegNegPtr { get; private set; }
        protected MatrixElement DIOposPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
            temp = provider.GetBehavior<TemperatureBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }
        
        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            DIOposPosPrimePtr = null;
            DIOnegPosPrimePtr = null;
            DIOposPrimePosPtr = null;
            DIOposPrimeNegPtr = null;
            DIOposPosPtr = null;
            DIOnegNegPtr = null;
            DIOposPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            DIOposNode = pins[0];
            DIOnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra nodes
            DIOposPrimeNode = load.DIOposPrimeNode;

            // Get matrix pointers
            DIOposPosPrimePtr = matrix.GetElement(DIOposNode, DIOposPrimeNode);
            DIOnegPosPrimePtr = matrix.GetElement(DIOnegNode, DIOposPrimeNode);
            DIOposPrimePosPtr = matrix.GetElement(DIOposPrimeNode, DIOposNode);
            DIOposPrimeNegPtr = matrix.GetElement(DIOposPrimeNode, DIOnegNode);
            DIOposPosPtr = matrix.GetElement(DIOposNode, DIOposNode);
            DIOnegNegPtr = matrix.GetElement(DIOnegNode, DIOnegNode);
            DIOposPrimePosPrimePtr = matrix.GetElement(DIOposPrimeNode, DIOposPrimeNode);
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
            DIOcapCharge = states.Create();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        /// <param name="sim">Simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
            var state = sim.State;
            double arg, sarg, capd;
            double vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

            // charge storage elements
            double czero = temp.DIOtJctCap * bp.DIOarea;
            if (vd < temp.DIOtDepCap)
            {
                arg = 1 - vd / mbp.DIOjunctionPot;
                sarg = Math.Exp(-mbp.DIOgradingCoeff * Math.Log(arg));
                DIOcapCharge.Value = mbp.DIOtransitTime * load.DIOcurrent + mbp.DIOjunctionPot * czero * (1 - arg * sarg) / (1 -
                        mbp.DIOgradingCoeff);
                capd = mbp.DIOtransitTime * load.DIOconduct + czero * sarg;
            }
            else
            {
                double czof2 = czero / modeltemp.DIOf2;
                DIOcapCharge.Value = mbp.DIOtransitTime * load.DIOcurrent + czero * temp.DIOtF1 + czof2 * (modeltemp.DIOf3 * (vd -
                    temp.DIOtDepCap) + (mbp.DIOgradingCoeff / (mbp.DIOjunctionPot + mbp.DIOjunctionPot)) * (vd * vd - temp.DIOtDepCap * temp.DIOtDepCap));
                capd = mbp.DIOtransitTime * load.DIOconduct + czof2 * (modeltemp.DIOf3 + mbp.DIOgradingCoeff * vd / mbp.DIOjunctionPot);
            }
            DIOcap = capd;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            var state = sim.State;
            double vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

            // This is the same calculation
            GetDCstate(sim);

            // Integrate
            DIOcapCharge.Integrate();
            double geq = DIOcapCharge.Jacobian(DIOcap);
            double ceq = DIOcapCharge.Current(geq, vd);

            // Load Rhs vector
            state.Rhs[DIOnegNode] += ceq;
            state.Rhs[DIOposPrimeNode] -= ceq;

            // Load Y-matrix
            DIOnegNegPtr.Add(geq);
            DIOposPrimePosPrimePtr.Add(geq);
            DIOnegPosPrimePtr.Sub(geq);
            DIOposPrimeNegPtr.Sub(geq);
        }

        /// <summary>
        /// Use local truncation error to cut timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            DIOcapCharge.LocalTruncationError(ref timestep);
        }
    }
}
