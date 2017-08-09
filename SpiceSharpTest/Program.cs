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

            Circuit ckt = new Circuit();
            ckt.Components.Add(
                new Voltagesource("V1", "1", "0", 1.0));
            ckt.Components["V1"].Set("acmag", 1.0);

            var sub = new Subcircuit("X1", "IN", "OUT");
            sub.Components.Add(
                new Resistor("R1", "IN", "OUT", 10e3),
                new Capacitor("C1", "OUT", "0", 1e-9));
            sub.Connect("1", "2");
            ckt.Components.Add(sub);

            AC ac = new AC("AC 1");
            ac.StartFreq = 100;
            ac.NumberSteps = 100;
            ac.StopFreq = 10e9;
            ac.StepType = AC.StepTypes.Decade;
            ac.ExportSimulationData += Ac_ExportSimulationData;

            ckt.Simulate(ac);

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < freq.Count; i++)
                {
                    sw.WriteLine(string.Join(";", freq[i], dbamp[i]));
                }
            }

            Console.ReadKey();
        }

        private static void Ac_ExportSimulationData(object sender, SimulationData data)
        {
            freq.Add(data.GetFrequency());
            dbamp.Add(data.GetDb("2"));
        }
    }
}