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
            bjt.ReadNodes(parameters, 4);

            if (parameters.Count <= 4)
                throw new ParseException(parameters[3], "Model name expected", false);
            bjt.Model = parameters[4].ReadModel<BipolarModel>(netlist);

            // Area
            if (parameters.Count > 5)
                bjt.Set("area", parameters[5].ReadValue());
            else if (parameters.Count > 6)
            {
                string state = parameters[6].ReadWord();
                switch (state.ToLower())
                {
                    case "on": bjt.Set("off", false); break;
                    case "off": bjt.Set("off", true); break;
                    default: throw new ParseException(parameters[6], "ON or OFF expected");
                }
            }

            // The rest are named parameters
            for (int i = 7; i < parameters.Count; i++)
            {
                string pname, pvalue;
                parameters[i].ReadAssignment(out pname, out pvalue);
                bjt.Set(pname, pvalue);
            }

            netlist.Circuit.Components.Add(bjt);
            return true;
        }
    }
}
