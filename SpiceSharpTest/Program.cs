using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;
using Spice2SpiceSharp;
using SpiceSharp.Parser;
using SpiceSharp.Parser.Readers;

namespace SpiceSharpTest
{
    class Program
    {
        private static List<double>[] exports = null;
        private static List<double> freq = new List<double>();

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            /* SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_bjt", "AN_pz", "AN_noise", "NOBYPASS", "NEWTRUNC", "PREDICTOR" });
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\bjt";
            dev.ITF = @"bjtitf.h";
            dev.Def = @"bjtdefs.h";
            SpiceClassGenerator scg = new SpiceClassGenerator(dev);
            scg.ExportModel("model.cs");
            scg.ExportDevice("device.cs");

            // Show all the warnings
            Console.WriteLine("Warnings:");
            foreach (string msg in ConverterWarnings.Warnings)
                Console.WriteLine(msg); */

            NetlistReader parser = new NetlistReader();
            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;
            parser.Parse("test.net");

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            parser.Netlist.OnExportSimulationData += Netlist_OnExportSimulationData;
            exports = new List<double>[parser.Netlist.Exports.Count];
            for (int i = 0; i < exports.Length; i++)
                exports[i] = new List<double>();
            parser.Netlist.Simulate();

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                string[] e = new string[exports.Length + 1];
                for (int k = 0; k < exports[0].Count; k++)
                {
                    e[0] = freq[k].ToString();
                    for (int i = 0; i < exports.Length; i++)
                        e[i + 1] = exports[i][k].ToString();
                    sw.WriteLine(string.Join(";", e));
                }
            }

            Console.ReadKey();
        }

        private static void Netlist_OnExportSimulationData(object sender, SimulationData data)
        {
            Netlist n = sender as Netlist;
            // exports[0].Add(data.GetTime());
            freq.Add(data.GetFrequency());
            for (int i = 0; i < n.Exports.Count; i++)
                exports[i].Add((double)n.Exports[i].Extract(data));
        }
    }
}