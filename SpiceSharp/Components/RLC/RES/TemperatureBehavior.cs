using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// The minimum resistance for any resistor.
        /// </summary>
        protected const double MinimumResistance = 1e-12;

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the default conductance for this model
        /// </summary>
        [ParameterName("g"), ParameterInfo("The conductance of the resistor.")]
        public double Conductance { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="context">The setup data provider</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            context.TryGetParameterSet("model", out ModelBaseParameters mbp);
            ModelParameters = mbp;

            State = ((BaseSimulation)simulation).RealState;
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            BaseParameters = null;
            ModelParameters = null;
            State = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            double factor;
            double resistance = BaseParameters.Resistance;

            // Default Value Processing for Resistor Instance
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = State.Temperature;
            if (!BaseParameters.Width.Given)
                BaseParameters.Width.RawValue = ModelParameters?.DefaultWidth ?? 0.0;

            if (ModelParameters != null)
            {
                if (!BaseParameters.Resistance.Given)
                {
                    if (ModelParameters.SheetResistance.Given && ModelParameters.SheetResistance > 0 && BaseParameters.Length > 0)
                        resistance = ModelParameters.SheetResistance * (BaseParameters.Length - ModelParameters.Narrow) / (BaseParameters.Width - ModelParameters.Narrow);
                    else
                    {
                        CircuitWarning.Warning(this, "{0}: resistance=0, set to 1000".FormatString(Name));
                        resistance = 1000;
                    }
                }

                var difference = BaseParameters.Temperature - ModelParameters.NominalTemperature;

                if (ModelParameters.ExponentialCoefficient.Given)
                    factor = Math.Pow(1.01, ModelParameters.ExponentialCoefficient * difference);
                else
                    factor = 1.0 + ModelParameters.TemperatureCoefficient1 * difference + ModelParameters.TemperatureCoefficient2 * difference * difference;
            }
            else
            {
                factor = 1.0;
            }

            if (resistance < MinimumResistance)
                resistance = MinimumResistance;

            Conductance = 1.0 / (resistance * factor);
        }
    }
}
