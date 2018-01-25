using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A collection of <see cref="ParameterSet"/>
    /// Only one instance of each type is allowed
    /// </summary>
    public class ParameterSetCollection : IEnumerable<ParameterSet>
    {
        /// <summary>
        /// Collection of parameters
        /// </summary>
        Dictionary<Type, ParameterSet> parameters = new Dictionary<Type, ParameterSet>();

        /// <summary>
        /// Set a <see cref="ParameterSet"/> in the collection
        /// If a set of the same type already exist, they are overwritten
        /// </summary>
        /// <param name="p">Parameters</param>
        public void Add(ParameterSet p)
        {
            // Update the parameter of that type
            parameters[p.GetType()] = p;
        }

        /// <summary>
        /// Get parameters
        /// </summary>
        /// <typeparam name="T">Parameters</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : ParameterSet
        {
            if (parameters.TryGetValue(typeof(T), out ParameterSet result))
                return (T)result;
            return null;
        }

        /// <summary>
        /// Remove parameters of a specific type
        /// </summary>
        /// <param name="t">The parameters type</param>
        public void Remove(Type t)
        {
            parameters.Remove(t);
        }

        /// <summary>
        /// Clear all parameters
        /// </summary>
        public void Clear()
        {
            parameters.Clear();
        }

        /// <summary>
        /// Get an enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ParameterSet> GetEnumerator() => parameters.Values.GetEnumerator();

        /// <summary>
        /// Get an enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => parameters.Values.GetEnumerator();

        /// <summary>
        /// Set a parameter by name
        /// If multiple parameters
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetProperty(string property, double value)
        {
            foreach (var ps in parameters.Values)
            {
                if (ps.Set(property, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetProperty(string property, object value)
        {
            foreach (var ps in parameters.Values)
            {
                if (ps.Set(property, value))
                    return true;
            }
            return false;
        }
    }
}
