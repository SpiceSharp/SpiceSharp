using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;
using SpiceSharp.Simulations;
using System.IO;

namespace SpiceSharpTest
{
    class Program
    {
        private static List<double> time = new List<double>();
        private static List<double> input = new List<double>();
        private static List<double> output = new List<double>();

        static void Main(string[] args)
        {
            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Components.Add(
                new Voltagesource("V1", "in", "GND", 0.0),
                new Resistor("R", "in", "out", 1e3),
                new Capacitor("C", "out", "GND", 1e-6 / 2.0 / Math.PI));
            ckt.Components["V1"].Set("acmag", 1.0);

            AC sim = new AC("ac1", AC.StepTypes.Decade, 1024, 1, 1e6);
            sim.ExportSimulationData += GetSimulation;
            ckt.Simulate(sim);

            // Display all the values
            using (StreamWriter writer = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < time.Count; i++)
                    writer.WriteLine(time[i] + ";" + input[i] + ";" + output[i]);
            }

            Console.ReadKey();
        }

        private static void GetSimulation(object sim, SimulationData data)
        {
            time.Add(Math.Log10(data.GetFrequency()));
            input.Add(data.GetDb("in"));
            output.Add(data.GetDb("out"));
        }
    }
}