using System;
using System.Collections.Generic;
using System.Reflection;
using SpiceSharp.Diagnostics;
using System.Linq;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// An abstract class that contains parameters
    /// These parameters can be write-only, read-only, ...
    /// A table of parameters is also available for such classes.
    /// </summary>
    public abstract class Parameterized
    {
        /// <summary>
        /// Get the name of the object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This dictionary contains all Spice keywords and their mapping
        /// </summary>
        private static Dictionary<Type, Dictionary<string, SpiceMember>> members { get; } = new Dictionary<Type, Dictionary<string, SpiceMember>>();

        /// <summary>
        /// All parameters for this instance
        /// </summary>
        private Dictionary<string, SpiceMember> parameters { get; } = new Dictionary<string, SpiceMember>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Parameterized(string name)
        {
            Name = name;

            // Build the parameters
            if (!members.ContainsKey(GetType()))
            {
                var d = new Dictionary<string, SpiceMember>();
                var tmembers = GetType().GetMembers(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public).Where(p => p.GetCustomAttributes<SpiceName>().Any());
                foreach (var m in tmembers)
                {
                    var nmember = new SpiceMember(m);

                    var names = m.GetCustomAttributes<SpiceName>();
                    foreach (var n in names)
                        d.Add(n.Name, nmember);
                }
                members.Add(GetType(), d);
                parameters = d;
            }
            else
                parameters = members[GetType()];
        }

        /// <summary>
        /// Specify a parameter for this object
        /// </summary>
        /// <param name="id">The parameter identifier</param>
        /// <param name="value">The value</param>
        /// <param name="ckt">The circuit if applicable</param>
        public virtual void Set(string name, object value = null, Circuit ckt = null)
        {
            // Use reflection to find the member associated with the name
            if (!parameters.ContainsKey(name))
            {
                CircuitWarning.Warning(this, $"{Name}: Parameter '{name}' does not exist");
                return;
            }

            parameters[name].Set(this, value, ckt);
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="ckt">The circuit if applicable</param>
        /// <returns></returns>
        public virtual object Ask(string name, Circuit ckt = null)
        {
            if (!parameters.ContainsKey(name))
                throw new CircuitException($"{Name}: Parameter '{name}' does not exist");

            return parameters[name].Get(this, ckt);
        }

        /// <summary>
        /// Get a list of all parameters
        /// </summary>
        /// <returns></returns>
        public static string[] GetParameterNames(Parameterized p)
        {
            var ms = members[p.GetType()];
            string[] parameters = new string[ms.Count];
            int index = 0;
            foreach (var m in ms.Keys)
                parameters[index++] = m;
            return parameters;
        }

        /// <summary>
        /// Get the description for a parameter
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        public static string GetParameterDescription(Parameterized p, string parameter)
        {
            var ms = members[p.GetType()];
            if (!ms.ContainsKey(parameter))
                throw new CircuitException($"Parameter {parameter} does not exist");
            var si = ms[parameter].Info.GetCustomAttribute<SpiceInfo>();
            if (si != null)
                return si.Description;
            return "";
        }

        /// <summary>
        /// Get the type of the parameter
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        public Type GetParameterType(Parameterized p, string parameter)
        {
            var ms = members[p.GetType()];
            if (!ms.ContainsKey(parameter))
                throw new CircuitException($"Parameter {parameter} does not exist");
            return ms[parameter].ValueType;
        }

        /// <summary>
        /// Get a parameter from the object by name
        /// The parameter must be defined as a property with a getter in order to 
        /// be found by this method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual Parameter<T> GetParameter<T>(string name)
        {
            // Get the member with this name
            var members = GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(Parameter<T>))
                .Where(q => q.GetCustomAttributes<SpiceName>().Where(r => r.Name == name).Any());
            if (members.Count() == 0)
                return null;

            // We found a parameter
            var member = members.First();
            return (Parameter<T>)member.GetValue(this);
        }
    }
}
