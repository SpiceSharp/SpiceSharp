using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseFrequencyBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private LoadBehavior _load;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Gets the G-S capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capgs"), ParameterInfo("Capacitance G-S")]
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the G-D capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capgd"), ParameterInfo("Capacitance G-D")]
        public double CapGd { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drain, _gate, _source, _drainPrime, _sourcePrime;
        protected MatrixElement<Complex> DrainDrainPtr { get; private set; }
        protected MatrixElement<Complex> GateGatePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name) : base(name)
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
            _drain = pins[0];
            _gate = pins[1];
            _source = pins[2];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
            _load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            _drainPrime = _load.DrainPrimeNode;
            _sourcePrime = _load.SourcePrimeNode;

            DrainDrainPtr = solver.GetMatrixElement(_drain, _drain);
            GateGatePtr = solver.GetMatrixElement(_gate, _gate);
            SourceSourcePtr = solver.GetMatrixElement(_source, _source);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(_drainPrime, _drainPrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(_sourcePrime, _sourcePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(_drain, _drainPrime);
            GateDrainPrimePtr = solver.GetMatrixElement(_gate, _drainPrime);
            GateSourcePrimePtr = solver.GetMatrixElement(_gate, _sourcePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(_source, _sourcePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(_drainPrime, _drain);
            DrainPrimeGatePtr = solver.GetMatrixElement(_drainPrime, _gate);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(_drainPrime, _sourcePrime);
            SourcePrimeGatePtr = solver.GetMatrixElement(_sourcePrime, _gate);
            SourcePrimeSourcePtr = solver.GetMatrixElement(_sourcePrime, _source);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(_sourcePrime, _drainPrime);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public override void InitializeParameters(FrequencySimulation simulation)
        {
            var vgs = _load.Vgs;
            var vgd = _load.Vgd;

            // Calculate charge storage elements
            var czgs = _temp.TempCapGs * _bp.Area;
            var czgd = _temp.TempCapGd * _bp.Area;
            var twop = _temp.TempGatePotential + _temp.TempGatePotential;
            var czgsf2 = czgs / _modeltemp.F2;
            var czgdf2 = czgd / _modeltemp.F2;
            if (vgs < _temp.CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / _temp.TempGatePotential);
                CapGs = czgs / sarg;
            }
            else
                CapGs = czgsf2 * (_modeltemp.F3 + vgs / twop);

            if (vgd < _temp.CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / _temp.TempGatePotential);
                CapGd = czgd / sarg;
            }
            else
                CapGd = czgdf2 * (_modeltemp.F3 + vgd / twop);
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _mbp = null;
            _temp = null;
            _modeltemp = null;
            _load = null;

            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            DrainPrimeGatePtr = null;
            DrainPrimeSourcePrimePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            SourcePrimeDrainPrimePtr = null;
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public override void Load(FrequencySimulation simulation)
        {
            var omega = simulation.ComplexState.Laplace.Imaginary;

            var gdpr=_mbp.DrainConductance * _bp.Area;
            var gspr=_mbp.SourceConductance * _bp.Area;
            var gm = _load.Gm;
            var gds = _load.Gds;
            var ggs = _load.Ggs;
            var xgs= CapGs * omega ;
            var ggd = _load.Ggd;
            var xgd= CapGd * omega ;

            DrainDrainPtr.Value += gdpr;
            GateGatePtr.Value += new Complex(ggd+ggs, xgd+xgs);
            SourceSourcePtr.Value += gspr;
            DrainPrimeDrainPrimePtr.Value += new Complex(gdpr+gds+ggd, xgd);
            SourcePrimeSourcePrimePtr.Value += new Complex(gspr+gds+gm+ggs, xgs);
            DrainDrainPrimePtr.Value -= gdpr;
            GateDrainPrimePtr.Value -= new Complex(ggd, xgd);
            GateSourcePrimePtr.Value -= new Complex(ggs, xgs);
            SourceSourcePrimePtr.Value -= gspr;
            DrainPrimeDrainPtr.Value -= gdpr;
            DrainPrimeGatePtr.Value += new Complex(-ggd+gm, -xgd);
            DrainPrimeSourcePrimePtr.Value += (-gds-gm);
            SourcePrimeGatePtr.Value -= new Complex(ggs + gm, xgs);
            SourcePrimeSourcePtr.Value -= gspr;
            SourcePrimeDrainPrimePtr.Value -= gds;
        }
    }
}
