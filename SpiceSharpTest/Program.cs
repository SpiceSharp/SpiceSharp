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
using Spice2SpiceSharp;

namespace SpiceSharpTest
{
    class Program
    {
        private static List<double> time = new List<double>();
        private static List<double> input = new List<double>();
        private static List<double> output = new List<double>();

        static void Main(string[] args)
        {
            SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_bsim3", "AN_pz", "NEWCONV", "AN_disto", "AN_noise", "NOBYPASS", "PREDICTOR" });
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\ftpv330\ftpv330\src";
            dev.ITF = "bsim3itf.h";
            dev.Def = "bsim3def.h";
            SpiceClassGenerator scg = new SpiceClassGenerator(dev);
            scg.ExportModel("model.cs");
            scg.ExportDevice("device.cs");

            foreach (string msg in ConverterWarnings.Warnings)
                Console.WriteLine(msg);

            /* Circuit ckt = new Circuit();

            ckt.Components.Add(
                new Voltagesource("V1", "IN", "GND", new Pulse(0.0, 5.0, 0, 1e-3, 1e-3, 10e-3, 30e-3)),
                new Voltagesource("Vsupply", "VDD", "GND", 5.0),
                new Resistor("R1", "VDD", "OUT", 10e3));

            MOS3Model mod = new MOS3Model("M1");
            MOS3 m = new MOS3("M1");
            m.Model = mod;
            m.Set("w", 1e-6);
            m.Set("l", 1e-6);
            m.Connect("OUT", "IN", "GND", "GND");
            ckt.Components.Add(m);

            // MOS level 3 example
            Dictionary<string, double> ps = new Dictionary<string, double>()
            {
                // + LEVEL = 3
                { "VTO", 1.922 },
                { "PHI", 0.933 },
                { "IS", 0.1E-12 },
                { "JS", 0 },
                { "THETA", 0.101E-01 },
                { "KP", 16.247 }
            };
            foreach (var p in ps)
                mod.Set(p.Key.ToLower(), p.Value);

            Transient.Configuration config = new Transient.Configuration();
            config.Step = 1e-6;
            config.FinalTime = 100e-3;
            Transient t = new Transient("TRAN1", config);
            t.ExportSimulationData += T_ExportSimulationData;
            ckt.Simulate(t);

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                sw.WriteLine("sep=;");
                for (int i = 0; i < time.Count; i++)
                {
                    sw.WriteLine(string.Join(";", time[i].ToString(), input[i].ToString(), output[i].ToString()));
                }
            }

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg); */

            Console.ReadKey();
        }

        private static void T_ExportSimulationData(object sender, SimulationData data)
        {
            // time.Add(data.GetTime());
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("IN"));
            output.Add(data.GetVoltage("OUT"));
        }
    }
}