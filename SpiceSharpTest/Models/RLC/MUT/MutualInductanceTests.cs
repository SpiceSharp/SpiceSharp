using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class MutualInductanceTests : Framework
    {
        [Test]
        public void When_MutualInductanceTransient_Expect_Reference()
        {
            /*
             * Step function generator connect to a resistor-inductor in series, coupled to an inductor shunted by another resistor.
             * This linear circuit can be solved analytically. The result may deviate because of truncation errors (going to discrete
             * time points).
             */
            // Create circuit
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double k = 0.693;
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1)
                    .SetParameter("ic", 0.0),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", k)
                );

            // Create simulation
            var tran = new Transient("tran", 1e-9, 1e-4, 1e-6);
            tran.TimeParameters.InitialConditions["1"] = 0;

            // Create exports
            var exports = new IExport<double>[1];
            exports[0] = new RealVoltageExport(tran, "OUT");

            // Create references
            double mut = k * Math.Sqrt(l1 * l2);
            double a = l1 * l2 - mut * mut;
            double b = r1 * l2 + r2 * l1;
            double c = r1 * r2;
            double discriminant = Math.Sqrt(b * b - 4 * a * c);
            double invtau1 = (-b + discriminant) / (2.0 * a);
            double invtau2 = (-b - discriminant) / (2.0 * a);
            double factor = mut * r2 / a / (invtau1 - invtau2);
            Func<double, double>[] references = [t => factor * (Math.Exp(t * invtau1) - Math.Exp(t * invtau2))];

            // Increase the allowed threshold
            // It should also be verfied that the error decreases if the maximum timestep is decreased
            AbsTol = 1.5e-3;

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_MutualInductanceSmallSignal_Expect_Reference()
        {
            // Create circuit
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double k = 0.693;
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", k)
                );

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(1, 1e8, 10));

            // Create exports
            var exports = new IExport<Complex>[1];
            exports[0] = new ComplexVoltageExport(ac, "OUT");

            // Create references
            double mut = k * Math.Sqrt(l1 * l2);
            double a = l1 * l2 - mut * mut;
            double b = r1 * l2 + r2 * l1;
            double c = r1 * r2;
            double num = mut * r2;
            Func<double, Complex>[] references = [
                f =>
                {
                    var s = new Complex(0.0, 2.0 * Math.PI * f);
                    var denom = (a * s + b) * s + c;
                    return num * s / denom;
                }
            ];

            // Run simulation
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
