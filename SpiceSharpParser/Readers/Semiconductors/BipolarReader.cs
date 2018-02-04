using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="BJT"/> components.
    /// </summary>
    public class BipolarReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BipolarReader() : base("q") { }

        /// <summary>
        /// Generate a bipolar transistor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override Entity Generate(string type, Identifier name, List<Token> parameters, Netlist netlist)
        {
            // I think the BJT definition is ambiguous (eg. QXXXX NC NB NE MNAME OFF can be either substrate = MNAME, model = OFF or model name = MNAME and transistor is OFF
            // We will only allow 3 terminals if there are only 4 parameters
            BJT bjt = new BJT(name);

            // If the component is of the format QXXX NC NB NE MNAME we will insert NE again before the model name
            if (parameters.Count == 4)
                parameters.Insert(3, parameters[2]);
            bjt.ReadNodes(netlist.Path, parameters);

            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Model expected", false);
            bjt.SetModel(netlist.FindModel<BJTModel>(parameters[4]));

            for (int i = 5; i < parameters.Count; i++)
            {
                switch (parameters[i].kind)
                {
                    case WORD:
                        switch (parameters[i].image.ToLower())
                        {
                            case "on": bjt.BJToff = false; break;
                            case "off": bjt.BJToff = true; break;
                            default: throw new ParseException(parameters[i], "ON or OFF expected");
                        }
                        break;

                    case ASSIGNMENT:
                        var at = parameters[i] as AssignmentToken;
                        if (at.Name.image.ToLower() == "ic")
                            bjt.SetIC(netlist.ParseDoubleVector(at.Value));
                        else
                            throw new ParseException(parameters[i], "IC expected");
                        break;

                    case VALUE:
                    case EXPRESSION:
                        if (!bjt.BJTarea.Given)
                            bjt.BJTarea.Set(netlist.ParseDouble(parameters[i]));
                        else if (!bjt.BJTtemp.Given)
                            bjt.BJT_TEMP = netlist.ParseDouble(parameters[i]);
                        else
                            throw new ParseException(parameters[i], "Invalid parameter");
                        break;

                    default:
                        throw new ParseException(parameters[i], "Invalid parameter");
                }
            }
            
            return bjt;
        }
    }
}
