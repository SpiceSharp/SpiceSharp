using System.Collections.Generic;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// Represents a collection of grouped statements (eg. subcircuit definitions).
    /// </summary>
    public class StatementsToken : Token
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<StatementType, List<Statement>> statements = new Dictionary<StatementType, List<Statement>>();
        private int count = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public StatementsToken()
        {
            kind = TokenConstants.BODY;
            statements.Add(StatementType.Subcircuit, new List<Statement>());
            statements.Add(StatementType.Component, new List<Statement>());
            statements.Add(StatementType.Model, new List<Statement>());
            statements.Add(StatementType.Control, new List<Statement>());
        }

        /// <summary>
        /// Add a statement
        /// </summary>
        /// <param name="st">The statement</param>
        public void Add(Statement st)
        {
            if (!statements.ContainsKey(st.Type))
                statements.Add(st.Type, new List<Statement>());
            statements[st.Type].Add(st);

            if (count == 0)
            {
                beginColumn = st.Name.beginColumn;
                beginLine = st.Name.beginLine;
            }
            count++;

            // This is technically not correct, but we can save time
            // Use CalculateLineColumn() if you wish to know the exact positions
            endColumn = st.Name.endColumn;
            endLine = st.Name.endLine;
            image = "Body (" + count + " statements)";
        }

        /// <summary>
        /// Remove a statement
        /// </summary>
        /// <param name="st">Statement</param>
        public void Remove(Statement st)
        {
            statements[st.Type].Remove(st);
            count--;
        }

        /// <summary>
        /// Clear all statements
        /// </summary>
        public void Clear()
        {
            statements.Clear();
        }

        /// <summary>
        /// Calculate line and column parameters
        /// </summary>
        public void CalculateLineColumn()
        {
            // Initialize
            beginColumn = int.MaxValue;
            beginLine = int.MaxValue;
            endColumn = 0;
            endLine = 0;

            foreach (var t in statements.Keys)
            {
                foreach (var st in statements[t])
                {
                    beginColumn = beginColumn < st.Name.beginColumn ? beginColumn : st.Name.beginColumn;
                    beginLine = beginLine < st.Name.beginLine ? beginLine : st.Name.beginLine;

                    int ec, el;
                    if (st.Parameters.Count > 0)
                    {
                        ec = st.Parameters[st.Parameters.Count - 1].endColumn;
                        el = st.Parameters[st.Parameters.Count - 1].endLine;
                    }
                    else
                    {
                        ec = st.Name.endColumn;
                        el = st.Name.endLine;
                    }
                    endColumn = endColumn > ec ? endColumn : ec;
                    endLine = endLine > el ? endLine : el;
                }
            }
        }

        /// <summary>
        /// Allow traversing through the statements
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Statement> Statements(StatementType type)
        {
            return statements[type];
        }
    }
}
