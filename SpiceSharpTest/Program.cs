using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;
using SpiceSharp.Simulations;
using System.IO;
using Spice2SpiceSharp;

namespace SpiceSharpTest
{
    class Program
    {
        private static List<double> freq = new List<double>();
        private static List<double> output = new List<double>();

        static void Main(string[] args)
        {
            // Allow conversion from Spice strings to doubles
            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;

            Circuit ckt = new Circuit();

            ckt.Components.Add(
                new Voltagesource("V1", "IN", "GND", 0),
                new Resistor("R1", "IN", "OUT", "1kOhm"),
                new Capacitor("C1", "OUT", "GND", "1uF"));
            ckt.Components["V1"].Set("acmag", 1);

            MOS1 m = new MOS1("M1");
            m.Set("ic", new string[] { "1.0", "1.5" });

            AC ac = new AC("AC1", "dec", 100, 1, "10meg");
            ac.ExportSimulationData += Ac_ExportSimulationData;
            ckt.Simulate(ac);

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < freq.Count; i++)
                    sw.WriteLine(string.Join(";", new string[] { freq[i].ToString(), output[i].ToString() }));
            }

            Console.ReadKey();
        }

        private static void Ac_ExportSimulationData(object sender, SimulationData data)
        {
            freq.Add(data.GetFrequency());
            output.Add(data.GetDb("OUT"));
        }
    }
}