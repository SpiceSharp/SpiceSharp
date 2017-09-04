using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Designer
{
    /// <summary>
    /// Executes a design step using the bisection method
    /// </summary>
    public class Bisection : DesignStep
    {
        /// <summary>
        /// Execute the bisection method
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            // Initialize
            double xmin = Math.Min(Minimum, Maximum);
            double xmax = Math.Max(Minimum, Maximum);

            // Find out y-values
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
                // Get the middle point
                double x = 0.5 * (xmin + xmax);
                Apply(this, ckt, x);
                double y = Measurement.Measure(ckt) - Target;
                tol = Math.Abs(x) * RelTol + AbsTol;

                // Narrow down our search region depending on the sign
                if (ymin * y >= 0.0)
                    xmin = x;
                else
                    xmax = x;
                width /= 2.0;
                Result = x;

                // Check iteration count
                Iterations++;
                if (Iterations > MaxIterations)
                    throw new DesignException("Maximum iteration count reached");
            }
        }
    }
}
