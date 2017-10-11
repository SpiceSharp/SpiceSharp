using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers.Collections
{
    /// <summary>
    /// A reader collection that just goes through all readers in order intil it finds one that can parse a statement.
    /// </summary>
    public class GenericReaderCollection : ReaderCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private List<Reader> readers = new List<Reader>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        public GenericReaderCollection(StatementType type)
            : base(type)
        {
        }

        /// <summary>
        /// Add a reader
        /// </summary>
        /// <param name="r">Reader</param>
        public override void Add(Reader r)
        {
            readers.Add(r);
        }

        /// <summary>
        /// Remove a reader
        /// </summary>
        /// <param name="r">Reader</param>
        public override void Remove(Reader r)
        {
            readers.Remove(r);
        }

        /// <summary>
        /// Clear all readers
        /// </summary>
        public override void Clear()
        {
            readers.Clear();
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override object Read(Statement st, Netlist netlist)
        {
            foreach (Reader r in readers)
            {
                if (r.Read(st.Name.image.ToLower(), st, netlist))
                    return r.Generated;
            }
            throw new ParseException(st.Name, "Unrecognized syntax");
        }

        /// <summary>
        /// Find a specific reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Find<T>()
        {
            foreach (var c in readers)
            {
                if (c is T res)
                    return res;
            }
            return null;
        }
    }
}
