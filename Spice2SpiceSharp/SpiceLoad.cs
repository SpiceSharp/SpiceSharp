using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class represents the loading of a Spice device
    /// </summary>
    public class SpiceLoad : SpiceIterator
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
        /// <param name="setup">The setup of the device</param>
        public SpiceLoad(SpiceDevice dev, SpiceSetup setup)
        {
            string content = dev.GetMethod(SpiceDevice.Methods.Load);
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

            return Code.Format(code);
        }

        /// <summary>
        /// Replace ckt-> stuff
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        private string ApplyCircuit(string code, string ckt = "ckt", string state = "state", string rstate = "rstate")
        {
            Regex sr = new Regex($@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTstate(?<state>\d+)\s*\+\s*(?<var>\w+)\s*\)");
            code = sr.Replace(code, (Match m) => $"{state}.States[{m.Groups["state"].Value}][{states} + {m.Groups["var"].Value}]");

            Regex oldsol = new Regex($@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTrhsOld\s*\+\s*(?<node>\w+)\s*\)");
            code = oldsol.Replace(code, (Match m) => $"{rstate}.OldSolution[{m.Groups["node"].Value}]");

            Regex rhs = new Regex($@"\*\s*\(\s*{ckt}\s*\-\>\s*CKTrhs\s*\+\s*(?<node>\w+)\s*\)");
            code = rhs.Replace(code, (Match m) => $"{rstate}.Rhs[{m.Groups["node"].Value}]");

            // Nodes
            foreach (string n in matrixnodes.Keys)
            {
                // Remove from the variables of the load method
                DeviceVariablesExtra.Remove(n);

                // Replace the nodes
                Regex mn = new Regex($@"\*\s*\(\s*{n}\s*\)");
                code = mn.Replace(code, (Match m) => $"{rstate}.Matrix[{matrixnodes[n].Item1}, {matrixnodes[n].Item2}]");
            }

            return code;
        }
    }
}
