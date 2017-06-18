using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Parameters
{
    public class SpiceMember
    {
        [Flags]
        public enum AccessFlags
        {
            // Access
            None = 0x00,
            Set = 0x01,
            Ask = 0x02,

            Parameter = 0x04,       // Instance of Parameter<>
            Uninteresting = 0x08,   // Uninteresting to list
            Principal = 0x10        // Principal parameter
        }

        /// <summary>
        /// Gets the access flags
        /// </summary>
        public AccessFlags Access { get; private set; }

        /// <summary>
        /// Gets the member type
        /// </summary>
        public MemberTypes MemberType { get; }

        /// <summary>
        /// Gets the value type
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the member information
        /// </summary>
        public MemberInfo Info { get; }

        /// <summary>
        /// Is the member a Parameter object?
        /// </summary>
        public bool IsParameter { get; }

        /// <summary>
        /// The number of parameters when the member is a method
        /// </summary>
        private int Parameters = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The member information</param>
        public SpiceMember(MemberInfo info)
        {
            Info = info;
            MemberType = info.MemberType;
            IsParameter = false;
            Access = AccessFlags.Set | AccessFlags.Ask;

            // Check the extra information
            SpiceInfo extra = info.GetCustomAttribute<SpiceInfo>();
            if (extra != null)
            {
                if (!extra.Interesting)
                    Access |= AccessFlags.Uninteresting;
                if (extra.IsPrincipal)
                    Access |= AccessFlags.Principal;
            }

            switch (MemberType)
            {
                case MemberTypes.Property:

                    PropertyInfo pi = info as PropertyInfo;

                    if (pi.PropertyType.Name == typeof(Parameter<>).Name)
                    {
                        IsParameter = true;
                        ValueType = pi.PropertyType.GenericTypeArguments[0];
                        Access |= AccessFlags.Parameter;
                    }
                    else
                    {
                        ValueType = pi.PropertyType;

                        if (pi.SetMethod == null || !pi.SetMethod.IsPublic)
                            Access &= ~AccessFlags.Set;
                        if (pi.GetMethod == null || !pi.GetMethod.IsPublic)
                            Access &= ~AccessFlags.Ask;
                    }
                    break;

                case MemberTypes.Field:

                    FieldInfo fi = info as FieldInfo;

                    if (fi.FieldType.Name == typeof(Parameter<>).Name)
                    {
                        IsParameter = true;
                        ValueType = fi.FieldType.GenericTypeArguments[0];
                        Access |= AccessFlags.Parameter;
                    }
                    else
                        ValueType = fi.FieldType;
                    break;

                case MemberTypes.Method:

                    MethodInfo mi = info as MethodInfo;
                    var parameters = mi.GetParameters();
                    Parameters = parameters.Length;

                    switch (Parameters)
                    {
                        case 0:
                            if (mi.ReturnType != typeof(void))
                            {
                                // val = Method()
                                Access = AccessFlags.Ask;
                                ValueType = mi.ReturnType;
                            }
                            else
                            {
                                // void Method()
                                Access = AccessFlags.Set;
                                ValueType = typeof(void);
                            }
                            break;

                        case 1:
                            if (parameters[0].ParameterType != typeof(Circuit) && mi.ReturnType == typeof(void))
                            {
                                // void Method(val)
                                Access = AccessFlags.Set;
                                ValueType = parameters[0].ParameterType;
                            }
                            else if (mi.ReturnType != typeof(void))
                            {
                                // val = Method(ckt)
                                Access = AccessFlags.Ask;
                                ValueType = mi.ReturnType;
                            }
                            else
                                throw new CircuitException($"Invalid method {mi.Name} for class {info.Name}");
                            break;

                        case 2:
                            if (parameters[0].ParameterType != typeof(Circuit) || mi.ReturnType != typeof(void))
                                throw new CircuitException($"Invalid method {mi.Name} for class {info.Name}");

                            // void Method(ckt, val)
                            Access = AccessFlags.Set;
                            ValueType = parameters[1].ParameterType;
                            break;

                        default:
                            throw new CircuitException($"Invalid method {mi.Name} for class {info.Name}");
                    }

                    break;
            }
        }

        /// <summary>
        /// Set the member on an object
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="value">The parameter value</param>
        /// <param name="ckt">The circuit if applicable</param>
        public void Set(Parameterized obj, object value = null, Circuit ckt = null)
        {
            if (!Access.HasFlag(AccessFlags.Set))
                throw new CircuitException($"Cannot set parameter");
            if (ValueType == typeof(void) && value != null)
                throw new ParameterTypeException(obj, typeof(void));
            else if (value != null && !ValueType.IsAssignableFrom(value.GetType()))
                throw new ParameterTypeException(obj, ValueType);

            switch (MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo pi = Info as PropertyInfo;
                    if (IsParameter)
                        ((IParameter)pi.GetValue(obj)).Set(value);
                    else
                        pi.SetValue(obj, value);
                    break;

                case MemberTypes.Field:
                    FieldInfo fi = Info as FieldInfo;
                    if (IsParameter)
                        ((IParameter)fi.GetValue(obj)).Set(value);
                    else
                        fi.SetValue(obj, value);
                    break;

                case MemberTypes.Method:
                    MethodInfo mi = Info as MethodInfo;
                    switch (Parameters)
                    {
                        case 0: mi.Invoke(obj, null); break;
                        case 1: mi.Invoke(obj, new object[] { value }); break;
                        case 2: mi.Invoke(obj, new object[] { ckt, value }); break;
                    }
                    break;

                default:
                    throw new CircuitException($"Invalid type for {Info.Name}");
            }
        }

        /// <summary>
        /// Get the member on a parameterized object
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="ckt">The circuit if applicable</param>
        /// <returns></returns>
        public object Get(Parameterized obj, Circuit ckt = null)
        {
            if (!Access.HasFlag(AccessFlags.Ask))
                throw new CircuitException($"Cannot ask parameter");
            
            switch (MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo pi = Info as PropertyInfo;
                    if (IsParameter)
                        return ((IParameter)pi.GetValue(obj)).Get();
                    else
                        return pi.GetValue(obj);

                case MemberTypes.Field:
                    FieldInfo fi = Info as FieldInfo;
                    if (IsParameter)
                        return ((IParameter)fi.GetValue(obj)).Get();
                    else
                        return fi.GetValue(obj);

                case MemberTypes.Method:
                    MethodInfo mi = Info as MethodInfo;
                    if (Parameters == 0)
                        return mi.Invoke(obj, null);
                    else
                        return mi.Invoke(obj, new object[] { ckt });

                default:
                    throw new CircuitException($"Invalid type for {Info.Name}");
            }
        }

        /// <summary>
        /// Get the member on a parameterized object
        /// </summary>
        /// <typeparam name="T">The expected return value</typeparam>
        /// <param name="obj">The parameterized object</param>
        /// <param name="ckt">The circuit if applicable</param>
        /// <returns></returns>
        public T Get<T>(Parameterized obj, Circuit ckt = null)
        {
            if (ValueType != typeof(T))
                throw new ParameterTypeException(obj, ValueType);
            return (T)Get(obj, ckt);
        }
    }
}
