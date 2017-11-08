using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;
using SpiceSharp.Circuits;

namespace Sandbox
{
    public partial class Main : Form
    {
        public List<double> input = new List<double>();
        public List<double> output = new List<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double dcVoltage = 10;
            double resistorResistance = 1e4;
            double capacitance = 1e-6;

            double tau = resistorResistance * capacitance;

            Circuit ckt = new Circuit();

            var capacitor = new Capacitor(
                new CircuitIdentifier("C_1"),
                new CircuitIdentifier("OUT"),
                new CircuitIdentifier("gnd"),
                capacitance
            );

            ckt.Objects.Add(
                new Voltagesource(
                    new CircuitIdentifier("V_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    dcVoltage),
                new Resistor(
                    new CircuitIdentifier("R_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("OUT"),
                    resistorResistance),
                capacitor
            );

            double maxVoltage = 0;
            Transient trans = new Transient("T", 1e-3, 5 * tau);
            trans.Circuit = ckt; //TODO: refactor this ..
            ckt.Simulation = trans; //TODO: refactor this ..
            trans.CurrentConfig.UseIC = true;
            trans.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var outVoltage = data.GetVoltage(new CircuitIdentifier("OUT"), new CircuitIdentifier("gnd"));
                if (outVoltage > maxVoltage)
                {
                    maxVoltage = outVoltage;
                }
                plotOutput.Points.AddXY(data.GetTime(), data.GetVoltage("OUT"));
            };
            trans.SetupAndExecute();
        }
    }
}