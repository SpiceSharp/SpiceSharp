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
        public Parameter<T> GetParameter<T>(string name) where T : struct
        {
            foreach (var ps in Values)
            {
                var p = ps.GetParameter<T>(name);
                if (p != null)
                    return p;
            }

            return null;
        }

        /// <summary>
        /// Get a parameter from the parameter set
        /// Returns null if no matching parameter was found
        /// </summary>
        /// <returns></returns>
        public Parameter<T> GetParameter<T>() where T : struct
        {
            foreach (var ps in Values)
            {
                var p = ps.GetParameter<T>();
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
        public Action<T> GetSetter<T>(string name) where T : struct
        {
            foreach (var ps in Values)
            {
                var s = ps.GetSetter<T>(name);
                if (s != null)
                    return s;
            }

            return null;
        }

        /// <summary>
        /// Get a setter for a parameter in the parameter set
        /// Returns null if no matching parameter was found
        /// </summary>
        /// <returns></returns>
        public Action<T> GetSetter<T>() where T : struct
        {
            foreach (var ps in Values)
            {
                var s = ps.GetSetter<T>();
                if (s != null)
                    return s;
            }

            return null;
        }

        /// <summary>
        /// Set a parameter by name
        /// If multiple parameters exist by this name, all of them will be set
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetParameter<T>(string name, T value) where T : struct
        {
            bool isset = false;
            foreach (var ps in Values)
            {
                if (ps.SetParameter(name, value))
                    isset = true;
            }

            return isset;
        }

        /// <summary>
        /// Set the principal parameter
        /// Only the first found principal parameter will be set
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool SetParameter<T>(T value) where T : struct
        {
            foreach (var ps in Values)
            {
                if (ps.SetParameter(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Calls a parameter method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SetParameter(string name)
        {
            bool isset = false;
            foreach (var ps in Values)
            {
                if (ps.SetParameter(name))
                    isset = true;
            }

            return isset;
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
                if (ps.SetParameter(name, value))
                    return true;
            }
            return false;
        }
    }
}
