using SpiceSharp.Attributes;
using SpiceSharp.Components;
using System.Globalization;
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
        public static void CopyPropertiesAndFields(object source, object destination)
        {
            var members = source.GetType().GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                if (member is PropertyInfo pi)
                {
                    if (pi.GetCustomAttribute(typeof(DerivedPropertyAttribute)) != null)
                    {
                        // skip properties with DerivedPropertyAttribute because their value will be set elsewhere
                        continue;
                    }

                    // property has Parameter or subclass of Parameter type
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
                        // property has Waveform or subclass of Waveform type
                        if (pi.PropertyType == typeof(Waveform) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Waveform)))
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
                else if (member is FieldInfo fi)
                {
                    // field has Parameter or subclass of Parameter type
                    if (fi.FieldType == typeof(Parameter) || fi.FieldType.GetTypeInfo().IsSubclassOf(typeof(Parameter)))
                    {
                        var parameter = (Parameter)fi.GetValue(source);
                        var clonedParameter = parameter.Clone();
                        fi.SetValue(destination, clonedParameter);
                    }
                    else
                    {
                        if (fi.FieldType == typeof(double))
                        {
                            var fieldValue = (double)fi.GetValue(source);
                            fi.SetValue(destination, fieldValue);
                        }
                        // field has Waveform or subclass of Waveform type
                        if (fi.FieldType == typeof(Waveform) || fi.FieldType.GetTypeInfo().IsSubclassOf(typeof(Waveform)))
                        {
                            var parameter = (Waveform)fi.GetValue(source);
                            if (parameter != null)
                            {
                                fi.SetValue(destination, parameter.DeepClone());
                            }
                        }
                    }
                }
            }
        }

    }
}
