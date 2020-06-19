using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for exporting data for a simulation.
    /// </summary>
    /// <typeparam name="S">The simulation for which the export works.</typeparam>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Export<S, T> : IExport<T> where S : IEventfulSimulation
    {
        /// <summary>
        /// Returns true if the exporter is currently valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (Extractor == null)
                    LazyLoad();
                return Extractor != null;
            }
        }

        /// <summary>
        /// Gets or sets the extractor function.
        /// </summary>
        protected Func<T> Extractor { get; set; }

        /// <summary>
        /// Gets the simulation from which the data needs to be extracted.
        /// </summary>
        public S Simulation
        {
            get => _simulation;
            set
            {
                if (_simulation != null)
                {
                    _simulation.AfterSetup -= Initialize;
                    _simulation.BeforeUnsetup -= Initialize;
                    Extractor = null;
                }
                _simulation = value;
                if (_simulation != null)
                {
                    _simulation.AfterSetup += Initialize;
                    _simulation.BeforeUnsetup += Finalize;
                }
            }
        }
        private S _simulation;

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
                if (Extractor == null)
                {
                    // Try initializing (lazy loading)
                    LazyLoad();
                    if (Extractor == null)
                        return default;
                }

                return Extractor();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Export{S, T}"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        protected Export(S simulation)
        {
            Simulation = simulation;
        }

        /// <summary>
        /// Destroys the export.
        /// </summary>
        public virtual void Destroy()
        {
            Simulation = default;
        }

        /// <summary>
        /// Load the export extractor if the simulation has already started.
        /// </summary>
        protected void LazyLoad()
        {
            if (EqualityComparer<S>.Default.Equals(_simulation, default))
                return;

            // If we're already too far, emulate a call from the simulation
            if (Simulation.Status == SimulationStatus.Setup || Simulation.Status == SimulationStatus.Running)
                Initialize(Simulation, EventArgs.Empty);
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected abstract void Initialize(object sender, EventArgs e);

        /// <summary>
        /// Finalizes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void Finalize(object sender, EventArgs e)
        {
            Extractor = null;
        }
    }
}
