using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers.Collections
{
    /// <summary>
    /// A reader collection indexed by the full name of the statement
    /// </summary>
    public class DictionaryReaderCollection : ReaderCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, Reader> readers = new Dictionary<string, Reader>();
        private List<Reader> any = new List<Reader>();

        /// <summary>
        /// Constructor
        /// </summary>
        public DictionaryReaderCollection(StatementType type)
            : base(type)
        {
        }

        /// <summary>
        /// Add a model reader
        /// </summary>
        /// <param name="r">The reader</param>
        public override void Add(Reader r)
        {
            // If there is no identifier specified, the reader will deal with it by itself
            if (r.Identifier == null)
            {
                if (!any.Contains(r))
                    any.Add(r);
            }
            else
            {
                // Add the model
                string[] ids = r.Identifier.Split(';');
                foreach (var id in ids)
                    readers.Add(id, r);
            }
        }

        /// <summary>
        /// Remove a model reader
        /// </summary>
        /// <param name="r">The reader</param>
        public override void Remove(Reader r)
        {
            if (r.Identifier == null)
                any.Remove(r);
            else
            {
                // Remove the model
                string[] ids = r.Identifier.Split(';');
                foreach (var id in ids)
                    readers.Remove(id);
            }
        }

        /// <summary>
        /// Clear
        /// </summary>
        public override void Clear()
        {
            any.Clear();
            readers.Clear();
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
            if (!readers.ContainsKey(type))
            {
                foreach (Reader r in any)
                {
                    if (r.Read(type, st, netlist))
                        return r.Generated;
                }
                throw new ParseException(st.Name, $"Cannot recognize \"{st.Name.image}\"");
            }
            else if (readers[type].Read(type, st, netlist))
                return readers[type].Generated;
            throw new ParseException(st.Name, $"Could not create \"{st.Name.image}\"");
        }

        /// <summary>
        /// Find a specific reader of a certain type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Find<T>()
        {
            foreach (var c in readers.Values)
            {
                if (c is T res)
                    return res;
            }
            foreach (var c in any)
            {
                if (c is T res)
                    return res;
            }
            return null;
        }
    }
}
