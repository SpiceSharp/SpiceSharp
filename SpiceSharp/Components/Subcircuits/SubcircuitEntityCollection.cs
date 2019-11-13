using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    public class SubcircuitEntityCollection : IEntityCollection
    {
        /// <summary>
        /// An entity that just refers to the parent entity. Creates the entity
        /// for the parent simulation if necessary.
        /// </summary>
        /// <seealso cref="Entity" />
        protected class ProxyEntity : Entity
        {
            private IEntity _parent;
            private IEntityCollection _parentEntities;
            private ISimulation _parentSimulation;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyEntity"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="parentEntities">The parent entities.</param>
            /// <param name="simulation">The simulation.</param>
            public ProxyEntity(IEntity parent, IEntityCollection parentEntities, ISimulation simulation)
                : base(parent.Name)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _parentEntities = parentEntities.ThrowIfNull(nameof(parentEntities));
                _parentSimulation = _parentSimulation.ThrowIfNull(nameof(_parentSimulation));
            }

            /// <summary>
            /// Creates behaviors for the specified simulation that describe this <see cref="Entity" />.
            /// </summary>
            /// <param name="simulation">The simulation requesting the behaviors.</param>
            /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
            public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
            {
                _parent.CreateBehaviors(_parentSimulation, _parentEntities);
            }

            /// <summary>
            /// Create one or more behaviors for the simulation.
            /// </summary>
            /// <param name="simulation">The simulation for which behaviors need to be created.</param>
            /// <param name="entities">The other entities.</param>
            /// <param name="behaviors">A container where all behaviors are to be stored.</param>
            protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
            {                
            }
        }

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
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => _currentEntities.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => _currentEntities.IsReadOnly;

        /// <summary>
        /// Gets the <see cref="IEntity"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IEntity"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEntity this[string name]
        {
            get
            {
                if (_currentEntities.TryGetEntity(name, out var result))
                    return result;
                return new ProxyEntity(_parentEntities[name], _parentEntities, _parentSimulation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitEntityCollection"/> class.
        /// </summary>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="current">The current.</param>
        /// <param name="parent">The parent.</param>
        public SubcircuitEntityCollection(ISimulation parentSimulation, IEntityCollection current, IEntityCollection parent)
        {
            _parentSimulation = parentSimulation.ThrowIfNull(nameof(parentSimulation));
            _currentEntities = current.ThrowIfNull(nameof(current));
            _parentEntities = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Removes the <see cref="Entity" /> with specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool Remove(string name) => _currentEntities.Remove(name);

        /// <summary>
        /// Determines whether this instance contains the object.
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
        public bool TryGetEntity(string name, out IEntity entity)
        {
            if (_currentEntities.TryGetEntity(name, out entity))
                return true;
            if (_parentEntities.TryGetEntity(name, out entity))
            {
                entity = new ProxyEntity(entity, _parentEntities, _parentSimulation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enumerates each <see cref="Entity" /> of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<IEntity> ByType(Type type) => _currentEntities.ByType(type);

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        public void Add(IEntity item) => _currentEntities.Add(item);

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _currentEntities.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.
        /// </returns>
        public bool Contains(IEntity item)
        {
            if (_currentEntities.Contains(item))
                return true;
            return _parentEntities.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="Array" />, starting at a particular <see cref="Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(IEntity[] array, int arrayIndex) => _currentEntities.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        public bool Remove(IEntity item) => _currentEntities.Remove(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IEntity> GetEnumerator() => _currentEntities.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
