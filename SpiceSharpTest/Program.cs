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
        private static List<double> time, output;

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            /* SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_bsim4", "AN_pz", "AN_noise", "NOBYPASS" });
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\BSIM480\BSIM480_Code";
            dev.ITF = @"bsim4itf.h";
            dev.Def = @"bsim4def.h";
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
            output = new List<double>();
            parser.Netlist.Simulations[0].ExportSimulationData += T_ExportSimulationData;
            parser.Netlist.Circuit.Simulate(parser.Netlist.Simulations[0]);

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < time.Count; i++)
                    sw.WriteLine(string.Join(";", time[i], output[i]));
            }

            Console.ReadKey();
        }

        private static void T_ExportSimulationData(object sender, SimulationData data)
        {
            time.Add(data.GetTime());
            output.Add(data.GetVoltage("2"));
        }
    }
}