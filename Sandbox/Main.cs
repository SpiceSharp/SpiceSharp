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
            Series rs = chMain.Series.Add("Reference");
            rs.ChartType = SeriesChartType.FastPoint;

            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            double[] reference = new double[]
            {
                2.970552105465777e-07, 2.053392001294538e-07, 1.425716401150457e-07, 1.005637052541777e-07, 7.253421438447867e-08, 5.355847444717597e-08, 4.037521102279383e-08, 3.093870755193646e-08, 2.399259594639712e-08, 1.876085041593681e-08, 1.475173003376309e-08, 1.164174697973875e-08, 9.209128460130984e-09, 7.295852862170559e-09, 5.785653492254035e-09, 4.590866976751766e-09, 3.644227374777685e-09, 2.893495350636724e-09, 2.297774776401085e-09, 1.824881474232857e-09, 1.449401554896117e-09, 1.151223737306888e-09, 9.144110142143362e-10, 7.263231967747751e-10, 5.769293633775424e-10, 4.582664502662651e-10, 3.640115671591474e-10, 2.891434647260522e-10, 2.296742247661058e-10, 1.824364329323833e-10, 1.449142737768106e-10, 1.151094398350513e-10, 9.143465704637279e-11, 7.262912784315901e-11, 5.769137465933203e-11, 4.582590036912043e-11, 3.640082154126390e-11, 2.891421652629465e-11, 2.296739538836536e-11, 1.824366775621906e-11, 1.449147767750264e-11, 1.151100723242943e-11, 9.143535443489147e-12, 7.262985775837507e-12, 5.769212087651685e-12, 4.582665475664515e-12, 3.640158002365927e-12, 2.891497706098719e-12, 2.296815695164321e-12, 1.824442983501082e-12, 1.449224001466344e-12, 1.151176969908153e-12, 9.144297975040658e-13, 7.263748339915787e-13, 5.769974668031978e-13, 4.583428064215170e-13, 3.640920595011466e-13, 2.892260300796565e-13, 2.297578290890759e-13, 1.825205579743038e-13, 1.449986597966671e-13, 1.151939566537973e-13, 9.151923941987867e-14, 7.271374307188275e-14, 5.777600635467486e-14, 4.591054031732387e-14, 3.648546562569634e-14, 2.899886268375259e-14, 2.305204258479738e-14, 1.832831547337173e-14, 1.457612565563390e-14, 1.159565534135988e-14, 9.228183617974503e-15, 7.347633983178169e-15, 5.853860311459011e-15, 4.667313707724732e-15, 3.724806238562389e-15, 2.976145944368217e-15, 2.381463934472801e-15, 1.909091223330287e-15, 1.533872241556531e-15, 1.235825210129142e-15, 9.990780377906112e-16, 8.110230743109810e-16, 6.616457071390669e-16, 5.429910467656397e-16, 4.487402998494061e-16, 3.738742704299892e-16, 3.144060694404477e-16, 2.671687983261964e-16, 2.296469001488209e-16
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                "M1 out g 0 0 NFET L = 6u W = 1u",
                ".MODEL NFET NMOS(LEVEL = 2 L = 1u W = 1u VTO = -1.44 KP = 8.64E-6 NSUB = 1E17 TOX = 20n KF = 0.5e-25)",
                "V1 in 0 DC 0 AC 1 0",
                "V2 vdd 0 DC 5.0",
                "R1 vdd out 10k",
                "R2 out g 10k",
                "Cin in g 1u",
                ".NOISE v(out) V1 dec 10 10 10g");
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double n = 10.0 * Math.Log10(data.GetOutputNoiseDensity());
                ns.Points.AddXY(data.GetFrequency(), n);

                n = 10.0 * Math.Log10(reference[index++]);
                rs.Points.AddXY(data.GetFrequency(), n);
            };
            nr.Netlist.Simulate();
        }
    }
}
