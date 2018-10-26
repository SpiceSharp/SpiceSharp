using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTemperatureBehavior" />
    public class TemperatureBehavior : BaseTemperatureBehavior
    {
        // Necessary behaviors and parameters
        private ModelBaseParameters _mbp;
        private BaseParameters _bp;
        private ModelTemperatureBehavior _modeltemp;

        public double TempSaturationCurrent { get; private set; }
        public double TempCapGs { get; private set; }
        public double TempCapGd { get; private set; }
        public double TempGatePotential { get; private set; }
        public double CorDepCap { get; private set; }
        public double F1 { get; private set; }
        public double Vcrit { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TemperatureBehavior(string name) : base(name)
        {
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
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _mbp = null;
            _modeltemp = null;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public override void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            if (!_bp.Temperature.Given)
                _bp.Temperature.RawValue = simulation.RealState.Temperature;
            var vt = _bp.Temperature * Circuit.KOverQ;
            var fact2 = _bp.Temperature / Circuit.ReferenceTemperature;
            var ratio1 = _bp.Temperature / _mbp.NominalTemperature - 1;
            TempSaturationCurrent = _mbp.GateSaturationCurrent * Math.Exp(ratio1 * 1.11 / vt);
            TempCapGs = _mbp.CapGs * _modeltemp.Cjfact;
            TempCapGd = _mbp.CapGd * _modeltemp.Cjfact;
            var kt = Circuit.Boltzmann * _bp.Temperature;
            var egfet = 1.16 - (7.02e-4 * _bp.Temperature * _bp.Temperature) / (_bp.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * 2 * Circuit.ReferenceTemperature);
            var pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);
            TempGatePotential = fact2 * _modeltemp.Pbo + pbfact;
            var gmanew = (TempGatePotential - _modeltemp.Pbo) / _modeltemp.Pbo;
            var cjfact1 = 1 + .5 * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);
            TempCapGs *= cjfact1;
            TempCapGd *= cjfact1;

            CorDepCap = _mbp.DepletionCapCoefficient * TempGatePotential;
            F1 = TempGatePotential * (1 - Math.Exp((1 - .5) * _modeltemp.Xfc)) / (1 - .5);
            Vcrit = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrent));

            if (TempGatePotential.Equals(0.0))
                throw new CircuitException("Invalid parameter " + nameof(TempGatePotential));
        }
    }
}
