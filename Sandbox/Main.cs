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

            int selectStep = 8;

            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var plotReference = chMain.Series.Add("Reference");
            plotReference.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            plotReference.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;

            // Provided by Spice 3f5
            double[] reference = new double[]
            {
                -6.097599869520956e-37, -7.656097977815080e-13, -1.556088591314619e-12, -2.117417352565099e-12, -2.785327524179593e-12, -3.538502824085299e-12, -4.362732397567015e-12, -5.201172825763933e-12, -6.110667527536862e-12, -7.048583938740194e-12, -8.100187187665142e-12, 8.841977827306400e-12, -2.163602630389505e-12, -3.065991904804832e-12, -3.645084234449314e-12, -4.384048679639818e-12, -5.186961971048731e-12, -6.053824108676054e-12, -6.991740519879386e-12, -7.958078640513122e-12, -8.981260180007666e-12, -1.006128513836302e-11, 4.376458554330768e-10, -1.342783662039437e-10, -1.416857742242428e-10, -1.486313294662978e-10, -1.557793893880444e-10, -1.629985035833670e-10, -1.702886720522656e-10, -1.776356839400250e-10, -1.850111175372149e-10, -1.924291836985503e-10, -1.999467258428922e-10, 2.168331802615867e-08, -1.248734982084443e-08, -1.310232278228796e-08, -1.371650881765163e-08, -1.433103591352847e-08, -1.494565537996095e-08, -1.556033168981230e-08, -1.617505063222779e-08, -1.678982641806215e-08, -1.740468746902479e-08, -1.801956273084215e-08, 1.118392232919441e-06, -1.167278709246489e-06, -1.225296056617253e-06, -1.283312641930934e-06, -1.341329095794208e-06, -1.399345663344320e-06, -1.457362287737851e-06, -1.515378940553092e-06, -1.573395707055170e-06, -1.631412473557248e-06, -1.689429296902745e-06, 6.188771226674805e-05, -1.090991015093579e-04, -1.145814695533431e-04, -1.200638297049750e-04, -1.255461931037871e-04, -1.310285563675961e-04, -1.365109193756098e-04, -1.419932821988823e-04, -1.474756447947811e-04, -1.529580072059389e-04, -1.584403693470904e-04, 3.275398650966012e-03, -1.012844747622310e-02, -1.064307519869701e-02, -1.115769871949368e-02, -1.167232013649766e-02, -1.218693916585778e-02, -1.270155580220944e-02, -1.321617004552422e-02, -1.373078189587318e-02, -1.424539135324210e-02, -1.475999841764519e-02, 4.151345410274419e-02, -6.252985299133353e-01, -6.572160484331135e-01, -6.892288999647604e-01, -7.212496869557441e-01, -7.532642014839865e-01, -7.852721632567068e-01, -8.172735697595357e-01, -8.492684236147454e-01, -8.812567275385561e-01, -9.132384842460510e-01, 9.945657384334780e-02, -4.310448312607067e+00, -4.534289357487243e+00, -4.753503929852833e+00, -4.974323324491031e+00, -5.195478565023656e+00, -5.416605381122110e+00, -5.637639257872564e+00, -5.858568864300622e+00, -6.079392270876383e+00, -6.300109206559384e+00
            };

            // Parse the netlist
            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".MODEL mjd44h11 npn",
                "+ IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
                "+ IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
                "+ NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
                "+ NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
                "+ RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
                "+ EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
                "+ TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
                "+ CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
                "+ FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
                "+ TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1",
                "Q1 c b 0 0 mjd44h11",
                "V2 c 0 0",
                "V1 b 0 0",
                ".SAVE I(V2)",
                ".DC V2 0 5 0.5 V1 0 0.8 0.1"
                );
            nr.Parse(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(netlist)));

            var net = nr.Netlist;
            int index = 0;
            net.Circuit.Nodes.Map("b");
            net.Circuit.Nodes.Map("c");
            net.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                // Only handle DC analysis
                DC sim = data.Circuit.Simulation as DC;
                if (sim == null)
                    return;

                // Only handle the selectStep
                if (sim.Sweeps[0].CurrentStep != selectStep)
                {
                    index++;
                    return;
                }

                // Get the values
                double vce = sim.Sweeps[1].CurrentValue;
                double actual = net.Exports[0].Extract(data);
                double expected = reference[index++];
                double difference = actual - expected;

                plotOutput.Points.AddXY(vce, -actual);
                plotReference.Points.AddXY(vce, difference);
            };
            net.Simulate();
        }
    }
}