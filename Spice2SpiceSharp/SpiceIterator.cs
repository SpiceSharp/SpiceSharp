using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class allows for processing methods that iterate over all device models and instances
    /// </summary>
    public abstract class SpiceIterator
    {
        /// <summary>
        /// The definitions before starting the iteration
        /// </summary>
        protected string Definition { get; private set; }

        /// <summary>
        /// The code run for each model but not for each instance
        /// </summary>
        protected string ModelCode { get; private set; }

        /// <summary>
        /// Get the parameter used for the iteration
        /// </summary>
        protected string ModelParameter { get; private set; }

        /// <summary>
        /// The code run for each instance
        /// </summary>
        protected string DeviceCode { get; private set; }

        /// <summary>
        /// Get the parameter used for the model iteration
        /// </summary>
        protected string DeviceParameter { get; private set; }

        /// <summary>
        /// Gets the set of model variables
        /// </summary>
        public Dictionary<string, string> ModelVariables { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the extra model variables that are not supposed to be edited by the user
        /// </summary>
        public HashSet<string> ModelVariablesExtra { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the set of device variables
        /// </summary>
        public Dictionary<string, string> DeviceVariables { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Get the extra device variables that are not supposed to be edited by the user
        /// </summary>
        public HashSet<string> DeviceVariablesExtra { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the set of model variables that are also used by devices
        /// </summary>
        public Dictionary<string, string> SharedLocalVariables { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Methods that need to be replaced
        /// </summary>
        private static Dictionary<string, string> replaceMethods = new Dictionary<string, string>()
        {
            { "log", "Math.Log" },
            { "exp", "Math.Exp" },
            { "FABS", "Math.Abs" },
            { "MAX", "Math.Max" },
            { "MIN", "Math.Min" },
            { "sqrt", "Math.Sqrt" },
            { "pow", "Math.Pow" },
            { "sin", "Math.Sin" },
            { "cos", "Math.Cos" },
            { "tan", "Math.Tan" },
            { "atan", "Math.Atan" },
            { "atan2", "Math.Atan2" }
        };
        private static Dictionary<string, string> replaceCircuitConstants = new Dictionary<string, string>()
        {
            { "REFTEMP", "CONSTRefTemp" },
            { "CHARGE", "CHARGE" },
            { "CONSTCtoK", "CONSTCtoK" },
            { "CONSTboltz", "CONSTBoltz" },
            { "CONSTroot2", "CONSTroot2" },
            { "CONSTvt0", "CONSTvt0" },
            { "CONSTKoverQ", "CONSTKoverQ" },
            { "CONSTE", "CONSTE" },
            { "CONSTPI", "CONSTPI" }
        };

        /// <summary>
        /// Read the method in which is iterated over all device models and instances and seperate the code parts
        /// The results are stored in the variables
        /// </summary>
        /// <param name="method">The full method definition</param>
        protected void ReadMethod(string method)
        {
            // Get the model loop content
            Regex modelloop = new Regex(@"for\s*\(\s*;\s*(?<var>\w+)\s*\!\=\s*NULL\s*;\s*\k<var>\s*\=\s*\k<var>\s*\-\>\s*\w+\s*\)\s*{");
            var m = modelloop.Match(method);
            if (m.Success)
            {
                string content = Code.GetMatchingBlock(method, m.Index + m.Length - 1);
                string model = m.Groups["var"].Value;

                // Get the instance loop content
                Regex instanceloop = new Regex(@"for\s*\(\s*(?<var>\w+)\s*\=\s*" + model + @"\s*\-\>\s*\w+\s*;\s*\k<var>\s*\!\=\s*NULL\s*;\s*\k<var>\s*\=\s*\k<var>\s*\-\>\s*\w+\s*\)\s*{");
                var m2 = instanceloop.Match(content);
                if (m2.Success)
                {
                    int s = m2.Index + m2.Length - 1;
                    int e = Code.GetMatchingParenthesis(content, s);
                    string content2 = content.Substring(s + 1, e - s - 1);

                    // Everything is ready to store!
                    int ms = method.IndexOf('{');
                    Definition = method.Substring(ms + 1, m.Index - ms - 1);
                    ModelCode = content.Substring(0, m2.Index - 1);
                    ModelParameter = m.Groups["var"].Value;
                    DeviceCode = content2;
                    DeviceParameter = m2.Groups["var"].Value;

                    // There is still a part after the device loop that could be used for the loop
                    ModelCode += content.Substring(e + 1);
                }
                else
                    throw new Exception("Could not find instance iterator");
            }
            else
                throw new Exception("Could not find model iterator");

            // Find all the declared variables
            FindSharedVariables();
        }

        /// <summary>
        /// Find all local variables that are declared in the model and used in the device
        /// </summary>
        /// <param name="vars">The set that will receive all the shared variables</param>
        private void FindSharedVariables()
        {
            // Variable assignments that aren't part of an object
            Regex def = new Regex(@"(?<!\w|\-\>)(?<var>\w+)\s*\=\s*[^;]+;");

            // Get all (local) model variables
            var ms = def.Matches(ModelCode);
            foreach (Match m in ms)
            {
                string var = m.Groups["var"].Value;

                // Try to find this variable in the device
                Match b = def.Match(DeviceCode);
                Match c = Regex.Match(DeviceCode, $@"(?<!\-\>\s*){var}(?!\w)");
                if (c.Success)
                {
                    if (!SharedLocalVariables.ContainsKey(var) && b.Success && b.Index != c.Index)
                        SharedLocalVariables.Add(var, GetVariableType(var));
                }
                else if (!ModelVariables.ContainsKey(var))
                    ModelVariables.Add(var, GetVariableType(var));
            }

            // Find all (local) device variables
            ms = def.Matches(DeviceCode);
            foreach (Match m in ms)
            {
                string var = m.Groups["var"].Value;
                if (!DeviceVariables.ContainsKey(var))
                    DeviceVariables.Add(var, GetVariableType(var));
            }
        }

        /// <summary>
        /// Find the type of a local variable
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns></returns>
        private string GetVariableType(string name)
        {
            // Use regular expression to find the type
            Regex decl = new Regex($@"^\s*(?<type>double|int)[^;]+{name}[^;]*;", RegexOptions.Multiline);
            Match m = decl.Match(Definition);
            if (m.Success)
                return m.Groups["type"].Value;
            m = decl.Match(ModelCode);
            if (m.Success)
                return m.Groups["type"].Value;
            m = decl.Match(DeviceCode);
            if (m.Success)
                return m.Groups["type"].Value;

            // Search the main part for the type
            ConverterWarnings.Add($"Could not find type of local variable {name}");
            return "double";
        }

        /// <summary>
        /// Apply parameters
        /// </summary>
        /// <param name="code">The code to be replaced</param>
        /// <param name="sp">The parameters</param>
        /// <param name="leftover">The leftover variables that could not be found</param>
        /// <returns></returns>
        private string ApplyParameters(string code, string obj, SpiceParam sp, HashSet<string> leftover, string prefix = "")
        {
            // The model parameters can be replaced by just their name
            Regex mv = new Regex($@"{obj}\s*\-\>\s*(?<var>\w+)");
            return mv.Replace(code, (Match m) =>
            {
                // Check if the model variable exists
                string var = m.Groups["var"].Value;
                if (sp.GivenVariable.ContainsKey(var))
                    return prefix + sp.GivenVariable[var] + ".Given";
                if (!sp.Variables.Contains(var))
                    leftover.Add(var);
                return prefix + var;
            });
        }

        /// <summary>
        /// Get the device code
        /// </summary>
        /// <param name="mparam">The model parameters</param>
        /// <param name="dparam">The device parameters</param>
        /// <param name="ckt">The circuit identifier</param>
        /// <param name="model">The model identifier</param>
        /// <returns></returns>
        protected string GetDeviceCode(SpiceParam mparam, SpiceParam dparam, string ckt = "ckt", string model = "Model")
        {
            string code = DeviceCode.Trim();
            code = ApplyGeneral(code, ckt);
            code = ApplyParameters(code, DeviceParameter, dparam, DeviceVariablesExtra);
            code = ApplyParameters(code, ModelParameter, mparam, ModelVariablesExtra, model + ".");

            // Replace all shared variables
            foreach (var s in SharedLocalVariables)
                code = Regex.Replace(code, $@"(?<!\w){s.Key}(?!\w)", $"{model}.{s.Key}");

            return code;
        }

        /// <summary>
        /// Get the model code
        /// </summary>
        /// <param name="mparam">The model parameters</param>
        /// <param name="ckt">The circuit identifier</param>
        /// <returns></returns>
        protected string GetModelCode(SpiceParam mparam, string ckt = "ckt")
        {
            string code = ModelCode.Trim();
            code = ApplyGeneral(code, ckt);
            code = ApplyParameters(code, ModelParameter, mparam, ModelVariablesExtra);

            return code;
        }

        /// <summary>
        /// Convert the methods, constants, etc. (eg. math methods)
        /// </summary>
        /// <param name="code">The original code</param>
        /// <returns></returns>
        private string ApplyGeneral(string code, string ckt = "ckt")
        {
            foreach (string m in replaceMethods.Keys)
                code = Regex.Replace(code, $@"(?<!\w){m}\s*\(", replaceMethods[m] + "(");

            foreach (string c in replaceCircuitConstants.Keys)
                code = Regex.Replace(code, $@"(?<!\w){c}(?!\w)", "Circuit." + replaceCircuitConstants[c]);

            code = Regex.Replace(code, $@"{ckt}\s*->\s*CKTgmin", "state.Gmin");

            return code;
        }

        /// <summary>
        /// Export the code for the model
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <returns></returns>
        public abstract string ExportModel(SpiceParam mparams);

        /// <summary>
        /// Export the code for the device
        /// </summary>
        /// <param name="mparams"></param>
        /// <param name="dparams"></param>
        /// <returns></returns>
        public abstract string ExportDevice(SpiceParam mparams, SpiceParam dparams);
    }
}
