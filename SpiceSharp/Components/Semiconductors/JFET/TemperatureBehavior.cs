using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFET" />.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected ModelTemperatureBehavior ModelTemperature { get; private set; }

        /// <summary>
        /// Gets the temperature-modified saturation current.
        /// </summary>
        public double TempSaturationCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate-source capacitance.
        /// </summary>
        public double TempCapGs { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate-drain capacitance.
        /// </summary>
        public double TempCapGd { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate potential.
        /// </summary>
        public double TempGatePotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance correction.
        /// </summary>
        public double CorDepCap { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 1.
        /// </summary>
        public double F1 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        public double Vcrit { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperatureBehavior>();
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            ModelParameters = context.ModelBehaviors.Parameters.GetValue<ModelBaseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            BiasingState.ThrowIfNotBound(this);
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = BiasingState.Temperature;
            var vt = BaseParameters.Temperature * Constants.KOverQ;
            var fact2 = BaseParameters.Temperature / Constants.ReferenceTemperature;
            var ratio1 = BaseParameters.Temperature / ModelParameters.NominalTemperature - 1;
            TempSaturationCurrent = ModelParameters.GateSaturationCurrent * Math.Exp(ratio1 * 1.11 / vt);
            TempCapGs = ModelParameters.CapGs * ModelTemperature.Cjfact;
            TempCapGd = ModelParameters.CapGd * ModelTemperature.Cjfact;
            var kt = Constants.Boltzmann * BaseParameters.Temperature;
            var egfet = 1.16 - (7.02e-4 * BaseParameters.Temperature * BaseParameters.Temperature) / (BaseParameters.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * 2 * Constants.ReferenceTemperature);
            var pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);
            TempGatePotential = fact2 * ModelTemperature.Pbo + pbfact;
            var gmanew = (TempGatePotential - ModelTemperature.Pbo) / ModelTemperature.Pbo;
            var cjfact1 = 1 + .5 * (4e-4 * (BaseParameters.Temperature - Constants.ReferenceTemperature) - gmanew);
            TempCapGs *= cjfact1;
            TempCapGd *= cjfact1;

            CorDepCap = ModelParameters.DepletionCapCoefficient * TempGatePotential;
            F1 = TempGatePotential * (1 - Math.Exp((1 - .5) * ModelTemperature.Xfc)) / (1 - .5);
            Vcrit = vt * Math.Log(vt / (Constants.Root2 * TempSaturationCurrent));

            if (TempGatePotential.Equals(0.0))
                throw new CircuitException("Invalid parameter " + nameof(TempGatePotential));
        }
    }
}
