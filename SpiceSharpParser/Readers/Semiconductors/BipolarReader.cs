using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read bipolar transistors
    /// </summary>
    public class BipolarReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'q' && name.image[0] != 'Q')
                return false;

            // I think the BJT definition is ambiguous (eg. QXXXX NC NB NE MNAME OFF can be either substrate = MNAME, model = OFF or model name = MNAME and transistor is OFF
            // I will force a 4-terminal device, which is much easier to implement here
            Bipolar bjt = new Bipolar(name.image);
            ReadNodes(bjt, parameters, 4);

            if (parameters.Count <= 4)
                ThrowAfter(parameters[3], "Model name expected");
            bjt.Model = ReadModel<BipolarModel>(parameters[4], netlist);

            // Area
            if (parameters.Count > 5)
                bjt.Set("area", ReadValue(parameters[5]));
            else if (parameters.Count > 6)
            {
                string state = ReadWord(parameters[6]);
                switch (state.ToLower())
                {
                    case "on": bjt.Set("off", false); break;
                    case "off": bjt.Set("off", true); break;
                    default: ThrowBefore(parameters[6], "On or Off expected"); break;
                }
            }

            // The rest are named parameters
            for (int i = 7; i < parameters.Count; i++)
            {
                string pname, pvalue;
                ReadNamed(parameters[i], out pname, out pvalue);
            }

            netlist.Circuit.Components.Add(bjt);
            return true;
        }
    }
}
