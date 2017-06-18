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
                new Resistor("R1", "in", "GND", 1e3),
                new VoltageControlledVoltagesource("A1", "vcvs", "GND", "in", "GND", 0.75),
                new CurrentControlledVoltagesource("B1", "ccvs", "vcvs", "V1", 0.25),
                new VoltageControlledCurrentsource("C1", "vccs", "GND", "in", "GND", 0.2),
                new Resistor("R2", "vccs", "GND", 1e3),
                new CurrentControlledCurrentsource("D1", "cccs", "GND", "V1", 0.9),
                new Resistor("R3", "cccs", "GND", 1e3));

            DC sim = new DC("DC1");
            DC.Sweep sweep = new DC.Sweep("V1", 0.0, 1.0, 1e-3);
            sim.Sweeps.Add(sweep);
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
            time.Add(data.GetVoltage("in"));
            input.Add(data.GetVoltage("vccs"));
            output.Add(data.GetVoltage("cccs"));
        }
    }
}