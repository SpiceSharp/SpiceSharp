using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class OP : BiasingSimulation
    {
        /// <summary>
        /// The constant returned when exporting the operating point.
        /// </summary>
        public const int ExportOperatingPoint = 0x01;

        /// <summary>
        /// Initializes a new instance of the <see cref="OP"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public OP(string name)
            : base(name)
        {
        }

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask = Exports)
        {
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            Op(BiasingParameters.DcMaxIterations);

            if ((mask & ExportOperatingPoint) != 0)
                yield return ExportOperatingPoint;
        }
    }
}
