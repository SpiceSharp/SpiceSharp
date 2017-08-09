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
        private static List<double> dbamp = new List<double>();
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

            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;
            NetlistReader nr = new NetlistReader();
            nr.Parse("test.net");

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            nr.Netlist.OnExportSimulationData += Netlist_OnExportSimulationData;
            nr.Netlist.Simulate();

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < freq.Count; i++)
                {
                    sw.WriteLine(string.Join(";", freq[i], dbamp[i]));
                }
            }

            Console.ReadKey();
        }

        private static void Netlist_OnExportSimulationData(object sender, SimulationData data)
        {
            Netlist n = sender as Netlist;
            freq.Add(data.GetFrequency());
            double d = data.GetDb("OUT");
            dbamp.Add((double)n.Exports[0].Extract(data));
        }
    }
}