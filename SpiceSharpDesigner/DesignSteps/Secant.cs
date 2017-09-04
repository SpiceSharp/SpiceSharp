using System;

namespace SpiceSharp.Designer
{
    public class Secant : DesignStep
    {
        /// <summary>
        /// Execute using the secant method
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            // Initialize
            double xmin = Math.Min(Minimum, Maximum);
            double xmax = Math.Max(Minimum, Maximum);

            // Find out xmin and xmax
            Apply(this, ckt, xmin);
            double ymin = Measurement.Measure(ckt) - Target;
            Apply(this, ckt, xmax);
            double ymax = Measurement.Measure(ckt) - Target;
            double width = xmax - xmin;
            double tol = Math.Abs(xmax) * RelTol + AbsTol;

            // Make sure the signs are opposite
            if (ymin == 0.0)
            {
                Result = xmin;
                return;
            }
            if (ymax == 0.0)
            {
                Result = xmax;
                return;
            }
            if (ymin * ymax > 0.0)
                throw new DesignException("Cannot find zero");

            // Perform a bisection step until the width has reached half the tolerance
            while (width > tol)
            {
                // Get the intersection with y=0
                double m = (ymax - ymin) / (xmax - xmin);
                double x = xmin - ymin / m;
                Apply(this, ckt, x);
                double y = Measurement.Measure(ckt) - Target;
                tol = Math.Abs(x) * RelTol + AbsTol;

                // Narrow down our search region depending on the sign
                if (ymin * y >= 0.0)
                    xmin = x;
                else
                    xmax = x;
                width = xmax - xmin;
                Result = x;

                // Check iteration count
                Iterations++;
                if (Iterations > MaxIterations)
                    throw new DesignException("Maximum iteration count reached");
            }
        }
    }
}
