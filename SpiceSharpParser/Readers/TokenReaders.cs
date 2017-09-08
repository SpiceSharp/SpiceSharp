using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a statement
    /// </summary>
    public class StatementReaders
    {
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
        public StatementReaders()
        {
        }

        /// <summary>
        /// Read a statement
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
            if (double.IsNaN(data.Output))
                throw new ParseException($"Could not parse \"{input}\"");
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
        /// Register (multiple) statement reader collections
        /// </summary>
        /// <param name="collections">Reader collections</param>
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
        /// Unregister all statement reader collection of a certain type
        /// </summary>
        /// <param name="type">Statement type</param>
        public void Unregister(StatementType type)
        {
            Readers.Remove(type);
        }

        /// <summary>
        /// Get a list statement reader collections by their type
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
            Output = double.NaN;
        }
    }

    /// <summary>
    /// An event handler for parsing a netlist expression
    /// </summary>
    /// <param name="sender">The StatementReaders object sending the event</param>
    /// <param name="data">The expression data</param>
    public delegate void ParseNetlistExpressionEventHandler(object sender, ExpressionData data);
}
