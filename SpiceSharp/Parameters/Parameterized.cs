using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using SpiceSharp.Diagnostics;
using System.Linq;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// An abstract class that implements <see cref="IParameterized"/> by using attributes.
    /// </summary>
    public abstract class Parameterized<T> : IParameterized
    {
        // Register our parameters
        static Parameterized()
        {
            Register();
        }

        /// <summary>
        /// Dictionaries for finding our parameters back
        /// </summary>
        private static Dictionary<string, Func<T, Parameter>> pgetter = new Dictionary<string, Func<T, Parameter>>();
        private static Dictionary<string, Func<T, double>> dgetter = new Dictionary<string, Func<T, double>>();
        private static Dictionary<string, Action<T, double>> dsetter = new Dictionary<string, Action<T, double>>();
        private static Dictionary<string, Func<T, Circuit, double>> dcgetter = new Dictionary<string, Func<T, Circuit, double>>();
        private static Dictionary<string, Action<T, string>> ssetter = new Dictionary<string, Action<T, string>>();
        private static Dictionary<string, Func<T, string>> sgetter = new Dictionary<string, Func<T, string>>();

        /// <summary>
        /// This method will register all the spice properties
        /// </summary>
        protected internal static void Register()
        {
            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public).Where(m => m.GetCustomAttributes<SpiceName>().Any());
            foreach (MemberInfo m in members)
            {
                // Create a delegate for the member

                // PROPERTIES
                if (m is PropertyInfo)
                {
                    PropertyInfo pi = m as PropertyInfo;

                    // TYPE Parameter
                    if (pi.PropertyType == typeof(Parameter))
                    {
                        Func<T, Parameter> getter = (Func<T, Parameter>)pi.GetGetMethod().CreateDelegate(typeof(Func<T, Parameter>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                            pgetter.Add((attr as SpiceName).Name, getter);
                    }

                    // TYPE double
                    else if (pi.PropertyType == typeof(double))
                    {
                        Func<T, double> getter = (Func<T, double>)pi.GetGetMethod()?.CreateDelegate(typeof(Func<T, double>));
                        Action<T, double> setter = (Action<T, double>)pi.GetSetMethod()?.CreateDelegate(typeof(Action<T, double>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            if (getter != null)
                                dgetter.Add(attr.Name, getter);
                            if (setter != null)
                                dsetter.Add(attr.Name, setter);
                        }
                    }

                    // TYPE string
                    else if (pi.PropertyType == typeof(string))
                    {
                        Func<T, string> getter = (Func<T, string>)pi.GetGetMethod()?.CreateDelegate(typeof(Func<T, string>));
                        Action<T, string> setter = (Action<T, string>)pi.GetSetMethod()?.CreateDelegate(typeof(Action<T, string>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            if (getter != null)
                                sgetter.Add(attr.Name, getter);
                            if (setter != null)
                                ssetter.Add(attr.Name, setter);
                        }
                    }
                }

                // FIELDS
                else if (m is FieldInfo)
                {
                    FieldInfo fi = m as FieldInfo;

                    // TYPE parameter
                    if (fi.FieldType == typeof(Parameter))
                    {
                        Func<T, Parameter> getter = CreateGetter<T, Parameter>(fi);
                        foreach (var attr in fi.GetCustomAttributes<SpiceName>())
                        {
                            if (getter != null)
                                pgetter.Add(attr.Name, getter);
                        }
                    }
                    
                    // TYPE double
                    if (fi.FieldType == typeof(double))
                    {
                        Func<T, double> getter = CreateGetter<T, double>(fi);
                        Action<T, double> setter = CreateSetter<T, double>(fi);
                        foreach (var attr in fi.GetCustomAttributes<SpiceName>())
                        {
                            if (getter != null)
                                dgetter.Add(attr.Name, getter);
                            if (setter != null)
                                dsetter.Add(attr.Name, setter);
                        }
                    }

                    // TYPE string
                    if (fi.FieldType == typeof(string))
                    {
                        Func<T, string> getter = CreateGetter<T, string>(fi);
                        Action<T, string> setter = CreateSetter<T, string>(fi);
                        foreach (var attr in fi.GetCustomAttributes<SpiceName>())
                        {
                            if (getter != null)
                                sgetter.Add(attr.Name, getter);
                            if (setter != null)
                                ssetter.Add(attr.Name, setter);
                        }
                    }
                }

                // METHODS
                else if (m is MethodInfo)
                {
                    MethodInfo mi = m as MethodInfo;

                    // Supported methods
                    var parameters = mi.GetParameters();

                    // TYPE double METHOD(Circuit ckt)
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Circuit) && mi.ReturnType == typeof(double))
                    {
                        Func<T, Circuit, double> getter = (Func<T, Circuit, double>)mi.CreateDelegate(typeof(Func<T, Circuit, double>));
                        foreach (var attr in mi.GetCustomAttributes<SpiceName>())
                        {
                            if (dcgetter != null)
                                dcgetter.Add(attr.Name, getter);
                        }
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
        public virtual void Set(string name, double value)
        {
            // Set the parameter
            if (dsetter.ContainsKey(name))
                dsetter[name].Invoke(me, value);
            else if (pgetter.ContainsKey(name))
                pgetter[name].Invoke(me).Set(value);
            else
                CircuitWarning.Warning(this, $"Unrecognized parameter \"{name}\" of type double");
        }

        /// <summary>
        /// Specify a parameter for this object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public virtual void Set(string name, string value)
        {
            // Set the parameter
            if (ssetter.ContainsKey(name))
                ssetter[name].Invoke(me, value);
            else
                CircuitWarning.Warning(this, $"Unrecognized parameter \"{name}\" of type string");
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <returns></returns>
        public virtual double Ask(string name)
        {
            if (dgetter.ContainsKey(name))
                return dgetter[name].Invoke(me);
            return pgetter[name].Invoke(me).Value;
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public virtual double Ask(string name, Circuit ckt) => dcgetter[name].Invoke(me, ckt);

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <returns></returns>
        public virtual string AskString(string name) => sgetter[name].Invoke(me);

        /// <summary>
        /// Enumerate all parameters
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> ParameterList(bool alias = true)
        {
            List<string> names = new List<string>();
            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public).Where(m => m.GetCustomAttributes<SpiceName>().Any());
            foreach (MemberInfo m in members)
            {
                // Create a delegate for the member
                if (m is PropertyInfo)
                {
                    PropertyInfo pi = m as PropertyInfo;
                    if (pi.PropertyType == typeof(Parameter))
                    {
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            names.Add(attr.Name);
                            if (!alias)
                                break;
                        }
                            
                    }
                    else if (pi.PropertyType == typeof(double))
                    {
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            names.Add(attr.Name);
                            if (!alias)
                                break;
                        }
                    }
                    else if (pi.PropertyType == typeof(string))
                    {
                        Action<T, string> setter = (Action<T, string>)pi.GetSetMethod()?.CreateDelegate(typeof(Action<T, string>));
                        foreach (var attr in pi.GetCustomAttributes<SpiceName>())
                        {
                            names.Add(attr.Name);
                            if (!alias)
                                break;
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
                        foreach (var attr in mi.GetCustomAttributes<SpiceName>())
                        {
                            names.Add(attr.Name);
                            if (!alias)
                                break;
                        }
                    }
                }
            }
            return names;
        }

        /// <summary>
        /// Create a getter for a field
        /// Code by Zotta (https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo)
        /// </summary>
        /// <typeparam name="S">Object type</typeparam>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        private static Func<S, R> CreateGetter<S, R>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(R), new Type[1] { typeof(S) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<S, R>)setterMethod.CreateDelegate(typeof(Func<S, R>));
        }

        /// <summary>
        /// Create a setter for a field
        /// Code by Zotta (https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo)
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        private static Action<S, P> CreateSetter<S, P>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2] { typeof(S), typeof(P) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Action<S, P>)setterMethod.CreateDelegate(typeof(Action<S, P>));
        }
    }
}
