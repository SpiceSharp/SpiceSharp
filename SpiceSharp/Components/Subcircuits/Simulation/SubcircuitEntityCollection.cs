using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using System.Collections;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An entity collection for usage in <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <remarks>
    /// If a local entity asks for an entity that does not exist in the subcircuit,
    /// this collection can fall back to the parent simulation and (sub-)circuit. This
    /// means for example that a component can still reference a component outside the
    /// subcircuit! Basically, scope rules.
    /// </remarks>
    /// <seealso cref="SpiceSharp.Circuits.EntityCollection" />
    public class SubcircuitEntityCollection : IEntityCollection
    {
        private ISimulation _parentSimulation;
        private IEntityCollection _parentEntities, _currentEntities;

        /// <summary>
        /// Gets the comparer used to compare <see cref="Entity" /> identifiers.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _currentEntities.Comparer;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        public int Count => _currentEntities.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => _currentEntities.IsReadOnly;

        /// <summary>
        /// Gets the <see cref="Entity"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="Entity"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Entity this[string name]
        {
            get
            {
                if (_currentEntities.TryGetEntity(name, out var result))
                    return result;
                return new SubcircuitEntityProxy(_parentEntities[name], _parentEntities, _parentSimulation);
            }
        }

        /// <summary>
        /// An entity that is just acting as a proxy for the original entity.
        /// </summary>
        /// <seealso cref="SpiceSharp.Circuits.Entity" />
        protected class SubcircuitEntityProxy : Entity
        {
            private Entity _parent;
            private IEntityCollection _parentEntities;
            private ISimulation _parentSimulation;

            /// <summary>
            /// Initializes a new instance of the <see cref="SubcircuitEntityProxy"/> class.
            /// </summary>
            /// <param name="parent">The parent entity.</param>
            /// <param name="parentEntities">The parent entities.</param>
            /// <param name="parentSimulation">The parent simulation.</param>
            public SubcircuitEntityProxy(Entity parent, IEntityCollection parentEntities, ISimulation parentSimulation)
                : base(parent?.Name)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _parentEntities = parentEntities.ThrowIfNull(nameof(parentEntities));
                _parentSimulation = parentSimulation.ThrowIfNull(nameof(parentSimulation));
            }

            /// <summary>
            /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
            /// </summary>
            /// <param name="simulation">The simulation requesting the behaviors.</param>
            /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
            /// <remarks>
            /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
            /// the most specific child class to be used that is necessary. For example, the <see cref="OP" /> simulation needs
            /// <see cref="ITemperatureBehavior" /> and an <see cref="IBiasingBehavior" />. The entity will first look for behaviors
            /// of type <see cref="IBiasingBehavior" />, and then for the behaviors of type <see cref="ITemperatureBehavior" />. However,
            /// if the behavior that was created for <see cref="IBiasingBehavior" /> also implements <see cref="ITemperatureBehavior" />,
            /// then then entity will not create a new instance of the behavior.
            /// </remarks>
            public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
            {
                // We want the parent entity to be created using its own simulation and entity collection
                _parent.CreateBehaviors(_parentSimulation, _parentEntities);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitEntityCollection"/> class.
        /// </summary>
        /// <param name="parentEntities">The parent entities.</param>
        /// <param name="currentEntities">The current entities.</param>
        /// <param name="parentSimulation">The parent simulation.</param>
        public SubcircuitEntityCollection(IEntityCollection parentEntities, IEntityCollection currentEntities, ISimulation parentSimulation)
        {
            _parentSimulation = parentSimulation.ThrowIfNull(nameof(parentSimulation));
            _currentEntities = currentEntities.ThrowIfNull(nameof(currentEntities));
            _parentEntities = parentEntities.ThrowIfNull(nameof(parentEntities));
        }

        /// <summary>
        /// Removes the <see cref="Entity" /> with specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool Remove(string name) => _currentEntities.Remove(name);

        /// <summary>
        /// Determines whether this instance contains an <see cref="Entity"/> with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string name)
        {
            if (_currentEntities.Contains(name))
                return true;
            return _parentEntities.Contains(name);
        }

        /// <summary>
        /// Tries to find an <see cref="Entity" /> in the collection.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>True</c> if the entity is found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetEntity(string name, out Entity entity)
        {
            if (_currentEntities.TryGetEntity(name, out entity))
                return true;
            if (_parentEntities.TryGetEntity(name, out entity))
            {
                entity = new SubcircuitEntityProxy(entity, _parentEntities, _parentSimulation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enumerates each <see cref="Entity" /> of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<Entity> ByType(Type type) => _currentEntities.ByType(type);

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(Entity item) => _currentEntities.Add(item);

        /// <summary>
        /// Removes all items from the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        public void Clear() => _currentEntities.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="System.Collections.Generic.ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="System.Collections.Generic.ICollection{T}" />; otherwise, false.
        /// </returns>
        public bool Contains(Entity item)
        {
            if (_currentEntities.Contains(item))
                return true;
            return _parentEntities.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="System.Collections.Generic.ICollection{T}" /> to an <see cref="System.Array" />, starting at a particular <see cref="System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="System.Collections.Generic.ICollection{T}" />. The <see cref="System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(Entity[] array, int arrayIndex) => _currentEntities.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="System.Collections.Generic.ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="System.Collections.Generic.ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </returns>
        public bool Remove(Entity item) => _currentEntities.Remove(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Entity> GetEnumerator() => _currentEntities.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
