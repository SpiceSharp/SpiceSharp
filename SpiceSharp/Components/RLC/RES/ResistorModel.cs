﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.ResistorBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for semiconductor <see cref="Resistor"/>
    /// </summary>
    public class ResistorModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResistorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
        }
    }
}
