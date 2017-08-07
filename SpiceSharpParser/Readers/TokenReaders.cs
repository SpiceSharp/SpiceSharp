﻿using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that describes all token readers
    /// </summary>
    public class TokenReaders
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<string, List<IReader>> Readers = new Dictionary<string, List<IReader>>();

        /// <summary>
        /// Read tokens
        /// </summary>
        /// <param name="type">The reader type</param>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public object Read(string type, Token name, List<object> parameters, Netlist netlist)
        {
            object result = null;
            type = type?.ToLower();
            if (Readers.ContainsKey(type))
            {
                bool found = false;
                foreach (var r in Readers[type])
                {
                    if (r.Read(name, parameters, netlist))
                    {
                        found = true;
                        result = r.Generated;
                    }
                }
                if (!found)
                    throw new ParseException(name, "Unrecognized syntax");
            }
            return result;
        }

        /// <summary>
        /// Register (multiple) token readers
        /// </summary>
        /// <param name="caller">The calling object</param>
        /// <param name="type">The type</param>
        /// <param name="readers">The readers</param>
        public void Register(string type, params IReader[] readers)
        {
            type = type?.ToLower();
            if (!Readers.ContainsKey(type))
                Readers.Add(type, new List<IReader>());
            Readers[type].AddRange(readers);
        }

        /// <summary>
        /// Get a list of IReaders by their type
        /// </summary>
        /// <param name="t">The parse type</param>
        /// <returns></returns>
        public List<IReader> this[string type]
        {
            get
            {
                type = type?.ToLower();
                if (Readers.ContainsKey(type))
                    return Readers[type];
                return null;
            }
        }
    }
}