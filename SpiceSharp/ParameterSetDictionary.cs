using System;

namespace SpiceSharp
{
    /// <summary>
    /// A collection of <see cref="ParameterSet"/>
    /// Only one instance of each type is allowed
    /// </summary>
    public class ParameterSetDictionary : TypeDictionary<ParameterSet>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterSetDictionary()
            : base(typeof(ParameterSet))
        {
        }

        /// <summary>
        /// Add a parameter set
        /// </summary>
        /// <param name="set">Parameter set</param>
        public void Add(ParameterSet set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            Add(set.GetType(), set);
        }

        /// <summary>
        /// Get a parameter from the parameter set
        /// Returns null if no matching parameter was found
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Parameter GetParameter(string name)
        {
            foreach (var ps in Values)
            {
                var p = ps.GetParameter(name);
                if (p != null)
                    return p;
            }

            return null;
        }

        /// <summary>
        /// Get a setter for a parameter in the parameter set
        /// Returns null if no matching parameter was found
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Action<double> GetSetter(string name)
        {
            foreach (var ps in Values)
            {
                var s = ps.GetSetter(name);
                if (s != null)
                    return s;
            }

            return null;
        }
        
        /// <summary>
        /// Set a parameter by name
        /// If multiple parameters
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetParameter(string name, double value)
        {
            foreach (var ps in Values)
            {
                if (ps.Set(name, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetParameter(string name, object value)
        {
            foreach (var ps in Values)
            {
                if (ps.Set(name, value))
                    return true;
            }
            return false;
        }
    }
}
