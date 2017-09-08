using System;
using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A statement consists of a name and parameters.
    /// Can be read by a <see cref="Reader"/>.
    /// </summary>
    public class Statement
    {
        /// <summary>
        /// The leading name
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// The parameters
        /// </summary>
        public List<Token> Parameters { get; }

        /// <summary>
        /// The type
        /// </summary>
        public StatementType Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of the statement</param>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        public Statement(StatementType type, Token name, List<Token> parameters)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of the statement</param>
        /// <param name="name">The name</param>
        /// <param name="parameters">parameters</param>
        public Statement(StatementType type, Token name, params Token[] parameters)
        {
            Type = type;
            Name = name;
            Parameters = new List<Token>();
            foreach (var t in parameters)
                Parameters.Add(t);
        }

        /// <summary>
        /// Nice representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = Name.image;
            for (int i = 0; i < Parameters.Count; i++)
                result += " " + Parameters[i].image;
            return result;
        }
    }

    /// <summary>
    /// Possible types of readers
    /// </summary>
    [Flags]
    public enum StatementType
    {
        None = 0x00,
        Control = 0x01,
        Component = 0x02,
        Model = 0x04,
        Export = 0x08,
        Waveform = 0x10,
        Subcircuit = 0x20,
        All = 0x3f
    }
}
