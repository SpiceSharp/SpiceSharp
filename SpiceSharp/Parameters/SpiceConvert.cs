using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This class contains some basic converter for spice values
    /// </summary>
    public static class SpiceConvert
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private static Regex spiceValue = new Regex(@"^(?<value>[\+\-]?(\d+(\.\d+)?|\.\d+))(e(?<exp>[\+\-]?\d+)|(?<mod>(meg|mil|[tgkmunpf]))\w*)?$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Default implementation for converting Spice-style values to doubles
        /// This method can be added to the event SpiceMember.SpiceMemberConvert
        /// </summary>
        /// <param name="sender">The SpiceMember sending the event</param>
        /// <param name="data">The event data</param>
        public static void EventMethod(object sender, SpiceMemberConvertData data)
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
                    if (TryParse(array[i].ToString(), out double d))
                        result[i] = d;
                    else
                        return;
                }
                data.Result = result;
            }
            else if (TryParse(data.Value.ToString(), out double d))
                    data.Result = d;
        }

        /// <summary>
        /// Convert a Spice parameter string to a double
        /// It accounts for modifiers and uses '.' as a decimal
        /// </summary>
        /// <param name="s">The string</param>
        /// <param name="d">The resulting double</param>
        /// <returns></returns>
        public static double ToDouble(string s)
        {
            double d;
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
                {
                    string v = m.Groups["value"].Value;
                    if (v[0] == '.')
                        v = "0" + v;
                    d = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                }
                return d;
            }

            // Failed
            throw new FormatException();
        }

        /// <summary>
        /// Try to parse a Spice value
        /// It accounts for modifiers and uses '.' as a decimal
        /// </summary>
        /// <param name="s">The string</param>
        /// <param name="d">The output value</param>
        /// <returns>True if the parsing succeeded</returns>
        public static bool TryParse(string s, out double d)
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
                {
                    string v = m.Groups["value"].Value;
                    if (v[0] == '.')
                        v = "0" + v;
                    d = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                }
                return true;
            }

            // Failed
            d = double.NaN;
            return false;
        }
    }
}
