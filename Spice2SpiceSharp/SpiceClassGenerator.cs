using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class can export the device and model class
    /// </summary>
    public class SpiceClassGenerator
    {
        [Flags]
        public enum Methods
        {
            Setup = 0x01,
            Temperature = 0x02,
            Load = 0x04,
            AcLoad = 0x08,
            PzLoad = 0x10,

            None = 0x00,
            All = 0x1F
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private string setupDev, setupMod;
        private string tempDev, tempMod;
        private string loadDev, loadMod;
        private string acloadDev, acloadMod;
        private string pzloadDev, pzloadMod;
        private SpiceParam paramDev, paramMod;
        private ParameterExtractor paramExtr;
        private string name;

        private SpiceSetup setup;
        private SpiceDefinitions definitions;
        private SpiceTemperature temp;
        private SpiceLoad load;
        private SpiceAcLoad acload;
        private SpicePzLoad pzload;

        private string offset = "";
        private Dictionary<string, string> shared = new Dictionary<string, string>();
        private HashSet<string> modelextra = new HashSet<string>();
        private HashSet<string> deviceextra = new HashSet<string>();

        private const int MaxLineLength = 128;

        /// <summary>
        /// Use the PzLoad function instead of the AcLoad analysis
        /// </summary>
        public bool UsePzForAc { get; set; } = false;

        /// <summary>
        /// Read a device
        /// </summary>
        /// <param name="dev"></param>
        public SpiceClassGenerator(SpiceDevice dev, Methods export = Methods.All)
        {
            // Get all model and device methods
            dev.BuildMethodTable();
            name = dev.DeviceName.ToUpper();

            // Extract the parameters
            paramExtr = new ParameterExtractor();
            paramExtr.Extract(dev);

            // Get all parameters
            paramMod = new SpiceParam(dev, paramExtr.Model, SpiceDevice.Methods.ModelParam, SpiceDevice.Methods.ModelAsk);
            paramDev = new SpiceParam(dev, paramExtr.Device, SpiceDevice.Methods.Param, SpiceDevice.Methods.Ask);

            // Extract the setup
            if (export.HasFlag(Methods.Setup))
            {
                setup = new SpiceSetup(dev);
                setupDev = setup.ExportDevice(paramMod, paramDev);
                setupMod = setup.ExportModel(paramMod);
                foreach (var v in setup.SharedLocalVariables)
                    shared.Add(v.Key, v.Value);
                foreach (var v in setup.ModelVariablesExtra)
                    modelextra.Add(v);
                foreach (var v in setup.DeviceVariablesExtra)
                    deviceextra.Add(v);

                // Update the methods
                paramMod.UpdateMethods(dev, setup);
                paramDev.UpdateMethods(dev, setup);
            }

            // Extract the state definitions
            if (setup != null)
            {
                definitions = new SpiceDefinitions(dev, setup);
                foreach (var v in definitions.StateNames)
                    paramDev.Variables.Add(v);
            }

            // Temperature-dependent calculations
            if (export.HasFlag(Methods.Temperature))
            {
                temp = new SpiceTemperature(dev);
                tempDev = temp.ExportDevice(paramMod, paramDev);
                tempMod = temp.ExportModel(paramMod);
                foreach (var v in temp.SharedLocalVariables)
                {
                    if (shared.ContainsKey(v.Key) && shared[v.Key] != v.Value)
                        throw new Exception($"Cannot share variable {v.Key}");
                    shared.Add(v.Key, v.Value);
                }
                foreach (var v in temp.ModelVariablesExtra)
                    modelextra.Add(v);
                foreach (var v in temp.DeviceVariablesExtra)
                    deviceextra.Add(v);
            }

            // Loading
            if (export.HasFlag(Methods.Load) && setup != null)
            {
                load = new SpiceLoad(dev, setup);
                loadDev = load.ExportDevice(paramMod, paramDev);
                loadMod = load.ExportModel(paramMod);
                foreach (var v in load.SharedLocalVariables)
                {
                    if (shared.ContainsKey(v.Key) && shared[v.Key] != v.Value)
                        throw new Exception($"Cannot share variable {v.Key}");
                    shared.Add(v.Key, v.Value);
                }
                foreach (var v in load.ModelVariablesExtra)
                    modelextra.Add(v);
                foreach (var v in load.DeviceVariablesExtra)
                    deviceextra.Add(v);
            }

            // AC loading
            if (export.HasFlag(Methods.AcLoad) && setup != null)
            {
                acload = new SpiceAcLoad(dev, setup);
                acloadDev = acload.ExportDevice(paramMod, paramDev);
                acloadMod = acload.ExportModel(paramMod);
                foreach (var v in acload.SharedLocalVariables)
                {
                    if (shared.ContainsKey(v.Key) && shared[v.Key] != v.Value)
                        throw new Exception($"Cannot share variable {v.Key}");
                    shared.Add(v.Key, v.Value);
                }
                foreach (var v in acload.ModelVariablesExtra)
                    modelextra.Add(v);
                foreach (var v in acload.DeviceVariablesExtra)
                    deviceextra.Add(v);
            }

            // PZ loading
            if (export.HasFlag(Methods.PzLoad))
            {
                pzload = new SpicePzLoad(dev, setup);
                pzloadDev = pzload.ExportDevice(paramMod, paramDev);
                pzloadMod = pzload.ExportModel(paramMod);
                foreach (var v in pzload.SharedLocalVariables)
                {
                    if (shared.ContainsKey(v.Key) && shared[v.Key] != v.Value)
                        throw new Exception($"Cannot share variable {v.Key}");
                    shared.Add(v.Key, v.Value);
                }
                foreach (var v in pzload.ModelVariablesExtra)
                    modelextra.Add(v);
                foreach (var v in pzload.DeviceVariablesExtra)
                    deviceextra.Add(v);
            }

            // Apply default values!
            string[] names = paramMod.Declarations.Keys.ToArray();
            foreach (string n in names)
            {
                // Find out if the name of the variable with this ID
                if (setup.ModelDefaultValues.ContainsKey(n))
                    paramMod.Declarations[n] = Regex.Replace(paramMod.Declarations[n], @"\(\);", $"({setup.ModelDefaultValues[n]});");
            }
            names = paramDev.Declarations.Keys.ToArray();
            foreach (string n in names)
            {
                // Find out if the name of the variable with this ID
                if (setup.DeviceDefaultValues.ContainsKey(n))
                    paramDev.Declarations[n] = Regex.Replace(paramDev.Declarations[n], @"\(\);", $"({setup.DeviceDefaultValues[n]});");
            }
        }

        /// <summary>
        /// Write the filename
        /// </summary>
        /// <param name="filename"></param>
        public void ExportModel(string filename)
        {
            offset = "";
            using (StreamWriter sw = new StreamWriter(filename))
            {
                // Write dependencies
                WriteCode(sw, "using System;", "using SpiceSharp.Circuits;", "using SpiceSharp.Diagnostics;", "using SpiceSharp.Parameters;");

                // Write namespace and class
                WriteCode(sw, "", "namespace SpiceSharp.Components", "{");
                WriteCode(sw, $"public class {name}Model : CircuitModel<{name}Model>", "{");
                WriteCode(sw, "/// <summary>", "/// Register our model parameters", "/// </summary>");
                WriteCode(sw, $"static {name}Model()", "{", "Register();", "}", "");

                // Write the model parameters
                WriteCode(sw, "/// <summary>", "/// Parameters", "/// </summary>");
                foreach (string decl in paramMod.Declarations.Values)
                    WriteCode(sw, decl);

                // Write the model methods
                if (paramMod.Methods.Count > 0)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Methods", "/// </summary>");
                    foreach (string method in paramMod.Methods)
                        WriteCode(sw, method);
                }

                // Shared variables
                WriteCode(sw, "", "/// <summary>", "/// Shared parameters", "/// </summary>");
                foreach (var v in shared)
                    WriteCode(sw, $"public {v.Value} {v.Key} {{ get; private set; }}");

                // Extra variables
                WriteCode(sw, "", "/// <summary>", "/// Extra variables", "/// </summary>");
                foreach (var v in modelextra)
                {
                    if (paramMod.GivenVariable.ContainsValue(v))
                        WriteCode(sw, $"public Parameter<double> {v} {{ get; }} = new Parameter<double>();");
                    else
                        WriteCode(sw, $"public double {v} {{ get; private set; }}");
                }

                // Write the constructor
                WriteCode(sw, "", "/// <summary>", "/// Constructor", "/// </summary>", "/// <param name=\"name\">The name of the device</param>");
                WriteCode(sw, $"public {name}Model(string name) : base(name)", "{", "}");

                // Setup method
                WriteCode(sw, "", "/// <summary>", "/// Setup the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Setup(Circuit ckt)", "{");
                foreach (string var in setup.ModelVariables.Keys)
                    WriteCode(sw, $"{setup.ModelVariables[var]} {var};");
                WriteCode(sw, "", setupMod, "}");

                // Temperature method
                WriteCode(sw, "", "/// <summary>", "/// Do temperature-dependent calculations", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Temperature(Circuit ckt)", "{");
                foreach (string var in temp.ModelVariables.Keys)
                    WriteCode(sw, $"{temp.ModelVariables[var]} {var};");
                WriteCode(sw, "", tempMod, "}");

                // Load method
                WriteCode(sw, "", "/// <summary>", "/// Load the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Load(Circuit ckt)", "{");
                foreach (string var in load.ModelVariables.Keys)
                    WriteCode(sw, $"{load.ModelVariables[var]} {var};");
                WriteCode(sw, "", loadMod, "}");

                // End class and namespace
                WriteCode(sw, "}", "}");
            }
        }

        /// <summary>
        /// Export the device code to a file
        /// </summary>
        /// <param name="filename">The filename</param>
        public void ExportDevice(string filename)
        {
            offset = "";
            using (StreamWriter sw = new StreamWriter(filename))
            {
                // Write dependencies
                WriteCode(sw, "using System;", "using SpiceSharp.Circuits;", "using SpiceSharp.Diagnostics;", "using SpiceSharp.Parameters;", "using System.Numerics;");

                // Write namespace and class
                WriteCode(sw, "", "namespace SpiceSharp.Components", "{");
                WriteCode(sw, $"public class {name} : CircuitComponent<{name}>", "{");

                WriteCode(sw, "/// <summary>", "/// Register our device parameters and terminals", "/// </summary>");
                WriteCode(sw, $"static {name}()", "{", "Register();", "terminals = new string[] {};", "}", "");

                // Write device declaration
                WriteCode(sw, "/// <summary>", "/// Gets or sets the device model", "/// </summary>");
                WriteCode(sw, $"public void SetModel({name}Model model) => Model = ({name}Model)model;");

                // Write the device parameters
                WriteCode(sw, "", "/// <summary>", "/// Parameters", "/// </summary>");
                foreach (string decl in paramDev.Declarations.Values)
                    WriteCode(sw, decl);

                // Write the device methods
                if (paramDev.Methods.Count > 0)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Methods", "/// </summary>");
                    foreach (string method in paramDev.Methods)
                        WriteCode(sw, method);
                }

                // Write extra device parameters
                WriteCode(sw, "", "/// <summary>", "/// Extra variables", "/// </summary>");
                foreach (var v in deviceextra)
                {
                    if (setup != null)
                    {
                        if (setup.Nodes.Contains(v))
                            continue;
                        if (setup.StatesVariable == v)
                            continue;
                    }
                    if (paramDev.GivenVariable.ContainsValue(v))
                        WriteCode(sw, $"public Parameter<double> {v} {{ get; }} = new Parameter<double>();");
                    else
                        WriteCode(sw, $"public double {v} {{ get; private set; }}");
                }

                // Write nodes not yet defined
                foreach (var v in setup.Nodes)
                {
                    if (!paramDev.Variables.Contains(v))
                        WriteCode(sw, $"public int {v} {{ get; private set; }}");
                }
                if (!paramDev.Variables.Contains(setup.StatesVariable))
                    WriteCode(sw, $"public int {setup.StatesVariable} {{ get; private set; }}");

                // Write device constants
                WriteCode(sw, "", "/// <summary>", "/// Constants", "/// </summary>");
                foreach (var state in definitions.States)
                    WriteCode(sw, $"private const int {state.Item2} = {state.Item1};");

                // Write the constructor
                WriteCode(sw, "", "/// <summary>", "/// Constructor", "/// </summary>", "/// <param name=\"name\">The name of the device</param>");
                WriteCode(sw, $"public {name}(string name) : base(name)", "{", "}");

                // Setup method
                if (setup != null)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Setup the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                    WriteCode(sw, "public override void Setup(Circuit ckt)", "{");
                    WriteCode(sw, $"var model = Model as {name}Model;");
                    foreach (string var in setup.DeviceVariables.Keys)
                        WriteCode(sw, $"{setup.DeviceVariables[var]} {var};");
                    WriteCode(sw, "", "// Allocate nodes");
                    WriteCode(sw, "var nodes = BindNodes(ckt);");
                    int index = 0;
                    foreach (var n in setup.Nodes)
                        WriteCode(sw, $"{n} = nodes[{index++}].Index;");
                    WriteCode(sw, "", "// Allocate states", $"{setup.StatesVariable} = ckt.State.GetState({definitions?.Count.ToString() ?? ""});");
                    WriteCode(sw, "", setupDev, "}");
                }

                // Temperature method
                if (temp != null)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Do temperature-dependent calculations", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                    WriteCode(sw, "public override void Temperature(Circuit ckt)", "{");
                    WriteCode(sw, $"var model = Model as {name}Model;");
                    foreach (string var in temp.DeviceVariables.Keys)
                        WriteCode(sw, $"{temp.DeviceVariables[var]} {var};");
                    WriteCode(sw, "", tempDev, "}");
                }

                // Load method
                if (load != null)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Load the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                    WriteCode(sw, "public override void Load(Circuit ckt)", "{");
                    WriteCode(sw, $"var model = Model as {name}Model;", "var state = ckt.State;", "var rstate = state.Real;", "var method = ckt.Method;");
                    foreach (string var in load.DeviceVariables.Keys)
                        WriteCode(sw, $"{load.DeviceVariables[var]} {var};");
                    WriteCode(sw, "", loadDev, "}");
                }

                // AcLoad method
                if (acload != null && !UsePzForAc)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Load the device for AC simulation", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                    WriteCode(sw, "public override void AcLoad(Circuit ckt)", "{");
                    WriteCode(sw, $"var model = Model as {name}Model;", "var state = ckt.State;", "var cstate = state.Complex;");
                    foreach (string var in acload.DeviceVariables.Keys)
                        WriteCode(sw, $"{acload.DeviceVariables[var]} {var};");
                    WriteCode(sw, "", acloadDev, "}");
                }
                
                // PzLoad method
                if (pzload != null && UsePzForAc)
                {
                    WriteCode(sw, "", "/// <summary>", "/// Load the device for AC simulation", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                    WriteCode(sw, "public override void AcLoad(Circuit ckt)", "{");
                    WriteCode(sw, $"var model = Model as {name}Model;", "var state = ckt.State;", "var cstate = state.Complex;");
                    foreach (string var in pzload.DeviceVariables.Keys)
                        WriteCode(sw, $"{pzload.DeviceVariables[var]} {var};");
                    WriteCode(sw, "", pzloadDev, "}");
                }

                // End class and namespace
                WriteCode(sw, "}", "}");
            }
        }

        /// <summary>
        /// Write lines of code
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="code"></param>
        private void WriteCode(StreamWriter sw, params string[] code)
        {
            foreach (var c in code)
                Indent(sw, c);
        }

        /// <summary>
        /// Indent code
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        private void Indent(StreamWriter sw, string code)
        {
            // Remove indents
            code = Regex.Replace(code, @"(?<cr>[\r\n]+)[\t ]+", (Match m) => m.Groups["cr"].Value);

            // All lines
            string[] lines = code.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                // Find any curly closing brackets
                int closing = 0;
                int opening = 0;
                for (int k = 0; k < lines[i].Length; k++)
                {
                    if (lines[i][k] == '}')
                        closing++;
                    if (lines[i][k] == '{')
                        opening++;
                }

                // Remove offsets
                if (closing > 0 && opening == 0)
                    offset = offset.Substring(0, Math.Max(0, offset.Length - closing));

                // Write the line
                sw.WriteLine(offset + multiline(lines[i]));

                // Add offsets
                if (opening > 0 && closing == 0)
                    offset = offset + new string('\t', opening);
            }
        }

        /// <summary>
        /// Turn long lines into multiple lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string multiline(string line)
        {
            // Don't process short lines
            if (line.Length < MaxLineLength)
                return line;

            // Allowed newline placement
            Regex opr = new Regex(@"\&\&|\|\||\*|\/|\+|\-|,");
            var ms = opr.Matches(line);
            List<int> ss = new List<int>();
            foreach (Match m in ms)
                ss.Add(m.Index + m.Length);

            // Divide the lines in smaller lines
            List<string> result = new List<string>();
            int r = 0;
            while (line.Length > MaxLineLength && ss.Count > 0)
            {
                // Find the index where we have to 
                int index = 0;
                while (ss.Count > 0)
                {
                    index = ss[0] - r;
                    ss.RemoveAt(0);
                    if (ss.Count == 0 || ss[0] - r > MaxLineLength)
                        break;
                }

                // Split the line and start again
                result.Add(line.Substring(0, index));
                line = line.Substring(index);
                r += index;
            }
            result.Add(line);

            return string.Join(Environment.NewLine + offset + "\t", result);
        }
    }
}
