using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Common logic for dynamic (time-dependent) parameters of a <see cref="Diode" />.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    public abstract class DynamicParameterBehavior : BiasingBehavior
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelBaseParameters ModelParameters { get; }

        /// <summary>
        /// Diode capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance => LocalCapacitance * BaseParameters.ParallelMultiplier / BaseParameters.SeriesMultiplier;

        /// <summary>
        /// The junction capacitance of a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalCapacitance;

        /// <summary>
        /// Gets or sets the capacitor charge.
        /// </summary>
        [ParameterName("charge"), ParameterInfo("Diode capacitor charge")]
        public double CapCharge => LocalCapCharge * BaseParameters.ParallelMultiplier;

        /// <summary>
        /// The charge on the junction capacitance of a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalCapCharge;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicParameterBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        protected DynamicParameterBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Calculates the capacitance based on the current voltage.
        /// </summary>
        /// <param name="vd">The vd.</param>
        protected void CalculateCapacitance(double vd)
        {
            // charge storage elements
            var czero = TempJunctionCap * BaseParameters.Area;
            if (vd < TempDepletionCap)
            {
                var arg = 1 - vd / ModelParameters.JunctionPotential;
                var sarg = Math.Exp(-ModelParameters.GradingCoefficient * Math.Log(arg));
                LocalCapCharge = ModelParameters.TransitTime * LocalCurrent + ModelParameters.JunctionPotential * czero *
                            (1 - arg * sarg) / (1 - ModelParameters.GradingCoefficient);
                LocalCapacitance = ModelParameters.TransitTime * LocalConductance + czero * sarg;
            }
            else
            {
                var czof2 = czero / ModelTemperature.F2;
                LocalCapCharge = ModelParameters.TransitTime * LocalCurrent + czero * TempFactor1 + czof2 *
                            (ModelTemperature.F3 * (vd - TempDepletionCap) + ModelParameters.GradingCoefficient /
                             (ModelParameters.JunctionPotential + ModelParameters.JunctionPotential) *
                             (vd * vd - TempDepletionCap * TempDepletionCap));
                LocalCapacitance = ModelParameters.TransitTime * LocalConductance + czof2 *
                              (ModelTemperature.F3 + ModelParameters.GradingCoefficient * vd /
                               ModelParameters.JunctionPotential);
            }
        }
    }
}
