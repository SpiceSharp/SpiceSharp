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
            /* SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_mos2", "AN_pz", "NEWCONV", "AN_disto", "AN_noise", "NOBYPASS", "PREDICTOR" });
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\mos2";
            dev.ITF = "mos2itf.h";
            dev.Def = "mos2defs.h";
            SpiceClassGenerator scg = new SpiceClassGenerator(dev);
            scg.ExportModel("model.cs");
            scg.ExportDevice("device.cs");

            foreach (string msg in ConverterWarnings.Warnings)
                Console.WriteLine(msg); */

            Circuit ckt = new Circuit();

            ckt.Components.Add(
                new Voltagesource("V1", "IN", "GND", new Pulse(0.0, 5.0, 0, 1e-3, 1e-3, 10e-3, 30e-3)),
                new Voltagesource("Vsupply", "VDD", "GND", 5.0),
                new Resistor("R1", "VDD", "OUT", 10e3));

            MOS1Model mod = new MOS1Model("M1");
            MOS1 m = new MOS1("M1");
            m.Model = mod;
            m.MOS1w.Set(1e-6);
            m.MOS1l.Set(1e-6);
            m.MOS1drainArea.Set(1e-12);
            m.MOS1sourceArea.Set(1e-12);
            m.Connect("OUT", "IN", "GND", "GND");
            ckt.Components.Add(m);

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

            Console.ReadKey();
        }

        private static void T_ExportSimulationData(object sender, SimulationData data)
        {
            time.Add(data.GetTime());
            input.Add(data.GetVoltage("IN"));
            output.Add(data.GetVoltage("OUT"));
        }
    }
}