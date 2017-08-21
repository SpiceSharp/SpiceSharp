using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Semiconductors
{
    [TestClass]
    public class SpiceSharpDiodeTest
    {
        /// <summary>
        /// Get our test model
        /// </summary>
        private static DiodeModel TestModel
        {
            get
            {
                // Free LTSpice model (Iave, Vpk, mfg, type are all ignored by the model)
                // .model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n Iave = 200m Vpk= 75 mfg= OnSemi type= silicon)
                DiodeModel model = new DiodeModel("1N914");
                model.Set("is", 2.52e-9); model.Set("rs", 0.568);
                model.Set("n", 1.752); model.Set("cjo", 4e-12);
                model.Set("m", 0.4); model.Set("tt", 20e-9);
                model.Set("iave", 200e-3); model.Set("vpk", 75.0);
                return model;
            }
        }

        [TestMethod]
        public void TestDiodeDC()
        {
            // Expected values:
            // As simulated by ngSpice via PartSim
            double[] reference = new double[]
            {
                -2.52268494982e-09, -2.52264542588e-09, -2.52260545786e-09, -2.52256504574e-09, -2.52252396749e-09,
                -2.5224822231e-09, -2.52244070076e-09, -2.52239829024e-09, -2.52235521359e-09, -2.52231213693e-09,
                -2.52226750597e-09, -2.522222875e-09, -2.5221775779e-09, -2.52213161467e-09, -2.52208454121e-09,
                -2.52203680162e-09, -2.5219883959e-09, -2.52193887995e-09, -2.52188847583e-09, -2.52183696148e-09,
                -2.52178455895e-09, -2.52173082416e-09, -2.52167597914e-09, -2.52161957981e-09, -2.52156207026e-09,
                -2.52150300639e-09, -2.52144238821e-09, -2.52137977164e-09, -2.52131560075e-09, -2.52124943145e-09,
                -2.52118126376e-09, -2.52111065357e-09, -2.5210376009e-09, -2.52096232778e-09, -2.52088394603e-09,
                -2.52080223362e-09, -2.52071741258e-09, -2.52062970496e-09, -2.52053755645e-09, -2.52044163318e-09,
                -2.52034126902e-09, -2.5202351317e-09, -2.52012455348e-09, -2.52000798007e-09, -2.51988530042e-09,
                -2.51975551535e-09, -2.51961851383e-09, -2.51947296359e-09, -2.51931842055e-09, -2.51915399652e-09,
                -2.51897847026e-09, -2.51879106461e-09, -2.51859011424e-09, -2.51837417586e-09, -2.51814169516e-09,
                -2.51789122885e-09, -2.51761977932e-09, -2.51732568124e-09, -2.51700582599e-09, -2.51665666084e-09,
                -2.51627518821e-09, -2.51585663413e-09, -2.5153960026e-09, -2.51488729841e-09, -2.51432430431e-09,
                -2.51369869364e-09, -2.51300047438e-09, -2.51221932146e-09, -2.51134102403e-09, -2.51034992793e-09,
                -2.50922627121e-09, -2.50794640611e-09, -2.50648174438e-09, -2.50479648134e-09, -2.50284681869e-09,
                -2.50057757833e-09, -2.49791948237e-09, -2.49478421255e-09, -2.49105863714e-09, -2.48659542956e-09,
                -2.48120068935e-09, -2.47461651171e-09, -2.46649423108e-09, -2.45635467522e-09, -2.44352804657e-09,
                -2.42705877618e-09, -2.40555292352e-09, -2.37734679165e-09, -2.34199551419e-09, -2.29792254669e-09,
                -2.242974112e-09, -2.17446485817e-09, -2.0890462693e-09, -1.98254289374e-09, -1.84974841e-09,
                -1.68417076118e-09, -1.47771497816e-09, -1.22028706584e-09, -8.9930070335e-10, -4.99061559439e-10,
                4.19700400808e-23, 6.22285889484e-10, 1.3982235092e-09, 2.36575362078e-09, 3.57218556124e-09,
                5.07651057302e-09, 6.95228646874e-09, 9.29123450399e-09, 1.22077242204e-08, 1.58443660969e-08,
                2.03789850828e-08, 2.60333155311e-08, 3.30838439977e-08, 4.18753288156e-08, 5.28376579711e-08,
                6.65068700667e-08, 8.35513651043e-08, 1.04804587564e-07, 1.31305780193e-07, 1.64350799414e-07,
                2.05555478749e-07, 2.56934636733e-07, 3.2100059344e-07, 4.00886009833e-07, 5.00497056855e-07,
                6.24704400043e-07, 7.79581334487e-07, 9.72700708035e-07, 1.21350514237e-06, 1.51376863711e-06,
                1.88817210722e-06, 2.35502095414e-06, 2.93713970112e-06, 3.66298734733e-06, 4.56804782933e-06,
                5.69656336236e-06, 7.1036950634e-06, 8.85821596497e-06, 1.1045867256e-05, 1.37735405481e-05,
                1.71744886281e-05, 2.14148163157e-05, 2.6701563884e-05, 3.32927706408e-05, 4.15099988637e-05,
                5.17539120195e-05, 6.45236403702e-05, 8.04408365013e-05, 0.000100279528261, 0.000125003122406,
                0.000155810203815, 0.000194191115755, 0.000241997696509, 0.000301528980717, 0.000375636133718,
                0.000467850341022, 0.000582537763506, 0.000725085893838, 0.000902125557011, 0.00112179215756,
                0.00139402826161, 0.00173092677667, 0.00214710931727, 0.00266012723336, 0.00329086273502,
                0.00406389443207, 0.0050077760663, 0.00615516123713, 0.00754269438417, 0.00921058510944, 0.0112017962695,
                0.0135608123125, 0.0163320145243, 0.0195577669876, 0.0232763937404, 0.0275202797614, 0.032314334323,
                0.0376750064757, 0.0436099498674, 0.0501183245372, 0.0571916278654, 0.064814887987, 0.0729680375306,
                0.0816273060216, 0.0907665106088, 0.100358171633, 0.110374421085, 0.120787702896, 0.131571283445,
                0.142699600496, 0.154148481585, 0.165895261523, 0.177918824937, 0.190199595333, 0.202719487699,
                0.215461837652, 0.228411316808, 0.241553841364, 0.254876478773, 0.268367355837, 0.282015570374
            };

            // Generate the circuit
            Circuit ckt = new Circuit();
            Diode d = new Diode("D1");
            d.Connect("OUT", "GND");
            d.SetModel(TestModel);
            ckt.Objects.Add(new Voltagesource("V1", "OUT", "GND", 0.0), d);

            // Generate the simulation
            DC dc = new DC("TestDiodeDC");

            // Make the simulation slightly more accurate (error / 4)
            // Might want to check why some time though
            dc.Config.RelTol = 0.25e-3;
            dc.Sweeps.Add(new DC.Sweep("V1", -1, 1, 10e-3));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double actual = d.GetCURRENT(data.Circuit);
                double expected = reference[index++];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestDiodeAC()
        {
            // Expected values:
            // As simulated by ngSpice via PartSim
            double[] reference_amp = new double[]
            {
                -154.403442611, -150.403442674, -146.403442698, -142.403442708, -138.403442712, -134.403442714, -130.403442715, -126.403442715, -122.403442715, -118.403442715,
                -114.403442715, -110.403442715, -106.403442715, -102.403442715, -98.4034427151, -94.4034427154, -90.4034427162, -86.4034427181, -82.403442723, -78.4034427352,
                -74.4034427658
            };
            double[] reference_ph = new double[]
            {
                1.57064166816, 1.57069873347, 1.57073473321, 1.57075743795, 1.57077174851, 1.57078075383, 1.57078639771, 1.5707898984, 1.57079201152, 1.57079319318, 1.57079369844,
                1.57079363638, 1.57079299359, 1.57079163132, 1.57078925554, 1.57078535342, 1.57077908269, 1.57076908981, 1.57075321782, 1.57072804073, 1.57068812405
            };

            // Generate the circuit
            Circuit ckt = new Circuit();
            Diode d = new Diode("D1");
            d.Connect("GND", "OUT");
            d.SetModel(TestModel);
            Voltagesource vsrc = new Voltagesource("V1");
            vsrc.VSRCacMag.Set(1.0);
            vsrc.VSRCdcValue.Set(1.0);
            vsrc.Connect("OUT", "GND");
            ckt.Objects.Add(vsrc, d);

            // Generate the simulation
            AC ac = new AC("TestDiodeAC");

            // Make the simulation slightly more accurate (ref. DC)
            ac.Config.RelTol = 0.25e-3;
            ac.StartFreq = 1e3; // 1kHz
            ac.StopFreq = 10e6; // 10megHz
            ac.NumberSteps = 5;
            ac.StepType = AC.StepTypes.Decade;
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double frequency = data.GetFrequency();
                var c = -vsrc.GetComplexCurrent(data.Circuit);

                // Check the amplitude
                double expected = reference_amp[index];
                double actual = 20.0 * Math.Log10(c.Magnitude);
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-3 + 1e-6;
                Assert.AreEqual(expected, actual, tol);

                // Check the phase (radians)
                expected = reference_ph[index];
                actual = Math.Atan2(c.Imaginary, c.Real);
                tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-3 + 1e-6;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(ac);
        }
    }
}
