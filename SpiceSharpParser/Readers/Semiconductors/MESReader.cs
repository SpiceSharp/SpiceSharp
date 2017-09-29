using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Readers <see cref="MES"/> components.
    /// </summary>
    public class MESReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MESReader() : base("z") { }

        /// <summary>
        /// Generate a MESFET
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
        {
            MES mes = new MES(name);
            mes.ReadNodes(parameters);

            // Read the model name
            if (parameters.Count < 4)
                throw new ParseException(parameters[2], "Model expected", false);
            mes.SetModel(netlist.FindModel<MESModel>(parameters[3]));

            // Read parameters
            for (int i = 4; i < parameters.Count; i++)
            {
                switch (parameters[i].kind)
                {
                    case WORD:
                        switch (parameters[i].image.ToLower())
                        {
                            case "on": mes.MESoff = false; break;
                            case "off": mes.MESoff = true; break;
                            default: throw new ParseException(parameters[i], "ON or OFF expected");
                        }
                        break;

                    case ASSIGNMENT:
                        var at = parameters[i] as AssignmentToken;
                        if (at.Name.image.ToLower() == "ic")
                            mes.SetIC(netlist.ParseDoubleVector(at.Value));
                        else
                            throw new ParseException(parameters[i], "IC expected");
                        break;

                    case VALUE:
                    case EXPRESSION:
                        if (!mes.MESarea.Given)
                            mes.MESarea.Set(netlist.ParseDouble(parameters[i]));
                        else
                            throw new ParseException(parameters[i], "Invalid parameter");
                        break;

                    default:
                        throw new ParseException(parameters[i], "Invalid parameter");
                }
            }

            return mes;
        }
    }
}
