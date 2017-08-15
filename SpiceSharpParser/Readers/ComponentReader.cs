using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A standard implementation for a component reader
    /// </summary>
    public abstract class ComponentReader : Reader
    {
        /// <summary>
        /// Constructor
        /// In order for a component to be detected, the name needs to start with the id-character
        /// </summary>
        /// <param name="id">The id character</param>
        protected ComponentReader(char cid)
            : base(StatementType.Component)
        {
            Identifier = cid.ToString().ToLower();
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
            // Apply the change to the circuit
            ICircuitObject result = Generate(st.Name.image, st.Parameters, netlist);
            Generated = result;
            if (result != null)
            {
                // Add the circuit component
                netlist.Path.Objects.Add(result);
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
        protected abstract ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist);
    }
}
