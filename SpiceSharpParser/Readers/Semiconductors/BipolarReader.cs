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
            // We will only allow 3 terminals if there are only 4 parameters
            BJT bjt = new BJT(name.image);
            if (parameters.Count <= 4)
                bjt.ReadNodes(parameters, 3);
            else
                bjt.ReadNodes(parameters, 4);

            if (parameters.Count == 3)
                throw new ParseException(parameters[2], "Model expected", false);
            if (parameters.Count == 4)
                bjt.Model = parameters[3].ReadModel<BJTModel>(netlist);
            else
                bjt.Model = parameters[4].ReadModel<BJTModel>(netlist);

            // Area
            if (parameters.Count > 5)
                bjt.Set("area", parameters[5].ReadValue());

            // ON/OFF
            if (parameters.Count > 6)
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
