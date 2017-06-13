using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp;
using SpiceSharp.Components;
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
            var vsrc = new Voltagesource("V1");
            vsrc.Connect("IN", "GND");
            vsrc.Set("waveform", new SpiceSharp.Components.Waveforms.Sine(0.0, 3.3, 100));
            ckt.Components.Add(vsrc);

            var res = new Resistor("R1");
            res.Connect("IN", "OUT");
            res.Set("resistance", 1e3);
            ckt.Components.Add(res);

            var dio = new Diode("D1");
            dio.Model = new DiodeModel("DM1");
            dio.Connect("OUT", "GND");
            ckt.Components.Add(dio);

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
            input.Add(data.GetVoltage("IN"));
            output.Add(data.GetVoltage("OUT"));
        }
    }
}