using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;

namespace SpiceSharpTest
{
    class Program
    {
        /// <summary>
        /// Output variables
        /// </summary>
        private static List<double> freq = new List<double>();
        private static List<double> output = new List<double>();

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            // Allow conversion from Spice strings to doubles
            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;

            // Create a circuit object
            Circuit ckt = new Circuit();

            // Create all components
            ckt.Components.Add(
                new Voltagesource("V1", "IN", "GND", 0),
                new Resistor("R1", "IN", "OUT", "1k"),
                new Capacitor("C1", "OUT", "GND", "1u"));
            ckt.Components["V1"].Set("acmag", 1);

            // Run the simulation
            AC ac = new AC("AC1", "dec", 5, 1, "10meg");
            ac.ExportSimulationData += Ac_ExportSimulationData;
            ckt.Simulate(ac);

            // Show all the warnings
            Console.WriteLine("Warnings:");
            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            Console.ReadKey();
        }

        /// <summary>
        /// Export simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private static void Ac_ExportSimulationData(object sender, SimulationData data)
        {
            double f = data.GetFrequency();
            double a = data.GetDb("OUT");

            Console.WriteLine(f + " - db = " + a);
        }
    }
}