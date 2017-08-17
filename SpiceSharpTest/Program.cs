using System;
using System.IO;
using System.Diagnostics;
using SpiceSharp.Parser;
using Spice2SpiceSharp;

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
            /* SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_mos1", "AN_pz", "AN_noise", "NOBYPASS", "NEWTRUNC", "PREDICTOR" });
            dev.ITF = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\mos1\mos1itf.h";
            dev.Def = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\mos1\mos1defs.h";
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\mos1";
            SpiceClassGenerator scg = new SpiceClassGenerator(dev, SpiceClassGenerator.Methods.All & ~SpiceClassGenerator.Methods.PzLoad);
            scg.ExportDevice("device.cs");
            scg.ExportModel("model.cs");

            foreach (var msg in ConverterWarnings.Warnings)
                Console.WriteLine(msg); */

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