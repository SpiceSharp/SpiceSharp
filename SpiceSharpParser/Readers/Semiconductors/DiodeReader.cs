using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="Diode"/> components.
    /// </summary>
    public class DiodeReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DiodeReader() : base("d") { }

        /// <summary>
        /// Generate a diode
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
        {
            Diode dio = new Diode(name);
            dio.ReadNodes(parameters);

            if (parameters.Count < 3)
                throw new ParseException(parameters[1], "Model expected", false);
            dio.SetModel(netlist.FindModel<DiodeModel>(parameters[2]));

            // Optional: Area
            if (parameters.Count > 3)
                dio.DIOarea.Set(netlist.ParseDouble(parameters[3]));

            // Read the rest of the parameters
            for (int i = 4; i < parameters.Count; i++)
            {
                switch (parameters[i].kind)
                {
                    case WORD:
                        switch (parameters[i].image.ToLower())
                        {
                            case "on":
                                dio.DIOoff = false;
                                break;
                            case "off":
                                dio.DIOoff = true;
                                break;
                            default:
                                throw new ParseException("ON or OFF expected");
                        }
                        break;
                    case ASSIGNMENT:
                        AssignmentToken at = parameters[i] as AssignmentToken;
                        if (at.Name.image.ToLower() != "ic")
                            dio.DIOinitCond = netlist.ParseDouble(at.Value);
                        break;
                    case VALUE:
                    case EXPRESSION:
                        dio.DIOtemp.Set(netlist.ParseDouble(parameters[i]));
                        break;
                    default:
                        throw new ParseException(parameters[i], "Unrecognized parameter");
                }
            }

            return dio;
        }
    }
}
