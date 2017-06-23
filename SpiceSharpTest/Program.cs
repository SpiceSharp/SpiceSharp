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
            var vsm = new VoltageSwitchModel("M1");
            vsm.VSWhyst.Set(0.5);
            vsm.VSWthresh.Set(0.0);
            ckt.Components.Add(
                new Voltagesource("V1", "in", "GND", new Pulse(0.0, 3.3, 1e-3, 1e-6, 1e-6, 10e-3, 30e-3)),
                new Resistor("R1", "in", "out", 1e3),
                new Capacitor("C1", "out", "GND", 1e-6));
                
            // ckt.Components["CS1"].Set("off");

            var sim = new Transient("Tran1", 0.01e-3, 30e-3);
            // ((Transient.Configuration)sim.Config).MaxStep = 10e-3;
            sim.ExportSimulationData += GetSimulation;
            sim.TimestepCut += Sim_TimestepCut;
            ckt.Simulate(sim);

            // Display all the values
            using (StreamWriter writer = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < time.Count; i++)
                    writer.WriteLine(time[i] + ";" + input[i] + ";" + output[i]);
            }

            Console.ReadKey();
        }

        private static void Sim_TimestepCut(object sender, TimestepCutData data)
        {
            // Console.WriteLine($"Rejected at time {data.Circuit.Method.Time - data.Circuit.Method.Delta}");
            // Console.WriteLine($"Cutting timestep from {data.Circuit.Method.Delta} to {data.NewDelta}. Reason: {data.Reason}");
            // Console.WriteLine($"We predicted {data.Circuit.Method.Prediction} but got {data.Circuit.State.Real.Solution}");
        }

        private static void GetSimulation(object sim, SimulationData data)
        {
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("in"));
            output.Add(data.GetVoltage("out"));
            // Console.WriteLine(data.GetTime());
        }
    }
}