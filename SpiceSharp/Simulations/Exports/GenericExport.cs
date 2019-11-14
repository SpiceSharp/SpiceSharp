using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class allows any function to be specified.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="Export{S, T}" />
    public class GenericExport<T> : Export<IEventfulSimulation, T>
    {
        /// <summary>
        /// Private extractor
        /// </summary>
        private readonly Func<T> _myExtractor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericExport{T}"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="extractor">The function for extracting information.</param>
        public GenericExport(IEventfulSimulation simulation, Func<T> extractor)
            : base(simulation)
        {
            _myExtractor = extractor.ThrowIfNull(nameof(extractor));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            Extractor = _myExtractor;
        }
    }
}
