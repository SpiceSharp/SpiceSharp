using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp
{
    /// <summary>
    /// Represents an electronic circuit.
    /// </summary>
    public class Circuit : EntityCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        public Circuit()
            : base()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" />.</param>
        public Circuit(IEqualityComparer<string> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(IEnumerable<Entity> entities)
            : base()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(params Entity[] entities)
            : base()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
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

        /// <summary>
        /// Merge a circuit with this one. Entities are merged by reference!
        /// </summary>
        /// <param name="ckt">The circuit to merge with.</param>
        public void Merge(Circuit ckt)
        {
            ckt.ThrowIfNull(nameof(ckt));
            foreach (var entity in ckt)
                Add(entity);
        }

        /// <summary>
        /// Instantiate another circuit as a subcircuit.
        /// </summary>
        /// <param name="data">The instance data.</param>
        public void Instantiate(InstanceData data)
        {
            data.ThrowIfNull(nameof(data));
            foreach (var entity in data.Subcircuit)
            {
                var clone = entity.Clone(data);
                Add(clone);
            }
        }
    }
}
