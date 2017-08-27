using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class will process the setup method
    /// </summary>
    public class SpiceSetup : SpiceIterator
    {
        /// <summary>
        /// Get all default device values
        /// </summary>
        public Dictionary<string, string> DeviceDefaultValues { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Get all default model values
        /// </summary>
        public Dictionary<string, string> ModelDefaultValues { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Get the matrix indices
        /// </summary>
        public Dictionary<string, Tuple<string, string>> MatrixNodes { get; } = new Dictionary<string, Tuple<string, string>>();

        /// <summary>
        /// Get the device nodes
        /// </summary>
        public HashSet<string> Nodes { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the name of the variable reference to the state
        /// </summary>
        public string StatesVariable { get; private set; }
        private string states;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The device</param>
        public SpiceSetup(SpiceDevice dev)
        {
            // Get the 
            string content = dev.GetMethod(SpiceDevice.Methods.Setup);

            // Get the parameters
            List<string> p = new List<string>();
            Code.GetMethodParameters(content, p, dev.GetMethodName(SpiceDevice.Methods.Setup));
            states = p[3];

            ReadMethod(content);
        }

        /// <summary>
        /// Export model code
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        public override string ExportModel(SpiceParam modelparams)
        {
            string code = GetModelCode(modelparams);
            code = GetDefaultValues(code, ModelDefaultValues);

            // Find temperature stuff
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTtemp", "ckt.State.Temperature");
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTnomTemp", "ckt.State.NominalTemperature");

            // -> Is never possible, so let's go for dots
            code = Regex.Replace(code, @"\s*\-\>\s*", ".");

            return Code.Format(code);
        }

        /// <summary>
        /// Export device code
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        public override string ExportDevice(SpiceParam deviceparams, SpiceParam modelparams)
        {
            string code = GetDeviceCode(deviceparams, modelparams);
            code = GetDefaultValues(code, DeviceDefaultValues);
            code = GetNodes(code);
            code = GetDeviceStates(code);

            // Find temperature stuff
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTtemp", "ckt.State.Temperature");
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTnomTemp", "ckt.State.NominalTemperature");

            // -> Is never possible, so let's go for dots
            code = Regex.Replace(code, @"\s*\-\>\s*", ".");

            return Code.Format(code);
        }

        /// <summary>
        /// Get the default values
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="defaults">The dictionary that should get the default values</param>
        /// <returns></returns>
        private string GetDefaultValues(string code, Dictionary<string, string> defaults)
        {
            // Find any default parameters
            Regex defdecl = new Regex(@"if\s*\(\s*\!\s*(?<var>\w+)\.Given\s*\)\s*\{?\s*\k<var>.Value\s*\=\s*(?<value>[^;]+);\s*\}?");
            code = defdecl.Replace(code, (Match m) =>
            {
                string var = m.Groups["var"].Value;
                string value = m.Groups["value"].Value.Trim();

                // Allowed
                bool allowed = false;
                if (Regex.IsMatch(value, @"^true|false$", RegexOptions.IgnoreCase)) // Booleans
                    allowed = true;
                if (Regex.IsMatch(value, @"^[\+\-]?\d+(\.\d*)?([eE][\+\-]?\d+)?$")) // Integer and Double without decimal
                    allowed = true;
                if (Regex.IsMatch(value, @"^[\+\-]?\.\d+([eE][\+\-]?\d+)?$")) // Rare case of doubles of the form '.5'
                    allowed = true;
                if (Regex.IsMatch(value, @"^""([^""]|\\"")*""$")) // Strings
                    allowed = true;

                // Don't replace
                if (allowed)
                {
                    // Remove values that are already the default
                    if (Regex.IsMatch(value, @"^(0+(\.0*)?|\.0+)$"))
                        return "";
                    if (Regex.IsMatch(value, "false", RegexOptions.IgnoreCase))
                        return "";
                    defaults.Add(var, value);
                    return "";
                }
                return m.Value;
            });
            return code;
        }

        /// <summary>
        /// Extract the node variables and matrix nodes
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        private string GetNodes(string code)
        {
            Regex mm = new Regex(@"TSTALLOC\s*\(\s*(?<mat>\w+)\s*,\s*(?<r>\w+)\s*,\s*(?<c>\w+)\s*\)");
            code = mm.Replace(code, (Match m) =>
            {
                string r = m.Groups["r"].Value;
                string c = m.Groups["c"].Value;

                // Store them
                Nodes.Add(r);
                Nodes.Add(c);
                MatrixNodes.Add(m.Groups["mat"].Value, new Tuple<string, string>(r, c));
                return "";
            });

            // Dynamically created nodes
            Regex dn = new Regex($@"(?<error>\w+)\s*\=\s*CKTmkVolt\s*\(\s*\w+\s*,\s*\&\s*(?<tmp>\w+)[^\)]+\);\s*if\s*\(\k<error>\s*\)\s*return\s*\(\s*\k<error>\s*\)\;\s*(?<node>\w+)\s*\=\s*\k<tmp>\s*\-\>number\s*;");
            code = dn.Replace(code, (Match m) => m.Groups["node"].Value + " = CreateNode(ckt).Index;");
            return code;
        }

        /// <summary>
        /// Get the device states
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string GetDeviceStates(string code)
        {
            Regex sr = new Regex($@"(?<!\w)(?<var>\w+)\s*\=\s*\*\s*{states}\s*;\s*\*\s*{states}\s*\+\s*\=\s*(?<count>\w+)\s*;");
            code = sr.Replace(code, (Match m) =>
            {
                // Save the variable
                StatesVariable = m.Groups["var"].Value;
                return "";
            });
            return code;
        }
    }
}
