using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Components.CAP;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// General behavior for <see cref="Components.Capacitor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("i"), SpiceInfo("Device current")]
        public double GetCurrent() => CAPqcap.Derivative;
        [SpiceName("p"), SpiceInfo("Instantaneous device power")]
        public double GetPower(Circuit ckt) => CAPqcap.Derivative * (ckt.State.Solution[CAPposNode] - ckt.State.Solution[CAPnegNode]);

        /// <summary>
        /// Nodes and states
        /// </summary>
        int CAPposNode, CAPnegNode;
        MatrixElement CAPposPosptr;
        MatrixElement CAPnegNegptr;
        MatrixElement CAPposNegptr;
        MatrixElement CAPnegPosptr;
        StateVariable CAPqcap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool of behaviors</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            // Get base parameters
            bp = parameters.Get<BaseParameters>();
        }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            CAPposNode = pins[0];
            CAPnegNode = pins[1];
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
            CAPqcap = states.Create();
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        /// <param name="sim"></param>
        public override void GetDCstate(TimeSimulation sim)
        {
            // Calculate the state for DC
            var sol = sim.Circuit.State.Solution;
            if (bp.CAPinitCond.Given)
                CAPqcap.Value = bp.CAPinitCond;
            else
                CAPqcap.Value = bp.CAPcapac * (sol[CAPposNode] - sol[CAPnegNode]);
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
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            var state = sim.Circuit.State;
            double vcap = state.Solution[CAPposNode] - state.Solution[CAPnegNode];

            // Integrate
            CAPqcap.Value = bp.CAPcapac * vcap;
            var result = CAPqcap.Integrate(bp.CAPcapac);

            // Load matrix
            CAPposPosptr.Add(result.Geq);
            CAPnegNegptr.Add(result.Geq);
            CAPposNegptr.Sub(result.Geq);
            CAPnegPosptr.Sub(result.Geq);

            // Load Rhs vector
            state.Rhs[CAPposNode] -= result.Ceq;
            state.Rhs[CAPnegNode] += result.Ceq;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            CAPqcap.LocalTruncationError(ref timestep);
        }
    }
}
