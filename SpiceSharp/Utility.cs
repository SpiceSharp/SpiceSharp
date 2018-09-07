using SpiceSharp.Attributes;
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

                    if (pi.CanWrite)
                    {
                        if (pi.PropertyType == typeof(double))
                            pi.SetValue(destination, (double) pi.GetValue(source));
                        else if (pi.PropertyType == typeof(int))
                            pi.SetValue(destination, (int) pi.GetValue(source));
                        else if (pi.PropertyType == typeof(string))
                            pi.SetValue(destination, (string) pi.GetValue(source));
                        else if (pi.PropertyType == typeof(bool))
                            pi.SetValue(destination, (bool) pi.GetValue(source));
                        else if (pi.PropertyType == typeof(BaseParameter) ||
                                 pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(BaseParameter)))
                        {
                            var target = (BaseParameter) pi.GetValue(destination);
                            var from = (BaseParameter) pi.GetValue(source);
                            if (target != null && from != null)
                                target.CopyFrom(from);
                            else
                                pi.SetValue(destination, from?.Clone());
                        }
                    }
                    else
                    {
                        // We can't write ourself, but maybe we can just copy
                        if (pi.PropertyType == typeof(BaseParameter) ||
                            pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(BaseParameter)))
                        {
                            var target = (BaseParameter) pi.GetValue(destination);
                            var from = (BaseParameter) pi.GetValue(source);
                            if (target != null && from != null)
                                target.CopyFrom(from);
                        }
                    }
                }
                else if (member is FieldInfo fi)
                {
                    if (fi.FieldType == typeof(double))
                        fi.SetValue(destination, (double) fi.GetValue(source));
                    else if (fi.FieldType == typeof(int))
                        fi.SetValue(destination, (int) fi.GetValue(source));
                    else if (fi.FieldType == typeof(string))
                        fi.SetValue(destination, (string) fi.GetValue(source));
                    else if (fi.FieldType == typeof(bool))
                        fi.SetValue(destination, (bool) fi.GetValue(source));
                    else if (fi.FieldType == typeof(BaseParameter) ||
                             fi.FieldType.GetTypeInfo().IsSubclassOf(typeof(BaseParameter)))
                    {
                        var target = (BaseParameter) fi.GetValue(destination);
                        var from = (BaseParameter) fi.GetValue(source);
                        if (target != null && from != null)
                            target.CopyFrom(from);
                        else
                            fi.SetValue(destination, from?.Clone());
                    }
                }
            }
        }
    }
}
