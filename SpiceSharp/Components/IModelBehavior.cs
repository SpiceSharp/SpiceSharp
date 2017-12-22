namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Interface for behaviors that use a model
    /// </summary>
    public interface IModelBehavior
    {
        /// <summary>
        /// Setup the behavior for a specific model
        /// </summary>
        /// <param name="parameters">Parameters of the model</param>
        /// <param name="pool">Pool of behaviors</param>
        void SetupModel(ParametersCollection parameters, BehaviorPool pool);
    }
}
