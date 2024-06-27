using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Common logic for dynamic (time-dependent) parameters of a <see cref="Diode" />.
    /// </summary>
    /// <seealso cref="Biasing" />
    public abstract class Dynamic : Biasing
    {
        /// <summary>
        /// Diode capacitance
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance => LocalCapacitance * Parameters.ParallelMultiplier / Parameters.SeriesMultiplier;

        /// <summary>
        /// The junction capacitance of a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalCapacitance;

        /// <summary>
        /// Gets or sets the capacitor charge.
        /// </summary>
        /// <value>
        /// The capacitor charge.
        /// </value>
        [ParameterName("charge"), ParameterInfo("Diode capacitor charge")]
        public double CapCharge => LocalCapCharge * Parameters.ParallelMultiplier;

        /// <summary>
        /// The charge on the junction capacitance of a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalCapCharge;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dynamic"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        protected Dynamic(IComponentBindingContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Calculates the capacitance based on the current voltage.
        /// </summary>
        /// <param name="vd">The diode voltage.</param>
        protected void CalculateCapacitance(double vd)
        {
            // charge storage elements
            double czero = TempJunctionCap * Parameters.Area;
            if (vd < TempDepletionCap)
            {
                double arg = 1 - vd / ModelParameters.JunctionPotential;
                double sarg = Math.Exp(-ModelParameters.GradingCoefficient * Math.Log(arg));
                LocalCapCharge = ModelParameters.TransitTime * LocalCurrent + ModelParameters.JunctionPotential * czero *
                            (1 - arg * sarg) / (1 - ModelParameters.GradingCoefficient);
                LocalCapacitance = ModelParameters.TransitTime * LocalConductance + czero * sarg;
            }
            else
            {
                double czof2 = czero / ModelTemperature.F2;
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
