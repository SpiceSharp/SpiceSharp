using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using System.IO;
using SpiceSharp.Parser;
using SpiceSharp.Parameters;

namespace SpiceSharpTest
{
    class Program
    {
        private static List<double> dbamp = new List<double>();
        private static List<double> freq = new List<double>();
        private static Dictionary<string, string> parameters = null;

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            SpiceMember.SpiceMemberConvert += SpiceMember_SpiceMemberConvert;

            NetlistReader nr = new NetlistReader();
            nr.Netlist.OnExportSimulationData += Netlist_OnExportSimulationData;
            nr.Netlist.Path.OnSubcircuitPathChanged += Path_OnSubcircuitPathChanged;
            nr.Parse("test.net");

            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            nr.Netlist.Simulate();

            using (StreamWriter sw = new StreamWriter("output.csv"))
            {
                for (int i = 0; i < freq.Count; i++)
                {
                    sw.WriteLine(string.Join(";", freq[i], dbamp[i]));
                }
            }

            Console.ReadKey();
        }

        private static void SpiceMember_SpiceMemberConvert(object sender, SpiceMemberConvertData data)
        {
            if (data.Value is string)
            {
                if (parameters != null && parameters.ContainsKey((string)data.Value))
                {
                    string result = parameters[(string)data.Value];
                    data.Result = SpiceConvert.ToDouble(result);
                }
                else
                    data.Result = SpiceConvert.ToDouble((string)data.Value);
            }
        }

        private static void Path_OnSubcircuitPathChanged(object sender, SpiceSharp.Parser.Subcircuits.SubcircuitPathChangedEventArgs e)
        {
            parameters = e.Parameters;
        }

        private static void Netlist_OnExportSimulationData(object sender, SimulationData data)
        {
            Netlist n = sender as Netlist;
            freq.Add(data.GetFrequency());
            double d = data.GetDb("OUT");
            dbamp.Add((double)n.Exports[0].Extract(data));
        }
    }
}