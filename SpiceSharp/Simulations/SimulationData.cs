using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Simulation data
    /// </summary>
    public class SimulationData
    {
        /// <summary>
        /// The circuit
        /// </summary>
        public Circuit Circuit { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ckt"></param>
        public SimulationData(Circuit ckt)
        {
            Circuit = ckt;
        }

        /// <summary>
        /// Get the voltage of a node in DC or Transient analysis
        /// </summary>
        /// <param name="node">The node name</param>
        /// <param name="reference">The reference (null if no reference)</param>
        /// <returns></returns>
        public double GetVoltage(string node, string reference = null)
        {
            double result = 0.0;

            // Get the positive node
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (Circuit.Nodes.Contains(node))
            {
                int index = Circuit.Nodes[node].Index;
                result = Circuit.State.Real.Solution[index];
            }
            else
                throw new CircuitException($"Could not find node '{node}'");

            // Get the negative node
            if (reference != null)
            {
                if (Circuit.Nodes.Contains(reference))
                {
                    int index = Circuit.Nodes[node].Index;
                    result -= Circuit.State.Real.Solution[index];
                }
                else
                    throw new CircuitException($"Could not find node '{reference}'");
            }

            return result;
        }

        /// <summary>
        /// Get the current timepoint
        /// </summary>
        /// <returns></returns>
        public double GetTime()
        {
            if (Circuit.Method != null)
                return Circuit.Method.Time;
            throw new CircuitException("Cannot get time without integration method");
        }

        /// <summary>
        /// Get a phasor
        /// This will give you the complex phasor of a node when doing an AC analysis
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="reference">The reference node (null if no reference)</param>
        /// <returns></returns>
        public Complex GetPhasor(string node, string reference = null)
        {
            Complex result;

            // Get the positive node
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (Circuit.Nodes.Contains(node))
            {
                int index = Circuit.Nodes[node].Index;
                result = Circuit.State.Complex.Solution[index].Magnitude;
            }
            else
                throw new CircuitException($"Could not find node '{node}'");

            // Get the negative node
            if (reference != null)
            {
                if (Circuit.Nodes.Contains(reference))
                {
                    int index = Circuit.Nodes[node].Index;
                    result -= Circuit.State.Complex.Solution[index];
                }
                else
                    throw new CircuitException($"Could not find node '{reference}'");
            }

            return result;
        }

        /// <summary>
        /// Get the amplitude in dB
        /// </summary>
        /// <param name="node">The node name</param>
        /// <returns></returns>
        public double GetDb(string node)
        {
            Complex r = GetPhasor(node);
            return 10.0 * Math.Log10(r.Real * r.Real + r.Imaginary * r.Imaginary);
        }

        /// <summary>
        /// Get the current frequency
        /// </summary>
        /// <returns></returns>
        public double GetFrequency()
        {
            var c = Circuit.State.Complex.Laplace;
            if (c.Real != 0.0)
                throw new CircuitException($"Cannot get the frequency of the complex number {c}");
            return c.Imaginary / 2.0 / Circuit.CONSTPI;
        }
    }

    /// <summary>
    /// Delegate for exporting simulation data
    /// </summary>
    /// <param name="sender">The sender of the simulation</param>
    /// <param name="data">The simulation data</param>
    public delegate void ExportSimulationDataEventHandler(object sender, SimulationData data);
}
