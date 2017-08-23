using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    public class SpicePzLoad : SpiceIterator
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, Tuple<string, string>> matrixnodes = new Dictionary<string, Tuple<string, string>>();
        private string states;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The spice device</param>
        /// <param name="setup"></param>
        public SpicePzLoad(SpiceDevice dev, SpiceSetup setup)
        {
            string content = dev.GetMethod(SpiceDevice.Methods.PzLoad);
            ReadMethod(content);

            // Copy the matrix nodes
            foreach (string n in setup.MatrixNodes.Keys)
                matrixnodes.Add(n, setup.MatrixNodes[n]);
            states = setup.StatesVariable;
        }

        /// <summary>
        /// Get the model code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <returns></returns>
        public override string ExportModel(SpiceParam mparams)
        {
            string code = GetModelCode(mparams);
            code = ApplyCircuit(code);

            return Code.Format(code);
        }

        /// <summary>
        /// Get the device code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <param name="dparams">The device parameters</param>
        /// <returns></returns>
        public override string ExportDevice(SpiceParam mparams, SpiceParam dparams)
        {
            string code = GetDeviceCode(mparams, dparams);
            code = ApplyCircuit(code);
            code = ComplexAssignments(code);

            return Code.Format(code);
        }

        /// <summary>
        /// Replace ckt-> stuff
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        private string ApplyCircuit(string code, string ckt = "ckt", string state = "state", string cstate = "cstate", string method = "method")
        {
            Regex sr = new Regex($@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTstate(?<state>\d+)\s*\+\s*(?<var>\w+)\s*\)");
            code = sr.Replace(code, (Match m) => $"{state}.States[{m.Groups["state"].Value}][{states} + {m.Groups["var"].Value}]");

            code = Regex.Replace(code, $@"{ckt}\s*\-\>\s*CKTomega", "cstate.Laplace");

            // Nodes
            foreach (string n in matrixnodes.Keys)
            {
                // Remove from the variables of the load method
                DeviceVariablesExtra.Remove(n);

                // Replace the nodes
                Regex mn = new Regex($@"\*\s*\(\s*{n}\s*\)");
                code = mn.Replace(code, (Match m) => $"{cstate}.Matrix[{matrixnodes[n].Item1}, {matrixnodes[n].Item2}]");
                Regex cmn = new Regex($@"\*\s*\(\s*{n}\s*\+\s*1\s*\)");
                code = cmn.Replace(code, (Match m) => $"{cstate}.Matrix[{matrixnodes[n].Item1}, {matrixnodes[n].Item2}].Imag");
            }

            // Basic state logic
            code = Regex.Replace(code, $@"(?<not>\!)?\({ckt}\s*\-\>\s*CKTmode\s*&\s*(?<flag>\w+)\)", (Match m) =>
            {
                string result = "";
                bool invert = m.Groups["not"].Success;
                switch (m.Groups["flag"].Value)
                {
                    case "MODETRAN":
                        result = $"({method} {(invert ? "!=" : "==")} null)";
                        break;
                    case "MODETRANOP":
                        result = $"{(invert ? "!" : "")}({state}.Domain == CircuitState.DomainTypes.Time && {state}.UseDC)";
                        break;
                    case "MODEINITTRAN":
                        result = (invert ? "!" : "") + $"({method} != null && {method}.SavedTime == 0.0)";
                        break;

                    case "MODEDCOP":
                        result = (invert ? "!" : "") + $"{state}.UseDC";
                        break;

                    case "MODEINITSMSIG":
                        result = (invert ? "!" : "") + $"{state}.UseSmallSignal";
                        break;

                    case "MODEDCTRANCURVE":
                        result = $"({state}.Domain {(invert ? "!=" : "==")} CircuitState.DomainTypes.None";
                        break;

                    case "MODEUIC":
                        result = (invert ? "!" : "") + $"{state}.UseIC";
                        break;

                    case "MODEAC": // Never reached...
                        result = "true";
                        break;

                    case "MODEINITJCT":
                        result = $"({state}.Init {(invert ? "!=" : "==")} CircuitState.InitFlags.InitJct)";
                        break;
                    case "INITFLOAT":
                        result = $"({state}.Init {(invert ? "!=" : "==")} CircuitState.InitFlags.InitFloat)";
                        break;
                    case "MODEINITFIX":
                        result = $"({state}.Init {(invert ? "!=" : "==")} CircuitState.InitFlags.InitFix)";
                        break;
                    default:
                        result = m.Value;
                        break;
                }

                return result;
            });
            code = Regex.Replace(code, $@"(?<not>\!)?\({ckt}\s*\-\>\s*CKTmode\s*&\s*\(\s*(?<flag>\w+)(\s*\|\s*(?<flag>\w+))*\)", (Match m) =>
            {
                // This is an OR for all the flags...
                string result = "";
                HashSet<string> flags = new HashSet<string>();
                HashSet<string> conditions = new HashSet<string>();
                foreach (Capture c in m.Groups["flag"].Captures)
                    flags.Add(c.Value);

                // MODEDC = MODEDCOP | MODETRANOP | MODEDCTRANCURVE
                if (flags.Contains("MODEDC"))
                {
                    flags.Remove("MODEDC");
                    flags.Add("MODEDCOP");
                    flags.Add("MODETRANOP");
                    flags.Add("MODEDCTRANCURVE");
                }

                // INITF = MODEINITFLOAT | MODEINITJCT | MODEINITFIX | MODEINITSMSIG | MODEINITTRAN | MODEINITPRED
                if (flags.Contains("INITF"))
                {
                    flags.Remove("INITF");
                    flags.Add("MODEINITFLOAT");
                    flags.Add("MODEINITJCT");
                    flags.Add("MODEINITFIX");
                    flags.Add("MODEINITSMSIG");
                    flags.Add("MODEINITTRAN");
                }

                // MODETRAN | MODETRANOP = (Domain == Time)
                if (flags.Contains("MODETRAN") && flags.Contains("MODETRANOP"))
                {
                    flags.Remove("MODETRAN");
                    flags.Remove("MODETRANOP");
                    flags.Add("TIMEDOMAIN");
                }

                // MODETRAN | MODEINITTRAN = MODEINITTRAN
                if (flags.Contains("MODEINITTRAN") && flags.Contains("MODETRAN"))
                    flags.Remove("MODETRAN");

                // MODEUIC = UseIC, but SpiceSharp will prioritize this variable in the following way:
                // - MODETRANOP + MODEUIC = MODEUIC
                // - MODETRAN + MODEUIC = MODEUIC
                if (flags.Contains("MODEUIC"))
                {
                    if (flags.Contains("MODETRANOP"))
                        flags.Remove("MODETRANOP");
                    if (flags.Contains("MODETRAN"))
                        flags.Remove("MODETRAN");
                }

                // Ignored flags
                if (flags.Contains("MODEAC"))
                    flags.Remove("MODEAC");
                if (flags.Contains("MODEINITPRED"))
                    flags.Remove("MODEINITPRED");

                // Build the conditions
                foreach (var flag in flags)
                {
                    switch (flag)
                    {
                        case "MODEUIC": conditions.Add($"{state}.UseIC"); break;
                        case "MODETRAN": conditions.Add($"{method} != null"); break;
                        case "MODETRANOP": conditions.Add($"({state}.Domain == CircuitState.DomainTypes.Time && {state}.UseDC)"); break;
                        case "MODEINITTRAN": conditions.Add($"({method} != null && {method}.SavedTime == 0.0)"); break;
                        case "MODEDCOP": conditions.Add($"{state}.UseDC"); break;
                        case "MODEINITSMSIG": conditions.Add($"{state}.UseSmallSignal"); break;
                        case "MODEDCTRANCURVE": conditions.Add($"{state}.Domain == CircuitState.DomainTypes.None"); break;

                        case "MODEINITJCT": conditions.Add($"{state}.Init == CircuitState.InitFlags.InitJct"); break;
                        case "MODEINITFLOAT": conditions.Add($"{state}.Init == CircuitState.InitFlags.InitFloat"); break;
                        case "MODEINITFIX": conditions.Add($"{state}.Init == CircuitState.InitFlags.InitFix"); break;

                        default:
                            // Cannot convert!
                            return m.Value;
                    }
                }

                // Construct the conditions
                if (m.Groups["not"].Success)
                    result = "!(" + string.Join(" || ", conditions);
                else
                    result = "(" + string.Join(" || ", conditions);
                return result;
            });

            // -> Is never possible, so let's go for dots
            code = Regex.Replace(code, @"\s*\-\>\s*", ".");

            return code;
        }

        /// <summary>
        /// Group complex assignments
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string ComplexAssignments(string code)
        {
            code = Regex.Replace(code, @"\(\s*(?<orig>cstate\.Matrix[^;]+)\);", (Match m) => m.Groups["orig"].Value);

            // In a PzLoad function, if a variable is multiplied by s->real, we can be sure there will be a s->imag too.
            Regex rm = new Regex(@"(?<mat>cstate\.Matrix\[\w+, \w+]) (?<sgn>[\+\-]\=) (?<value>[^;]+)\s*\*\s*s\-\>real\s*;\s*\k<mat>\.Imag \k<sgn> \k<value>\s*\*\s*s\-\>imag\s*;");
            code = rm.Replace(code, (Match m) =>
            {
                return m.Groups["mat"].Value + " " + m.Groups["sgn"].Value + " " + m.Groups["value"] + " * cstate.Laplace";
            });

            return code;
        }
    }
}
