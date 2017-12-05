using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// A helper class for identifying and extracting Spice-related parameters
    /// </summary>
    public static class SpiceParameters
    {
        /// <summary>
        /// Build a list of available Spice parameters for a type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public static List<SpiceParameter> List(Type t)
        {
            List<SpiceParameter> result = new List<SpiceParameter>();

            var members = t.GetMembers(BindingFlags.Instance | BindingFlags.Public).Where(m => m.GetCustomAttributes<SpiceName>().Any());
            foreach (MemberInfo m in members)
            {
                result.Add(new SpiceParameter(m));
            }

            // Return the list
            return result;
        }
    }


}
