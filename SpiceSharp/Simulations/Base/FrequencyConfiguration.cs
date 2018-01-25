using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Parameters for a <see cref="FrequencySimulation"/>
    /// </summary>
    public class FrequencyConfiguration : ParameterSet
    {
        /// <summary>
        /// Keep operating point information
        /// </summary>
        public bool KeepOpInfo = false;

        /// <summary>
        /// Gets or sets the number of steps
        /// </summary>
        [PropertyName("steps"), PropertyName("n"), PropertyInfo("The number of steps")]
        public double Steps
        {
            get => NumberSteps;
            set => NumberSteps = (int)(Math.Round(value) + 0.1);
        }
        public int NumberSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the starting frequency
        /// </summary>
        [PropertyName("start"), PropertyInfo("Starting frequency")]
        public double StartFreq { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the stopping frequency
        /// </summary>
        [PropertyName("stop"), PropertyInfo("Stopping frequency")]
        public double StopFreq { get; set; } = 1.0e3;

        /// <summary>
        /// Gets or sets the step type (string version)
        /// </summary>
        [PropertyName("type"), PropertyInfo("The step type")]
        public string _StepType
        {
            get
            {
                switch (StepType)
                {
                    case StepTypes.Linear: return "lin";
                    case StepTypes.Octave: return "oct";
                    case StepTypes.Decade: return "dec";
                }
                return null;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                switch (value.ToLower())
                {
                    case "linear":
                    case "lin": StepType = StepTypes.Linear; break;
                    case "octave":
                    case "oct": StepType = StepTypes.Octave; break;
                    case "decade":
                    case "dec": StepType = StepTypes.Decade; break;
                    default:
                        throw new CircuitException($"Invalid step type {value}");
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of step used
        /// </summary>
        public StepTypes StepType { get; set; } = StepTypes.Decade;

        /// <summary>
        /// Constructor
        /// </summary>
        public FrequencyConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n">Number of steps</param>
        /// <param name="start">Starting frequency</param>
        /// <param name="stop">Stopping frequency</param>
        public FrequencyConfiguration(string steptype, int n, double start, double stop)
        {
            switch (steptype)
            {
                case "lin":
                case "linear": StepType = StepTypes.Linear; break;
                case "dec":
                case "decade": StepType = StepTypes.Decade; break;
                case "oct":
                case "octave": StepType = StepTypes.Octave; break;
                default:
                    throw new CircuitException($"Invalid step type \"{steptype}\"");
            }

            NumberSteps = n;
            StartFreq = start;
            StopFreq = stop;
        }
    }
    /// <summary>
    /// Enumerations
    /// </summary>
    public enum StepTypes { Decade, Octave, Linear };
}
