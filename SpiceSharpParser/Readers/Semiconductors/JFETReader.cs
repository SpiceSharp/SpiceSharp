using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="JFET"/> components.
    /// </summary>
    public class JFETReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public JFETReader() : base("j") { }

        /// <summary>
        /// Generate a JFET
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
        {
            JFET jfet = new JFET(name);
            jfet.ReadNodes(parameters);

            // Get the model name
            jfet.SetModel(netlist.FindModel<JFETModel>(parameters[3]));

            // Read the rest of the parameters
            for (int i = 4; i < parameters.Count; i++)
            {
                switch (parameters[i].kind)
                {
                    case WORD:
                        switch (parameters[i].image.ToLower())
                        {
                            case "on":
                                jfet.JFEToff = false;
                                break;
                            case "off":
                                jfet.JFEToff = true;
                                break;
                            default:
                                throw new ParseException(parameters[i], "ON or OFF expected");
                        }
                        break;

                    case ASSIGNMENT:
                        AssignmentToken at = parameters[i] as AssignmentToken;
                        if (at.Name.image.ToLower() != "ic")
                            jfet.SetIC(netlist.ParseDoubleVector(at.Value));
                        else
                            throw new ParseException(parameters[i], "IC expected");
                        break;

                    case VALUE:
                    case EXPRESSION:
                        if (!jfet.JFETarea.Given)
                            jfet.JFETarea.Set(netlist.ParseDouble(parameters[i]));
                        else if (!jfet.JFETtemp.Given)
                            jfet.JFET_TEMP = netlist.ParseDouble(parameters[i]);
                        else
                            throw new ParseException(parameters[i], "Invalid parameter");
                        break;

                    default:
                        throw new ParseException(parameters[i], "Invalid parameter");
                }
            }
            return jfet;
        }
    }
}
