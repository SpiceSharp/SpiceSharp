using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Exported simulation data. Can be used by simulations to pass exported simulation data as an event argument.
    /// This class contains some helper methods for extracting data from the simulation.
    /// </summary>
    public class ExportDataEventArgs : EventArgs
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Simulation _simulation;

        /// <summary>
        /// Gets the time if the simulation supports it
        /// </summary>
        public double Time
        {
            get
            {
                if (_simulation is TimeSimulation timeSimulation && timeSimulation.Method != null)
                    return timeSimulation.Method.Time;
                throw new CircuitException("Simulation {0} does not simulate in time".FormatString(_simulation));
            }
        }

        /// <summary>
        /// Gets the frequency if the simulation supports it
        /// </summary>
        public double Frequency
        {
            get
            {
                if (_simulation is FrequencySimulation frequencySimulation && frequencySimulation.ComplexState != null)
                {
                    if (!frequencySimulation.ComplexState.Laplace.Real.Equals(0.0))
                        throw new CircuitException("Simulation {0} uses whole complex plane".FormatString(_simulation));
                    return frequencySimulation.ComplexState.Laplace.Imaginary / (2.0 * Math.PI);
                }
                throw new CircuitException("Simulation {0} does not simulate in frequency".FormatString(_simulation));
            }
        }

        /// <summary>
        /// Gets the laplace variable if the simulation supports it
        /// </summary>
        public Complex Laplace
        {
            get
            {
                if (_simulation is FrequencySimulation frequencySimulation && frequencySimulation.ComplexState != null)
                    return frequencySimulation.ComplexState.Laplace;
                throw new CircuitException("Simulation {0} does not simulate in the complex plain".FormatString(_simulation));
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">The simulation using the event arguments</param>
        public ExportDataEventArgs(Simulation simulation)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }

        /// <summary>
        /// Gets the voltage at a specific node
        /// For better performance, use the <see cref="RealVoltageExport"/>
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public double GetVoltage(Identifier node) => GetVoltage(node, null);

        /// <summary>
        /// Gets the voltage at a specific node
        /// For better performance, use the <see cref="RealVoltageExport"/>
        /// </summary>
        /// <param name="positive">Positive node</param>
        /// <param name="negative">Negative node</param>
        /// <returns></returns>
        public double GetVoltage(Identifier positive, Identifier negative)
        {
            if (positive == null)
                throw new ArgumentNullException(nameof(positive));
            var state = _simulation.States.Get<RealState>() ??
                        throw new CircuitException("Simulation does not support real voltages");

            // Get the voltage of the positive node
            int index = _simulation.Circuit.Nodes[positive]?.Index ?? 
                        throw new CircuitException("Could not find node {0}".FormatString(positive));
            double voltage = state.Solution[index];

            // Subtract negative node if necessary
            if (negative != null)
            {
                index = _simulation.Circuit.Nodes[negative]?.Index ??
                        throw new CircuitException("Could not find node {0}".FormatString(negative));
                voltage -= state.Solution[index];
            }

            return voltage;
        }

        /// <summary>
        /// Gets the complex voltage at a specific node
        /// For better performance, use the <see cref="ComplexVoltageExport"/>
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <returns></returns>
        public Complex GetComplexVoltage(Identifier node) => GetComplexVoltage(node, null);

        /// <summary>
        /// Gets the complex voltage at a specific node
        /// For better performance, use the <see cref="ComplexVoltageExport"/>
        /// </summary>
        /// <param name="positive">Positive node</param>
        /// <param name="negative">Negative node</param>
        /// <returns></returns>
        public Complex GetComplexVoltage(Identifier positive, Identifier negative)
        {
            if (positive == null)
                throw new ArgumentNullException(nameof(positive));
            var state = _simulation.States.Get<ComplexState>() ??
                        throw new CircuitException("Simulation does not support real voltages");

            // Get the voltage of the positive node
            int index = _simulation.Circuit.Nodes[positive]?.Index ??
                        throw new CircuitException("Could not find node {0}".FormatString(positive));
            Complex voltage = state.Solution[index];

            // Subtract negative node if necessary
            if (negative != null)
            {
                index = _simulation.Circuit.Nodes[negative]?.Index ??
                        throw new CircuitException("Could not find node {0}".FormatString(negative));
                voltage -= state.Solution[index];
            }

            return voltage;
        }

        /// <summary>
        /// Get a property
        /// For better performance, use <see cref="RealPropertyExport"/>
        /// </summary>
        /// <param name="entity">Entity identifier</param>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public double GetProperty(Identifier entity, string property)
        {
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("Invalid property");

            var state = _simulation.States.Get<RealState>() ??
                        throw new CircuitException("Simulation does not have a real state");

            // Find the behaviors that we will need to get a property from
            var eb = _simulation.Behaviors.GetEntityBehaviors(entity);

            // 1) Transient behavior
            Behavior behavior = eb.Get<BaseTransientBehavior>();
            Func<RealState, double> extractor = null;
            if (behavior != null)
                extractor = behavior.CreateExport(property);

            // 2) Load behavior
            if (extractor == null)
            {
                behavior = eb.Get<BaseLoadBehavior>();
                if (behavior != null)
                    extractor = behavior.CreateExport(property);
            }

            // 3) Temperature behavior
            if (extractor == null)
            {
                behavior = eb.Get<BaseTemperatureBehavior>();
                if (behavior != null)
                    extractor = behavior.CreateExport(property);
            }

            if (extractor != null)
                return extractor(state);
            throw new CircuitException("Could not find property {0} in entity {1}".FormatString(property, entity));
        }

        /// <summary>
        /// Get a complex property
        /// For better performance, use <see cref="ComplexPropertyExport"/>
        /// </summary>
        /// <param name="entity">Entity identifier</param>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public Complex GetComplexProperty(Identifier entity, string property)
        {
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("Invalid property");

            var state = _simulation.States.Get<ComplexState>() ?? 
                        throw new CircuitException("Simulation does not have a complex state");

            // Get the behaviors for the entity
            var eb = _simulation.Behaviors.GetEntityBehaviors(entity) ??
                     throw new CircuitException("Could not find entity behaviors for {0}".FormatString(entity));

            // 1) AC analysis
            var behavior = eb.Get<BaseFrequencyBehavior>();
            Func<ComplexState, Complex> extractor = null;
            if (behavior != null)
                extractor = behavior.CreateAcExport(property);

            if (extractor != null)
                return extractor(state);
            throw new CircuitException("Could not find complex property {0} in entity {1}".FormatString(property, entity));
        }
    }
}
