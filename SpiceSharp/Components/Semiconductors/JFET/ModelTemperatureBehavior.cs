using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTemperatureBehavior" />
    public class ModelTemperatureBehavior : BaseTemperatureBehavior
    {
        // Necessary behaviors and parameters
        private ModelBaseParameters _mbp;

        public double F2 { get; private set; }
        public double F3 { get; private set; }
        public double BFactor { get; private set; }
        public double Pbo { get; private set; }
        public double Xfc { get; private set; }
        public double Cjfact { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public ModelTemperatureBehavior(string name) : base(name)
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
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>();
        }
        
        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Temperature(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            if (_mbp.NominalTemperature.Given)
                _mbp.NominalTemperature.RawValue = simulation.RealState.NominalTemperature;

            var vtnom = Constants.KOverQ * _mbp.NominalTemperature;
            var fact1 = _mbp.NominalTemperature / Constants.ReferenceTemperature;
            var kt1 = Constants.Boltzmann * _mbp.NominalTemperature;
            var egfet1 = 1.16 - (7.02e-4 * _mbp.NominalTemperature * _mbp.NominalTemperature) /
                         (_mbp.NominalTemperature + 1108);
            var arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * 2 * Constants.ReferenceTemperature);
            var pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Constants.Charge * arg1);
            Pbo = (_mbp.GatePotential - pbfact1) / fact1;
            var gmaold = (_mbp.GatePotential - Pbo) / Pbo;
            Cjfact = 1 / (1 + .5 * (4e-4 * (_mbp.NominalTemperature - Constants.ReferenceTemperature) - gmaold));

            if (_mbp.DepletionCapCoefficient > 0.95)
            {
                CircuitWarning.Warning(this,
                    "{0}: Depletion capacitance coefficient too large, limited to 0.95".FormatString(Name));
                _mbp.DepletionCapCoefficient.Value = .95;
            }

            Xfc = Math.Log(1 - _mbp.DepletionCapCoefficient);
            F2 = Math.Exp((1 + 0.5) * Xfc);
            F3 = 1 - _mbp.DepletionCapCoefficient * (1 + 0.5);
            /* Modification for Sydney University JFET model */
            BFactor = (1 - _mbp.B) / (_mbp.GatePotential - _mbp.Threshold);
        }
    }
}
