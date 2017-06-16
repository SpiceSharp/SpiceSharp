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
            Circuit ckt = new Circuit();

            ckt.Components.Add(
                new Voltagesource("V1", "in", "GND", new Sine(1.0, 0.2, 100)),
                new Voltagesource("Vdd", "vdd", "GND", 3.3),
                new Resistor("Rb", "in", "b", 1e3),
                new Resistor("Re", "vdd", "out", 1e3));
            var bip = new Bipolar("B1");
            bip.Connect("out", "b", "GND", "GND");
            bip.Model = new BipolarModel("BM1");
            ckt.Components.Add(bip);

            ckt.Setup();
            foreach (var c in ckt.Components)
                c.Temperature(ckt);

            Transient.Configuration config = new Transient.Configuration();
            config.FinalTime = 30e-3;
            config.Step = 100e-6;
            Transient sim = new Transient("TRAN1", config);
            sim.ExportSimulationData += GetSimulation;
            sim.Execute(ckt);

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
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("in"));
            output.Add(data.GetVoltage("out"));
        }
    }
}