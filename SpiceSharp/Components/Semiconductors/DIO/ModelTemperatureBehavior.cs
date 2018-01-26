using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        ModelBaseParameters mbp;

        /// <summary>
        /// Conductance
        /// </summary>
        [PropertyName("cond"), PropertyInfo("Ohmic conductance")]
        public double Conductance { get; internal set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double Vtnom { get; protected set; }
        public double Xfc { get; protected set; }
        public double F2 { get; internal set; }
        public double F3 { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            mbp = provider.GetParameterSet<ModelBaseParameters>(0);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            if (!mbp.NominalTemperature.Given)
            {
                mbp.NominalTemperature.Value = simulation.State.NominalTemperature;
            }
            Vtnom = Circuit.KOverQ * mbp.NominalTemperature;

            // limit grading coeff to max of .9
            if (mbp.GradingCoefficient > .9)
            {
                mbp.GradingCoefficient.Value = 0.9;
                CircuitWarning.Warning(this, $"{Name}: grading coefficient too large, limited to 0.9");
            }

            // limit activation energy to min of .1
            if (mbp.ActivationEnergy < .1)
            {
                mbp.ActivationEnergy.Value = 0.1;
                CircuitWarning.Warning(this, $"{Name}: activation energy too small, limited to 0.1");
            }

            // limit depletion cap coeff to max of .95
            if (mbp.DepletionCapCoefficient > .95)
            {
                mbp.DepletionCapCoefficient.Value = 0.95;
                CircuitWarning.Warning(this, $"{Name}: coefficient Fc too large, limited to 0.95");
            }

            if (!mbp.Resistance.Given || mbp.Resistance.Value == 0)
                Conductance = 0;
            else
                Conductance = 1 / mbp.Resistance;
            Xfc = Math.Log(1 - mbp.DepletionCapCoefficient);

            F2 = Math.Exp((1 + mbp.GradingCoefficient) * Xfc);
            F3 = 1 - mbp.DepletionCapCoefficient * (1 + mbp.GradingCoefficient);
        }
    }
}
