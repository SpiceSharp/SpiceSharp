using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers.Collections
{
    /// <summary>
    /// Strategy for reading models
    /// </summary>
    public class DictionaryReaderCollection : ReaderCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, Reader> models = new Dictionary<string, Reader>();

        /// <summary>
        /// Constructor
        /// </summary>
        public DictionaryReaderCollection(StatementType type)
            : base(type)
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
            // The name should be the identifier
            string type = st.Name.image.ToLower();
            if (!models.ContainsKey(type))
                throw new ParseException(st.Name, $"Cannot recognize \"{st.Name.image}\"");
            if (models[type].Read(st, netlist))
                return models[type].Generated;
            throw new ParseException(st.Name, $"Could not create \"{st.Name.image}\"");
        }
    }
}
