using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
            var plotInput = chMain.Series.Add("Input");
            plotInput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;

            double[] tref = { 0.000000000000000e+00, 1.000000000000000e-10, 2.000000000000000e-10, 4.000000000000000e-10, 8.000000000000000e-10, 1.600000000000000e-09, 3.200000000000000e-09, 6.400000000000000e-09, 1.280000000000000e-08, 2.560000000000000e-08, 5.120000000000000e-08, 1.024000000000000e-07, 2.048000000000000e-07, 4.096000000000000e-07, 8.192000000000000e-07, 1.638400000000000e-06, 3.276800000000000e-06, 6.553600000000000e-06, 1.310720000000000e-05, 2.621440000000000e-05, 5.242880000000000e-05, 1.048576000000000e-04, 2.097152000000000e-04, 4.194304000000000e-04, 8.388608000000000e-04, 1.677721600000000e-03, 3.355443200000000e-03, 5.355443200000001e-03, 7.355443200000001e-03, 9.355443200000001e-03, 1.135544320000000e-02, 1.335544320000000e-02, 1.535544320000000e-02, 1.735544320000000e-02, 1.935544320000000e-02, 2.135544320000000e-02, 2.335544320000001e-02, 2.535544320000001e-02, 2.735544320000001e-02, 2.935544320000001e-02, 3.135544320000001e-02, 3.335544320000002e-02, 3.535544320000002e-02, 3.735544320000002e-02, 3.935544320000002e-02, 4.135544320000002e-02, 4.335544320000002e-02, 4.535544320000003e-02, 4.735544320000003e-02, 4.935544320000003e-02, 5.135544320000003e-02, 5.335544320000003e-02, 5.535544320000003e-02, 5.735544320000004e-02, 5.935544320000004e-02, 6.135544320000004e-02, 6.335544320000004e-02, 6.535544320000004e-02, 6.735544320000005e-02, 6.935544320000005e-02, 7.135544320000005e-02, 7.335544320000005e-02, 7.535544320000005e-02, 7.735544320000005e-02, 7.935544320000006e-02, 8.135544320000006e-02, 8.335544320000006e-02, 8.535544320000006e-02, 8.735544320000006e-02, 8.935544320000006e-02, 9.135544320000007e-02, 9.335544320000007e-02, 9.535544320000007e-02, 9.735544320000007e-02, 9.935544320000007e-02, 1.000000000000000e-01 };
            double[] vref = { 0.000000000000000e+00, 9.999999900000002e-08, 1.999999970000001e-07, 3.999999910000003e-07, 7.999999670000012e-07, 1.599999871000007e-06, 3.199999487000059e-06, 6.399997951000471e-06, 1.279999180700375e-05, 2.559996723102996e-05, 5.119986892723967e-05, 1.023994757129174e-04, 2.047979028623391e-04, 4.095916115137118e-04, 8.191664465486828e-04, 1.638265790124269e-03, 3.276263191910061e-03, 6.551453018886084e-03, 1.309861408489752e-02, 2.618007240491956e-02, 5.229161799088397e-02, 1.043098965166932e-01, 2.075325440220415e-01, 4.107644303401534e-01, 8.047045973172554e-01, 1.545011174702788e+00, 2.853738876045696e+00, 4.153059080401023e+00, 5.216139247600837e+00, 6.085932111673411e+00, 6.797580818641880e+00, 7.379838851616082e+00, 7.856231787685882e+00, 8.246007826288444e+00, 8.564915494235999e+00, 8.825839949829453e+00, 9.039323595315008e+00, 9.213992032530461e+00, 9.356902572070375e+00, 9.473829377148489e+00, 9.569496763121487e+00, 9.647770078917578e+00, 9.711811882750743e+00, 9.764209722250603e+00, 9.807080681841400e+00, 9.842156921506600e+00, 9.870855663050854e+00, 9.894336451587058e+00, 9.913548005843962e+00, 9.929266550235969e+00, 9.942127177465792e+00, 9.952649508835648e+00, 9.961258689047348e+00, 9.968302563766011e+00, 9.974065733990370e+00, 9.978781055083031e+00, 9.982639045067927e+00, 9.985795582328299e+00, 9.988378203723153e+00, 9.990491257591668e+00, 9.992220119847724e+00, 9.993634643511774e+00, 9.994791981055087e+00, 9.995738893590520e+00, 9.996513640210422e+00, 9.997147523808524e+00, 9.997666155843335e+00, 9.998090491144543e+00, 9.998437674572804e+00, 9.998721733741380e+00, 9.998954145788399e+00, 9.999144301099591e+00, 9.999299882717844e+00, 9.999427176769142e+00, 9.999531326447478e+00, 9.999560591955882e+00 };

            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new Voltagesource("V1", "IN", "0", dcVoltage)
                );
            ckt.Nodes.IC["OUT"] = 0.0;

            // Create simulation, exports and references
            Transient tran = new Transient("tran", 1e-8, 10e-2);
            Func<State, double> export = null;
            Func<double, double> reference = (double t) => dcVoltage * (1.0 - Math.Exp(-t / tau));

            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                export = tran.CreateExport("C1", "v");
            };
            tran.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                plotInput.Points.AddXY(data.GetTime(), data.GetVoltage("OUT"));
            };
            tran.Run(ckt);

            for (int i = 0; i < tref.Length; i++)
            {
                plotOutput.Points.AddXY(tref[i], vref[i]);
            }
        }

        /// <summary>
        /// Assign parameters to an entity
        /// </summary>
        /// <param name="src">String with parameters</param>
        /// <param name="e">Entity</param>
        void AssignParameters(string src, Entity e)
        {
            // Split in parameters
            string[] assignments = src.Split(' ');

            // Assign parameters
            foreach (var assignment in assignments)
            {
                // Split in name and value
                if (string.IsNullOrWhiteSpace(assignment))
                    continue;
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                    continue;
                string parameter = parts[0].Trim();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Assign the parameter
                e.Parameters.Set(parameter.ToLower(), value);
            }
        }
    }
}