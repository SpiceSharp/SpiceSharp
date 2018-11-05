using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common temperature behavior for MOS.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTemperatureBehavior" />
    public abstract class ModelTemperatureBehavior : BaseTemperatureBehavior
    {
        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        // Necessary behaviors and parameters
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double Factor1 { get; protected set; }
        public double VtNominal { get; protected set; }
        public double EgFet1 { get; protected set; }
        public double PbFactor1 { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected ModelTemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>();
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

            // Perform model defaulting
            if (!_mbp.NominalTemperature.Given)
                _mbp.NominalTemperature.RawValue = simulation.RealState.NominalTemperature;
            Factor1 = _mbp.NominalTemperature / Circuit.ReferenceTemperature;
            VtNominal = _mbp.NominalTemperature * Circuit.KOverQ;
            var kt1 = Circuit.Boltzmann * _mbp.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * _mbp.NominalTemperature * _mbp.NominalTemperature / (_mbp.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Circuit.Charge * arg1);
        }
    }
}
