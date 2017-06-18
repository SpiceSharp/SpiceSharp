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
            CurrentSwitchModel csm = new CurrentSwitchModel("M1");
            csm.CSWhyst.Set(0.5e-3);
            csm.CSWthresh.Set(0.0);
            ckt.Components.Add(
                new Voltagesource("V1", "in", "GND", new Sine(0.0, 1.0, 100)),
                new Resistor("R1", "in", "GND", 1e3),
                new CurrentSwitch("CS1", "out", "GND", "V1") { Model = csm },
                new Resistor("RL", "vdd", "out", 1e3),
                new Voltagesource("VDD", "vdd", "GND", 3.3));
            ckt.Components["CS1"].Set("off");

            var sim = new Transient("Tran1", 1e-3, 10e-3);
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
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("in"));
            output.Add(data.GetVoltage("out"));
        }
    }
}