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
        /// <summary>
        /// Private variables
        /// </summary>
        private string setupDev, setupMod;
        private string tempDev, tempMod;
        private string loadDev, loadMod;
        private SpiceParam paramDev, paramMod;
        private ParameterExtractor paramExtr;
        private string name;

        private SpiceSetup setup;
        private SpiceDefinitions definitions;
        private SpiceTemperature temp;
        private SpiceLoad load;

        private string offset = "";
        private Dictionary<string, string> shared = new Dictionary<string, string>();
        private HashSet<string> modelextra = new HashSet<string>();
        private HashSet<string> deviceextra = new HashSet<string>();

        /// <summary>
        /// Read a device
        /// </summary>
        /// <param name="dev"></param>
        public SpiceClassGenerator(SpiceDevice dev)
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
            setup = new SpiceSetup(dev);
            setupDev = setup.ExportDevice(paramMod, paramDev);
            setupMod = setup.ExportModel(paramMod);
            foreach (var v in setup.SharedLocalVariables)
                shared.Add(v.Key, v.Value);
            foreach (var v in setup.ModelVariablesExtra)
                modelextra.Add(v);
            foreach (var v in setup.DeviceVariablesExtra)
                deviceextra.Add(v);

            // Extract the state definitions
            definitions = new SpiceDefinitions(dev, setup);
            foreach (var v in definitions.StateNames)
                paramDev.Variables.Add(v);

            // Temperature-dependent calculations
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

            // Loading
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

            // Apply default values!
            string[] names = paramMod.Declarations.Keys.ToArray();
            foreach (string n in names)
            {
                // Find out if the name of the variable with this ID
                if (setup.ModelDefaultValues.ContainsKey(n))
                {
                    // Replace the declaration
                    paramMod.Declarations[n] = Regex.Replace(paramMod.Declarations[n], @"\(\);", $"({setup.ModelDefaultValues[n]});");
                }
            }
        }

        /// <summary>
        /// Write the filename
        /// </summary>
        /// <param name="filename"></param>
        public void ExportModel(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                // Write dependencies
                WriteCode(sw, "using System;", "using SpiceSharp.Circuits;", "using SpiceSharp.Diagnostics;", "using SpiceSharp.Parameters;");

                // Write namespace and class
                WriteCode(sw, "", "namespace SpiceSharp.Components", "{");
                WriteCode(sw, $"public class {name}Model : CircuitModel", "{");

                // Write the model parameters
                WriteCode(sw, "/// <summary>", "/// Parameters", "/// </summary>");
                foreach (string decl in paramMod.Declarations.Values)
                    WriteCode(sw, decl);

                // Shared variables
                WriteCode(sw, "", "/// <summary>", "/// Shared parameters", "/// </summary>");
                foreach (var v in shared)
                    WriteCode(sw, $"public {v.Value} {v.Key} {{ get; private set; }}");

                // Extra variables
                WriteCode(sw, "", "/// <summary>", "/// Extra variables", "/// </summary>");
                foreach (var v in modelextra)
                    WriteCode(sw, $"public double {v} {{ get; private set; }}");

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
            using (StreamWriter sw = new StreamWriter(filename))
            {
                // Write dependencies
                WriteCode(sw, "using System;", "using SpiceSharp.Circuits;", "using SpiceSharp.Diagnostics;", "using SpiceSharp.Parameters;");

                // Write namespace and class
                WriteCode(sw, "", "namespace SpiceSharp.Components", "{");
                WriteCode(sw, $"public class {name} : CircuitComponent", "{");

                // Write device declaration
                WriteCode(sw, "/// <summary>", "/// Gets or sets the device model", "/// </summary>");
                WriteCode(sw, $"public {name}Model Model {{ get; set; }}");

                // Write the device parameters
                WriteCode(sw, "", "/// <summary>", "/// Parameters", "/// </summary>");
                foreach (string decl in paramDev.Declarations.Values)
                    WriteCode(sw, decl);

                // Write extra device parameters
                WriteCode(sw, "", "/// <summary>", "/// Extra variables", "/// </summary>");
                foreach (var v in deviceextra)
                    WriteCode(sw, $"public double {v} {{ get; private set; }}");

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
                for (int i = 0; i < definitions.States.Length; i++)
                {
                    if (!string.IsNullOrEmpty(definitions.States[i]))
                        WriteCode(sw, $"private const int {definitions.States[i]} = {i};");
                }

                // Write the constructor
                WriteCode(sw, "", "/// <summary>", "/// Constructor", "/// </summary>", "/// <param name=\"name\">The name of the device</param>");
                WriteCode(sw, $"public {name}(string name) : base(name, 2)", "{", "}");

                // Get the model
                WriteCode(sw, "", "/// <summary>", "/// Get the model", "/// </summary>", "/// <returns></returns>");
                WriteCode(sw, "public override CircuitModel GetModel() => Model;");

                // Setup method
                WriteCode(sw, "", "/// <summary>", "/// Setup the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Setup(Circuit ckt)", "{");
                foreach (string var in setup.DeviceVariables.Keys)
                    WriteCode(sw, $"{setup.DeviceVariables[var]} {var};");
                WriteCode(sw, "", "// Allocate nodes");
                WriteCode(sw, "var nodes = BindNodes(ckt);");
                int index = 0;
                foreach (var n in setup.Nodes)
                    WriteCode(sw, $"{n} = nodes[{index++}].Index;");
                WriteCode(sw, "", "// Allocate states", $"{setup.StatesVariable} = ckt.State.GetState();");
                WriteCode(sw, "", setupDev, "}");

                // Temperature method
                WriteCode(sw, "", "/// <summary>", "/// Do temperature-dependent calculations", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Temperature(Circuit ckt)", "{");
                foreach (string var in temp.DeviceVariables.Keys)
                    WriteCode(sw, $"{temp.DeviceVariables[var]} {var};");
                WriteCode(sw, "", tempDev, "}");

                // Load method
                WriteCode(sw, "", "/// <summary>", "/// Load the device", "/// </summary>", "/// <param name=\"ckt\">The circuit</param>");
                WriteCode(sw, "public override void Load(Circuit ckt)", "{");
                WriteCode(sw, "var state = ckt.State;", "var rstate = state.Real;");
                foreach (string var in load.DeviceVariables.Keys)
                    WriteCode(sw, $"{load.DeviceVariables[var]} {var};");
                WriteCode(sw, "", loadDev, "}");

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
                sw.WriteLine(offset + lines[i]);

                // Add offsets
                if (opening > 0 && closing == 0)
                    offset = offset + new string('\t', opening);
            }

            // Indent from scratch
            /* int index = code.IndexOf('{');
            while (index >= 0)
            {
                int end = Code.GetMatchingParenthesis(code, index);

                // Single out the piece of codes
                string before = code.Substring(0, index + 1);
                string after = code.Substring(end);
                string indent = code.Substring(index + 1, end - index - 1);
                indent = Regex.Replace(indent, @"\r\n(?!\t*$)", "\r\n\t");

                code = before + indent + after;

                index = code.IndexOf('{', index + 1);
            }

            // Add the final offset
            code = offset + Regex.Replace(code, @"\r\n", "\r\n" + offset); */
        }
    }
}
