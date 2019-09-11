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
        /// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="System.Collections.Generic.EqualityComparer{T}" />.</param>
        public Circuit(IEqualityComparer<string> comparer)
            : base(comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(IEnumerable<IEntity> entities)
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
        public Circuit(params IEntity[] entities)
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
    }
}
