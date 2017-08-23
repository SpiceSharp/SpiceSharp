using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    public class SpiceAcLoad : SpiceIterator
    {
        /// <summary>
        /// Complex assignment
        /// </summary>
        private class ComplexAssignment
        {
            /// <summary>
            /// The starting index of the statement
            /// </summary>
            public int Index;

            /// <summary>
            /// The length of the statement
            /// </summary>
            public int Length;

            /// <summary>
            /// The nodes of the statement
            /// </summary>
            public Tuple<string, string> Nodes;

            /// <summary>
            /// The operator of the statement
            /// </summary>
            public string Op;

            /// <summary>
            /// Is it a real part or imaginary?
            /// </summary>
            public bool Real;

            /// <summary>
            /// The value of the statement
            /// </summary>
            public string Value;
        }

        /// <summary>
        /// A group of assignments
        /// </summary>
        private class AssignmentGroup
        {
            /// <summary>
            /// Private variables
            /// </summary>
            private HashSet<ComplexAssignment> Real = new HashSet<ComplexAssignment>();
            private HashSet<ComplexAssignment> Imag = new HashSet<ComplexAssignment>();
            private Dictionary<int, ComplexAssignment> map = new Dictionary<int, ComplexAssignment>();
            private Dictionary<Tuple<string, string>, HashSet<ComplexAssignment>> nmap = new Dictionary<Tuple<string, string>, HashSet<ComplexAssignment>>();

            /// <summary>
            /// Add a complex assignment
            /// </summary>
            /// <param name="a"></param>
            public void Add(ComplexAssignment a)
            {
                if (a.Real)
                    Real.Add(a);
                else
                    Imag.Add(a);

                if (!nmap.ContainsKey(a.Nodes))
                    nmap.Add(a.Nodes, new HashSet<ComplexAssignment>());
                nmap[a.Nodes].Add(a);
                map.Add(a.Index, a);
            }

            /// <summary>
            /// Remove a complex assignment
            /// </summary>
            /// <param name="a"></param>
            public void Remove(ComplexAssignment a)
            {
                if (Real.Contains(a))
                    Real.Remove(a);
                if (Imag.Contains(a))
                    Imag.Remove(a);
                map.Remove(a.Index);
                nmap[a.Nodes].Remove(a);
            }

            /// <summary>
            /// Clear all assignments
            /// </summary>
            public void Clear()
            {
                Real.Clear();
                Imag.Clear();
                map.Clear();
                nmap.Clear();
            }

            /// <summary>
            /// Does the group contain a statement at this index?
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool Contains(int index) => map.ContainsKey(index);

            /// <summary>
            /// Does the group contain a real statement at this index?
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool ContainsReal(int index) => map.ContainsKey(index) ? Real.Contains(map[index]) : false;

            /// <summary>
            /// Does the group contain an imaginary statement at this index?
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool ContainsImag(int index) => map.ContainsKey(index) ? Imag.Contains(map[index]) : false;

            /// <summary>
            /// Allow enumeration through all imaginary statements by nodes
            /// </summary>
            /// <param name="nodes">Nodes</param>
            /// <returns></returns>
            public IEnumerable<ComplexAssignment> ImaginaryByNode(Tuple<string, string> nodes)
            {
                if (!nmap.ContainsKey(nodes))
                    return new ComplexAssignment[] { };
                List<ComplexAssignment> assignments = new List<ComplexAssignment>();
                foreach (ComplexAssignment item in nmap[nodes])
                {
                    if (!item.Real)
                        assignments.Add(item);
                }
                return assignments;
            }

            /// <summary>
            /// Enumerable of all imaginary assignments
            /// </summary>
            public IEnumerable<ComplexAssignment> AllImaginary => Imag;

            /// <summary>
            /// Enumerable of all real assignments
            /// </summary>
            public IEnumerable<ComplexAssignment> AllReal => Real;

            /// <summary>
            /// Enumerable of all nodes
            /// </summary>
            public IEnumerable<Tuple<string, string>> AllNodes => nmap.Keys;

            /// <summary>
            /// Allow enumeration through all real statements by their nodes
            /// </summary>
            /// <param name="nodes">Nodes</param>
            /// <returns></returns>
            public IEnumerable<ComplexAssignment> RealByNode(Tuple<string, string> nodes)
            {
                if (!nmap.ContainsKey(nodes))
                    return new ComplexAssignment[] { };
                List<ComplexAssignment> assignments = new List<ComplexAssignment>();
                foreach (ComplexAssignment item in nmap[nodes])
                {
                    if (item.Real)
                        assignments.Add(item);
                }
                return assignments;
            }

            /// <summary>
            /// Get a complex assignment at a certain position
            /// </summary>
            /// <param name="index">The index</param>
            /// <returns></returns>
            public ComplexAssignment this[int index] => map[index];
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, Tuple<string, string>> matrixnodes = new Dictionary<string, Tuple<string, string>>();
        private string states;
        private AssignmentGroup assignments = new AssignmentGroup();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The Spice device</param>
        public SpiceAcLoad(SpiceDevice dev, SpiceSetup setup)
        {
            string content = dev.GetMethod(SpiceDevice.Methods.AcLoad);
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

            code = Regex.Replace(code, $@"{ckt}\s*\-\>\s*CKTomega", "cstate.Laplace.Imaginary");

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
        /// Group the assignments
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string ComplexAssignments(string code)
        {
            // Find assignments to the imaginary part and try to combine them with the real counterpart
            // The imaginary assignments are of the form: cstate.Matrix[<node1>, <node2>].Imag +=/-= <expression>;
            // We need to make sure that conditions are not broken while matching equations...

            // First clean up some annoying brackets
            code = Regex.Replace(code, @"\((?<inner>cstate.Matrix\[\w+, \w+\][^;]+)\);", (Match m) => m.Groups["inner"].Value + ";");

            SortedDictionary<int, ComplexAssignment> remove = new SortedDictionary<int, ComplexAssignment>();
            SortedDictionary<int, ComplexAssignment> insert = new SortedDictionary<int, ComplexAssignment>();

            // Find all the statements and their starting positions
            assignments.Clear();
            MatchCollection ms = Regex.Matches(code, @"cstate\.Matrix\[(?<n1>\w+), (?<n2>\w+)\](?<imag>\.Imag)? (?<op>[\+\-]\=) (?<value>[^;]+);");
            foreach (Match m in ms)
            {
                ComplexAssignment a = new ComplexAssignment();
                a.Nodes = new Tuple<string, string>(m.Groups["n1"].Value, m.Groups["n2"].Value);
                a.Op = m.Groups["op"].Value;
                a.Value = m.Groups["value"].Value;
                if (m.Groups["imag"].Success)
                    a.Real = false;
                else
                    a.Real = true;
                a.Index = m.Index;
                a.Length = m.Length;
                assignments.Add(a);
            }

            // Now match them while keeping track of if-statements, comments and strings
            Stack<AssignmentGroup> currentgroup = new Stack<AssignmentGroup>();
            currentgroup.Push(new AssignmentGroup());
            Code.LevelExecute(code,
                (int i, int lvl) => currentgroup.Push(new AssignmentGroup()),
                (int i, int lvl) => MatchAssignments(currentgroup.Pop(), insert, remove),
                (int i, int lvl) => {
                    if (assignments.Contains(i))
                        currentgroup.Peek().Add(assignments[i]);
                });
            MatchAssignments(currentgroup.Pop(), insert, remove);

            // Remove the old statement assignments and add the new ones
            int offset = 0;
            var it_insert = insert.GetEnumerator();
            var it_remove = remove.GetEnumerator();
            bool ic = it_insert.MoveNext(), rc = it_remove.MoveNext();
            while (ic || rc)
            {
                if (ic && rc)
                {
                    // First try to add
                    if (it_insert.Current.Key <= it_remove.Current.Key)
                    {
                        int start = it_insert.Current.Key + offset;
                        code = code.Substring(0, start) + it_insert.Current.Value.Value + code.Substring(start);
                        offset += it_insert.Current.Value.Length;
                        ic = it_insert.MoveNext();
                    }
                    else
                    {
                        // Only now remove
                        int start = it_remove.Current.Key + offset;
                        code = code.Substring(0, start) + code.Substring(start + it_remove.Current.Value.Length);
                        offset -= it_remove.Current.Value.Length;
                        rc = it_remove.MoveNext();
                    }
                }
                else if (ic)
                {
                    int start = it_insert.Current.Key + offset;
                    code = code.Substring(0, start) + it_insert.Current.Value.Value + code.Substring(start);
                    offset += it_insert.Current.Value.Length;
                    ic = it_insert.MoveNext();
                }
                else if (rc)
                {
                    // Only now remove
                    int start = it_remove.Current.Key + offset;
                    code = code.Substring(0, start) + code.Substring(start + it_remove.Current.Value.Length);
                    offset -= it_remove.Current.Value.Length;
                    rc = it_remove.MoveNext();
                }
            }

            return code;
        }

        /// <summary>
        /// Match assignments
        /// </summary>
        private void MatchAssignments(AssignmentGroup group, SortedDictionary<int, ComplexAssignment> insert, SortedDictionary<int, ComplexAssignment> remove)
        {
            // Try to match all the statements in the group
            foreach (var node in group.AllNodes)
            {
                ComplexAssignment na = new ComplexAssignment();
                na.Nodes = node;
                na.Op = null;
                na.Index = int.MaxValue;
                na.Length = 0;
                na.Real = true;

                // Create the real part
                string rpart = "";
                foreach (var a in group.RealByNode(node))
                {
                    if (na.Op == null)
                    {
                        na.Op = a.Op;
                        rpart = a.Value;
                    }
                    else
                    {
                        if (a.Op == na.Op)
                        {
                            if (rpart.Length > 0)
                                rpart += " + ";
                            rpart += a.Value;
                        }
                        else
                        {
                            if (rpart.Length > 0)
                                rpart += " - ";
                            else
                                rpart += "-";
                            if (a.Value.Contains('+') || a.Value.Contains('-'))
                                rpart += "(" + a.Value + ")";
                            else
                                rpart += a.Value;
                        }
                    }

                    // Keep the first assignment to insert
                    if (a.Index < na.Index)
                        na.Index = a.Index;
                    remove.Add(a.Index, a);
                }

                // Create the imaginary part
                string ipart = "";
                foreach (var a in group.ImaginaryByNode(node))
                {
                    if (na.Op == null)
                    {
                        na.Op = a.Op;
                        ipart = a.Value;
                    }
                    else
                    {
                        if (a.Op == na.Op)
                        {
                            if (ipart.Length > 0)
                                ipart += " + ";
                            ipart += a.Value;
                        }
                        else
                        {
                            if (ipart.Length > 0)
                                ipart += " - ";
                            else
                                ipart += "-";
                            if (a.Value.Contains('+') || a.Value.Contains('-'))
                                ipart += "(" + a.Value + ")";
                            else
                                ipart += a.Value;
                        }
                    }

                    // Keep the first assignment to insert
                    if (a.Index < na.Index)
                        na.Index = a.Index;
                    remove.Add(a.Index, a);
                }

                // Build the value
                if (string.IsNullOrEmpty(ipart))
                    na.Value = $"cstate.Matrix[{node.Item1}, {node.Item2}] {na.Op} {rpart};";
                else
                {
                    if (string.IsNullOrEmpty(rpart))
                        rpart = "0.0";
                    na.Value = $"cstate.Matrix[{node.Item1}, {node.Item2}] {na.Op} new Complex({rpart}, {ipart});";
                }
                na.Length = na.Value.Length;
                insert.Add(na.Index, na);
            }
        }
    }
}
