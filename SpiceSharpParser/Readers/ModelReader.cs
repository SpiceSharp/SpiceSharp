using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of reading models
    /// </summary>
    public abstract class ModelReader : Reader
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private string Id = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        public ModelReader(string id) : base(StatementType.Model)
        {
            Id = id;
        }

        /// <summary>
        /// Generate a model of the right type
        /// </summary>
        /// <returns></returns>
        protected abstract CircuitModel GenerateModel(string name);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            // The name needs to be .model
            if (!st.Name.TryReadLiteral("model"))
                return false;

            // Errors
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "Model name expected", false);
                case 1: throw new ParseException(st.Parameters[0], "Model type expected", false);
            }

            // Extract the name of the model
            string name = st.Parameters[0].ReadIdentifier();
            CircuitModel result = null;

            // Type: .MODEL <NAME> <TYPE> <PAR1>=<VAL1> ...
            if (st.Parameters[1] is Token)
            {
                string modeltype = st.Parameters[1].ReadWord();
                if (modeltype != Id)
                    return false;

                // Read all the parameters for the model
                // 0 = Name, 1 = type, 2+ are the parameters
                result = GenerateModel(name);
                result.ReadParameters(st.Parameters, 2);
            }

            // Type: .MODEL <NAME> <TYPE>(<PAR1>=<VAL1> ...)
            else if (st.Parameters[1].TryReadBracket(out BracketToken bt))
            {
                string modeltype = bt.Name.ReadWord();
                if (modeltype != Id)
                    return false;

                // Read all the parameters for the model
                // bt.Parameters 
                result = GenerateModel(name);
                result.ReadParameters(bt.Parameters);
            }
            else
                throw new ParseException(st.Name, "Invalid model definition");

            netlist.Path.Components.Add(result);
            Generated = result;
            return true;
        }
    }
}
