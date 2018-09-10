using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations.IntegrationMethods.Timesteps
{
    /// <summary>
    /// Class capable of managing timesteps and time
    /// </summary>
    public class Timesteps
    {
        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; protected set; }

        /// <summary>
        /// Gets the current timestep
        /// </summary>
        public double Delta { get; protected set; }

        /// <summary>
        /// Gets the history of timesteps
        /// </summary>
        public ReadOnlyHistory<double> DeltaHistory { get; }

        /// <summary>
        /// Get a timestep in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public double this[int index] => DeltaOld[index];

        /// <summary>
        /// Gets the history of timesteps
        /// </summary>
        protected ArrayHistory<double> DeltaOld { get; }

        /// <summary>
        /// Private variables
        /// </summary>
        protected double SaveTime { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="history">Number of points needed</param>
        public Timesteps(int history)
        {
            DeltaOld = new ArrayHistory<double>(history);
            DeltaHistory = new ReadOnlyHistory<double>(DeltaOld);
        }

        /// <summary>
        /// Initialize the controller
        /// The history is cleared with the initial timestep
        /// </summary>
        /// <param name="delta">Initial timestep</param>
        public virtual void Initialize(double delta)
        {
            DeltaOld.Clear(i => delta);
            Delta = delta;
            SaveTime = 0.0;
            Time = 0.0;
        }

        /// <summary>
        /// Probe a new time point
        /// </summary>
        /// <param name="delta">Timestep</param>
        public virtual void Probe(double delta)
        {
            Delta = delta;
            Time = SaveTime + delta;
            DeltaOld.Current = delta;
        }

        /// <summary>
        /// Accept the new time point
        /// The previous timestep will be used as the new timestep
        /// </summary>
        public virtual void Accept()
        {
            // Flag the current timepoint as accepted
            SaveTime = Time;
            DeltaOld.Store(Delta);
        }
    }
}
