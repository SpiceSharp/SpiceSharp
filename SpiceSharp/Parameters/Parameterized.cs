using System;
using System.Collections.Generic;
using System.Reflection;
using SpiceSharp.Diagnostics;
using System.Linq;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This class allows referring to spice properties using their name, as specified by the SpiceName() attribute
    /// </summary>
    public abstract class Parameterized<T> : IParameterized
    {
        /// <summary>
        /// Dictionaries for finding our parameters back
        /// </summary>
        private static Dictionary<string, Func<T, Parameter>> pgetter = new Dictionary<string, Func<T, Parameter>>();
        private static Dictionary<string, Func<T, double>> dgetter = new Dictionary<string, Func<T, double>>();
        private static Dictionary<string, Action<T, double>> dsetter = new Dictionary<string, Action<T, double>>();
        private static Dictionary<string, Func<T, Circuit, double>> dcgetter = new Dictionary<string, Func<T, Circuit, double>>();

        /// <summary>
        /// This method will register all the spice properties
        /// </summary>
        protected internal static void Register()
        {
            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public).Where(m => m.GetCustomAttributes<SpiceName>().Any());
            foreach (MemberInfo m in members)
            {
                // Create a delegate for the member
                if (m is PropertyInfo)
                {
                    PropertyInfo pi = m as PropertyInfo;
                    if (pi.PropertyType == typeof(Parameter))
                    {
                        Func<T, Parameter> getter = (Func<T, Parameter>)pi.GetGetMethod().CreateDelegate(typeof(Func<T, Parameter>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                            pgetter.Add((attr as SpiceName).Name, getter);
                    }
                    else if (pi.PropertyType == typeof(double))
                    {
                        Func<T, double> getter = (Func<T, double>)pi.GetGetMethod()?.CreateDelegate(typeof(Func<T, double>));
                        Action<T, double> setter = (Action<T, double>)pi.GetSetMethod()?.CreateDelegate(typeof(Action<T, double>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            SpiceName sn = attr as SpiceName;
                            if (getter != null)
                                dgetter.Add(sn.Name, getter);
                            if (setter != null)
                                dsetter.Add(sn.Name, setter);
                        }
                    }
                }

                else if (m is MethodInfo)
                {
                    MethodInfo mi = m as MethodInfo;

                    // The only allowed parameter is a Circuit object
                    var parameters = mi.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Circuit) && mi.ReturnType == typeof(double))
                    {
                        Func<T, Circuit, double> getter = (Func<T, Circuit, double>)mi.CreateDelegate(typeof(Func<T, Circuit, double>));
                        foreach (var attr in mi.GetCustomAttributes<SpiceName>())
                            dcgetter.Add(attr.Name, getter);
                    }
                }
            }
        }

        private T me;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Parameterized()
        {
            me = (T)(object)this;
        }

        /// <summary>
        /// Specify a parameter for this object
        /// </summary>
        /// <param name="id">The parameter identifier</param>
        /// <param name="value">The value</param>
        /// <param name="ckt">The circuit if applicable</param>
        public virtual void Set(string name, double value)
        {
            // Set the parameter
            if (dsetter.ContainsKey(name))
                dsetter[name].Invoke(me, value);
            else
                pgetter[name].Invoke(me).Set(value);
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <returns></returns>
        public virtual double Ask(string name)
        {
            return dgetter[name].Invoke(me);
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public virtual double Ask(string name, Circuit ckt)
        {
            return dcgetter[name].Invoke(me, ckt);
        }
    }
}
