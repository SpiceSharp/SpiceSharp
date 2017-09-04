using System;

namespace SpiceSharp.Designer
{
    /// <summary>
    /// Class that uses Newton algorithm to find the root
    /// </summary>
    public class Newton : DesignStep
    {
        /// <summary>
        /// The relative factor for taking the numerical derivative
        /// </summary>
        public double RelDiff = 1e-6;

        /// <summary>
        /// The absolute factor for taking the numerical derivative
        /// </summary>
        public double AbsDiff = 1e-12;

        /// <summary>
        /// Gets or sets the starting value
        /// </summary>
        public double Start = 0.0;

        /// <summary>
        /// Execute method of Newton
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            // Initialize
            double xmin = Math.Min(Minimum, Maximum);
            double xmax = Math.Max(Minimum, Maximum);
            Result = Start;
            if (Result < xmin)
                Result = xmin;
            if (Result > xmax)
                Result = xmax;
            double error = double.PositiveInfinity;
            double tol = 0.0;
            Apply(this, ckt, Result);
            double y = Measurement.Measure(ckt);

            // Start iterations
            while (Math.Abs(error) > tol)
            {
                // Numerical derivative calculation
                double delta = RelDiff * Result + AbsDiff;
                double m = 0.0;
                if (Result - delta < xmin)
                {
                    Apply(this, ckt, Result + delta);
                    m = (Measurement.Measure(ckt) - y) / delta;
                }
                else
                {
                    Apply(this, ckt, Result - delta);
                    m = (y - Measurement.Measure(ckt)) / delta;
                }
                if (m == 0.0)
                    m = Math.Sign(error) * 1e-12;

                // Calculate the new point
                double nx = Result - (y - Target) / m;
                if (nx < xmin)
                    nx = xmin;
                if (nx > xmax)
                    nx = xmax;
                Apply(this, ckt, nx);
                y = Measurement.Measure(ckt);
                error = (nx - Result) / Math.Abs(Math.Max(nx, Result));
                Result = nx;
                tol = Math.Max(Math.Abs(nx), Math.Abs(Target)) * RelTol + AbsTol;

                // Track iteration count
                Iterations++;
                if (Iterations > MaxIterations)
                    throw new DesignException("Maximum iteration count reached");
            }
        }
    }
}
