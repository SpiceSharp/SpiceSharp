using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Generic export class
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    public class GenericExport<T> : Export<T>
    {
        /// <summary>
        /// Private extractor
        /// </summary>
        private readonly Func<T> _myExtractor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="extractor">Method extracting the property</param>
        public GenericExport(Simulation simulation, Func<T> extractor)
            : base(simulation)
        {
            _myExtractor = extractor;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected override void Initialize(object sender, InitializeSimulationEventArgs e)
        {
            Extractor = _myExtractor;
        }
    }
}
