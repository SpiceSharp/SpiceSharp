using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using System.IO;
using SpiceSharp.Parser;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Subcircuits;

namespace SpiceSharpTest
{
    class Program
    {
        private static Dictionary<string, string> parameters = null;
        private static StreamWriter sw = null;

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

            using (sw = new StreamWriter("output.csv"))
            {
                nr.Netlist.Simulate();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Convert a Spice value from string to double
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private static void SpiceMember_SpiceMemberConvert(object sender, SpiceMemberConvertData data)
        {
            if (data.Value is string && data.TargetType == typeof(double))
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

        /// <summary>
        /// Called when a subcircuit is invoked
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void Path_OnSubcircuitPathChanged(object sender, SubcircuitPathChangedEventArgs e)
        {
            parameters = e.Parameters;
        }

        /// <summary>
        /// Write to a CSV file
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The simulation data</param>
        private static void Netlist_OnExportSimulationData(object sender, SimulationData data)
        {
            Netlist n = sender as Netlist;
            var state = n.Circuit.State;

            // Depending on the simulation type, let's write some information
            if (state.Domain == SpiceSharp.Circuits.CircuitState.DomainTypes.Frequency)
                sw.Write(data.GetFrequency().ToString());
            else if (state.Domain == SpiceSharp.Circuits.CircuitState.DomainTypes.Time)
                sw.Write(data.GetTime().ToString());

            for (int i = 0; i < n.Exports.Count; i++)
            {
                sw.Write(";");
                sw.Write(n.Exports[i].Extract(data).ToString());
            }
            sw.Write(Environment.NewLine);
        }
    }
}