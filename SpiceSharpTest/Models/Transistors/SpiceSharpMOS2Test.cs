using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    [TestClass]
    public class SpiceSharpMOS2Test
    {
        /**
         * Note to self:
         * SmartSpice uses extended models, or propriety models for mosfet parasitic capacitances for LEVEL=1,2,3. If they are not specified by the model,
         * use CAPMOD=1 to use the legacy parasitic capacitance calculations!
         **/

        /// <summary>
        /// Test model
        /// </summary>
        public MOS2Model TestModel
        {
            get
            {
                // Taken from https://ecee.colorado.edu/~bart/book/book/chapter7/ch7_5.htm
                // .MODEL NFET NMOS (LEVEL=2 L=1u W=1u VTO=-1.44 KP=8.64E-6
                // + NSUB = 1E17 TOX = 20n)
                MOS2Model model = new MOS2Model("NFET");
                model.Set("vto", -1.44);
                model.Set("kp", 8.64e-6);
                model.Set("nsub", 1e17);
                model.Set("tox", 20e-9);
                return model;
            }
        }

        [TestMethod]
        public void TestMOS2_DC()
        {
            // Reference as simulated by SmartSpice
            // GMIN = 1e-12
            // vds,vgs: 0->5V in 0.5V steps
            double[] reference = new double[]
            {
                0.000000000000000e+000, 3.065946955721369e-005, 4.039078627363777e-005, 4.072779155040638e-005, 4.108672454042423e-005, 4.146353734283336e-005, 4.185399539701826e-005, 4.225416257546583e-005, 4.266069153043544e-005, 4.307092332607250e-005, 4.348285628966794e-005, 
                0.000000000000000e+000, 4.493808970069147e-005, 6.923332402997572e-005, 7.449410552602093e-005, 7.512597844086872e-005, 7.579626406531587e-005, 7.649731539541672e-005, 7.722141471938818e-005, 7.796157769196914e-005, 7.871198470933137e-005, 7.946808578660294e-005, 
                0.000000000000000e+000, 5.908041817046382e-005, 9.778463350994733e-005, 1.167943321413643e-004, 1.195425327455456e-004, 1.205712435609748e-004, 1.216584259883742e-004, 1.227916542338228e-004, 1.239587710920938e-004, 1.251490285747729e-004, 1.263536298135912e-004, 
                0.000000000000000e+000, 7.310065200358163e-005, 1.260544552760367e-004, 1.596008721104391e-004, 1.740536191008663e-004, 1.758187736446735e-004, 1.773511159575291e-004, 1.789649420129389e-004, 1.806419102127723e-004, 1.823645583704573e-004, 1.841177637433146e-004, 
                0.000000000000000e+000, 8.701353347404769e-005, 1.540735573642977e-004, 2.019734611329872e-004, 2.310885444218944e-004, 2.415534519252298e-004, 2.435692350157333e-004, 2.457160875293864e-004, 2.479697938169747e-004, 2.503050314981817e-004, 2.526982696432049e-004, 
                0.000000000000000e+000, 1.008333534430462e-004, 1.818734384932145e-004, 2.439613447099695e-004, 2.875310222610550e-004, 3.127631333270144e-004, 3.203033600040037e-004, 3.230122937404239e-004, 3.258880135027855e-004, 3.288977553955329e-004, 3.320082425640685e-004, 
                0.000000000000000e+000, 1.145732958649945e-004, 2.094843666782934e-004, 2.856145246732734e-004, 3.434503555793590e-004, 3.832355666188250e-004, 4.050284333037319e-004, 4.108271956550755e-004, 4.143440893953698e-004, 4.180660053772416e-004, 4.219503920151544e-004, 
                0.000000000000000e+000, 1.282450880235548e-004, 2.369340706124784e-004, 3.269807805938462e-004, 3.989166027700345e-004, 4.530426639980074e-004, 4.894846375718752e-004, 5.082148362525304e-004, 5.132999546779390e-004, 5.177427235722176e-004, 5.224307092679068e-004, 
                0.000000000000000e+000, 1.418588881405744e-004, 2.642470518485619e-004, 3.681036574037114e-004, 4.539964810343353e-004, 5.222759576587043e-004, 5.731300969411061e-004, 6.066030145306771e-004, 6.226013249312515e-004, 6.278821206131733e-004, 6.333713917446226e-004, 
                0.000000000000000e+000, 1.554233322722904e-004, 2.914443905530320e-004, 4.090214184153269e-004, 5.087505626072234e-004, 5.910224570896638e-004, 6.560791399974708e-004, 7.040322202474709e-004, 7.348646007773319e-004, 7.484678355714059e-004, 7.547220089197408e-004, 
                0.000000000000000e+000, 1.689456715735864e-004, 3.185438836083763e-004, 4.497667814047746e-004, 5.632318214562965e-004, 6.593609388105144e-004, 7.384403809941280e-004, 8.006408357386458e-004, 8.460181447317057e-004, 8.745060193465872e-004, 8.864672583316284e-004, 
            };

            // Create the circuit
            Circuit ckt = new Circuit();
            MOS2 m = new MOS2("M1");
            m.SetModel(TestModel);
            m.Connect("D", "G", "GND", "GND");
            m.Set("w", 6e-6); m.Set("l", 1e-6);
            ckt.Objects.Add(
                new Voltagesource("V1", "G", "GND", 0.0),
                new Voltagesource("V2", "D", "GND", 0.0),
                new Resistor("Rgmin", "D", "GND", 1e12), // To match SmartSpice
                m);

            // Create the simulation
            DC dc = new DC("TestMOS2_DC");
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vgs = dc.Sweeps[0].CurrentValue;
                double vds = dc.Sweeps[1].CurrentValue;

                double expected = reference[index];
                double actual = -data.Ask("V2", "i");

                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;

                Assert.AreEqual(expected, actual, tol);
                index = index + 1;
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestMOS2_AC()
        {
            // Simulation data by SmartSpice
            double[] reference_db = new double[]
            {
                -1.331580575421686e+001, -1.331580575444651e+001, -1.331580575502338e+001, -1.331580575647240e+001, -1.331580576011218e+001,
                -1.331580576925488e+001, -1.331580579222032e+001, -1.331580584990689e+001, -1.331580599480900e+001, -1.331580635878662e+001,
                -1.331580727305693e+001, -1.331580956959928e+001, -1.331581533824749e+001, -1.331582982840284e+001, -1.331586622581419e+001,
                -1.331595765063233e+001, -1.331618729090288e+001, -1.331676406762406e+001, -1.331821252742933e+001, -1.332184876434620e+001,
                -1.333096917083409e+001, -1.335379446037310e+001, -1.341060532535723e+001, -1.355011318032805e+001, -1.388196235858962e+001,
                -1.461835027871999e+001, -1.605394738403521e+001, -1.837618141287601e+001, -2.147256155852958e+001, -2.505130517786086e+001,
                -2.886699815029079e+001, -3.277985161519292e+001, -3.671567143011745e+001, -4.061799465137896e+001, -4.440479371968640e+001,
                -4.791951233475476e+001, -5.089925215047126e+001, -5.306463669076427e+001, -5.435688720829361e+001, -5.500167584892493e+001,
                -5.528761983236600e+001
            };
            double[] reference_ph = new double[]
            {
                1.799996585001908e+002, 1.799994587592772e+002, 1.799991421912631e+002, 1.799986404647725e+002, 1.799978452818736e+002,
                1.799965850019121e+002, 1.799945875927877e+002, 1.799914219126922e+002, 1.799864046479715e+002, 1.799784528197182e+002,
                1.799658500230327e+002, 1.799458759434489e+002, 1.799142191889129e+002, 1.798640467265070e+002, 1.797845291796750e+002,
                1.796585041416520e+002, 1.794587750052565e+002, 1.791422538724830e+002, 1.786407139753730e+002, 1.778462734698253e+002,
                1.765889445545543e+002, 1.746032392570457e+002, 1.714837141267253e+002, 1.666459567953569e+002, 1.593697719591175e+002,
                1.491350282230488e+002, 1.364341457825508e+002, 1.232829766706907e+002, 1.119613688005846e+002, 1.033370199601098e+002,
                9.695992720565420e+001, 9.191450870196132e+001, 8.725159100346041e+001, 8.204161846976989e+001, 7.533079523306120e+001,
                6.623876698403353e+001, 5.448282461746905e+001, 4.122442650503621e+001, 2.885020187032657e+001, 1.914224355430000e+001,
                1.234663731073372e+001
            };

            // Make the circuit
            Circuit ckt = new Circuit();
            MOS2 m = new MOS2("M1");
            m.SetModel(TestModel);
            m.Set("w", 6e-6);
            m.Set("l", 1e-6);
            m.Connect("OUT", "IN", "GND", "GND");
            Voltagesource vsrc;
            ckt.Objects.Add(
                vsrc = new Voltagesource("V1", "IN", "GND", 5.0),
                new Voltagesource("V2", "VDD", "GND", 5.0),
                new Resistor("R1", "VDD", "OUT", 1e3),
                new Capacitor("C1", "OUT", "GND", 1e-12),
                m);
            vsrc.Set("acmag", 1.0);

            // Make the simulation
            AC ac = new AC("TestMOS1_AC");
            ac.Config = new SimulationConfiguration() { Gmin = 0 }; // I simulated the original data with GMIN=0 too
            ac.Config.RelTol = 1e-6; // The DC operating point is a little bit off here
            ac.StartFreq = 1e3;
            ac.StopFreq = 100e9;
            ac.NumberSteps = 5;
            ac.StepType = AC.StepTypes.Decade;
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double frequency = data.GetFrequency();

                double expected = reference_db[index];
                double actual = data.GetDb("OUT");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                expected = reference_ph[index];
                actual = data.GetPhase("OUT");
                tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(ac);
        }
    }
}
