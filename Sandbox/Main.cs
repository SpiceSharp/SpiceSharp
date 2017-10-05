using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Designer;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;

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

            Series ns = chMain.Series.Add("Output noise density");
            ns.ChartType = SeriesChartType.FastLine;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 10e3),
                new Capacitor("C1", "out", "0", 1e-9));
            (ckt.Objects["V1"] as Voltagesource).VSRCacMag.Set(1.0);

            Noise noise = new Noise("Noise 1");
            noise.StartFreq = 10.0;
            noise.StopFreq = 10e9;
            noise.StepType = Noise.StepTypes.Decade;
            noise.NumberSteps = 10;
            noise.Input = ckt.Objects["V1"];
            noise.Output = "out";
            noise.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                ns.Points.AddXY(data.GetFrequency(), Math.Log10(data.Circuit.State.Noise.outNdens * data.Circuit.State.Noise.GainSqInv) * 10.0);
            };
            ckt.Simulate(noise);
        }
    }
}
