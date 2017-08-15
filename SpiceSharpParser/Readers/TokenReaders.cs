using System;
using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that describes all token readers
    /// </summary>
    public class TokenReaders
    {
        /// <summary>
        /// Currently active readers
        /// </summary>
        public StatementType Active { get; set; } = StatementType.All;

        /// <summary>
        /// The event that is fired when an expression needs to be parsed
        /// </summary>
        public event ParseNetlistExpressionEventHandler OnParseExpression;

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<StatementType, ReaderCollection> Readers = new Dictionary<StatementType, ReaderCollection>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenReaders()
        {
        }

        /// <summary>
        /// Read tokens
        /// </summary>
        /// <param name="type">The reader type</param>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns>Returns the last generated object by a reader</returns>
        public object Read(Statement st, Netlist netlist)
        {
            // Check if the type exists
            if (!Readers.ContainsKey(st.Type))
                throw new ParseException(st.Name, "Unrecognized command type");

            // Ignore without warning if the reader is not active
            if ((st.Type & Active) == StatementType.None)
                return null;
            return Readers[st.Type].Read(st, netlist);
        }

        /// <summary>
        /// Parse a value or expression
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns></returns>
        public double ParseDouble(string input)
        {
            ExpressionData data = new ExpressionData(input);
            OnParseExpression?.Invoke(this, data);
            return data.Output;
        }

        /// <summary>
        /// Register (multiple) token readers
        /// </summary>
        /// <param name="caller">The calling object</param>
        /// <param name="type">The type</param>
        /// <param name="readers">The readers</param>
        public void Register(params Reader[] readers)
        {
            for (int i = 0; i < readers.Length; i++)
            {
                if (!Readers.ContainsKey(readers[i].Type))
                    throw new ParseException("No suitable reader collection");
                Readers[readers[i].Type].Add(readers[i]);
            }
        }

        /// <summary>
        /// Register (multiple) token reader collections
        /// </summary>
        /// <param name="collections"></param>
        public void Register(params ReaderCollection[] collections)
        {
            for (int i = 0; i < collections.Length; i++)
            {
                if (Readers.ContainsKey(collections[i].Type))
                    throw new ParseException("There already is a reader collection for this type");
                Readers.Add(collections[i].Type, collections[i]);
            }
        }

        /// <summary>
        /// Unregister a token reader collection of a certain type
        /// </summary>
        /// <param name="type"></param>
        public void Unregister(StatementType type)
        {
            Readers.Remove(type);
        }

        /// <summary>
        /// Get a list of Readers by their type
        /// </summary>
        /// <param name="t">The parse type</param>
        /// <returns></returns>
        public ReaderCollection this[StatementType type]
        {
            get
            {
                if (Readers.ContainsKey(type))
                    return Readers[type];
                return null;
            }
        }
    }

    /// <summary>
    /// Expression data
    /// </summary>
    public class ExpressionData
    {
        /// <summary>
        /// The input expression
        /// </summary>
        public string Input { get; }

        /// <summary>
        /// The output expression
        /// </summary>
        public double Output { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input"></param>
        public ExpressionData(string input)
        {
            Input = input;
        }
    }

    /// <summary>
    /// An event handler for parsing a netlist expression
    /// </summary>
    /// <param name="sender">The TokenReaders object sending the event</param>
    /// <param name="data">The expression data</param>
    public delegate void ParseNetlistExpressionEventHandler(object sender, ExpressionData data);
}
