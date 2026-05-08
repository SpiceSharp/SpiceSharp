using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// LTspice-style idealized diode current law.
    /// </summary>
    /// <remarks>
    /// This intentionally stays local to the diode implementation: it is a runtime compatibility
    /// branch for LTspice/vendor decks, not a replacement for the Berkeley exponential diode.
    /// </remarks>
    internal static class IdealDiode
    {
        private const double MinimumConductance = 0.0;

        /// <summary>
        /// Evaluates the current and small-signal conductance for one ideal diode.
        /// </summary>
        /// <param name="parameters">The model parameters.</param>
        /// <param name="biasingParameters">The simulation biasing parameters.</param>
        /// <param name="voltage">The voltage across one diode.</param>
        /// <param name="area">The diode area multiplier.</param>
        /// <param name="current">The output current.</param>
        /// <param name="conductance">The output conductance.</param>
        public static void Evaluate(
            ModelParameters parameters,
            BiasingParameters biasingParameters,
            double voltage,
            double area,
            out double current,
            out double conductance)
        {
            double onResistance = parameters.IdealOnResistance.Given ? parameters.IdealOnResistance.Value : 1.0;
            double onConductance = 1.0 / onResistance;

            double offConductance = parameters.IdealOffResistance.Given
                ? 1.0 / parameters.IdealOffResistance.Value
                : Math.Max(biasingParameters.Gmin, MinimumConductance);

            double forwardVoltage = parameters.IdealForwardVoltage.Given ? parameters.IdealForwardVoltage.Value : 0.0;

            // Start from the off-state line, then splice in reverse and forward regions.
            current = offConductance * voltage;
            conductance = offConductance;

            if (parameters.IdealReverseVoltage.Given)
            {
                double reverseVoltage = Math.Abs(parameters.IdealReverseVoltage.Value);
                double reverseResistance = parameters.IdealReverseResistance.Given
                    ? parameters.IdealReverseResistance.Value
                    : onResistance;
                double reverseConductance = 1.0 / reverseResistance;
                double reverseIntercept = reverseConductance * reverseVoltage;
                double boundary = FindIntersection(
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    -reverseVoltage);

                EvaluateTransition(
                    voltage,
                    boundary,
                    parameters.IdealReverseEpsilon,
                    reverseConductance,
                    reverseIntercept,
                    offConductance,
                    0.0,
                    out current,
                    out conductance);
            }

            double onIntercept = -onConductance * forwardVoltage;
            double forwardBoundary = FindIntersection(
                offConductance,
                0.0,
                onConductance,
                onIntercept,
                forwardVoltage);

            double forwardWidth = parameters.IdealForwardEpsilon.Given ? parameters.IdealForwardEpsilon.Value : 0.0;
            double forwardStart = forwardBoundary - (Math.Max(forwardWidth, 0.0) / 2.0);
            if (voltage > forwardBoundary || (forwardWidth > 0.0 && voltage >= forwardStart))
            {
                EvaluateTransition(
                    voltage,
                    forwardBoundary,
                    parameters.IdealForwardEpsilon,
                    offConductance,
                    0.0,
                    onConductance,
                    onIntercept,
                    out current,
                    out conductance);
            }

            ApplyCurrentLimits(parameters, ref current, ref conductance);

            current *= area;
            conductance *= area;
        }

        private static double FindIntersection(
            double leftSlope,
            double leftIntercept,
            double rightSlope,
            double rightIntercept,
            double fallback)
        {
            double denominator = leftSlope - rightSlope;
            if (Math.Abs(denominator) <= 1e-30)
                return fallback;
            return (rightIntercept - leftIntercept) / denominator;
        }

        private static void EvaluateTransition(
            double voltage,
            double boundary,
            GivenParameter<double> epsilon,
            double leftSlope,
            double leftIntercept,
            double rightSlope,
            double rightIntercept,
            out double current,
            out double conductance)
        {
            double width = epsilon.Given ? epsilon.Value : 0.0;
            if (width <= 0.0)
            {
                if (voltage < boundary)
                {
                    current = (leftSlope * voltage) + leftIntercept;
                    conductance = leftSlope;
                }
                else
                {
                    current = (rightSlope * voltage) + rightIntercept;
                    conductance = rightSlope;
                }

                return;
            }

            double start = boundary - (width / 2.0);
            double end = boundary + (width / 2.0);
            if (voltage <= start)
            {
                current = (leftSlope * voltage) + leftIntercept;
                conductance = leftSlope;
                return;
            }

            if (voltage >= end)
            {
                current = (rightSlope * voltage) + rightIntercept;
                conductance = rightSlope;
                return;
            }

            // LTspice documents a quadratic join around ideal-diode corners. Centering the
            // voltage window on the line intersection keeps both current and slope continuous.
            double distance = voltage - start;
            double slopeDelta = rightSlope - leftSlope;
            current = (leftSlope * start) + leftIntercept
                + (leftSlope * distance)
                + (slopeDelta * distance * distance / (2.0 * width));
            conductance = leftSlope + (slopeDelta * distance / width);
        }

        private static void ApplyCurrentLimits(ModelParameters parameters, ref double current, ref double conductance)
        {
            if (current > 0.0 && parameters.IdealForwardCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.IdealForwardCurrentLimit.Value, ref current, ref conductance);
            }
            else if (current < 0.0 && parameters.IdealReverseCurrentLimit.Given)
            {
                ApplyCurrentLimit(parameters.IdealReverseCurrentLimit.Value, ref current, ref conductance);
            }
        }

        private static void ApplyCurrentLimit(double limit, ref double current, ref double conductance)
        {
            limit = Math.Abs(limit);
            if (limit <= 0.0)
                return;

            double normalized = current / limit;
            double limited = Math.Tanh(normalized);
            current = limit * limited;
            conductance *= 1.0 - (limited * limited);
        }
    }
}
