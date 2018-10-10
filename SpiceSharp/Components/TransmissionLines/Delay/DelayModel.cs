namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Delay" /> component.
    /// </summary>
    /// <remarks>
    /// This model is exclusively used to keep track of the history.
    /// </remarks>
    /// <seealso cref="SpiceSharp.Components.Model" />
    public class DelayModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelayModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public DelayModel(string name)
            : base(name)
        {
        }
    }
}
