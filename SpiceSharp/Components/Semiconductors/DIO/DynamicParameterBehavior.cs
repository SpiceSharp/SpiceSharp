using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Common logic for dynamic (time-dependent) parameters of a <see cref="Diode" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.DiodeBehaviors.BiasingBehavior" />
    public abstract class DynamicParameterBehavior : BiasingBehavior
    {
        /// <summary>
        /// Diode capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }

        /// <summary>
        /// Gets or sets the capacitor charge.
        /// </summary>
        [ParameterName("charge"), ParameterInfo("Diode capacitor charge")]
        public virtual double CapCharge { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicParameterBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        protected DynamicParameterBehavior(string name) : base(name)
        {
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
                CapCharge = ModelParameters.TransitTime * Current + ModelParameters.JunctionPotential * czero *
                            (1 - arg * sarg) / (1 - ModelParameters.GradingCoefficient);
                Capacitance = ModelParameters.TransitTime * Conductance + czero * sarg;
            }
            else
            {
                var czof2 = czero / ModelTemperature.F2;
                CapCharge = ModelParameters.TransitTime * Current + czero * TempFactor1 + czof2 *
                            (ModelTemperature.F3 * (vd - TempDepletionCap) + ModelParameters.GradingCoefficient /
                             (ModelParameters.JunctionPotential + ModelParameters.JunctionPotential) *
                             (vd * vd - TempDepletionCap * TempDepletionCap));
                Capacitance = ModelParameters.TransitTime * Conductance + czof2 *
                              (ModelTemperature.F3 + ModelParameters.GradingCoefficient * vd /
                               ModelParameters.JunctionPotential);
            }
        }
    }
}
