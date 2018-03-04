using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp
{
    /// <summary>
    /// Represents an electronic circuit.
    /// </summary>
    public class Circuit
    {
        /// <summary>
        /// Common constants
        /// </summary>
        public const double Charge = 1.6021918e-19;
        public const double CelsiusKelvin = 273.15;
        public const double Boltzmann = 1.3806226e-23;
        public const double ReferenceTemperature = 300.15; // 27degC
        public const double Root2 = 1.4142135623730951;
        public const double Vt0 = Boltzmann * (27.0 + CelsiusKelvin) / Charge;
        public const double KOverQ = Boltzmann / Charge;

        /// <summary>
        /// Gets the nodes in the circuit
        /// </summary>
        public Nodes Nodes { get; } = new Nodes();

        /// <summary>
        /// Gets a collection of all circuit objects
        /// </summary>
        public EntityCollection Objects { get; } = new EntityCollection();

        /// <summary>
        /// Constructor
        /// </summary>
        public Circuit()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entities">Entities</param>
        public Circuit(IEnumerable<Entity> entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
            {
                Objects.Add(entity);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entities">Entities</param>
        public Circuit(params Entity[] entities)
        {
            Objects.Add(entities);
        }

        /// <summary>
        /// Clear all objects, nodes, etc. in the circuit
        /// </summary>
        public void Clear()
        {
            // Clear all values
            Nodes.Clear();
            Objects.Clear();
        }

        /// <summary>
        /// Check the circuit for floating nodes, voltage loops and more
        /// </summary>
        public void Validate()
        {
            Validator validator = new Validator();
            validator.Validate(this);
        }
    }
}
