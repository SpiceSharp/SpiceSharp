using System;
using System.IO;
using System.Diagnostics;
using SpiceSharp.Parser;

namespace SpiceSharpTest
{
    class Program
    {
        static StreamWriter writer;

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            NetlistReader nr = new NetlistReader();
            sw.Start();
            nr.Parse("test.net");
            sw.Stop();
            Console.WriteLine("Time taken to parse: " + sw.ElapsedMilliseconds + " ms");

            using (writer = new StreamWriter("output.csv"))
            {
                nr.Netlist.OnExportSimulationData += Netlist_OnExportSimulationData;
                nr.Netlist.Simulate();
            }

            foreach (var w in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine("Warning: " + w);

            Console.WriteLine("Parsing finished");
            Console.ReadKey();
        }

        private static void Netlist_OnExportSimulationData(object sender, SpiceSharp.Simulations.SimulationData data)
        {
            Netlist n = sender as Netlist;
            writer.Write(data.GetTime());
            for (int i = 0; i < n.Exports.Count; i++)
            {
                writer.Write(";");
                writer.Write(n.Exports[i].Extract(data));
            }
            writer.WriteLine();
        }
    }
}