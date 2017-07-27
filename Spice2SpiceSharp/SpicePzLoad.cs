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
        private string ApplyCircuit(string code, string ckt = "ckt", string state = "state", string cstate = "cstate")
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
