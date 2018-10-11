using SpiceSharp.Components.DelayBehaviors;

namespace SpiceSharp.Components
{
    public class VoltageDelayModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDelayModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public VoltageDelayModel(string name)
            : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
        }
    }
}
