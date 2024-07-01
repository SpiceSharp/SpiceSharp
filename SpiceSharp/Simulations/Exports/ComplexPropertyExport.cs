using SpiceSharp.Simulations.Base;
using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex property values.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class ComplexPropertyExport : Export<Complex>
    {
        /// <summary>
        /// Gets the path to the entity.
        /// </summary>
        public Reference Entity { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The path to the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> or <paramref name="propertyName"> is <c>null</c>.</paramref></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="entity"/> is empty.</exception>
        public ComplexPropertyExport(ISimulation simulation, Reference entity, string propertyName)
            : base(simulation)
        {
            if (entity.Length == 0)
                throw new ArgumentException(Properties.Resources.References_IsEmptyReference, nameof(entity));
            Entity = entity;
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <inheritdoc />
        protected override Func<Complex> BuildExtractor(ISimulation simulation)
        {
            if (simulation is not null &&
                Entity.TryGetContainer(simulation, out var container))
                return container.CreatePropertyGetter<Complex>(PropertyName);
            return null;
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>Returns the export represented as a string.</returns>
        public override string ToString()
            => "@{0}[{1}]".FormatString(Entity, PropertyName);
    }
}
