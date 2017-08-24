using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// The testing model
        /// </summary>
        public MOS3Model TestModel
        {
            get
            {
                // Model part of the FDC604P (ONSemi)
                // M1 2 1 4x 4x DMOS L = 1u W = 1u
                // .MODEL DMOS PMOS(VTO= -0.7 KP= 3.8E+1 THETA = .25 VMAX= 3.5E5 LEVEL= 3)
                MOS3Model model = new MOS3Model("DMOS");
                model.SetPMOS(true);
                model.Set("vto", -0.7);
                model.Set("kp", 3.8e1);
                model.Set("theta", 0.25);
                model.Set("vmax", 3.5e5);
                return model;
            }
        }

        // Simulation data by SmartSpice
        // vds/vgs sweep 0V to 5V / 0.5V
        // GMIN = 0
        /* double[] reference = new double[]
        {
                -0.000000000000000e+000, -1.193317505745569e-012, -2.386635011491139e-012, -3.579952517236708e-012, -4.773270022982277e-012, -5.966587528727846e-012, -7.159905034473416e-012, -8.353222540218986e-012, -9.546540045964554e-012, -1.073985755171012e-011, -1.193317505745569e-011,
                -0.000000000000000e+000, -1.193317505745569e-012, -2.386635011491139e-012, -3.579952517236708e-012, -4.773270022982277e-012, -5.966587528727846e-012, -7.159905034473416e-012, -8.353222540218986e-012, -9.546540045964554e-012, -1.073985755171012e-011, -1.193317505745569e-011,
                -0.000000000000000e+000, -1.303055880462224e+000, -1.303854453474042e+000, -1.304654001252499e+000, -1.305454525583467e+000, -1.306256028257186e+000, -1.307058511068305e+000, -1.307861975815817e+000, -1.308666424303170e+000, -1.309471858338180e+000, -1.310278279733131e+000,
                -0.000000000000000e+000, -7.485888108118882e+000, -7.889439814532503e+000, -7.899015098439936e+000, -7.908613435292367e+000, -7.918234908449147e+000, -7.927879601672001e+000, -7.937547599127484e+000, -7.947238985389417e+000, -7.956953845441355e+000, -7.966692264679018e+000,
                -0.000000000000000e+000, -1.355675878208314e+001, -1.810901879639731e+001, -1.815513858017998e+001, -1.818280716313539e+001, -1.821055921660628e+001, -1.823839511893684e+001, -1.826631525076166e+001, -1.829431999502235e+001, -1.832240973698574e+001, -1.835058486426072e+001,
                -0.000000000000000e+000, -1.863934818354159e+001, -2.843101073940651e+001, -3.080744511962866e+001, -3.085913477074452e+001, -3.091099591369201e+001, -3.096302940347890e+001, -3.101523610080663e+001, -3.106761687211694e+001, -3.112017258964091e+001, -3.117290413144557e+001,
                -0.000000000000000e+000, -2.295681659983424e+001, -3.727541692370272e+001, -4.416818220331999e+001, -4.518945364851280e+001, -4.526846247250650e+001, -4.534774432179258e+001, -4.542730061429560e+001, -4.550713277777779e+001, -4.558724224991991e+001, -4.566763047841186e+001,
                -0.000000000000000e+000, -2.666983943784593e+001, -4.493831846505864e+001, -5.590926545675782e+001, -6.050125087895213e+001, -6.081239739093378e+001, -6.092000947136231e+001, -6.102799787624304e+001, -6.113636458344637e+001, -6.124511158472826e+001, -6.135424088585021e+001,
                -0.000000000000000e+000, -2.989704620359440e+001, -5.164164378477782e+001, -6.624061315660958e+001, -7.454154119070969e+001, -7.723367370315049e+001, -7.736950187368520e+001, -7.750580213256292e+001, -7.764257694573968e+001, -7.777982879637786e+001, -7.791756018499366e+001,
                -0.000000000000000e+000, -3.272792933144395e+001, -5.755501494224056e+001, -7.540174347945555e+001, -8.705095872704199e+001, -9.317266212171649e+001, -9.448444386088325e+001, -9.464824632114627e+001, -9.481261010276764e+001, -9.497753809654854e+001, -9.514303321317586e+001,
                -0.000000000000000e+000, -3.523127226268278e+001, -6.281030027931791e+001, -8.358082307856608e+001, -9.826693575001642e+001, -1.074935705749834e+002, -1.118029101809799e+002, -1.123057273638063e+002, -1.124964906880771e+002, -1.126878946743418e+002, -1.128799425560088e+002
        }; */
        // Simulated data by LTSpiceXVII
        // GMIN = 0
        // vds, vgs: 0->5V, 0.5V steps
        double[] reference = new double[]
        {
            0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
            0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
            0.000000e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, 
            0.000000e+000, -8.127778e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, 
            0.000000e+000, -1.414177e+001, -2.031503e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, 
            0.000000e+000, -1.917674e+001, -3.046696e+001, -3.505858e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, 
            0.000000e+000, -2.345376e+001, -3.916565e+001, -4.822222e+001, -5.151583e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, 
            0.000000e+000, -2.713200e+001, -4.670229e+001, -5.970438e+001, -6.696503e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, 
            0.000000e+000, -3.032897e+001, -5.329517e+001, -6.980789e+001, -8.063262e+001, -8.641838e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, 
            0.000000e+000, -3.313334e+001, -5.911111e+001, -7.876699e+001, -9.280997e+001, -1.018468e+002, -1.064000e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, 
            0.000000e+000, -3.561322e+001, -6.427981e+001, -8.676569e+001, -1.037282e+002, -1.157347e+002, -1.232772e+002, -1.267850e+002, -1.269506e+002, -1.269506e+002, -1.269506e+002
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            Series[] series = new Series[11];
            Series[] refseries = new Series[series.Length];
            Series[] diffseries = new Series[series.Length];
            for (int i = 0; i < series.Length; i++)
            {
                series[i] = chMain.Series.Add("Ids (" + i + ")");
                series[i].ChartType = SeriesChartType.FastLine;
                refseries[i] = chMain.Series.Add("Reference (" + i + ")");
                refseries[i].ChartType = SeriesChartType.FastLine;
                refseries[i].BorderDashStyle = ChartDashStyle.Dash;
                diffseries[i] = chMain.Series.Add("Difference (" + i + ")");
                diffseries[i].ChartType = SeriesChartType.FastPoint;
                diffseries[i].YAxisType = AxisType.Secondary;
            }
            // chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            SpiceSharp.Diagnostics.CircuitWarning.WarningGenerated += CircuitWarning_WarningGenerated;

            // Create the circuit
            Circuit ckt = new Circuit();
            MOS3 m = new MOS3("M1");
            m.Set("w", 1e-6); m.Set("l", 1e-6);
            m.SetModel(TestModel);
            m.Connect("D", "G", "0", "0");
            ckt.Objects.Add(
                new Voltagesource("V1", "0", "G", 0.0),
                new Voltagesource("V2", "0", "D", 0.0),
                m);

            // Create the simulation
            DC dc = new DC("TestMOS3_DC");
            dc.Config.Gmin = 1e-12;
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps[1].CurrentValue;
                double expected = reference[index];
                double actual = data.Ask("V2", "i");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;

                int r = index / 11;
                series[r].Points.AddXY(vds, actual);
                refseries[r].Points.AddXY(vds, expected);
                if (Math.Abs(expected) > 1e-14)
                    diffseries[r].Points.AddXY(vds, actual - expected);

                index++;
            };
            ckt.Simulate(dc);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
