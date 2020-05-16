using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class OP : BiasingSimulation
    {
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
        protected override void Execute()
        {
            base.Execute();
            Op(BiasingParameters.DcMaxIterations);
            var exportargs = new ExportDataEventArgs(this);
            OnExport(exportargs);
        }
    }
}
