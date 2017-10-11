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
            Series sref = chMain.Series.Add("Reference");
            sref.ChartType = SeriesChartType.FastPoint;
            // chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            // Reference
            double[] reference = new double[]
            {
                8.534638344391308e-14, 6.779535126890105e-14, 5.385407086373468e-14, 4.278011820970267e-14, 3.398376494660514e-14, 2.699657318711771e-14, 2.144644949112401e-14, 1.703782953318393e-14, 1.353593822442315e-14, 1.075428708293890e-14, 8.544743042104929e-15, 6.789639824603726e-15, 5.395511784087090e-15, 4.288116518683889e-15, 3.408481192374137e-15, 2.709762016425394e-15, 2.154749646826024e-15, 1.713887651032016e-15, 1.363698520155939e-15, 1.085533406007514e-15, 8.645790019241169e-16, 6.890686801739967e-16, 5.496558761223330e-16, 4.389163495820129e-16, 3.509528169510376e-16, 2.810808993561634e-16, 2.255796623962265e-16, 1.814934628168257e-16, 1.464745497292180e-16, 1.186580383143755e-16, 9.656259790603582e-17, 7.901156573102381e-17, 6.507028532585746e-17, 5.399633267182545e-17, 4.519997940872793e-17, 3.821278764924051e-17, 3.266266395324682e-17, 2.825404399530675e-17, 2.475215268654597e-17, 2.197050154506173e-17, 1.976095750422776e-17, 1.800585428672656e-17, 1.661172624620993e-17, 1.550433098080673e-17, 1.462469565449698e-17, 1.392597647854824e-17, 1.337096410894887e-17, 1.293010211315486e-17, 1.257991298227878e-17, 1.230174786813036e-17, 1.208079346404696e-17, 1.190528314229684e-17, 1.176587033824518e-17, 1.165513081170486e-17, 1.156716727907389e-17, 1.149729536147901e-17, 1.144179412451907e-17, 1.139770792493967e-17, 1.136268901185206e-17, 1.133487250043722e-17, 1.131277706002888e-17, 1.129522602785387e-17, 1.128128474744870e-17, 1.127021079479467e-17, 1.126141444153157e-17, 1.125442724977209e-17, 1.124887712607609e-17, 1.124446850611815e-17, 1.124096661480939e-17, 1.123818496366791e-17, 1.123597541962708e-17, 1.123422031640957e-17, 1.123282618836906e-17, 1.123171879310365e-17, 1.123083915777734e-17, 1.123014043860140e-17, 1.122958542623180e-17, 1.122914456423600e-17, 1.122879437510513e-17, 1.122851620999098e-17, 1.122829525558689e-17, 1.122811974526514e-17, 1.122798033246109e-17, 1.122786959293455e-17, 1.122778162940192e-17, 1.122771175748433e-17, 1.122765625624737e-17, 1.122761217004779e-17, 1.122757715113470e-17, 1.122754933462328e-17, 1.122752723918288e-17
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n Kf=1e-14 Af=0.9)",
                "V1 in 0 DC 1 AC 1 0",
                "R1 in out 10k",
                "D1 out 0 1N914",
                ".tran 1p 10n");
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                // double n = data.GetOutputNoiseDensity();
                // ns.Points.AddXY(data.GetFrequency(), Math.Log10(n) * 10.0);
                // sref.Points.AddXY(data.GetFrequency(), Math.Log10(reference[index++]) * 10.0);
                ns.Points.AddXY(data.GetTime(), data.GetVoltage(new SpiceSharp.Circuits.CircuitIdentifier("out")));
            };
            nr.Netlist.Simulate();
        }
    }
}
