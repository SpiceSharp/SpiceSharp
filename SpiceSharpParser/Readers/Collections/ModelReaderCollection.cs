using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Parser.Readers.Collections
{
    /// <summary>
    /// A collection for searching models
    /// </summary>
    public class ModelReaderCollection : ReaderCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, Reader> models = new Dictionary<string, Reader>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelReaderCollection()
            : base(StatementType.Model)
        { }

        /// <summary>
        /// Add a model reader
        /// </summary>
        /// <param name="r">The reader</param>
        public override void Add(Reader r)
        {
            // Add the model
            models.Add(r.Identifier, r);
        }

        /// <summary>
        /// Remove a model reader
        /// </summary>
        /// <param name="r">The reader</param>
        public override void Remove(Reader r)
        {
            models.Remove(r.Identifier);
        }

        /// <summary>
        /// Clear
        /// </summary>
        public override void Clear()
        {
            models.Clear();
        }

        /// <summary>
        /// Read a statement
        /// </summary>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override object Read(Statement st, Netlist netlist)
        {
            // The name is not the name now, it is the type
            List<Token> parameters = new List<Token>();

            string type = null;
            if (st.Parameters.Count < 1)
                throw new ParseException(st.Name, "Invalid model definition");

            switch (st.Parameters[0].kind)
            {
                case TokenConstants.BRACKET:
                    BracketToken bt = st.Parameters[0] as BracketToken;
                    type = bt.Name.image.ToLower();
                    foreach (var p in bt.Parameters)
                        parameters.Add(p);
                    break;

                case SpiceSharpParserConstants.WORD:
                    type = parameters[0].image.ToLower();
                    for (int i = 1; i < parameters.Count; i++)
                        parameters.Add(parameters[i]);
                    break;

                default:
                    throw new ParseException(st.Name, "Invalid model definition");
            }

            // The name should be the identifier
            if (!models.ContainsKey(type))
                throw new ParseException(st.Name, $"Cannot recognize \"{st.Name.image}\"");
            if (models[type].Read(st, netlist))
                return models[type].Generated;
            throw new ParseException(st.Name, $"Could not create \"{st.Name.image}\"");
        }
    }
}
