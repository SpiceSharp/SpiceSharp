using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages a collection circuit objects.
    /// </summary>
    public class EntityCollection : IEnumerable<Entity>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        Dictionary<Identifier, Entity> objects = new Dictionary<Identifier, Entity>();
        List<Entity> ordered = new List<Entity>();

        /// <summary>
        /// Gets whether or not the list is already ordered
        /// </summary>
        bool isOrdered;

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityCollection()
        {
            isOrdered = false;
        }

        /// <summary>
        /// Search for an object by path
        /// </summary>
        /// <param id="path">The path of the object</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public Entity this[Identifier id] => objects[id];
        
        /// <summary>
        /// The amount of circuit objects
        /// </summary>
        public int Count => objects.Count;

        /// <summary>
        /// Clear all circuit objects
        /// </summary>
        public void Clear()
        {
            objects.Clear();
            ordered.Clear();
            isOrdered = false;
        }

        /// <summary>
        /// Add one or more circuit objects
        /// </summary>
        /// <param id="cs">The objects that need to be added</param>
        public void Add(params Entity[] cs)
        {
            if (cs == null)
                return;
            foreach (var c in cs)
            {
                if (c == null)
                    throw new CircuitException("No entity specified");
                if (objects.ContainsKey(c.Name))
                    throw new CircuitException("A component with the id {0} already exists".FormatString(c.Name));
                objects.Add(c.Name, c);
                isOrdered = false;
            }
        }

        /// <summary>
        /// Remove specific circuit objects from the collection
        /// </summary>
        /// <param id="names">Names of the objects that need to be deleted</param>
        public void Remove(params Identifier[] ids)
        {
            if (ids == null)
                return;
            foreach (var id in ids)
            {
                if (id == null)
                    throw new CircuitException("No identifier specified");
                objects.Remove(id);

                // Note: Removing objects does not interfere with the order!
            }
        }

        /// <summary>
        /// Check if a component exists
        /// Multiple names can be specified, in which case the first names will refer to subcircuits
        /// </summary>
        /// <param id="id">A list of names. If there are multiple names, the first names will refer to a subcircuit</param>
        /// <returns></returns>
        public bool Contains(Identifier id) => objects.ContainsKey(id);

        /// <summary>
        /// Gets a circuit object
        /// </summary>
        /// <param id="id">Identifier</param>
        /// <param id="obj"></param>
        /// <returns></returns>
        public bool TryGetEntity(Identifier id, out Entity obj) => objects.TryGetValue(id, out obj);

        /// <summary>
        /// Gets all objects of a specific type
        /// </summary>
        /// <param id="type">The type of objects you wish to find</param>
        /// <returns></returns>
        public Entity[] ByType(Type type)
        {
            List<Entity> result = new List<Entity>();
            foreach (var c in objects.Values)
            {
                if (c.GetType() == type)
                    result.Add(c);
            }
            return result.ToArray();
        }

        /// <summary>
        /// This method is called when building an ordered list of circuit objects
        /// Circuit objects will be called by descending priority
        /// </summary>
        public void BuildOrderedComponentList()
        {
            if (isOrdered)
                return;

            // Initialize
            ordered.Clear();
            HashSet<Entity> added = new HashSet<Entity>();

            // Build our list
            foreach (var c in objects.Values)
            {
                // Add the object to the ordered list
                ordered.Add(c);
                added.Add(c);

                // Automatically add models to the ordered list
                if (c is Component component)
                {
                    var model = component.Model;
                    if (model != null && !added.Contains(model))
                    {
                        added.Add(model);
                        ordered.Add(model);
                    }
                }
            }

            // Sort the list based on priority
            ordered.Sort((Entity a, Entity b) => {
                return b.Priority.CompareTo(a.Priority);
            });
            isOrdered = true;
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Entity> GetEnumerator() => ordered.GetEnumerator();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ordered.GetEnumerator();
    }
}
