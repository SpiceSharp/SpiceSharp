using SpiceSharp.Components.CapacitorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a semiconductor <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorModel : Model
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CapacitorModel"/> class.
        /// </summary>
        /// <param name="name"></param>
        public CapacitorModel(string name) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
        }
    }
}
