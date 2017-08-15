using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers.Collections
{
    /// <summary>
    /// This class will read component statements
    /// </summary>
    public class ComponentReaderCollection : ReaderCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        public Dictionary<char, Reader> readers = new Dictionary<char, Reader>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentReaderCollection()
            : base(StatementType.Component)
        {   
        }

        /// <summary>
        /// Add a reader
        /// </summary>
        /// <param name="r">Reader</param>
        public override void Add(Reader r)
        {
            char c = r.Identifier[0];
            readers.Add(c, r);
        }

        /// <summary>
        /// Remove a reader
        /// </summary>
        /// <param name="r">Reader</param>
        public override void Remove(Reader r)
        {
            readers.Remove(r.Identifier[0]);
        }

        /// <summary>
        /// Clear all readers
        /// </summary>
        public override void Clear()
        {
            readers.Clear();
        }

        /// <summary>
        /// Read a component statement
        /// </summary>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override object Read(Statement st, Netlist netlist)
        {
            char id = char.ToLower(st.Name.image[0]);
            if (!readers.ContainsKey(id))
                throw new ParseException(st.Name, $"Cannot recognized component \"{st.Name.image}\"");

            if (readers[id].Read(st, netlist))
                return readers[id].Generated;
            throw new ParseException(st.Name, $"Cannot create component \"{st.Name.image}\"");
        }
    }
}
