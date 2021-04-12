using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.SamplerBehaviors
{
    /// <summary>
    /// The binding context for a <see cref="Sampler"/>.
    /// </summary>
    /// <seealso cref="BindingContext"/>
    [BindingContextFor(typeof(Sampler))]
    public class SamplerBindingContext : BindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The behavior.</param>
        public SamplerBindingContext(IEntity entity, ISimulation simulation, IBehaviorContainer behaviors)
            : base(entity, simulation, behaviors)
        {
        }

        /// <summary>
        /// Registers a method to the <see cref="IEventfulSimulation.ExportSimulationData"/> event.
        /// </summary>
        /// <param name="event">The method.</param>
        public void RegisterToExportEvent(EventHandler<ExportDataEventArgs> @event)
        {
            if (Simulation is IEventfulSimulation sim)
                sim.ExportSimulationData += @event;
            else
                throw new SpiceSharpException(Properties.Resources.Sampler_NoExportEvent.FormatString(Simulation.Name));
        }
    }
}
