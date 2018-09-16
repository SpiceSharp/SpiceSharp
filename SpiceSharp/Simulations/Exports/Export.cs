﻿using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Export method
    /// </summary>
    public abstract class Export<T>
    {
        /// <summary>
        /// Gets if the export is currently valid
        /// </summary>
        public bool IsValid => Simulation.State >= RunState.BeforeExecute || Extractor != null;

        /// <summary>
        /// The extractor used to 
        /// </summary>
        protected Func<T> Extractor { get; set; }

        /// <summary>
        /// Gets the parent simulation
        /// </summary>
        protected Simulation Simulation { get; }

        /// <summary>
        /// Gets the current value of the export
        /// </summary>
        public T Value
        {
            get
            {
                if (Simulation.State >= RunState.BeforeExecute && Extractor == null)
                {
                    Initialize(Simulation, new EventArgs());
                }

                if (Extractor == null)
                    return default(T);
                return Extractor();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Simulation</param>
        protected Export(Simulation simulation)
        {
            Simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            simulation.AfterSetup += Initialize;
            simulation.BeforeUnsetup += Finalize;

        }

        /// <summary>
        /// Initialize the export
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected abstract void Initialize(object sender, EventArgs e);

        /// <summary>
        /// Finalize the export
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Finalize(object sender, EventArgs e)
        {
            Extractor = null;
        }
    }
}
