using System.Collections.Generic;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A standard implementation for a component reader
    /// </summary>
    public abstract class ComponentReader : Reader
    {
        /// <summary>
        /// The identifier for the component
        /// </summary>
        protected char id = '\0', ID = '\0';

        /// <summary>
        /// Constructor
        /// In order for a component to be detected, the name needs to start with the id-character
        /// </summary>
        /// <param name="id">The id character</param>
        protected ComponentReader(char cid)
            : base(StatementType.Component)
        {
            id = char.ToLower(cid);
            ID = char.ToUpper(cid);
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            // Find the name
            string n = st.Name.ReadWord();
            if (n[0] != id && n[0] != ID)
                return false;

            // Apply the change to the circuit
            CircuitComponent result = Generate(n, st.Parameters, netlist);
            Generated = result;
            if (result != null)
            {
                // Add the circuit component
                netlist.Path.Components.Add(result);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Generate the component
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        protected abstract CircuitComponent Generate(string name, List<object> parameters, Netlist netlist);
    }
}
