using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.NewSparse;

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

            var output = chMain.Series.Add("Output");
            output.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             */
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage)
                );
            ckt.Nodes.InitialConditions["OUT"] = 0.0;

            // Create simulation, exports and references
            Transient tran = new Transient("tran", 1e-8, 10e-6);
            Export<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { (double t) => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            tran.OnExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                output.Points.AddXY(args.Time, exports[0].Value);
            };
            tran.Run(ckt);
        }
    }
}