using System;
using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This interface can read tokens
    /// </summary>
    public abstract class Reader
    {
        /// <summary>
        /// The reader type
        /// </summary>
        public StatementType Type { get; private set; }

        /// <summary>
        /// An optional identifier that can be used by the ReaderCollection
        /// to find the right reader
        /// </summary>
        public string Identifier { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of reader</param>
        protected Reader(StatementType type)
        {
            Type = type;
        }

        /// <summary>
        /// Get the generated object by the reader
        /// </summary>
        public object Generated { get; protected set; } = null;

        /// <summary>
        /// Read a line
        /// </summary>
        /// <param name="name">The opening name/id</param>
        /// <param name="parameters">The following parameters</param>
        /// <param name="netlist">The resulting netlist</param>
        /// <returns></returns>
        public abstract bool Read(Statement st, Netlist netlist);
    }
}
