using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.NewSparse;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
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
        public double Capacitance { get; protected set; }
        [PropertyName("id"), PropertyName("c"), PropertyInfo("Diode current")]
        public double Current { get; protected set; }

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        public StateDerivative CapCharge { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, posPrimeNode;
        protected MatrixElement<double> PosPosPrimePtr { get; private set; }
        protected MatrixElement<double> NegPosPrimePtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPtr { get; private set; }
        protected MatrixElement<double> PosPrimeNegPtr { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }
        protected MatrixElement<double> PosPrimePosPrimePtr { get; private set; }
        protected VectorElement<double> PosPrimePtr { get; private set; }
        protected VectorElement<double> NegPtr { get; private set; }

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>("entity");
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }
        
        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            PosPosPrimePtr = null;
            NegPosPrimePtr = null;
            PosPrimePosPtr = null;
            PosPrimeNegPtr = null;
            PosPosPtr = null;
            NegNegPtr = null;
            PosPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets equation pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra nodes
            posPrimeNode = load.PosPrimeNode;

            // Get solver pointers
            PosPosPrimePtr = solver.GetMatrixElement(posNode, posPrimeNode);
            NegPosPrimePtr = solver.GetMatrixElement(negNode, posPrimeNode);
            PosPrimePosPtr = solver.GetMatrixElement(posPrimeNode, posNode);
            PosPrimeNegPtr = solver.GetMatrixElement(posPrimeNode, negNode);
            PosPosPtr = solver.GetMatrixElement(posNode, posNode);
            NegNegPtr = solver.GetMatrixElement(negNode, negNode);
            PosPrimePosPrimePtr = solver.GetMatrixElement(posPrimeNode, posPrimeNode);
            PosPrimePtr = solver.GetRhsElement(posPrimeNode);
            NegPtr = solver.GetRhsElement(negNode);
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            CapCharge = states.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg, sarg, capd;
            double vd = state.Solution[posPrimeNode] - state.Solution[negNode];

            // charge storage elements
            double czero = temp.TempJunctionCap * bp.Area;
            if (vd < temp.TempDepletionCap)
            {
                arg = 1 - vd / mbp.JunctionPotential;
                sarg = Math.Exp(-mbp.GradingCoefficient * Math.Log(arg));
                CapCharge.Current = mbp.TransitTime * load.Current + mbp.JunctionPotential * czero * (1 - arg * sarg) / (1 -
                        mbp.GradingCoefficient);
                capd = mbp.TransitTime * load.Conduct + czero * sarg;
            }
            else
            {
                double czof2 = czero / modeltemp.F2;
                CapCharge.Current = mbp.TransitTime * load.Current + czero * temp.TempFactor1 + czof2 * (modeltemp.F3 * (vd -
                    temp.TempDepletionCap) + (mbp.GradingCoefficient / (mbp.JunctionPotential + mbp.JunctionPotential)) * (vd * vd - temp.TempDepletionCap * temp.TempDepletionCap));
                capd = mbp.TransitTime * load.Conduct + czof2 * (modeltemp.F3 + mbp.GradingCoefficient * vd / mbp.JunctionPotential);
            }
            Capacitance = capd;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vd = state.Solution[posPrimeNode] - state.Solution[negNode];

            // This is the same calculation
            GetDCState(simulation);

            // Integrate
            CapCharge.Integrate();
            double geq = CapCharge.Jacobian(Capacitance);
            double ceq = CapCharge.RhsCurrent(geq, vd);

            // Store the current
            Current = load.Current + CapCharge.Derivative;

            // Load Rhs vector
            NegPtr.Value += ceq;
            PosPrimePtr.Value -= ceq;

            // Load Y-matrix
            NegNegPtr.Value += geq;
            PosPrimePosPrimePtr.Value += geq;
            NegPosPrimePtr.Value -= geq;
            PosPrimeNegPtr.Value -= geq;
        }

        /// <summary>
        /// Use local truncation error to cut timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            CapCharge.LocalTruncationError(ref timestep);
        }
    }
}
