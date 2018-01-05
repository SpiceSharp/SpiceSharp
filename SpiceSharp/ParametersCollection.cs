using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A collection of <see cref="Parameters"/>
    /// Only one instance of each type is allowed
    /// </summary>
    public class ParametersCollection : IEnumerable<Parameters>
    {
        /// <summary>
        /// Collection of parameters
        /// </summary>
        Dictionary<Type, Parameters> parameters = new Dictionary<Type, Parameters>();

        /// <summary>
        /// Register parameters in the collection
        /// If parameters of the same type already exist, they are overwritten
        /// </summary>
        /// <param name="p">Parameters</param>
        public void Register(Parameters p)
        {
            // Update the parameter of that type
            parameters[p.GetType()] = p;
        }

        /// <summary>
        /// Get parameters
        /// </summary>
        /// <typeparam name="T">Parameters</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : Parameters
        {
            if (parameters.TryGetValue(typeof(T), out Parameters result))
                return (T)result;
            return null;
        }

        /// <summary>
        /// Remove parameters
        /// </summary>
        /// <param name="t">The parameters type</param>
        public void Remove(Type t)
        {
            parameters.Remove(t);
        }

        /// <summary>
        /// Get an enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Parameters> GetEnumerator() => parameters.Values.GetEnumerator();

        /// <summary>
        /// Get an enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => parameters.Values.GetEnumerator();

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Values</param>
        /// <returns></returns>
        public bool Set(string parameter, double value)
        {
            foreach (var ps in parameters.Values)
            {
                if (ps.Set(parameter, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool Set(string parameter, object value)
        {
            foreach (var ps in parameters.Values)
            {
                if (ps.Set(parameter, value))
                    return true;
            }
            return false;
        }
    }
}
