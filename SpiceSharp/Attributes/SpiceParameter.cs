using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// A class for parameter information
    /// </summary>
    public class SpiceParameter<T>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private MemberInfo member;

        /// <summary>
        /// Get all names for the parameter
        /// </summary>
        public SpiceName[] Names { get; private set; }

        /// <summary>
        /// Get all information about the parameter
        /// </summary>
        public SpiceInfo Info { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="names">Names of the parameter</param>
        /// <param name="info">Info of the parameter</param>
        public SpiceParameter(MemberInfo member)
        {
            this.member = member;
            List<SpiceName> names = new List<SpiceName>();
            foreach (var attr in member.GetCustomAttributes())
            {
                if (attr is SpiceName name)
                    names.Add(name);
                if (attr is SpiceInfo info)
                    Info = info;
            }
            Names = names.ToArray();
        }

        /// <summary>
        /// Create a getter for the parameter
        /// </summary>
        /// <typeparam name="R">Return type</typeparam>
        /// <returns></returns>
        public Func<T, R> CreateGetter<R>()
        {
            // Properties
            if (member is PropertyInfo pi)
            {
                return (Func<T, R>)pi.GetGetMethod().CreateDelegate(typeof(Func<T, R>));
            }

            // Fields
            if (member is FieldInfo fi)
            {
                return CreateGetter<T, R>(fi);
            }

            // Method
            if (member is MethodInfo mi)
            {
                var parameters = mi.GetParameters();

                // R method() is allowed
                if (parameters.Length == 0 && mi.ReturnType == typeof(R))
                {
                    return (Func<T, R>)mi.CreateDelegate(typeof(Func<T, R>));
                }
            }

            throw new CircuitException("Invalid parameter type");
        }

        /// <summary>
        /// Create an asking method
        /// </summary>
        /// <typeparam name="R">Return type</typeparam>
        /// <returns></returns>
        public Func<T, Circuit, R> CreateAsker<R>()
        {
            // Method
            if (member is MethodInfo mi)
            {
                var parameters = mi.GetParameters();

                // R method(ckt) is allowed
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Circuit) && mi.ReturnType == typeof(R))
                {
                    return (Func<T, Circuit, R>)mi.CreateDelegate(typeof(Func<T, Circuit, R>));
                }
            }

            throw new CircuitException("Invalid parameter type");
        }

        /// <summary>
        /// Create a setter for the parameter
        /// </summary>
        /// <typeparam name="P">The parameter type</typeparam>
        /// <returns></returns>
        public Action<T, P> CreateSetter<P>()
        {
            // Property
            if (member is PropertyInfo pi)
            {
                return (Action<T, P>)pi.GetSetMethod().CreateDelegate(typeof(Action<T, P>));
            }

            // Field
            if (member is FieldInfo fi)
            {
                return CreateSetter<T, P>(fi);
            }

            // Method
            if (member is MethodInfo mi)
            {
                var parameters = mi.GetParameters();

                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(P))
                {
                    return (Action<T, P>)mi.CreateDelegate(typeof(Action<T, P>));
                }
            }

            throw new CircuitException("Invalid parameter type");
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
