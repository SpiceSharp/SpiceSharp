using System;
using System.Numerics;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Exported simulation data. Can be used by simulations to pass exported simulation data as an event argument.
    /// </summary>
    public class SimulationData : EventArgs
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
                    int index = Circuit.Nodes[reference].Index;
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
                result = Circuit.State.Complex.Solution[index];
            }
            else
                throw new CircuitException($"Could not find node '{node}'");

            // Get the negative node
            if (reference != null)
            {
                if (Circuit.Nodes.Contains(reference))
                {
                    int index = Circuit.Nodes[reference].Index;
                    result -= Circuit.State.Complex.Solution[index];
                }
                else
                    throw new CircuitException($"Could not find node '{reference}'");
            }

            return result;
        }

        /// <summary>
        /// Get the voltage amplitude (in dB)
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="reference">The reference</param>
        /// <returns></returns>
        public double GetDb(string node, string reference = null)
        {
            Complex r = GetPhasor(node, reference);
            return 10.0 * Math.Log10(r.Real * r.Real + r.Imaginary * r.Imaginary);
        }

        /// <summary>
        /// Get the  phase
        /// </summary>
        /// <param name="node"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public double GetPhase(string node, string reference = null)
        {
            Complex r = GetPhasor(node, reference);
            return 180.0 / Math.PI * Math.Atan2(r.Imaginary, r.Real);
        }

        /// <summary>
        /// Get both amplitude (in dB) and phase
        /// </summary>
        /// <param name="node">The node name</param>
        /// <param name="reference"></param>
        /// <param name="db">The voltage amplitude in decibels</param>
        /// <param name="phase">The phase in degrees</param>
        public void GetDbPhase(string node, string reference, out double db, out double phase)
        {
            Complex r = GetPhasor(node, reference);
            db = 10.0 * Math.Log10(r.Real * r.Real + r.Imaginary * r.Imaginary);
            phase = 180.0 / Math.PI * Math.Atan2(r.Imaginary, r.Real);
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

        /// <summary>
        /// Get an object
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns></returns>
        public ICircuitObject GetObject(string name)
        {
            return Circuit.Objects[name];
        }

        /// <summary>
        /// Ask a component parameter using the circuit
        /// </summary>
        /// <param name="component">The component name</param>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        public double Ask(string component, string parameter)
        {
            IParameterized p = (IParameterized)Circuit.Objects[component];
            return p.Ask(parameter, Circuit);
        }
    }

    /// <summary>
    /// Delegate for initializing a simulation export
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ckt"></param>
    public delegate void InitializeSimulationExportEventHandler(object sender, Circuit ckt);

    /// <summary>
    /// Delegate for exporting simulation data
    /// </summary>
    /// <param name="sender">The simulation sending the event</param>
    /// <param name="data">The simulation data</param>
    public delegate void ExportSimulationDataEventHandler(object sender, SimulationData data);

    /// <summary>
    /// Delegate for finalizing a simulation export
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ckt"></param>
    public delegate void FinalizeSimulationExportEventHandler(object sender, Circuit ckt);
}
