using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private LoadBehavior _load;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative Qgs { get; private set; }
        protected StateDerivative Qgd { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _gate, _drainPrime, _sourcePrime;
        protected VectorElement<double> GateNodePtr { get; private set; }
        protected VectorElement<double> DrainPrimeNodePtr { get; private set; }
        protected VectorElement<double> SourcePrimeNodePtr { get; private set; }
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<double> GateGatePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the G-S capacitance.
        /// </summary>
        /// <value>
        /// The G-S capacitance.
        /// </value>
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the G-D capacitance.
        /// </summary>
        /// <value>
        /// The G-D capacitance.
        /// </value>
        public double CapGd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name) : base(name)
        {
        }
        
        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        /// <exception cref="ArgumentNullException">pins</exception>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            _gate = pins[1];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
            _drainPrime = _load.DrainPrimeNode;
            _sourcePrime = _load.SourcePrimeNode;

            GateNodePtr = solver.GetRhsElement(_gate);
            DrainPrimeNodePtr = solver.GetRhsElement(_drainPrime);
            SourcePrimeNodePtr = solver.GetRhsElement(_sourcePrime);
            GateDrainPrimePtr = solver.GetMatrixElement(_gate, _drainPrime);
            GateSourcePrimePtr = solver.GetMatrixElement(_gate, _sourcePrime);
            DrainPrimeGatePtr = solver.GetMatrixElement(_drainPrime, _gate);
            SourcePrimeGatePtr = solver.GetMatrixElement(_sourcePrime, _gate);
            GateGatePtr = solver.GetMatrixElement(_gate, _gate);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(_drainPrime, _drainPrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(_sourcePrime, _sourcePrime);
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        public override void CreateStates(IntegrationMethod method)
        {
            Qgs = method.CreateDerivative();
            Qgd = method.CreateDerivative();
        }
        
        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="T:SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public override void GetDcState(TimeSimulation simulation)
        {
            var vgs = _load.Vgs;
            var vgd = _load.Vgd;
            CalculateStates(vgs, vgd);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Transient(TimeSimulation simulation)
        {
            // Calculate the states
            var vgs = _load.Vgs;
            var vgd = _load.Vgd;
            CalculateStates(vgs, vgd);

            // Integrate and add contributions
            Qgs.Integrate();
            var ggs = Qgs.Jacobian(CapGs);
            var cg = Qgs.Derivative;
            Qgd.Integrate();
            var ggd = Qgd.Jacobian(CapGd);
            cg = cg + Qgd.Derivative;
            var cd = -Qgd.Derivative;
            var cgd = Qgd.Derivative;

            var ceqgd = _mbp.JFETType * (cgd - ggd * vgd);
            var ceqgs = _mbp.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = _mbp.JFETType * (cd + cgd);
            GateNodePtr.Value += -ceqgs - ceqgd;
            DrainPrimeNodePtr.Value += -cdreq + ceqgd;
            SourcePrimeNodePtr.Value += cdreq + ceqgs;

            // Load Y-matrix
            GateDrainPrimePtr.Value += -ggd;
            GateSourcePrimePtr.Value += -ggs;
            DrainPrimeGatePtr.Value += -ggd;
            SourcePrimeGatePtr.Value += -ggs;
            GateGatePtr.Value += ggd + ggs;
            DrainPrimeDrainPrimePtr.Value += ggd;
            SourcePrimeSourcePrimePtr.Value += ggs;
        }

        /// <summary>
        /// Calculates the states.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vgd">The VGD.</param>
        private void CalculateStates(double vgs, double vgd)
        {
            // Charge storage elements
            var czgs = _temp.TempCapGs * _bp.Area;
            var czgd = _temp.TempCapGd * _bp.Area;
            var twop = _temp.TempGatePotential + _temp.TempGatePotential;
            var fcpb2 = _temp.CorDepCap * _temp.CorDepCap;
            var czgsf2 = czgs / _modeltemp.F2;
            var czgdf2 = czgd / _modeltemp.F2;
            if (vgs < _temp.CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / _temp.TempGatePotential);
                Qgs.Current = twop * czgs * (1 - sarg);
                CapGs = czgs / sarg;
            }
            else
            {
                Qgs.Current = czgs * _temp.F1 + czgsf2 *
                              (_modeltemp.F3 * (vgs - _temp.CorDepCap) + (vgs * vgs - fcpb2) / (twop + twop));
                CapGs = czgsf2 * (_modeltemp.F3 + vgs / twop);
            }

            if (vgd < _temp.CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / _temp.TempGatePotential);
                Qgd.Current = twop * czgd * (1 - sarg);
                CapGd = czgd / sarg;
            }
            else
            {
                Qgd.Current = czgd * _temp.F1 + czgdf2 *
                              (_modeltemp.F3 * (vgd - _temp.CorDepCap) + (vgd * vgd - fcpb2) / (twop + twop));
                CapGd = czgdf2 * (_modeltemp.F3 + vgd / twop);
            }
        }
    }
}
