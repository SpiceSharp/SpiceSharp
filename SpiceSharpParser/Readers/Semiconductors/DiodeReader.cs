using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a diode model
    /// </summary>
    public class DiodeReader : Reader
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
            if (name.image[0] != 'd' && name.image[0] != 'D')
                return false;

            Diode dio = new Diode(name.image);
            ReadNodes(dio, parameters, 2);

            if (parameters.Count < 3)
                ThrowAfter(parameters[1], "Model expected");
            dio.Model = ReadModel<DiodeModel>(parameters[2], netlist);

            // Optional: Area
            if (parameters.Count > 3)
                dio.Set("area", ReadValue(parameters[3]));

            // Read the rest of the parameters
            for (int i = 4; i < parameters.Count; i++)
            {
                string pname, pvalue;
                if (TryReadLiteral(parameters[i], "on"))
                    dio.Set("off", false);
                else if (TryReadLiteral(parameters[i], "off"))
                    dio.Set("on", true);
                else if (TryReadNamed(parameters[i], out pname, out pvalue))
                {
                    if (pname.ToLower() != "ic")
                        ThrowBefore(parameters[i], "IC expected");
                    dio.Set("ic", pvalue);
                }
                else if (TryReadValue(parameters[i], out pvalue))
                    dio.Set("temp", pvalue);
                else
                    ThrowBefore(parameters[i], "Uncrecognized parameter");
            }

            netlist.Circuit.Components.Add(dio);
            return true;
        }
    }
}
