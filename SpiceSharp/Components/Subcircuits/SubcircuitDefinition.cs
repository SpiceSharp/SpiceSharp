using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="ISubcircuitDefinition" />
    /// <seealso cref="IParameterized{P}"/>
    public class SubcircuitDefinition : ISubcircuitDefinition
    {
        private readonly string[] _pins;

        /// <inheritdoc/>
        [ParameterName("entities"), ParameterInfo("The entities in the subcircuit.")]
        public IEntityCollection Entities { get; }

        /// <inheritdoc/>
        [ParameterName("pins"), ParameterInfo("The pins that connect this subcircuit definition to the outside.")]
        public IReadOnlyList<string> Pins => new ReadOnlyCollection<string>(_pins);

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinition"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        public SubcircuitDefinition(IEntityCollection entities, params string[] pins)
        {
            Entities = entities.ThrowIfNull(nameof(entities));
            if (pins != null)
            {
                _pins = new string[pins.Length];
                for (int i = 0; i < pins.Length; i++)
                    _pins[i] = pins[i].ThrowIfNull("node {0}".FormatString(i + 1));
            }
            else
                _pins = Array<string>.Empty();
        }

        /// <inheritdoc/>
        public ISubcircuitDefinition Clone()
            => new SubcircuitDefinition(Entities.Clone(), [.. Pins]);
    }
}
