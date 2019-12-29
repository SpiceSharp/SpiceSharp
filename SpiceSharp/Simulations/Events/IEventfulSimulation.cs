using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulation"/> that raises events.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public interface IEventfulSimulation : ISimulation
    {
        /// <summary>
        /// Occurs when simulation data can be exported.
        /// </summary>
        event EventHandler<ExportDataEventArgs> ExportSimulationData;

        /// <summary>
        /// Occurs before the simulation is set up.
        /// </summary>
        event EventHandler<EventArgs> BeforeSetup;

        /// <summary>
        /// Occurs after the simulation is set up.
        /// </summary>
        event EventHandler<EventArgs> AfterSetup;

        /// <summary>
        /// Occurs before the simulation starts validating the input.
        /// </summary>
        event EventHandler<EventArgs> BeforeValidation;

        /// <summary>
        /// Occurs after the simulation has validated the input.
        /// </summary>
        event EventHandler<EventArgs> AfterValidation;

        /// <summary>
        /// Occurs before the simulation starts its execution.
        /// </summary>
        event EventHandler<BeforeExecuteEventArgs> BeforeExecute;

        /// <summary>
        /// Occurs after the simulation has executed.
        /// </summary>
        event EventHandler<AfterExecuteEventArgs> AfterExecute;

        /// <summary>
        /// Occurs before the simulation is destroyed.
        /// </summary>
        event EventHandler<EventArgs> BeforeUnsetup;

        /// <summary>
        /// Occurs after the simulation is destroyed.
        /// </summary>
        event EventHandler<EventArgs> AfterUnsetup;
    }
}
