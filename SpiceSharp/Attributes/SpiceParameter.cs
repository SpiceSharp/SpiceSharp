using System.Reflection;
using System.Collections.Generic;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// A class for parameter information
    /// </summary>
    public class SpiceParameter
    {
        /// <summary>
        /// Private variables
        /// </summary>
        public MemberInfo Member { get; }

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
            Member = member;

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
        /// Set the value of the parameter
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        public void Set(object obj, double value)
        {
            // Property
            if (Member is PropertyInfo pi)
            {
                if (pi.PropertyType == typeof(Parameter))
                    ((Parameter)pi.GetValue(obj)).Set(value);
                else
                    pi.SetValue(obj, value);
            }

            // Field
            else if (Member is FieldInfo fi)
            {
                fi.SetValue(obj, value);
            }

            // Method
            else if (Member is MethodInfo mi)
            {
                mi.Invoke(obj, new object[] { value });
            }
        }

        /// <summary>
        /// Set the value of the parameter
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        public void Set(object obj, object value)
        {
            // Property
            if (Member is PropertyInfo pi)
            {
                pi.SetValue(obj, value);
            }

            // Field
            else if (Member is FieldInfo fi)
            {
                fi.SetValue(obj, value);
            }

            // Method
            else if (Member is MethodInfo mi)
            {
                mi.Invoke(obj, new object[] { value });
            }
        }

        /// <summary>
        /// Get the value of the parameter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public double Get(object obj)
        {
            // Property
            if (Member is PropertyInfo pi)
            {
                if (pi.PropertyType == typeof(Parameter))
                    return ((Parameter)pi.GetValue(obj)).Value;
                else
                    return (double)pi.GetValue(obj);
            }

            // Field
            if (Member is FieldInfo fi)
            {
                return (double)fi.GetValue(obj);
            }

            // Method
            if (Member is MethodInfo mi)
            {
                return (double)mi.Invoke(obj, null);
            }

            return double.NaN;
        }
    }
}
