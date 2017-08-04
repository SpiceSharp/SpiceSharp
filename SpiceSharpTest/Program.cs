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
        private static List<double> time, input, output;

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

            NetlistReader parser = new NetlistReader(new FileStream("test.net", FileMode.Open));
            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;
            parser.Parse();

            time = new List<double>();
            input = new List<double>();
            output = new List<double>();
            parser.Netlist.Simulations[0].ExportSimulationData += T_ExportSimulationData;
            parser.Netlist.Circuit.Simulate(parser.Netlist.Simulations[0]);

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                sw.WriteLine("Time;Output");
                for (int i = 0; i < time.Count; i++)
                    sw.WriteLine(string.Join(";", time[i], output[i]));
            }

            Console.ReadKey();
        }

        private static void T_ExportSimulationData(object sender, SimulationData data)
        {
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("IN"));
            output.Add(data.GetVoltage("OUT"));
        }
    }
}