using System.Collections.Generic;

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
            string[] ids = r.Identifier.Split(';');
            foreach (var id in ids)
                models.Add(id, r);
        }

        /// <summary>
        /// Remove a model reader
        /// </summary>
        /// <param name="r">The reader</param>
        public override void Remove(Reader r)
        {
            string[] ids = r.Identifier.Split(';');
            foreach (var id in ids)
                models.Remove(id);
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
                    type = st.Parameters[0].image.ToLower();
                    for (int i = 1; i < st.Parameters.Count; i++)
                        parameters.Add(st.Parameters[i]);
                    break;

                default:
                    throw new ParseException(st.Name, "Invalid model definition");
            }

            Statement mst = new Statement(StatementType.Model, st.Name, parameters);

            // The name should be the identifier
            if (!models.ContainsKey(type))
                throw new ParseException(st.Name, $"Cannot read model \"{st.Name.image}\" of type \"{type}\"");
            if (models[type].Read(type, mst, netlist))
                return models[type].Generated;
            throw new ParseException(st.Name, $"Could not create model \"{st.Name.image}\"");
        }
    }
}
