﻿using System.Collections.Generic;
using SpiceSharp.Parser.Readers;

namespace SpiceSharp.Parser
{
    public class Netlist
    {
        /// <summary>
        /// Available classes for reading components
        /// </summary>
        public List<IReader> ComponentReaders { get; } = new List<IReader>();

        /// <summary>
        /// Available classes for reading control statements
        /// </summary>
        public List<IReader> ControlReaders { get; } = new List<IReader>();

        /// <summary>
        /// Available classes for reading waveforms
        /// </summary>
        public List<WaveformReader> WaveformReaders { get; } = new List<WaveformReader>();

        /// <summary>
        /// Types that need to be parsed
        /// </summary>
        public enum ParseTypes
        {
            None = 0x00,
            Control = 0x01,
            Component = 0x02,
            All = 0x03
        };
        public ParseTypes Parse = ParseTypes.All;

        /// <summary>
        /// Gets the circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Gets the list of simulations to be performed
        /// </summary>
        public List<Simulation> Simulations { get; } = new List<Simulation>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public Netlist(Circuit ckt)
        {
            Circuit = ckt;
        }
    }
}
