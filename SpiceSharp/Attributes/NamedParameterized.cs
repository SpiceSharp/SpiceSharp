using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// A templerate for a class with named parameters
    /// Named parameters are parameters one or more <see cref="ParameterNameAttribute"/>
    /// </summary>
    public abstract class NamedParameterized : Parameterized
    {
        /// <summary>
        /// Returns all members tagged with a certain name
        /// </summary>
        /// <param name="name">The name of the member</param>
        /// <returns></returns>
        protected IEnumerable<MemberInfo> Named(string name)
        {
            return Members.Where(m =>
            {
                foreach (var pn in m.GetCustomAttributes<ParameterNameAttribute>())
                    if (pn.Name == name)
                        return true;
                return false;
            });
        }

        /// <summary>
        /// Returns the first principal member of the class
        /// </summary>
        protected MemberInfo Principal
        {
            get
            {
                return Members.FirstOrDefault(m =>
                {
                    foreach (var pi in m.GetCustomAttributes<ParameterInfoAttribute>())
                        if (pi.IsPrincipal)
                            return true;
                    return false;
                });
            }
        }

        /// <summary>
        /// Create setters for all named parameters
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <returns></returns>
        public Dictionary<string, Action<T>> CreateSetters<T>() where T : struct
        {
            var result = new Dictionary<string, Action<T>>();
            foreach (var member in Members)
            {
                if (!member.IsDefined(typeof(ParameterNameAttribute)))
                    continue;

                // Create a setter
                var setter = CreateSetter<T>(member);
                if (setter != null)
                {
                    foreach (var pn in member.GetCustomAttributes<ParameterNameAttribute>())
                        result[pn.Name] = setter;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a setter for the first parameter with a name that can be written
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Action<T> CreateSetter<T>(string name) where T : struct
        {
            foreach (var member in Named(name))
            {
                var result = CreateSetter<T>(member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a setter for the principal parameter
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <returns></returns>
        public Action<T> CreateSetter<T>() where T : struct
        {
            var p = Principal;
            if (p != null)
                return CreateSetter<T>(p);
            return null;
        }

        /// <summary>
        /// Returns a getter for the first parameter with a name that can be read
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public Func<T> CreateGetter<T>(string name) where T : struct
        {
            foreach (var member in Named(name))
            {
                var result = CreateGetter<T>(member);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a getter for the principal parameter
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <returns></returns>
        public Func<T> CreateGetter<T>() where T : struct
        {
            var p = Principal;
            if (p != null)
                return CreateGetter<T>(p);
            return null;
        }

        /// <summary>
        /// Create getters for all named parameters
        /// </summary>
        /// <typeparam name="T">Base value type</typeparam>
        /// <returns></returns>
        public Dictionary<string, Func<T>> CreateGetters<T>() where T : struct
        {
            var result = new Dictionary<string, Func<T>>();
            foreach (var member in Members)
            {
                if (!member.IsDefined(typeof(ParameterNameAttribute)))
                    continue;

                // Create a getter
                var getter = CreateGetter<T>(member);
                if (getter != null)
                {
                    foreach (var pn in member.GetCustomAttributes<ParameterNameAttribute>())
                        result[pn.Name] = getter;
                }
            }

            return result;
        }
    }
}
