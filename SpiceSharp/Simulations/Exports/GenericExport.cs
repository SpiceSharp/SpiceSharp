using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class allows any function to be specified.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="Export{T}" />
    public class GenericExport<T> : Export<T>
    {
        private readonly Func<T> _myExtractor;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericExport{T}"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="extractor">The function for extracting information.</param>
        /// <param name="name">The name of the export.</param>
        public GenericExport(ISimulation simulation, Func<T> extractor, string name = null)
            : base(simulation)
        {
            _myExtractor = extractor.ThrowIfNull(nameof(extractor));
            _name = name ?? "Generic";
        }

        /// <inheritdoc />
        protected override Func<T> BuildExtractor(ISimulation simulation)
            => _myExtractor;

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => _name;
    }
}
