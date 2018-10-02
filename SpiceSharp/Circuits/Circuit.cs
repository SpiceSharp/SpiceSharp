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
        /// Gets a collection of all entities in the circuit.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public EntityCollection Entities { get; }

        /// <summary>
        /// Gets a collection of all circuit objects. Obsolete, use <see cref="Entities" /> instead.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        [Obsolete] public EntityCollection Objects => Entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        public Circuit()
        {
            Entities = new EntityCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for identifiers.</param>
        public Circuit(IEqualityComparer<Identifier> comparer)
        {
            Entities = new EntityCollection(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(IEnumerable<Entity> entities)
        {
            Entities = new EntityCollection();
            if (entities == null)
                return;
            foreach (var entity in entities)
                Entities.Add(entity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(params Entity[] entities)
        {
            Entities = new EntityCollection
            {
                entities
            };
        }

        /// <summary>
        /// Clear all entities in the circuit.
        /// </summary>
        public void Clear()
        {
            // Clear all values
            Entities.Clear();
        }

        /// <summary>
        /// Validates the circuit. Checks for voltage loops, floating nodes, etc.
        /// </summary>
        /// <seealso cref="Validator"/>
        public void Validate()
        {
            var validator = new Validator();
            validator.Validate(this);
        }
    }
}
