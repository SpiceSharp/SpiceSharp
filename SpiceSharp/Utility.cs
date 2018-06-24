using SpiceSharp.Components;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// Some utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Format a string with the current culture
        /// </summary>
        /// <param name="format">The formatting string</param>
        /// <param name="args">The arguments</param>
        /// <returns></returns>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Copies properties from a source object to a destination object.
        /// </summary>
        /// <param name="source">A source object.</param>
        /// <param name="destination">A destination object</param>
        public static void CopyProperties(object source, object destination)
        {
            var members = source.GetType().GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                if (member is PropertyInfo pi)
                {
                    if (pi.PropertyType == typeof(Parameter) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter)))
                    {
                        var parameter = (Parameter)pi.GetValue(source);

                        if (pi.CanWrite)
                        {
                            // property has a setter
                            var clonedParameter = parameter.Clone();
                            pi.SetValue(destination, clonedParameter);
                        }
                        else
                        {
                            // if parameter is not given, don't deal with it
                            if (parameter is GivenParameter gp && gp.Given == false)
                            {
                                continue;
                            }

                            // property doesn't have a setter
                            var destinationParameter = (Parameter)pi.GetValue(destination);
                            destinationParameter.Value = parameter.Value;
                        }
                    }
                    else
                    {
                        if (pi.PropertyType == typeof(double))
                        {
                            if (pi.CanWrite)
                            {
                                // double property has a setter, so it can be set
                                var propertyValue = (double)pi.GetValue(source);
                                pi.SetValue(destination, propertyValue);
                            }
                        }

                        if (pi.PropertyType == typeof(Waveform))
                        {
                            if (pi.CanWrite)
                            {
                                var parameter = (Waveform)pi.GetValue(source);
                                if (parameter != null)
                                {
                                    pi.SetValue(destination, parameter.DeepClone());
                                }
                            }
                        }
                    }
                }
                else if (member is MethodInfo mi)
                {
                    // for properties with only getter and default value there is a method instead of property
                    if (mi.Name.StartsWith("get_", StringComparison.Ordinal))
                    {
                        if (mi.ReturnType == typeof(GivenParameter) && mi.GetParameters().Length == 0)
                        {
                            var parameter = ((GivenParameter)mi.Invoke(source, new object[0]));
                            if (parameter.Given == false)
                            {
                                continue;
                            }

                            var destinationParameter = (GivenParameter)mi.Invoke(destination, new object[0]);
                            destinationParameter.Value = parameter.Value;
                        }

                        if (mi.ReturnType == typeof(double) && mi.GetParameters().Length == 0)
                        {
                            var value = (double)mi.Invoke(source, new object[0]);
                            var setter = (MethodInfo)members.SingleOrDefault(member2 => member2 is MethodInfo mi2 && mi2.Name == "set_" + (mi.Name.Replace("get_", "")));
                            if (setter != null)
                            {
                                setter.Invoke(destination, new object[] { value });
                            }
                        }
                    }
                }
            }
        }

    }
}
