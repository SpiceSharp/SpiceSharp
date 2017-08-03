using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

using static SpiceSharp.Parser.SpiceSharpParserConstants;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of reading models
    /// </summary>
    public class ModelReader : Reader
    {
        /// <summary>
        /// A list of model readers
        /// </summary>
        public Dictionary<string, Reader> ModelReaders { get; } = new Dictionary<string, Reader>();

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            // Model declaration?
            if (name.kind != WORD)
                return false;
            if (name.image.ToLower() != "model")
                return false;

            // Extract the name of the model
            if (parameters.Count < 2)
                throw new ParseException($"Error at line {GetBeginLine(name)}, invalid model declaration");
            if (!(parameters[0] is Token))
                throw new ParseException($"Error at line {GetBeginLine(name)}, invalid model name");
            Token modelname = parameters[0] as Token;
            parameters.RemoveAt(0);

            // We have two options for the model: bracketted or not
            // - .MODEL MNAME TYPE(PAR1=VAL1 PAR2=VAL2 ...)
            // - .MODEL MNAME TYPE PAR1=VAL1 PAR2=VAL2 ...
            string modeltype;
            if (parameters[0] is Token)
            {
                modeltype = ReadWord(parameters[0]).ToLower();
                parameters.RemoveAt(0);
            }
            else if (parameters[0] is SpiceSharpParser.Bracketed)
            {
                SpiceSharpParser.Bracketed b = parameters[0] as SpiceSharpParser.Bracketed;
                modeltype = ReadWord(b.Name).ToLower();
                parameters = b.Parameters;
            }
            else
                throw new ParseException($"Error at line {GetBeginLine(parameters[1])}, invalid model declaration");

            // Find the right type in our
            if (!ModelReaders.ContainsKey(modeltype))
                throw new ParseException($"Error at line {GetBeginLine(parameters[1])}, column {GetBeginColumn(parameters[1])}, unrecognized model type");
            return ModelReaders[modeltype].Read(modelname, parameters, netlist);
        }
    }
}
