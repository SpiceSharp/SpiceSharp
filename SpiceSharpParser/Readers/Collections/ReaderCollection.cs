namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of searching through readers
    /// </summary>
    public abstract class ReaderCollection
    {
        /// <summary>
        /// Get the type of reader collections
        /// </summary>
        public StatementType Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        public ReaderCollection(StatementType type)
        {
            Type = type;
        }

        /// <summary>
        /// Read a statement
        /// The strategy for finding the right reader will depend on the class
        /// </summary>
        /// <param name="st">The statement</param>
        public abstract object Read(Statement st, Netlist netlist);

        /// <summary>
        /// Add a reader
        /// </summary>
        /// <param name="r">The reader</param>
        public abstract void Add(Reader r);

        /// <summary>
        /// Remove a reader
        /// </summary>
        /// <param name="r">The reader</param>
        public abstract void Remove(Reader r);

        /// <summary>
        /// Clear all readers
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Find a specific reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T Find<T>() where T : Reader;
    }
}
