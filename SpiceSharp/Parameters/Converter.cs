using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

namespace SpiceSharp.Parameters
{
    public static class Converter
    {
        private static Regex spiceValue = new Regex(@"^(?<value>[\+\-]?\d+(\.\d+)?)(e(?<exp>[\+\-]?\d+)|(?<mod>(meg|mil|[tgkmunpf]))\w*)?$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Default implementation for converting Spice-style values to doubles
        /// This method can be added to SpiceMember.SpiceMemberConvert
        /// </summary>
        /// <param name="sender">The SpiceMember sending the event</param>
        /// <param name="data">The event data</param>
        public static void SpiceConvert(object sender, SpiceMemberConvertData data)
        {
            // Ignore non-doubles
            if (data.TargetType != typeof(double) && data.TargetType != typeof(double[]))
                return;

            // Ignore non-strings
            var type = data.Value.GetType();

            // Convert to a string
            if (type.IsArray)
            {
                object[] array = (data.Value as Array).Cast<object>().ToArray();
                double[] result = new double[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    double d;
                    if (ConvertString(array[i].ToString(), out d))
                        result[i] = d;
                    else
                        return;
                }
                data.Result = result;
            }
            else
            {
                double d;
                if (ConvertString(data.Value.ToString(), out d))
                    data.Result = d;
            }
        }

        /// <summary>
        /// Convert a Spice parameter string to a double
        /// </summary>
        /// <param name="s">The string</param>
        /// <param name="d">The resulting double</param>
        /// <returns></returns>
        private static bool ConvertString(string s, out double d)
        {
            var m = spiceValue.Match(s);
            if (m.Success)
            {
                if (m.Groups["exp"].Success)
                    d = double.Parse(m.Groups["value"].Value + "e" + m.Groups["exp"].Value, System.Globalization.CultureInfo.InvariantCulture);
                else if (m.Groups["mod"].Success)
                {
                    d = double.Parse(m.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
                    switch (m.Groups["mod"].Value.ToLower())
                    {
                        case "mil": d *= 25.4e6; break;
                        case "t": d *= 1e12; break;
                        case "g": d *= 1e9; break;
                        case "meg": d *= 1e6; break;
                        case "k": d *= 1e3; break;
                        case "m": d *= 1e-3; break;
                        case "u": d *= 1e-6; break;
                        case "n": d *= 1e-9; break;
                        case "p": d *= 1e-12; break;
                        case "f": d *= 1e-15; break;
                    }
                }
                else
                    d = double.Parse(m.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }

            d = double.NaN;
            return false;
        }
    }
}
