using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// Spice parameter
    /// </summary>
    public class SpiceParam
    {
        /// <summary>
        /// Get the declarations of the parameters
        /// </summary>
        public Dictionary<string, string> Declarations { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Get methods that are not meant to be referenced in code
        /// </summary>
        public List<string> Methods { get; } = new List<string>();

        /// <summary>
        /// Get the device variables
        /// </summary>
        public HashSet<string> Variables { get; } = new HashSet<string>();

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, string> param = new Dictionary<string, string>();
        private Dictionary<string, string> ask = new Dictionary<string, string>();
        private List<string> par_parameters = new List<string>();
        private List<string> ask_parameters = new List<string>();
        private string par_here, ask_here, par_value, ask_value;

        /// <summary>
        /// Get the parameter by its "Given" parameter
        /// </summary>
        public Dictionary<string, string> GivenVariable { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The device</param>
        public SpiceParam(SpiceDevice dev, Dictionary<string, ParameterExtractor.DeviceParameter> dp, SpiceDevice.Methods p, SpiceDevice.Methods a)
        {
            // Get the param method
            string content = dev.GetMethod(p);
            Code.GetSwitchCases(content, param);
            Code.GetMethodParameters(content, par_parameters, dev.GetMethodName(p));

            // Get the "here" variable
            var m = Regex.Match(content, $@"(\w+)\s*\*\s*(?<var>\w+)\s*\=\s*\(\s*\1\s*\*\s*\)\s*{par_parameters[2]}");
            if (m.Success)
                par_here = m.Groups["var"].Value;
            else
                throw new Exception("Could not find 'here' parameter");
            par_value = par_parameters[1];

            // Get the ask method
            content = dev.GetMethod(a);
            Code.GetSwitchCases(content, ask);
            Code.GetMethodParameters(content, ask_parameters, dev.GetMethodName(a));

            // Get the "here" variable
            m = Regex.Match(content, $@"(\w+)\s*\*\s*(?<var>\w+)\s*\=\s*\(\s*\1\s*\*\s*\)\s*{ask_parameters[1]}");
            if (m.Success)
                ask_here = m.Groups["var"].Value;
            else
                throw new Exception("Could not find 'here' parameter");
            ask_value = ask_parameters[3];

            BuildParameters(dp);
        }

        /// <summary>
        /// Build the list of parameters
        /// </summary>
        /// <param name="param"></param>
        /// <param name="ask"></param>
        /// <param name="declarations"></param>
        /// <param name="variables"></param>
        private void BuildParameters(Dictionary<string, ParameterExtractor.DeviceParameter> ps)
        {
            // First make a list of all possible ID's
            HashSet<string> ids = new HashSet<string>();
            foreach (var k in param.Keys)
                ids.Add(k);
            foreach (var k in ask.Keys)
                ids.Add(k);
            List<string> attr = new List<string>();
            List<string> decl = new List<string>();

            // Build a declaration for each
            foreach (var id in ids)
            {
                // Get the declarations
                string param_decl = param.ContainsKey(id) ? Code.RemoveComments(param[id]).Trim() : null;
                string ask_decl = ask.ContainsKey(id) ? Code.RemoveComments(ask[id]).Trim() : null;
                if (!ps.ContainsKey(id))
                {
                    // Warning!
                    ConverterWarnings.Add($"Could not find definition for ID '{id}'");
                    continue;
                }
                var info = ps[id];
                string type = GetTypeByFlag(info.FlagType);
                string name = null;

                // Build the attributes
                attr.Clear();
                foreach (var n in info.Names)
                    attr.Add($"SpiceName(\"{n}\")");
                attr.Add($"SpiceInfo(\"{info.Description}\")");
                List<string> declaration = new List<string>();
                decl.Clear();
                decl.Add($"[{string.Join(", ", attr)}]");

                // Create a declaration
                if (param_decl != null && ask_decl != null)
                {
                    // Let's hope it's a Parameter<>
                    string paramGet, paramGiven;
                    string[] multiS, multiG;
                    bool defSet = IsDefaultParameterSet(param_decl, out name, out paramGiven);
                    bool defGet = IsDefaultGet(ask_decl, out paramGet);
                    if (defSet && defGet && name == paramGet)
                    {
                        GivenVariable.Add(paramGiven, name);
                        switch (type.ToLower())
                        {
                            case "double":
                            case "int":
                                decl.Add($"public Parameter {name} {{ get; }} = new Parameter();");
                                break;
                            default:
                                decl.Add($"public Parameter<{type}> {name} {{ get; }} = new Parameter<{type}>();");
                                break;
                        }
                    }
                    else if (IsDefaultSet(param_decl, out name) && defGet && name == paramGet)
                        decl.Add($"public {type} {name} {{ get; set; }}");
                    else if (AnySet(ref param_decl, out multiS) && AnyGet(ask_decl, out multiG))
                    {
                        decl.AddRange(new string[] { $"public {type} {id}",
                            "{",
                            "get", "{", fmtMethod(ask_decl), "}",
                            "set", "{", fmtMethod(param_decl), "}",
                            "}" });

                        // Find the first common one
                        if (multiS[0] == multiG[0])
                        {
                            name = multiS[0];
                            if (GivenVariable.ContainsValue(name))
                            {
                                switch (type.ToLower())
                                {
                                    case "double":
                                    case "int":
                                        decl.Add($"public Parameter {name} {{ get; }} = new Parameter();");
                                        break;
                                    default:
                                        decl.Add($"public Parameter<{type}> {name} = new Parameter<{type}>();");
                                        break;
                                }
                            }
                            else
                                decl.Add($"private {type} {name};");
                        }
                    }
                    else
                    {
                        ConverterWarnings.Add($"Could not process ID '{id}'");
                        continue;
                    }
                }
                else if (param_decl != null)
                {
                    // Check for a default anyway, Spice mistakes sometimes...
                    string given;
                    if (IsDefaultParameterSet(param_decl, out name, out given))
                    {
                        GivenVariable.Add(given, name);
                        switch (type.ToLower())
                        {
                            case "double":
                            case "int":
                                decl.Add($"public Parameter {name} {{ get;}} = new Parameter();");
                                break;
                            default:
                                decl.Add($"public Parameter<{type}> {name} {{ get; }} = new Parameter<{type}>();");
                                break;
                        }
                    }
                    else if (IsDefaultSet(param_decl, out name))
                        decl.Add($"public {type} {name} {{ get; set; }}");
                    else
                    {
                        string[] names;
                        AnySet(ref param_decl, out names);
                        decl.AddRange(new string[] { $"public void Set{id}({type} value)", "{", fmtMethod(param_decl), "}" });
                    }
                }
                else if (ask_decl != null)
                {
                    if (IsDefaultGet(ask_decl, out name))
                        decl.Add($"public {type} {name} {{ get; private set; }}");
                    else
                        decl.AddRange(new string[] { $"public {type} Get{id}(Circuit ckt)", "{", fmtMethod(ask_decl), "}" });
                }
                else
                    throw new Exception("Invalid declaration(?)");

                // Store
                if (name != null)
                {
                    Variables.Add(name);
                    Declarations.Add(name, string.Join(Environment.NewLine, decl));
                }
                else
                    Methods.Add(string.Join(Environment.NewLine, decl));
            }
        }

        /// <summary>
        /// Check if the code describes a default parameter set of the form
        /// myvalue = value;
        /// myvalueGiven = TRUE;
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool IsDefaultParameterSet(string code, out string param, out string given)
        {
            Regex psr = new Regex($@"^{par_here}\s*\-\>\s*(?<var>\w+)\s*\=\s*{par_value}\s*\-\>\s*[ris]Value\s*;\s*{par_here}\s*\-\>\s*(?<given>\w+Given)\s*\=\s*TRUE\s*;\s*(return\s*\(\s*OK\s*\);|break\s*;)\s*$");
            param = null;
            given = null;

            var m = psr.Match(code);
            if (m.Success)
            {
                param = m.Groups["var"].Value;
                given = m.Groups["given"].Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the code describes a default set of the form
        /// myvalue = value;
        /// </summary>
        /// <param name="code"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsDefaultSet(string code, out string param)
        {
            Regex sr = new Regex($@"^{par_here}\s*\-\>(?<var>\w+)\s*\=\s*{par_value}\s*\-\>\s*[ris]Value\s*;\s*(return\s*\(\s*OK\s*\);|break\s*;)\s*$");
            param = null;

            var m = sr.Match(code);
            if (m.Success)
            {
                param = m.Groups["var"].Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempt to find any variables that are assigned in the code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool AnySet(ref string code, out string[] p)
        {
            Regex psr = new Regex($@"{par_here}\s*\-\>\s*(?<var>\w+)\s*\=\s*(?<value>[^;]+);\s*{par_here}\s*\-\>\s*(?<given>\w+Given)\s*\=\s*TRUE\s*;", RegexOptions.Multiline);
            Regex asr = new Regex($@"{par_here}\s*\-\>\s*(?<var>\w+)\s*\=\s*[^;]+;");
            HashSet<string> vars = new HashSet<string>();

            // Find any variable assignments
            code = psr.Replace(code, (Match m) =>
            {
                vars.Add(m.Groups["var"].Value);
                if (!GivenVariable.ContainsKey(m.Groups["given"].Value))
                    GivenVariable.Add(m.Groups["given"].Value, m.Groups["var"].Value);
                return $"{m.Groups["var"].Value}.Set({m.Groups["value"].Value});";
            });

            // Find any loose variables
            var ms = asr.Matches(code);
            foreach (Match m in ms)
            {
                if (!GivenVariable.ContainsKey(m.Groups["var"].Value))
                    vars.Add(m.Groups["var"].Value);
            }

            p = vars.ToArray();
            return vars.Count > 0;
        }

        /// <summary>
        /// Check if the code describes a default parameter get of the form
        /// value->rValue = myvalue;
        /// </summary>
        /// <param name="code"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private bool IsDefaultGet(string code, out string param)
        {
            Regex sar = new Regex($@"^{ask_value}\s*\-\>\s*([ris]Value)\s*\=\s*{ask_here}\s*\-\>\s*(?<var>\w+)\s*;\s*(return\s*\(\s*OK\s*\);|break\s*;)\s*$");
            param = null;

            var m = sar.Match(code);
            if (m.Success)
            {
                param = m.Groups["var"].Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to find any variable assigned to the output in the code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool AnyGet(string code, out string[] p)
        {
            Regex agr = new Regex($@"\=[^;]*{ask_here}\s*\-\>\s*(?<var>\w+)[^;]*;");
            HashSet<string> vars = new HashSet<string>();

            var ms = agr.Matches(code);
            foreach (Match m in ms)
            {
                vars.Add(m.Groups["var"].Value);
            }

            p = vars.ToArray();
            return vars.Count > 0;
        }

        /// <summary>
        /// Get the type by its flag
        /// </summary>
        /// <param name="flagtype">The flag for the type</param>
        /// <returns></returns>
        private string GetTypeByFlag(string flagtype)
        {
            switch (flagtype.ToUpper())
            {
                case "IF_REAL": return "double";
                case "IF_FLAG": return "bool";
                case "IF_COMPLEX": return "Complex";
                case "IF_STRING": return "string";
                case "IF_REALVEC": return "double[]";
                case "IF_INTEGER": return "int";
                case "IF_VECTOR": return "double[]";
                default:
                    throw new Exception($"Could not recognized type flag '{flagtype}'");
            }
        }

        /// <summary>
        /// Format ask and 
        /// </summary>
        /// <param name="ask"></param>
        /// <param name="par"></param>
        private string fmtMethod(string decl)
        {
            decl = Regex.Replace(decl, $@"{par_here}\s*\-\>\s*", "");
            decl = Regex.Replace(decl, $@"{par_value}\s*\-\>\s*[irs]Value", "value");

            // Remove the last break/return statements
            decl = Regex.Replace(decl, @"break\s*;\s*$", "", RegexOptions.IgnoreCase);
            decl = Regex.Replace(decl, @"return\s*\(\s*ok\s*\)\s*;\s*$", "", RegexOptions.IgnoreCase);

            return Code.Format(decl);
        }

        /// <summary>
        /// Update methods
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="setup"></param>
        public void UpdateMethods(SpiceDevice dev, SpiceSetup setup, string ckt = "ckt")
        {
            // Get-set
            string[] keys = Declarations.Keys.ToArray();
            foreach (var key in keys)
            {
                string code = Regex.Replace(Declarations[key], $@"get\s*\{{\s*value\s*\=\s*\w+\s*\-\>\s*(?<var>\w+)\s*;\s*\}}", (Match m) => $"get => {m.Groups["var"].Value};", RegexOptions.Multiline);
                Declarations[key] = code;
            }

            // Methods
            for (int i = 0; i < Methods.Count; i++)
            {
                string code = Regex.Replace(Methods[i], 
                    $@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTstate(?<state>\d+)\s*\+\s*(?<node>\w+)\s*\)",
                    (Match m) => $"ckt.State.States[{m.Groups["state"].Value}][{setup.StatesVariable} + {m.Groups["node"].Value}]");
                code = Regex.Replace(code, dev.DeviceName + "_", "", RegexOptions.IgnoreCase);
                code = Regex.Replace(code, $@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTrhsOld\s*\+\s*(?<node>\w+)\s*\)", (Match m) => $"ckt.State.Real.Solution[{m.Groups["node"].Value}]");

                // If it just a simple setter, then use the shorthand notation
                Regex simpleset = new Regex(@"\s*\{\s*value\s*\=\s*(?<value>[^;]+);(\s*return\s*\(\s*OK\s*\)\s*;)?\s*\}\s*$");
                code = simpleset.Replace(code, (Match m) => $" => {m.Groups["value"].Value};");

                Methods[i] = code;
            }
        }
    }
}
