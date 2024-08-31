using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for exporting data for a simulation.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Export<T> : IExport<T>
    {
        private int _currentRun;
        private Func<T> _extractor;
        private ISimulation _simulation;

        /// <inheritdoc />
        public ISimulation Simulation
        {
            get => _simulation;
            set
            {
                _simulation = value;
                _extractor = null;
                _currentRun = -1;
            }
        }

        /// <summary>
        /// Gets the current value from the simulation.
        /// </summary>
        /// <remarks>
        /// This property will return a default if there is nothing to extract.
        /// </remarks>
        public T Value
        {
            get
            {
                if (_simulation.CurrentRun < 0)
                {
                    _extractor = null;
                    return default(T); // The simulation hasn't started
                }

                if (_simulation.CurrentRun != _currentRun)
                {
                    _extractor = BuildExtractor(Simulation);
                    _currentRun = _simulation.CurrentRun;
                }

                // The simulation has started, and we have already 
                if (_extractor is null)
                    return default(T);
                return _extractor();
            }
        }

        /// <summary>
        /// Creates a new <see cref="Export{T}"/>.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        protected Export(ISimulation simulation)
        {
            _simulation = simulation;
            _extractor = null;
            _currentRun = -1;
        }

        /// <summary>
        /// Builds the extractor for the given simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns>The extractor.</returns>
        protected abstract Func<T> BuildExtractor(ISimulation simulation);
    }
}
