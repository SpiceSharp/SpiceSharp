using SpiceSharp.Components;
using SpiceSharp.Simulations.Base;
using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex currents.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class ComplexCurrentExport : Export<IFrequencySimulation, Complex>
    {
        /// <summary>
        /// Gets the name of the voltage source.
        /// </summary>
        public Reference Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is empty.</exception>
        public ComplexCurrentExport(IFrequencySimulation simulation, Reference source)
            : base(simulation)
        {
            if (source.Length == 0)
                throw new ArgumentException(Properties.Resources.References_IsEmptyReference, nameof(source));
            Source = source;
        }

        /// <inheritdoc />
        protected override void Initialize(object sender, EventArgs e)
        {
            var branch = Source.GetContainer(Simulation)
                .GetValue<IBranchedBehavior<Complex>>()
                .Branch;
            Extractor = () => branch.Value;
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>Returns the string representation.</returns>
        public override string ToString()
            => "I({0})".FormatString(Source);
    }
}
