using System;

namespace SpiceSharp
{
    /// <summary>
    /// A collection of <see cref="ParameterSet"/>
    /// Only one instance of each type is allowed
    /// </summary>
    public class ParameterSetCollection : TypeDictionary<ParameterSet>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterSetCollection()
            : base(typeof(ParameterSet))
        {
        }

        /// <summary>
        /// Add a parameter set
        /// </summary>
        /// <param name="set"></param>
        public void Add(ParameterSet set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            Add(set.GetType(), set);
        }

        /// <summary>
        /// Get a parameter set if it exists, else returns null
        /// </summary>
        /// <typeparam name="T">Parameters</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : ParameterSet
        {
            if (TryGetValue(typeof(T), out ParameterSet result))
                return (T)result;
            return null;
        }
        
        /// <summary>
        /// Set a parameter by name
        /// If multiple parameters
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetProperty(string property, double value)
        {
            foreach (var ps in Values)
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
            foreach (var ps in Values)
            {
                if (ps.Set(property, value))
                    return true;
            }
            return false;
        }
    }
}
