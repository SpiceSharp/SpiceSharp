using System;
using System.Collections.Generic;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    public class Statement
    {
        /// <summary>
        /// The leading name
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// The parameters
        /// </summary>
        public List<object> Parameters { get; }

        /// <summary>
        /// The type
        /// </summary>
        public StatementType Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of the parameter</param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        public Statement(StatementType type, Token name, List<object> parameters)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Nice representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = Name.Image();
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i] is List<Statement>)
                    result += " body";
                else
                    result += " " + Parameters[i].Image();
            }
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
