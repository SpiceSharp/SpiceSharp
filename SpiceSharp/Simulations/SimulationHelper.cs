using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Helper methods for extracting data from an <see cref="ISimulation"/>.
    /// </summary>
    public static class SimulationHelper
    {
        /// <summary>
        /// Tries to get a real voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The reference to the node.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetVoltage(this ISimulation simulation, Reference name, out double result)
        {
            if (simulation is not null &&
                name.Length > 0 &&
                name.TryGetVariable<double, IBiasingSimulationState>(simulation, out var variable))
            {
                result = variable.Value;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a real voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The reference to the node.</param>
        /// <returns>Returns the value of the voltage.</returns>
        /// <exception cref="ArgumentException">Thrown if the voltage could not be found.</exception>
        public static double GetVoltage(this ISimulation simulation, Reference name)
        {
            if (!TryGetVoltage(simulation, name, out double result))
                throw new ArgumentException(Properties.Resources.Simulations_VoltageNotFound.FormatString(name), nameof(name));
            return result;
        }

        /// <summary>
        /// Tries to get a real voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="positive">The positive node reference.</param>
        /// <param name="negative">The negative node reference.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetVoltage(this ISimulation simulation, Reference positive, Reference negative, out double result)
        {
            if (simulation is null)
            {
                result = default;
                return false;
            }
            if (positive.Length > 0 && negative.Length > 0)
            {
                if (positive.TryGetVariable<double, IBiasingSimulationState>(simulation, out var posVariable) &&
                    negative.TryGetVariable<double, IBiasingSimulationState>(simulation, out var negVariable))
                {
                    result = posVariable.Value - negVariable.Value;
                    return true;
                }
            }
            else if (positive.Length > 0)
            {
                if (positive.TryGetVariable<double, IBiasingSimulationState>(simulation, out var posVariable))
                {
                    result = posVariable.Value;
                    return true;
                }
            }
            else if (negative.Length > 0)
            {
                if (negative.TryGetVariable<double, IBiasingSimulationState>(simulation, out var negVariable))
                {
                    result = -negVariable.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a real voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="positive">The positive node reference.</param>
        /// <param name="negative">The negative node reference.</param>
        /// <returns>Returns the value of the voltage.</returns>
        /// <exception cref="ArgumentException">Thrown if the voltage could not be found.</exception>
        public static double GetVoltage(this ISimulation simulation, Reference positive, Reference negative)
        {
            if (!TryGetVoltage(simulation, positive, negative, out double result))
                throw new ArgumentException(Properties.Resources.Simulations_DifferentialVoltageNotFound.FormatString(positive, negative));
            return result;
        }

        /// <summary>
        /// Tries to get a real current from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity that defines the branch current.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetCurrent(this ISimulation simulation, Reference entity, out double result)
        {
            if (simulation is not null &&
                entity.TryGetContainer(simulation, out var container) &&
                container.TryGetValue<IBranchedBehavior<double>>(out var behavior))
            {
                result = behavior.Branch.Value;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a real current from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity reference.</param>
        /// <returns>Returns the value of the current.</returns>
        /// <exception cref="ArgumentException">Thrown if the branch current could not be found.</exception>
        public static double GetCurrent(this ISimulation simulation, Reference entity)
        {
            if (!TryGetCurrent(simulation, entity, out double result))
                throw new ArgumentException(Properties.Resources.Simulations_CurrentNotFound.FormatString(entity), nameof(entity));
            return result;
        }

        /// <summary>
        /// Tries to get a real property value from an entity behavior in a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns the property value if found; otherwise, <c>NaN</c>.</returns>
        public static bool TryGetProperty(this ISimulation simulation, Reference entity, string propertyName, out double result)
        {
            if (simulation is not null &&
                entity.TryGetContainer(simulation, out var container) &&
                container.TryGetProperty<double>(propertyName, out result))
                return true;
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a real property value from an entity behavior in a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>Returns the value of the property.</returns>
        /// <exception cref="ArgumentException">Thrown if the entity or property could not be found.</exception>
        public static double GetProperty(this ISimulation simulation, Reference entity, string propertyName)
        {
            if (!TryGetProperty(simulation, entity, propertyName, out double result))
                throw new ArgumentException(Properties.Resources.Simulations_PropertyNotfound.FormatString(entity, propertyName ?? string.Empty));
            return result;
        }

        /// <summary>
        /// Tries to get a complex voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The reference to the node.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetComplexVoltage(this ISimulation simulation, Reference name, out Complex result)
        {
            if (simulation is not null &&
                name.Length > 0 &&
                name.TryGetVariable<Complex, IComplexSimulationState>(simulation, out var variable))
            {
                result = variable.Value;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a complex voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="name">The reference to the node.</param>
        /// <returns>Returns the value of the voltage.</returns>
        /// <exception cref="ArgumentException">Thrown if the voltage could not be found.</exception>
        public static Complex GetComplexVoltage(this ISimulation simulation, Reference name)
        {
            if (!TryGetComplexVoltage(simulation, name, out Complex result))
                throw new ArgumentException(Properties.Resources.Simulations_VoltageNotFound.FormatString(name), nameof(name));
            return result;
        }

        /// <summary>
        /// Tries to get a complex voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="positive">The positive node reference.</param>
        /// <param name="negative">The negative node reference.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetComplexVoltage(this ISimulation simulation, Reference positive, Reference negative, out Complex result)
        {
            if (simulation is null)
            {
                result = default;
                return false;
            }
            if (positive.Length > 0 && negative.Length > 0)
            {
                if (positive.TryGetVariable<Complex, IComplexSimulationState>(simulation, out var posVariable) &&
                    negative.TryGetVariable<Complex, IComplexSimulationState>(simulation, out var negVariable))
                {
                    result = posVariable.Value - negVariable.Value;
                    return true;
                }
            }
            else if (positive.Length > 0)
            {
                if (positive.TryGetVariable<Complex, IComplexSimulationState>(simulation, out var posVariable))
                {
                    result = posVariable.Value;
                    return true;
                }
            }
            else if (negative.Length > 0)
            {
                if (negative.TryGetVariable<Complex, IComplexSimulationState>(simulation, out var negVariable))
                {
                    result = -negVariable.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a complex voltage from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="positive">The positive node reference.</param>
        /// <param name="negative">The negative node reference.</param>
        /// <returns>Returns the value of the voltage.</returns>
        /// <exception cref="ArgumentException">Thrown if the voltage could not be found.</exception>
        public static Complex GetComplexVoltage(this ISimulation simulation, Reference positive, Reference negative)
        {
            if (!TryGetComplexVoltage(simulation, positive, negative, out Complex result))
                throw new ArgumentException(Properties.Resources.Simulations_DifferentialVoltageNotFound.FormatString(positive, negative));
            return result;
        }

        /// <summary>
        /// Tries to get a complex current from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity that defines the branch current.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the voltage was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetComplexCurrent(this ISimulation simulation, Reference entity, out Complex result)
        {
            if (simulation is not null &&
                entity.TryGetContainer(simulation, out var container) &&
                container.TryGetValue<IBranchedBehavior<Complex>>(out var behavior))
            {
                result = behavior.Branch.Value;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a complex current from a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity reference.</param>
        /// <returns>Returns the value of the current.</returns>
        /// <exception cref="ArgumentException">Thrown if the branch current could not be found.</exception>
        public static Complex GetComplexCurrent(this ISimulation simulation, Reference entity)
        {
            if (!TryGetComplexCurrent(simulation, entity, out Complex result))
                throw new ArgumentException(Properties.Resources.Simulations_CurrentNotFound.FormatString(entity), nameof(entity));
            return result;
        }

        /// <summary>
        /// Tries to get a real property value from an entity behavior in a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns the property value if found; otherwise, <c>NaN</c>.</returns>
        public static bool TryGetComplexProperty(this ISimulation simulation, Reference entity, string propertyName, out Complex result)
        {
            if (simulation is not null &&
                entity.TryGetContainer(simulation, out var container) &&
                container.TryGetProperty<Complex>(propertyName, out result))
                return true;
            result = default;
            return false;
        }

        /// <summary>
        /// Gets a real property value from an entity behavior in a simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>Returns the value of the property.</returns>
        /// <exception cref="ArgumentException">Thrown if the entity or property could not be found.</exception>
        public static Complex GetComplexProperty(this ISimulation simulation, Reference entity, string propertyName)
        {
            if (!TryGetComplexProperty(simulation, entity, propertyName, out Complex result))
                throw new ArgumentException(Properties.Resources.Simulations_PropertyNotfound.FormatString(entity, propertyName ?? string.Empty));
            return result;
        }

        /// <summary>
        /// Runs a simulation to the end.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="actions">The actions for each type of export.</param>
        public static void RunToEnd(this ISimulation simulation, IEntityCollection entities,
            IDictionary<int, Action> actions = null)
        {
            // Compute the key
            int mask = 0;
            if (actions is not null)
            {
                foreach (var key in actions.Keys)
                    mask |= key;

                foreach (int type in simulation.Run(entities, mask))
                {
                    if (actions is not null &&
                        actions.TryGetValue(type, out var action))
                        action();
                }
            }
            else
            {
                foreach (int _ in simulation.Run(entities, 0))
                { }
            }
        }
    }
}
