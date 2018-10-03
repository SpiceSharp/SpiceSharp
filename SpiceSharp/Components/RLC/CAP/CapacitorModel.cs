using SpiceSharp.Components.CapacitorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(string name) : base(name)
        {
            // Register parameters
            ParameterSets.Add(new ModelBaseParameters());
        }
    }
}
